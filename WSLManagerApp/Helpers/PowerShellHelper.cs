// Fichier : Helpers/PowerShellHelper.cs
// Auteur : Le Geek Zen
// Description : Fournit une méthode utilitaire pour exécuter des commandes PowerShell ou WSL de façon asynchrone et récupérer la sortie, en garantissant l'encodage UTF-8 ou CP850/Unicode selon le contexte.
// Emplacement : WSLManagerApp/Helpers/PowerShellHelper.cs
//
// Notes perso :
// - Si la commande commence par "wsl", on exécute wsl.exe directement (plus fiable pour l'encodage).
// - Pour wsl.exe, il faut tester l'encodage : UTF-16 (Unicode) sur certains Windows, CP850 sur d'autres.
// - Pour les autres commandes, on passe par PowerShell et Set-Content -Encoding utf8.
// - CleanOutput harmonise les retours à la ligne et supprime les caractères parasites.
// - Si jamais la sortie est vide, vérifier l'encodage ou tester la commande en console.
//
// Astuce debug :
// - Pour voir la sortie brute, écrire output dans un fichier temporaire avant CleanOutput.

using System.Diagnostics;
using System.Threading.Tasks;
using System.Text;
using System.IO;

namespace WSLManagerApp.Helpers
{
    public static class PowerShellHelper
    {
        /// <summary>
        /// Exécute une commande PowerShell ou WSL de façon asynchrone et retourne la sortie texte.
        /// Utilise wsl.exe direct si la commande commence par 'wsl', sinon PowerShell avec Set-Content.
        /// Gère l'encodage selon le contexte (UTF-8, CP850, Unicode).
        /// </summary>
        public static async Task<string> RunCommandAsync(string command)
        {
            // Si la commande commence par "wsl", on exécute wsl.exe directement
            if (command.TrimStart().StartsWith("wsl"))
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "wsl.exe",
                    Arguments = command[3..].Trim(), // retire "wsl"
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.Unicode,
                    StandardErrorEncoding = Encoding.Unicode
                };

                using var process = new Process { StartInfo = psi };
                process.Start();
                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();
                process.WaitForExit();

                if (string.IsNullOrWhiteSpace(output) && !string.IsNullOrWhiteSpace(error))
                {
                    return $"[ERREUR WSL]\n{error}";
                }

                return CleanOutput(output);
            }
            else
            {
                // Pour les autres commandes, on garde la version PowerShell précédente
                string tempFile = Path.GetTempFileName();
                string errorFile = Path.GetTempFileName();
                try
                {
                    string fullCommand = $"{command} | Set-Content -Path '{tempFile}' -Encoding utf8 2> '{errorFile}'";
                    var psi = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{fullCommand}\"",
                        RedirectStandardOutput = false,
                        RedirectStandardError = false,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using var process = new Process { StartInfo = psi };
                    process.Start();
                    process.WaitForExit();

                    string output = await File.ReadAllTextAsync(tempFile, Encoding.UTF8);
                    string error = await File.ReadAllTextAsync(errorFile, Encoding.UTF8);

                    if (string.IsNullOrWhiteSpace(output) && !string.IsNullOrWhiteSpace(error))
                    {
                        return $"[ERREUR POWERSHELL]\n{error}";
                    }

                    return CleanOutput(output);
                }
                finally
                {
                    if (File.Exists(tempFile))
                        File.Delete(tempFile);
                    if (File.Exists(errorFile))
                        File.Delete(errorFile);
                }
            }
        }

        /// <summary>
        /// Nettoie la sortie texte pour harmoniser les retours à la ligne et supprimer les caractères parasites.
        /// </summary>
        private static string CleanOutput(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            return input
                .Replace("\r\n", "\n")
                .Replace("\r", "\n")
                .Replace("\u2028", "\n")
                .Replace("\u2029", "\n")
                .Replace("\u0085", "\n")
                .Replace("\u000B", "\n")
                .Replace("\u000C", "\n")
                .Replace("\u0A0D", "\n")
                .Replace("\u0A0A", "\n")
                .Trim();
        }
    }
} 