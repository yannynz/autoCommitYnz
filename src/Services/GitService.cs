using System;
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
            // Tenta ler user.name e user.email do config do repo (ou global)
            var nameEntry  = _repo.Config.Get<string>("user.name");
            var emailEntry = _repo.Config.Get<string>("user.email");

            string authorName  = nameEntry?.Value  ?? "AutoCLI";
            string authorEmail = emailEntry?.Value ?? "auto@cli";

            var author = new Signature(authorName, authorEmail, DateTimeOffset.Now);
            _repo.Commit(message, author, author);

            AnsiConsole.MarkupLine(
                $"[green]Commit criado:[/] \"{message}\" por {authorName} <{authorEmail}>");
        }

        public void Tag(string version)
        {
            var tagName = $"v{version}";

            // Reutiliza assinatura do commit
            var nameEntry  = _repo.Config.Get<string>("user.name");
            var emailEntry = _repo.Config.Get<string>("user.email");
            string taggerName  = nameEntry?.Value  ?? "AutoCLI";
            string taggerEmail = emailEntry?.Value ?? "auto@cli";
            var tagger = new Signature(taggerName, taggerEmail, DateTimeOffset.Now);

            _repo.ApplyTag(tagName, tagger, $"Tag {version}");

            AnsiConsole.MarkupLine($"[green]Tag criada:[/] {tagName}");
        }

        public void Push(string user, string pass, string version)
        {
            var remote = _repo.Network.Remotes["origin"];
            var opts = new PushOptions
            {
                CredentialsProvider = (_,_,_) =>
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
    }
}

