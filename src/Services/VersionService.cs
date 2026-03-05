using System.Linq;
using System.Text.RegularExpressions;
using LibGit2Sharp;

namespace AccCli.Services
{
    public static class VersionService
    {
        private static readonly Regex SemVerTagRegex = new(@"^v?(\d+)\.(\d+)\.(\d+)$", RegexOptions.Compiled);

        public static string CalculateNextVersion(bool minor, bool major)
        {
            using var repo = new Repository(".");
            var latestTag = repo.Tags
                .Select(t => ParseTagVersion(t.FriendlyName))
                .Where(v => v is not null)
                .Select(v => v!)
                .OrderByDescending(v => v)
                .FirstOrDefault() ?? new System.Version(0, 0, 0);

            var ver = latestTag;
            int newMajor = ver.Major;
            int newMinor = ver.Minor;
            int newPatch = ver.Build < 0 ? 0 : ver.Build;

            if (major)
            {
                newMajor++;
                newMinor = 0;
                newPatch = 0;
            }
            else if (minor)
            {
                newMinor++;
                if (newMinor >= 10)
                {
                    newMajor++;
                    newMinor = 0;
                }
                newPatch = 0;
            }
            else
            {
                newPatch++;
                if (newPatch >= 10)
                {
                    newPatch = 0;
                    newMinor++;
                    if (newMinor >= 10)
                    {
                        newMinor = 0;
                        newMajor++;
                    }
                }
            }

            return $"{newMajor}.{newMinor}.{newPatch}";
        }

        private static System.Version? ParseTagVersion(string tagName)
        {
            if (!SemVerTagRegex.IsMatch(tagName))
            {
                return null;
            }

            var normalized = tagName.StartsWith("v") ? tagName[1..] : tagName;
            return System.Version.TryParse(normalized, out var parsed)
                ? parsed
                : null;
        }
    }
}
