// Fichier : Pages/SystemInfoPage.xaml.cs
// Auteur : Le Geek Zen
// Description : Code-behind pour la page d'information système. Gère l'affichage et les actions liées à l'état du système et de WSL.
//
// Emplacement : WSLManagerApp/Pages/SystemInfoPage.xaml.cs

using System.Windows.Controls;
using System.Windows;
using System.Threading.Tasks;
using WSLManagerApp.Helpers;
using System.Text.RegularExpressions;
using System.Security.Principal;

namespace WSLManagerApp.Pages
{
    public partial class SystemInfoPage : UserControl
    {
        [GeneratedRegex(@"""WindowsProductName"":\s*""([^""]+)""")]
        private static partial Regex WindowsProductNameRegex();

        [GeneratedRegex(@"""CurrentBuild"":\s*""([^""]+)""")]
        private static partial Regex CurrentBuildRegex();

        [GeneratedRegex(@"""UBR"":\s*""([^""]+)""")]
        private static partial Regex UBRRegex();

        [GeneratedRegex(@"Version WSL\s*:\s*([^\r\n]+)")]
        private static partial Regex WSLVersionFrenchRegex();

        [GeneratedRegex(@"WSL\s+version\s+([^\s]+)")]
        private static partial Regex WSLVersionEnglishRegex();

        [GeneratedRegex(@"""Status"":\s*""([^""]+)""")]
        private static partial Regex ServiceStatusStringRegex();

