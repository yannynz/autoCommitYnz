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

        public static async Task InitAsync(string user, string pass)
        {
            var dir = Path.GetDirectoryName(ConfigPath)!;
            Directory.CreateDirectory(dir);

            var cfg  = new { Username = user, Password = pass };
            var json = JsonSerializer.Serialize(cfg, new JsonSerializerOptions { WriteIndented = true });

            try
            {
                await File.WriteAllTextAsync(ConfigPath, json);
                Console.WriteLine($"Configuração salva em {ConfigPath}");
            }
            catch (UnauthorizedAccessException)
            {
                Console.Error.WriteLine("Permissão negada ao gravar o arquivo de configuração.");
                Console.Error.WriteLine("Tente rodar: sudo autocli init --username <user> --password <pass>");
                Environment.Exit(1);
            }
        }

        public static async Task<(string Username, string Password)> LoadAsync()
        {
            if (!File.Exists(ConfigPath))
                throw new InvalidOperationException("Config não encontrado. Rode 'autocli init' primeiro.");

            try
            {
                var text = await File.ReadAllTextAsync(ConfigPath);
                var obj  = JsonSerializer.Deserialize<Dictionary<string, string>>(text)!;
                return (obj["Username"], obj["Password"]);
            }
            catch (UnauthorizedAccessException)
            {
                Console.Error.WriteLine("Permissão negada ao ler o arquivo de configuração.");
                Console.Error.WriteLine("Tente rodar: sudo autocli init --username <user> --password <pass>");
                Environment.Exit(1);
                return default!; // não alcançado
            }
        }
    }
}

