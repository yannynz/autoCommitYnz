using System.Linq;
using System.Text.RegularExpressions;
using LibGit2Sharp;
using Version = System.Version;

namespace AccCli.Services
{
    public static class VersionService
    {
        public static string CalculateNextVersion(bool minor, bool major)
        {
            using var repo = new Repository(".");
            var latest = repo.Tags
                .Select(t => t.FriendlyName)
                .Where(t => Regex.IsMatch(t, @"^v\d+\.\d+\.\d+$"))
                .Select(t => t.Substring(1))
                .OrderByDescending(v => new Version(v))
                .FirstOrDefault() ?? "0.0.0";

            var ver = new Version(latest);
            return major
                ? $"{ver.Major + 1}.0.0"
                : minor
                    ? $"{ver.Major}.{ver.Minor + 1}.0"
                    : $"{ver.Major}.{ver.Minor}.{ver.Build + 1}";
        }
    }
}

