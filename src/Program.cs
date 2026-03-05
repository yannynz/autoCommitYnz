using System;
using System.CommandLine;
using System.Threading.Tasks;
using AccCli.Services;

namespace AccCli
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var root = new RootCommand("Auto Commit CLI – versionamento automático com SemVer");

            // Opções comuns
            var usernameOpt = new Option<string>("--username", "Usuário Git (HTTPS)") { IsRequired = true };
            var passwordOpt = new Option<string>("--password", "Senha ou Personal Access Token") { IsRequired = true };
            var minorOpt    = new Option<bool>("--minor",   "Incrementa a versão minor");
            var majorOpt    = new Option<bool>("--major",   "Incrementa a versão major");
            var dryRunOpt   = new Option<bool>("--dry-run","Simula execução sem alterar nada");
            var messageOpt  = new Option<string>("-m",       "Mensagem de commit customizada");

            // comando init
            var gitNameOpt  = new Option<string?>("--git-name",  () => null, "Nome para configurar em git config --global user.name");
            var gitEmailOpt = new Option<string?>("--git-email", () => null, "Email para configurar em git config --global user.email");

            var initCmd = new Command("init", "Cria ou atualiza as credenciais de acesso e garante identidade Git");
            initCmd.AddOption(usernameOpt);
            initCmd.AddOption(passwordOpt);
            initCmd.AddOption(gitNameOpt);
            initCmd.AddOption(gitEmailOpt);
            initCmd.SetHandler<string, string, string?, string?>(async (u, p, gitName, gitEmail) =>
            {
                await ConfigService.InitAsync(u, p, gitName, gitEmail);
            }, usernameOpt, passwordOpt, gitNameOpt, gitEmailOpt);
            root.AddCommand(initCmd);

            // comando commit
            var commitCmd = new Command("commit", "Executa add, commit, tag e push SemVer");
            commitCmd.AddOption(minorOpt);
            commitCmd.AddOption(majorOpt);
            commitCmd.AddOption(dryRunOpt);
            commitCmd.AddOption(messageOpt);
            commitCmd.SetHandler<bool, bool, bool, string?>(async (minor, major, dry, msg) =>
            {
                try
                {
                    var config     = await ConfigService.LoadAsync();
                    var git         = new GitService();
                    GitConfigService.EnsureIdentity(config.GitName, config.GitEmail, config.Username);
                    git.FetchRemote(config.Username, config.Password);
                    git.EnsureBranchUpToDateWithRemote();
                    var version     = VersionService.CalculateNextVersion(minor, major);

                    LoggingService.Info($"Próxima versão: {version}");

                    if (dry)
                    {
                        LoggingService.Info("Simulação (dry-run) completa. Nenhuma ação foi executada.");
                        return;
                    }

                    git.StageAll();
                    git.Commit(msg ?? $"Versão {version}");
                    git.Tag(version);
                    git.Push(config.Username, config.Password, version);

                    Environment.ExitCode = 0;
                }
                catch (InvalidOperationException ex)
                {
                    LoggingService.Error(ex.Message);
                    Environment.ExitCode = 1;
                }
                catch (Exception ex)
                {
                    LoggingService.Error($"Erro desconhecido: {ex.Message}");
                    Environment.ExitCode = 99;
                }
            },
            minorOpt, majorOpt, dryRunOpt, messageOpt);
            root.AddCommand(commitCmd);

            return await root.InvokeAsync(args);
        }
    }
}

