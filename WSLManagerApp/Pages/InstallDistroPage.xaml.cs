// Fichier : Pages/InstallDistroPage.xaml.cs
// Auteur : Le Geek Zen
// Description : Code-behind pour la page d'installation des distributions WSL.
// Emplacement : WSLManagerApp/Pages/InstallDistroPage.xaml.cs
//
// Notes perso :
// - Le parsing de la liste des distributions découpe sur 2 espaces ou plus (voir ParseDistroList).
// - Le nom personnalisé ne doit pas contenir d'espace ni de caractères spéciaux (voir validation avant installation).
// - Si la distribution est "héritée", on relance l'installation sans --name/--location et on prévient l'utilisateur.
// - Le titre du GroupBox change dynamiquement selon la sélection.
// - La ProgressBar s'affiche pendant l'installation.
// - Penser à bien gérer l'état des boutons pour éviter les doubles clics.

using System.Windows.Controls;
using System.Windows;
using System.Threading.Tasks;
using WSLManagerApp.Helpers;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace WSLManagerApp.Pages
{
    public partial class InstallDistroPage : UserControl
    {
        [GeneratedRegex(@" {2,}")]
        private static partial Regex GeneratedRegex();

        [GeneratedRegex(@"^[A-Za-z0-9_-]+$")]
        private static partial Regex CustomNameRegex();

        // Classe pour lier le nom technique et le nom lisible
        private class DistroItem
        {
            public required string Name { get; set; }
            public required string FriendlyName { get; set; }
            public override string ToString() => FriendlyName;
        }

        public InstallDistroPage()
        {
            InitializeComponent();
            Loaded += InstallDistroPage_Loaded;
            BtnRefreshList.Click += BtnRefreshList_Click;
            BtnInstallDistro.Click += BtnInstallDistro_Click;
            ComboDistros.SelectionChanged += ComboDistros_SelectionChanged;
        }

        private async void InstallDistroPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDistroListAsync();
        }

        private async void BtnRefreshList_Click(object sender, RoutedEventArgs e)
        {
            await LoadDistroListAsync();
        }

        private async Task LoadDistroListAsync()
        {
            ComboDistros.ItemsSource = null;
            ComboDistros.Items.Clear();
            ComboDistros.Items.Add("Chargement en cours...");
            ComboDistros.SelectedIndex = 0;
            BtnRefreshList.IsEnabled = false;

            try
            {
                string output = await PowerShellHelper.RunCommandAsync("wsl --list --online");
                var distros = ParseDistroList(output);
                ComboDistros.Items.Clear();
                if (distros.Count > 0)
                {
                    foreach (var distro in distros)
                        ComboDistros.Items.Add(distro);
                    ComboDistros.SelectedIndex = 0;
                }
                else
                {
                    ComboDistros.Items.Add("Aucune distribution trouvée");
                    ComboDistros.SelectedIndex = 0;
                }
            }
            catch
            {
                ComboDistros.Items.Clear();
                ComboDistros.Items.Add("Erreur lors du chargement");
                ComboDistros.SelectedIndex = 0;
            }
            finally
            {
                BtnRefreshList.IsEnabled = true;
            }
        }

        /// <summary>
        /// Parse la sortie de wsl --list --online pour extraire la liste des distributions disponibles.
        /// Découpe chaque ligne sur deux espaces ou plus pour séparer NAME et FRIENDLY NAME.
        /// </summary>
        private static List<DistroItem> ParseDistroList(string output)
        {
            var distros = new List<DistroItem>();
            bool start = false;
            foreach (var line in output.Split('\n'))
            {
                var trimmed = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmed)) continue;
                if (trimmed.StartsWith("NAME") && trimmed.Contains("FRIENDLY NAME"))
                {
                    start = true;
                    continue;
                }
                if (!start) continue;

                // Découpe sur 2 espaces ou plus
                var parts = GeneratedRegex().Split(trimmed);
                if (parts.Length >= 2)
                {
                    distros.Add(new DistroItem
                    {
                        Name = parts[0].Trim(),
                        FriendlyName = parts[1].Trim()
                    });
                }
            }
            return distros;
        }

        /// <summary>
        /// Handler du clic sur le bouton d'installation. Valide les champs, construit la commande,
        /// gère les cas hérités, affiche les résultats et gère l'état de l'UI.
        /// </summary>
        private async void BtnInstallDistro_Click(object sender, RoutedEventArgs e)
        {
            if (ComboDistros.SelectedItem is not DistroItem selected)
            {
                MessageBox.Show("Veuillez sélectionner une distribution.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string name = selected.Name;
            string customName = TxtCustomName.Text.Trim();
            string location = TxtLocation.Text.Trim();
            bool launch = ChkLaunchAfterInstall.IsChecked == true;

            // Validation du nom personnalisé
            if (!string.IsNullOrEmpty(customName))
            {
                if (!CustomNameRegex().IsMatch(customName))
                {
                    MessageBox.Show(
                        "Le nom personnalisé ne doit contenir que des lettres, chiffres, tirets ou underscores, et aucun espace.",
                        "Nom personnalisé invalide",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    BtnInstallDistro.IsEnabled = true;
                    BtnInstallDistro.Content = "Installer la distribution";
                    return;
                }
            }

            // Construction de la commande
            string command = $"wsl --install -d {name}";
            if (!string.IsNullOrEmpty(customName))
                command += $" --name \"{customName}\"";
            if (!string.IsNullOrEmpty(location))
                command += $" --location \"{location}\"";
            command += " --web-download";
            if (!launch)
                command += " --no-launch";

            BtnInstallDistro.IsEnabled = false;
            BtnInstallDistro.Content = "Installation en cours...";
            InstallProgressBar.Visibility = Visibility.Visible;

            try
            {
                string result = await PowerShellHelper.RunCommandAsync(command);

                // Si erreur héritée, relancer sans --name et --location
                if (result.Contains("n’est pas pris en charge lors de l’installation des distributions héritées") ||
                    result.Contains("is not supported when installing legacy distributions"))
                {
                    MessageBox.Show(
                        "L'option --name ou --location n'est pas supportée pour cette distribution.\nL'installation va être relancée sans ces options.",
                        "Distribution héritée détectée", MessageBoxButton.OK, MessageBoxImage.Information);

                    string fallbackCommand = $"wsl --install -d {name} --web-download";
                    if (!launch)
                        fallbackCommand += " --no-launch";
                    string fallbackResult = await PowerShellHelper.RunCommandAsync(fallbackCommand);

                    MessageBox.Show(fallbackResult, "Résultat de l'installation (mode héritée)", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(result, "Résultat de l'installation", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'installation :\n{ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                BtnInstallDistro.IsEnabled = true;
                BtnInstallDistro.Content = "Installer la distribution";
                InstallProgressBar.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Met à jour dynamiquement le titre du GroupBox selon la distribution sélectionnée.
        /// </summary>
        private void ComboDistros_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GroupBoxInstall.Header = ComboDistros.SelectedItem is DistroItem selected 
                ? $"Installation de la distribution {selected.FriendlyName}"
                : "Installation de la distribution";
        }

        /// <summary>
        /// Nettoie la sortie PowerShell/WSL pour harmoniser les retours à la ligne et supprimer les caractères parasites.
        /// </summary>
        private static string CleanOutput(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            // Remplace les retours chariot/ligne spéciaux par des vrais \n
            return input
                .Replace("\r\n", "\n")
                .Replace("\r", "\n")
                .Replace("\u2028", "\n") // Séparateur de ligne unicode
                .Replace("\u2029", "\n") // Séparateur de paragraphe unicode
                .Replace("\u0085", "\n") // Next line unicode
                .Replace("\u000B", "\n") // Vertical tab
                .Replace("\u000C", "\n") // Form feed
                .Replace("\u0A0D", "\n") // <--- Ajout pour ton cas
                .Replace("\u0A0A", "\n") // <--- Ajout pour ton cas
                .Trim();
        }
    }
} 