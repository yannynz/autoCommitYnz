using Spectre.Console;

namespace AccCli.Services
{
    public static class LoggingService
    {
        public static void Info(string msg) =>
            AnsiConsole.MarkupLine($"[green]{msg}[/]");

        public static void Error(string msg) =>
            AnsiConsole.MarkupLine($"[red]{msg}[/]");
    }
}

