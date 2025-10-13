using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using System;
using System.Collections.Generic;


namespace AccCli.Services
{
    public static class ConfigService
    {
        private static readonly string ConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)!,
            ".autocli", "config.json"
        );

        public static async Task InitAsync(string user, string pass, string? gitName, string? gitEmail)
        {
            var dir = Path.GetDirectoryName(ConfigPath)!;
            Directory.CreateDirectory(dir);

            var cfg = new ConfigModel
            {
                Username = user,
                Password = pass,
                GitName  = gitName,
                GitEmail = gitEmail
            };
            var json = JsonSerializer.Serialize(cfg, new JsonSerializerOptions { WriteIndented = true });

            try
            {
                await File.WriteAllTextAsync(ConfigPath, json);
                Console.WriteLine($"Configuração salva em {ConfigPath}");
                GitConfigService.EnsureIdentity(gitName, gitEmail, user);
            }
            catch (UnauthorizedAccessException)
            {
                Console.Error.WriteLine("Permissão negada ao gravar o arquivo de configuração.");
                Console.Error.WriteLine("Tente rodar: sudo autocli init --username <user> --password <pass>");
                Environment.Exit(1);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Falha ao configurar identidade Git: {ex.Message}");
            }
        }

        public static async Task<ConfigData> LoadAsync()
        {
            if (!File.Exists(ConfigPath))
                throw new InvalidOperationException("Config não encontrado. Rode 'autocli init' primeiro.");

            try
            {
                var text = await File.ReadAllTextAsync(ConfigPath);
                var obj  = JsonSerializer.Deserialize<ConfigModel>(text)
                           ?? throw new InvalidOperationException("Arquivo de configuração inválido.");

                if (string.IsNullOrWhiteSpace(obj.Username) || string.IsNullOrWhiteSpace(obj.Password))
                {
                    throw new InvalidOperationException("Arquivo de configuração incompleto. Rode 'autocli init' novamente.");
                }

                return new ConfigData(obj.Username, obj.Password, obj.GitName, obj.GitEmail);
            }
            catch (UnauthorizedAccessException)
            {
                Console.Error.WriteLine("Permissão negada ao ler o arquivo de configuração.");
                Console.Error.WriteLine("Tente rodar: sudo autocli init --username <user> --password <pass>");
                Environment.Exit(1);
                return default!; // não alcançado
            }
        }

        public record ConfigData(string Username, string Password, string? GitName, string? GitEmail);

        private class ConfigModel
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string? GitName { get; set; }
            public string? GitEmail { get; set; }
        }
    }
}
