using System;
using System.Linq;
using LibGit2Sharp;
using Spectre.Console;

namespace AccCli.Services
{
    public class GitService
    {
        private readonly Repository _repo;

        public GitService(string path = ".")
        {
            if (!Repository.IsValid(path))
            {
                AnsiConsole.MarkupLine("[red]Erro:[/] Diretório atual não é um repositório Git válido.");
                Environment.Exit(10);
            }
            _repo = new Repository(path);
        }

        public void StageAll()
        {
            Commands.Stage(_repo, "*");
            AnsiConsole.MarkupLine("[green]Staging:[/] git add .");
        }

        public void Commit(string message)
        {
            Signature author;
            try
            {
                var local = _repo.Config.Get<string>("user.name", ConfigurationLevel.Local);
                var global = _repo.Config.Get<string>("user.name", ConfigurationLevel.Global);
                var name = local?.Value ?? global?.Value ?? throw new LibGit2SharpException("Missing user.name");
                var emailLocal = _repo.Config.Get<string>("user.email", ConfigurationLevel.Local);
                var emailGlobal = _repo.Config.Get<string>("user.email", ConfigurationLevel.Global);
                var mail = emailLocal?.Value ?? emailGlobal?.Value ?? throw new LibGit2SharpException("Missing user.email");

                author = new Signature(name, mail, DateTimeOffset.Now);
            }
            catch (LibGit2SharpException)
            {
                AnsiConsole.MarkupLine("[red]Erro:[/] configure seu nome e email no Git antes de commitar:");
                AnsiConsole.MarkupLine("  git config --global user.name  \"Seu Nome\"");
                AnsiConsole.MarkupLine("  git config --global user.email \"seu@email\"");
                Environment.Exit(1);
                return;
            }

            _repo.Commit(message, author, author);
            AnsiConsole.MarkupLine(
                $"[green]Commit criado:[/] \"{message}\" por {author.Name} <{author.Email}>");
        }

        public void Tag(string version)
        {
            Signature tagger;
            try
            {
                // Mesma assinatura do commit
                tagger = _repo.Config.BuildSignature(DateTimeOffset.Now);
            }
            catch (LibGit2SharpException)
            {
                // Se não tiver config, usamos mesmo fallback do Commit
                tagger = new Signature("AutoCLI", "auto@cli", DateTimeOffset.Now);
            }

            var tagName = $"v{version}";
            _repo.ApplyTag(tagName, tagger, $"Tag {version}");
            AnsiConsole.MarkupLine($"[green]Tag criada:[/] {tagName} por {tagger.Name} <{tagger.Email}>");
        }

        public void Push(string user, string pass, string version)
        {
            var remote = _repo.Network.Remotes["origin"];
            var opts = new PushOptions
            {
                CredentialsProvider = (_, _, _) =>
                    new UsernamePasswordCredentials { Username = user, Password = pass }
            };

            // 1) Push do branch atual
            string branch = _repo.Head.FriendlyName;
            _repo.Network.Push(remote, $"refs/heads/{branch}", opts);
            AnsiConsole.MarkupLine($"[green]Branch {branch} enviado para remote.[/]");

            // 2) Push da tag específica
            var tagName = $"v{version}";
            _repo.Network.Push(remote, $"refs/tags/{tagName}", opts);
            AnsiConsole.MarkupLine($"[green]Tag enviada para remote:[/] {tagName}");
        }

        public void FetchRemote(string? user, string? pass)
        {
            if (!_repo.Network.Remotes.Any())
            {
                AnsiConsole.MarkupLine("[yellow]Aviso:[/] Nenhum remote configurado. Usando tags locais.");
                return;
            }

            var remote = _repo.Network.Remotes["origin"] ?? _repo.Network.Remotes.First();
            var refSpecs = remote.FetchRefSpecs.Select(spec => spec.Specification).ToList();
            var options = new FetchOptions
            {
                TagFetchMode = TagFetchMode.All
            };

            if (!string.IsNullOrEmpty(user) || !string.IsNullOrEmpty(pass))
            {
                options.CredentialsProvider = (_, _, _) => new UsernamePasswordCredentials
                {
                    Username = user ?? string.Empty,
                    Password = pass ?? string.Empty
                };
            }

            try
            {
                Commands.Fetch(_repo, remote.Name, refSpecs, options, null);
                AnsiConsole.MarkupLine($"[green]Fetch realizado:[/] {remote.Name}");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[yellow]Aviso:[/] Não foi possível buscar tags remotas ({ex.Message}). Usando tags locais.");
            }
        }
    }
}
