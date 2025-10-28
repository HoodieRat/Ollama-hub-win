using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MyOllamaHub3
{
    internal static class BrowserLauncher
    {
        private static readonly string[] ChromeCandidatePaths = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Google", "Chrome", "Application", "chrome.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Google", "Chrome", "Application", "chrome.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google", "Chrome", "Application", "chrome.exe")
        };

        public static bool TryOpenUrl(string? url, out string? error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(url))
            {
                error = "URL is empty.";
                return false;
            }

            var chromePath = ChromeCandidatePaths.FirstOrDefault(File.Exists);

            try
            {
                if (!string.IsNullOrWhiteSpace(chromePath) && File.Exists(chromePath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = chromePath,
                        Arguments = $"\"{url}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });
                    return true;
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }
    }
}
