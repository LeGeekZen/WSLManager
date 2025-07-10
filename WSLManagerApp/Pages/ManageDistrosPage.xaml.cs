// Fichier : Pages/ManageDistrosPage.xaml.cs
// Auteur : Le Geek Zen
// Description : Code-behind pour la page de gestion des distributions installées.
// Emplacement : WSLManagerApp/Pages/ManageDistrosPage.xaml.cs
//
// Notes perso :
// - Affiche la liste des distributions installées avec leur statut, par défaut, etc.
// - Le parsing découpe la sortie de 'wsl --list --verbose' sur les espaces multiples.

using System.Windows.Controls;
using System.Windows;
using System.Threading.Tasks;
using WSLManagerApp.Helpers;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace WSLManagerApp.Pages
{
    public partial class ManageDistrosPage : UserControl
    {
        [GeneratedRegex(@" {2,}")]
        private static partial Regex GeneratedRegex();

        // Représente une distribution installée
        private class DistroInfo
        {
            public string? Name { get; set; }
            public  string? State { get; set; }
            public string? Version { get; set; }
            public bool IsDefault { get; set; }
            public string? OriginalState { get; set; } // Pour la logique des boutons
        }

        public ManageDistrosPage()
        {
            InitializeComponent();
            Loaded += ManageDistrosPage_Loaded;
            DataGridDistros.SelectionChanged += DataGridDistros_SelectionChanged;
            BtnRefresh.Click += BtnRefresh_Click;
            BtnStartDistro.Click += BtnStartDistro_Click;
            BtnStopDistro.Click += BtnStopDistro_Click;
            BtnSetDefault.Click += BtnSetDefault_Click;
            BtnUnregisterDistro.Click += BtnUnregisterDistro_Click;
            BtnUpgradeToWSL2.Click += BtnUpgradeToWSL2_Click;
        }

        /// <summary>
        /// Met à jour l'état des boutons d'action selon la distribution sélectionnée.
        /// </summary>
        private void UpdateActionButtons()
        {
            if (DataGridDistros.SelectedItem is not DistroInfo selected || 
                string.IsNullOrWhiteSpace(selected.Name) || 
                selected.Name is "Chargement..." or "Erreur lors du chargement")
            {
                BtnStartDistro.IsEnabled = false;
                BtnStopDistro.IsEnabled = false;
                BtnSetDefault.IsEnabled = false;
                BtnUnregisterDistro.IsEnabled = false;
                BtnUpgradeToWSL2.IsEnabled = false;
                return;
            }

            BtnStartDistro.IsEnabled = selected.OriginalState == "Stopped";
            BtnStopDistro.IsEnabled = selected.OriginalState == "Running";
            BtnSetDefault.IsEnabled = !selected.IsDefault;
            BtnUnregisterDistro.IsEnabled = selected.OriginalState != "Running";
            BtnUpgradeToWSL2.IsEnabled = selected.Version != "2";
        }

        private void DataGridDistros_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateActionButtons();
        }

        /// <summary>
        /// Charge la liste des distributions installées et les affiche dans le DataGrid.
        /// </summary>
        private async void ManageDistrosPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadInstalledDistrosAsync();
        }

        private async Task LoadInstalledDistrosAsync()
        {
            DataGridDistros.ItemsSource = null;
            DataGridDistros.Items.Clear();
            DataGridDistros.Items.Add(new DistroInfo 
            { 
                Name = "Chargement...", 
                State = "", 
                Version = "", 
                OriginalState = "",
                IsDefault = false 
            });

            try
            {
                string output = await PowerShellHelper.RunCommandAsync("wsl --list --verbose");
                var distros = ParseInstalledDistros(output);
                DataGridDistros.Items.Clear();
                foreach (var distro in distros)
                    DataGridDistros.Items.Add(distro);
                // Met à jour les boutons après chargement
                UpdateActionButtons();
            }
            catch
            {
                DataGridDistros.Items.Clear();
                DataGridDistros.Items.Add(new DistroInfo 
                { 
                    Name = "Erreur lors du chargement", 
                    State = "", 
                    Version = "", 
                    OriginalState = "",
                    IsDefault = false 
                });
                UpdateActionButtons();
            }
        }

        /// <summary>
        /// Parse la sortie de 'wsl --list --verbose' pour extraire les infos des distributions installées.
        /// </summary>
        private static List<DistroInfo> ParseInstalledDistros(string output)
        {
            var distros = new List<DistroInfo>();
            bool start = false;
            foreach (var line in output.Split('\n'))
            {
                var trimmed = line.TrimEnd('\r').Trim();
                if (string.IsNullOrWhiteSpace(trimmed)) continue;
                if (trimmed.StartsWith("NAME") && trimmed.Contains("STATE") && trimmed.Contains("VERSION"))
                {
                    start = true;
                    continue;
                }
                if (!start) continue;

                // Découpe sur 2 espaces ou plus
                var parts = GeneratedRegex().Split(trimmed);
                if (parts.Length >= 3)
                {
                    bool isDefault = false;
                    string name = parts[0].Trim();
                    string state = parts[1].Trim();
                    string version = parts[2].Trim();

                    // Si le nom commence par '*', c'est la distro par défaut
                    if (name.StartsWith('*'))
                    {
                        isDefault = true;
                        name = name.TrimStart('*').Trim();
                    }

                    // Traduction des états pour l'affichage
                    string displayState = state switch
                    {
                        "Running" => "Démarré",
                        "Stopped" => "Arrêté",
                        _ => state
                    };

                    if (!string.IsNullOrEmpty(name))
                    {
                        distros.Add(new DistroInfo
                        {
                            Name = name,
                            State = displayState, // Affichage traduit
                            OriginalState = state, // État original pour la logique
                            Version = version,
                            IsDefault = isDefault
                        });
                    }
                }
            }
            return distros;
        }

        /// <summary>
        /// Handler pour lancer la distribution sélectionnée.
        /// </summary>
        private async void BtnStartDistro_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridDistros.SelectedItem is not DistroInfo selected) return;
            string command = $"wsl --distribution {selected.Name}";
            try
            {
                string result = await PowerShellHelper.RunCommandAsync(command);
                MessageBox.Show(result, $"Lancement de {selected.Name}", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadInstalledDistrosAsync();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Erreur lors du lancement :\n{ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handler pour arrêter la distribution sélectionnée.
        /// </summary>
        private async void BtnStopDistro_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridDistros.SelectedItem is not DistroInfo selected) return;
            string command = $"wsl --terminate {selected.Name}";
            try
            {
                string result = await PowerShellHelper.RunCommandAsync(command);
                MessageBox.Show(result, $"Arrêt de {selected.Name}", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadInstalledDistrosAsync();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'arrêt :\n{ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handler pour supprimer la distribution sélectionnée.
        /// </summary>
        private async void BtnUnregisterDistro_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridDistros.SelectedItem is not DistroInfo selected) return;
            if (MessageBox.Show($"Êtes-vous sûr de vouloir supprimer la distribution '{selected.Name}' ? Cette action est irréversible.", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;
            string command = $"wsl --unregister {selected.Name}";
            try
            {
                string result = await PowerShellHelper.RunCommandAsync(command);
                MessageBox.Show(result, $"Suppression de {selected.Name}", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadInstalledDistrosAsync();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Erreur lors de la suppression :\n{ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handler pour définir la distribution sélectionnée comme par défaut.
        /// </summary>
        private async void BtnSetDefault_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridDistros.SelectedItem is not DistroInfo selected) return;
            string command = $"wsl --set-default {selected.Name}";
            try
            {
                string result = await PowerShellHelper.RunCommandAsync(command);
                MessageBox.Show(result, $"Définir {selected.Name} par défaut", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadInstalledDistrosAsync();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Erreur lors du changement de distribution par défaut :\n{ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handler pour mettre à jour la distribution sélectionnée vers WSL2.
        /// </summary>
        private async void BtnUpgradeToWSL2_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridDistros.SelectedItem is not DistroInfo selected) return;
            string command = $"wsl --set-version {selected.Name} 2";
            try
            {
                string result = await PowerShellHelper.RunCommandAsync(command);
                MessageBox.Show(result, $"Mise à jour de {selected.Name} vers WSL2", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadInstalledDistrosAsync();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Erreur lors de la mise à jour vers WSL2 :\n{ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handler pour rafraîchir manuellement la liste des distributions installées.
        /// </summary>
        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadInstalledDistrosAsync();
        }
    }
}