        [GeneratedRegex(@"""Status"":\s*(\d+)")]
        private static partial Regex ServiceStatusNumberRegex();

        public SystemInfoPage()
        {
            InitializeComponent();
            Loaded += SystemInfoPage_Loaded;
            
            // Ajouter les gestionnaires d'événements pour les boutons
            BtnInstallWSLFeature.Click += BtnInstallWSLFeature_Click;
            BtnRemoveWSLFeature.Click += BtnRemoveWSLFeature_Click;
            BtnInstallVMPlatform.Click += BtnInstallVMPlatform_Click;
            BtnRemoveVMPlatform.Click += BtnRemoveVMPlatform_Click;
            BtnUpdateWSL.Click += BtnUpdateWSL_Click;
            BtnShutdownWSL.Click += BtnShutdownWSL_Click;
        }

        /// <summary>
        /// Charge les informations système au chargement de la page.
        /// </summary>
        private async void SystemInfoPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadSystemInfoAsync();
        }

        /// <summary>
        /// Charge et affiche toutes les informations système.
        /// </summary>
        private async Task LoadSystemInfoAsync()
        {
            try
            {
                // Charger les informations en parallèle pour de meilleures performances
                var windowsVersionTask = GetWindowsVersionAsync();
                var wslFeatureTask = CheckWSLFeatureAsync();
                var vmPlatformTask = CheckVMPlatformAsync();
                var wslInfoTask = GetWSLInfoAsync();
                var wslServiceTask = GetWSLServiceStatusAsync();

                // Attendre que toutes les tâches se terminent
                await Task.WhenAll(windowsVersionTask, wslFeatureTask, vmPlatformTask, wslInfoTask, wslServiceTask);

                // Afficher les résultats
                TxtWindowsVersion.Text = await windowsVersionTask;
                var wslFeature = await wslFeatureTask;
                TxtWSLFeature.Text = wslFeature;
                UpdateWSLFeatureButtons(wslFeature == "Activé");

                var vmPlatform = await vmPlatformTask;
                TxtVMPlatform.Text = vmPlatform;
                UpdateVMPlatformButtons(vmPlatform == "Activé");

                var wslInfo = await wslInfoTask;
                TxtWSLInstalled.Text = wslInfo.Installed ? "Oui" : "Non";
                TxtWSLVersion.Text = wslInfo.Version;
                
                var wslServiceStatus = await wslServiceTask;
                TxtWSLServiceStatus.Text = wslServiceStatus;
                
                // Mettre à jour l'état des boutons selon les privilèges administrateur et l'état du service
                UpdateAdminButtons();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des informations système :\n{ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Récupère la version de Windows.
        /// </summary>
        private static async Task<string> GetWindowsVersionAsync()
        {
            try
            {
                // Récupérer le nom du produit
                string productInfo = await PowerShellHelper.RunCommandAsync("Get-ComputerInfo | Select-Object WindowsProductName | ConvertTo-Json");
                var productMatch = WindowsProductNameRegex().Match(productInfo);
                
                if (productMatch.Success)
                {
                    string productName = productMatch.Groups[1].Value;
                    
                    // Récupérer le numéro de build depuis la clé de registre
                    string buildInfo = await PowerShellHelper.RunCommandAsync("Get-ItemProperty 'HKLM:\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion' | Select-Object CurrentBuild, UBR | ConvertTo-Json");
                    var buildMatch = CurrentBuildRegex().Match(buildInfo);
                    var ubrMatch = UBRRegex().Match(buildInfo);
                    
                    if (buildMatch.Success && ubrMatch.Success)
                    {
                        string currentBuild = buildMatch.Groups[1].Value;
                        string ubr = ubrMatch.Groups[1].Value;
                        string fullBuild = $"{currentBuild}.{ubr}";
                        
                        return $"{productName} (Build {fullBuild})";
                    }
                    else
                    {
                        // Fallback avec OSVersion - extraire seulement Build.Revision
                        string buildNumber = await PowerShellHelper.RunCommandAsync("[System.Environment]::OSVersion.Version.Build");
                        string revisionNumber = await PowerShellHelper.RunCommandAsync("[System.Environment]::OSVersion.Version.Revision");
                        if (!string.IsNullOrWhiteSpace(buildNumber) && !string.IsNullOrWhiteSpace(revisionNumber))
                        {
                            return $"{productName} (Build {buildNumber.Trim()}.{revisionNumber.Trim()})";
                        }
                    }
                }
                return "Non détecté";
            }
            catch
            {
                return "Erreur de détection";
            }
        }

        /// <summary>
        /// Vérifie si la fonctionnalité WSL est activée.
        /// </summary>
        private static async Task<string> CheckWSLFeatureAsync()
        {
            try
            {
                // Essayer d'abord avec Get-WindowsOptionalFeature
                string output = await PowerShellHelper.RunCommandAsync("Get-WindowsOptionalFeature -Online -FeatureName Microsoft-Windows-Subsystem-Linux -ErrorAction SilentlyContinue");
                if (output.Contains("Enabled"))
                    return "Activé";
                else if (output.Contains("Disabled"))
                    return "Désactivé";
                
                // Fallback : vérifier via la clé de registre
                string regOutput = await PowerShellHelper.RunCommandAsync("Get-ItemProperty 'HKLM:\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Component Based Servicing\\Packages' -Name 'Microsoft-Windows-Subsystem-Linux*' -ErrorAction SilentlyContinue | Select-Object -First 1");
                if (!string.IsNullOrWhiteSpace(regOutput) && regOutput.Contains("Microsoft-Windows-Subsystem-Linux"))
                    return "Activé";
                
                // Fallback : vérifier via DISM
                string dismOutput = await PowerShellHelper.RunCommandAsync("dism /online /get-featureinfo /featurename:Microsoft-Windows-Subsystem-Linux");
                if (dismOutput.Contains("État : Activé"))
                    return "Activé";
                else if (dismOutput.Contains("État : Désactivé"))
                    return "Désactivé";
                
                return "Non installé";
            }
            catch
            {
                return "Erreur de détection";
            }
        }

        /// <summary>
        /// Vérifie si la plateforme de machine virtuelle est activée.
        /// </summary>
        private static async Task<string> CheckVMPlatformAsync()
        {
            try
            {
                // Essayer d'abord avec Get-WindowsOptionalFeature
                string output = await PowerShellHelper.RunCommandAsync("Get-WindowsOptionalFeature -Online -FeatureName VirtualMachinePlatform -ErrorAction SilentlyContinue");
                if (output.Contains("Enabled"))
                    return "Activé";
                else if (output.Contains("Disabled"))
                    return "Désactivé";
                
                // Fallback : vérifier via la clé de registre
                string regOutput = await PowerShellHelper.RunCommandAsync("Get-ItemProperty 'HKLM:\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Component Based Servicing\\Packages' -Name 'Microsoft-Windows-VirtualMachinePlatform*' -ErrorAction SilentlyContinue | Select-Object -First 1");
                if (!string.IsNullOrWhiteSpace(regOutput) && regOutput.Contains("Microsoft-Windows-VirtualMachinePlatform"))
                    return "Activé";
                
                // Fallback : vérifier via DISM
                string dismOutput = await PowerShellHelper.RunCommandAsync("dism /online /get-featureinfo /featurename:VirtualMachinePlatform");
                if (dismOutput.Contains("État : Activé"))
                    return "Activé";
                else if (dismOutput.Contains("État : Désactivé"))
                    return "Désactivé";
                
                return "Non installé";
            }
            catch
            {
                return "Erreur de détection";
            }
        }

        /// <summary>
        /// Récupère les informations sur WSL installé.
        /// </summary>
        private static async Task<(bool Installed, string Version)> GetWSLInfoAsync()
        {
            try
            {
                string output = await PowerShellHelper.RunCommandAsync("wsl --version");
                if (output.Contains("WSL"))
                {
                    // Gestion des formats français et anglais
                    var match = WSLVersionFrenchRegex().Match(output);
                    if (!match.Success)
                    {
                        // Fallback pour le format anglais
                        match = WSLVersionEnglishRegex().Match(output);
                    }
                    
                    if (match.Success)
                        return (true, match.Groups[1].Value.Trim());
                    else
                        return (true, "Version inconnue");
                }
                else
                {
                    return (false, "Non installé");
                }
            }
            catch
            {
                return (false, "Erreur de détection");
            }
        }

        /// <summary>
        /// Récupère l'état du service WSL.
        /// </summary>
        private static async Task<string> GetWSLServiceStatusAsync()
        {
            try
            {
                // Récupérer l'état du service WSLService
                string wslServiceOutput = await PowerShellHelper.RunCommandAsync("Get-Service -Name 'WSLService' -ErrorAction SilentlyContinue | Select-Object Status | ConvertTo-Json -Compress");
                
                string wslServiceStatus = "Non trouvé";
                
                // Parser WSLService - gérer les valeurs numériques et textuelles
                if (!string.IsNullOrWhiteSpace(wslServiceOutput) && wslServiceOutput != "null")
                {
                    // Essayer d'abord de parser comme une chaîne
                    var stringMatch = ServiceStatusStringRegex().Match(wslServiceOutput);
                    if (stringMatch.Success)
                    {
                        wslServiceStatus = stringMatch.Groups[1].Value switch
                        {
                            "Running" => "Démarré",
                            "Stopped" => "Arrêté",
                            "Paused" => "En pause",
                            _ => stringMatch.Groups[1].Value
                        };
                    }
                    else
                    {
                        // Parser comme un nombre
                        var numberMatch = ServiceStatusNumberRegex().Match(wslServiceOutput);
                        if (numberMatch.Success)
                        {
                            int statusCode = int.Parse(numberMatch.Groups[1].Value);
                            wslServiceStatus = statusCode switch
                            {
                                1 => "Arrêté",
                                2 => "Démarré",
                                3 => "En pause",
                                4 => "Démarré", // Code 4 = Running pour WSLService
                                5 => "Arrêt",
                                _ => $"Code {statusCode}"
                            };
                        }
                    }
                }
                
                return wslServiceStatus != "Non trouvé" ? wslServiceStatus : "Non trouvé";
            }
            catch
            {
                return "Erreur de détection";
            }
        }

        /// <summary>
        /// Met à jour l'état des boutons de fonctionnalité WSL.
        /// </summary>
        private void UpdateWSLFeatureButtons(bool isEnabled)
        {
            BtnInstallWSLFeature.IsEnabled = !isEnabled;
            BtnRemoveWSLFeature.IsEnabled = isEnabled;
        }

        /// <summary>
        /// Met à jour l'état des boutons de plateforme VM.
        /// </summary>
        private void UpdateVMPlatformButtons(bool isEnabled)
        {
            BtnInstallVMPlatform.IsEnabled = !isEnabled;
            BtnRemoveVMPlatform.IsEnabled = isEnabled;
        }

        /// <summary>
        /// Vérifie si l'application s'exécute avec des privilèges administrateur.
        /// </summary>
        private static bool IsRunningAsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        /// <summary>
        /// Met à jour l'état des boutons selon les privilèges administrateur et l'état du service WSL.
        /// </summary>
        private void UpdateAdminButtons()
        {
            bool isAdmin = IsRunningAsAdministrator();
            
            if (!isAdmin)
            {
                // Désactiver tous les boutons d'installation/suppression
                BtnInstallWSLFeature.IsEnabled = false;
                BtnRemoveWSLFeature.IsEnabled = false;
                BtnInstallVMPlatform.IsEnabled = false;
                BtnRemoveVMPlatform.IsEnabled = false;
                
                // Ajouter un indicateur visuel plus court avec ToolTip
                if (!string.IsNullOrEmpty(TxtWSLFeature.Text))
                {
                    TxtWSLFeature.Text += " ⚠️";
                    TxtWSLFeature.ToolTip = "Privilèges administrateur requis pour installer/supprimer cette fonctionnalité";
                }
                if (!string.IsNullOrEmpty(TxtVMPlatform.Text))
                {
                    TxtVMPlatform.Text += " ⚠️";
                    TxtVMPlatform.ToolTip = "Privilèges administrateur requis pour installer/supprimer cette fonctionnalité";
                }
            }
            
            // Mettre à jour les boutons de service WSL selon l'état du service
            UpdateWSLServiceButtons();
        }
        
        /// <summary>
        /// Met à jour l'état des boutons de service WSL selon l'état du service.
        /// </summary>
        private void UpdateWSLServiceButtons()
        {
            bool isAdmin = IsRunningAsAdministrator();
            string serviceStatus = TxtWSLServiceStatus.Text;
            
            if (isAdmin)
            {
                // Activer/désactiver les boutons selon l'état du service
                bool isServiceRunning = serviceStatus.Contains("Démarré");
                
                BtnStartWSLService.IsEnabled = !isServiceRunning; // Grisé si démarré
                BtnShutdownWSL.IsEnabled = isServiceRunning;      // Grisé si arrêté
            }
            else
            {
                // Désactiver les boutons si pas d'admin
                BtnStartWSLService.IsEnabled = false;
                BtnShutdownWSL.IsEnabled = false;
            }
        }

        /// <summary>
        /// Handler pour installer la fonctionnalité WSL.
        /// </summary>
        private async void BtnInstallWSLFeature_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string result = await PowerShellHelper.RunCommandAsync("Enable-WindowsOptionalFeature -Online -FeatureName Microsoft-Windows-Subsystem-Linux -All");
                MessageBox.Show(result, "Installation WSL", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadSystemInfoAsync(); // Recharger les informations
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'installation de WSL :\n{ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handler pour supprimer la fonctionnalité WSL.
        /// </summary>
        private async void BtnRemoveWSLFeature_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Êtes-vous sûr de vouloir supprimer la fonctionnalité WSL ?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            try
            {
                string result = await PowerShellHelper.RunCommandAsync("Disable-WindowsOptionalFeature -Online -FeatureName Microsoft-Windows-Subsystem-Linux");
                MessageBox.Show(result, "Suppression WSL", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadSystemInfoAsync(); // Recharger les informations
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Erreur lors de la suppression de WSL :\n{ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handler pour installer la plateforme de machine virtuelle.
        /// </summary>
        private async void BtnInstallVMPlatform_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string result = await PowerShellHelper.RunCommandAsync("Enable-WindowsOptionalFeature -Online -FeatureName VirtualMachinePlatform -All");
                MessageBox.Show(result, "Installation VirtualMachinePlatform", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadSystemInfoAsync(); // Recharger les informations
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'installation de VirtualMachinePlatform :\n{ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handler pour supprimer la plateforme de machine virtuelle.
        /// </summary>
        private async void BtnRemoveVMPlatform_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Êtes-vous sûr de vouloir supprimer la plateforme de machine virtuelle ?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            try
            {
                string result = await PowerShellHelper.RunCommandAsync("Disable-WindowsOptionalFeature -Online -FeatureName VirtualMachinePlatform");
                MessageBox.Show(result, "Suppression VirtualMachinePlatform", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadSystemInfoAsync(); // Recharger les informations
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Erreur lors de la suppression de VirtualMachinePlatform :\n{ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handler pour mettre à jour WSL.
        /// </summary>
        private async void BtnUpdateWSL_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string result = await PowerShellHelper.RunCommandAsync("wsl --update");
                MessageBox.Show(result, "Mise à jour WSL", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadSystemInfoAsync(); // Recharger les informations
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Erreur lors de la mise à jour de WSL :\n{ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handler pour démarrer le service WSL.
        /// </summary>
        private async void BtnStartWSLService_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Essayer de démarrer WSL via la commande wsl
                string result = await PowerShellHelper.RunCommandAsync("wsl --status");
                
                // Si WSL fonctionne, essayer de démarrer le service WSLService
                if (!result.Contains("error") && !result.Contains("Error"))
                {
                    await PowerShellHelper.RunCommandAsync("Start-Service -Name 'WSLService'");
                    MessageBox.Show("Le service WSL a été démarré avec succès.", "Démarrage du service WSL", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Si WSL ne fonctionne pas, essayer de l'activer
                    await PowerShellHelper.RunCommandAsync("wsl --install --no-distribution --web-download");
                    MessageBox.Show("WSL a été activé. Un redémarrage peut être nécessaire.", "Activation WSL", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                
                await LoadSystemInfoAsync(); // Recharger les informations
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'activation de WSL :\n{ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handler pour arrêter le service WSL.
        /// </summary>
        private async void BtnShutdownWSL_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Êtes-vous sûr de vouloir arrêter le service WSL ? Cela arrêtera complètement WSL.", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            try
            {
                // D'abord arrêter les distributions
                await PowerShellHelper.RunCommandAsync("wsl --shutdown");
                
                // Puis arrêter le service WSLService
                await PowerShellHelper.RunCommandAsync("Stop-Service -Name 'WSLService' -Force");
                MessageBox.Show("Le service WSL a été arrêté avec succès.", "Arrêt du service WSL", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadSystemInfoAsync(); // Recharger les informations
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'arrêt du service WSL :\n{ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
} 