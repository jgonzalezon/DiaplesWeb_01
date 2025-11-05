using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DiaplesWeb.Localization
{
    public static class LocalizationSettings
    {
        public const string DefaultCulture = "es";

        private static readonly CultureInfo[] _supportedCultures =
        {
            CultureInfo.GetCultureInfo("es"),
            CultureInfo.GetCultureInfo("en"),
            CultureInfo.GetCultureInfo("an-ES")
        };

        private static readonly IReadOnlyDictionary<string, string> _aliases =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["an"] = "an-ES",
                ["arg"] = "an-ES",
                ["an-es"] = "an-ES"
            };

        public static IReadOnlyList<CultureInfo> SupportedCultures => _supportedCultures;

        public static IReadOnlyDictionary<string, string> CultureAliases => _aliases;

        public static string NormalizeCultureName(string? cultureName)
        {
            if (string.IsNullOrWhiteSpace(cultureName))
            {
                return DefaultCulture;
            }

            if (_aliases.TryGetValue(cultureName, out var normalized))
            {
                return normalized;
            }

            return cultureName;
        }

        public static bool IsSupportedCulture(string? cultureName)
        {
            if (string.IsNullOrWhiteSpace(cultureName))
            {
                return false;
            }

            var normalized = NormalizeCultureName(cultureName);
            return _supportedCultures.Any(culture =>
                string.Equals(culture.Name, normalized, StringComparison.OrdinalIgnoreCase));
        }
    }
}
