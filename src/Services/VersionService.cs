using System.Linq;
using System.Text.RegularExpressions;
using LibGit2Sharp;

namespace AccCli.Services
{
    public static class VersionService
    {
        public static string CalculateNextVersion(bool minor, bool major)
        {
            using var repo = new Repository(".");
            var latestTag = repo.Tags
                .Select(t => t.FriendlyName)
                .Where(t => Regex.IsMatch(t, @"^v\d+\.\d+\.\d+$"))
                .Select(t => t.Substring(1))
                .OrderByDescending(v => new System.Version(v))
                .FirstOrDefault() ?? "0.0.0";

            var ver = new System.Version(latestTag);
            int newMajor = ver.Major;
            int newMinor = ver.Minor;
            int newPatch = ver.Build;

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
    }
}

