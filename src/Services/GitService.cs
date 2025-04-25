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
            var author = new Signature("AutoCLI", "auto@cli", DateTimeOffset.Now);
            _repo.Commit(message, author, author);
            AnsiConsole.MarkupLine($"[green]Commit criado:[/] \"{message}\"");
        }

        public void Tag(string version)
        {
            var tagName = $"v{version}";
            _repo.ApplyTag(tagName,
                new Signature("AutoCLI", "auto@cli", DateTimeOffset.Now),
                $"Tag {version}");
            AnsiConsole.MarkupLine($"[green]Tag criada:[/] {tagName}");
        }

        public void Push((string Username, string Password) cfg)
        {
            var remote = _repo.Network.Remotes["origin"];
            var opts = new PushOptions
            {
                CredentialsProvider = (_, _, _) =>
                    new UsernamePasswordCredentials
                    {
                        Username = cfg.Username,
                        Password = cfg.Password
                    }
            };

            _repo.Network.Push(remote, $"refs/heads/{_repo.Head.FriendlyName}", opts);
            AnsiConsole.MarkupLine($"[green]Branch {_repo.Head.FriendlyName} enviado para remote.[/]");

            _repo.Network.Push(remote, "refs/tags/*", opts);
            AnsiConsole.MarkupLine("[green]Tags enviadas para remote.[/]");
        }
    }
}

