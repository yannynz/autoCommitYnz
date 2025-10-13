using System;
using System.Diagnostics;

namespace AccCli.Services
{
    public static class GitConfigService
    {
        public static void EnsureIdentity(string? gitName, string? gitEmail, string usernameFallback)
        {
            try
            {
                var targetName = string.IsNullOrWhiteSpace(gitName)
                    ? usernameFallback
                    : gitName;

                if (!string.IsNullOrWhiteSpace(targetName))
                {
                    ConfigureIfMissing("user.name", targetName);
                }

                var targetEmail = string.IsNullOrWhiteSpace(gitEmail)
                    ? BuildFallbackEmail(usernameFallback)
                    : gitEmail;

                if (!string.IsNullOrWhiteSpace(targetEmail))
                {
                    ConfigureIfMissing("user.email", targetEmail);
                }
            }
            catch (Exception ex)
            {
                LoggingService.Error($"Não foi possível ajustar as configurações globais do Git: {ex.Message}");
            }
        }

        private static void ConfigureIfMissing(string key, string value)
        {
            var current = ReadConfig(key);
            if (!string.IsNullOrWhiteSpace(current))
            {
                return;
            }

            var psi = CreateGitStartInfo();
            psi.ArgumentList.Add("config");
            psi.ArgumentList.Add("--global");
            psi.ArgumentList.Add(key);
            psi.ArgumentList.Add(value);

            using var process = Process.Start(psi)!;
            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                LoggingService.Info($"Git config global '{key}' definido para '{value}'.");
            }
            else
            {
                var err = process.StandardError.ReadToEnd().Trim();
                throw new InvalidOperationException(
                    string.IsNullOrWhiteSpace(err)
                        ? $"git config retornou código {process.ExitCode}"
                        : err);
            }
        }

        private static string? ReadConfig(string key)
        {
            var psi = CreateGitStartInfo();
            psi.ArgumentList.Add("config");
            psi.ArgumentList.Add("--global");
            psi.ArgumentList.Add("--get");
            psi.ArgumentList.Add(key);

            using var process = Process.Start(psi)!;
            var output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();

            return process.ExitCode == 0 ? output : null;
        }

        private static ProcessStartInfo CreateGitStartInfo()
        {
            return new ProcessStartInfo
            {
                FileName = "git",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
        }

        private static string? BuildFallbackEmail(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return null;
            }

            var sanitized = username.Trim().Replace(" ", "").ToLowerInvariant();
            return $"{sanitized}@users.noreply.github.com";
        }
    }
}
