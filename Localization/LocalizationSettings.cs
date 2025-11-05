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
            CultureInfo.GetCultureInfo("an")
        };

        private static readonly IReadOnlyDictionary<string, string> _aliases =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["an"] = "an",
                ["arg"] = "an",
                ["an-es"] = "an",
                ["an-ES"] = "an",
                ["an_es"] = "an"
            };

        private static readonly IReadOnlyDictionary<string, string> _htmlLanguageTags =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["es"] = "es",
                ["en"] = "en",
                ["an"] = "an-ES"
            };

        public static IReadOnlyList<CultureInfo> SupportedCultures => _supportedCultures;

        public static IReadOnlyDictionary<string, string> CultureAliases => _aliases;

        public static string NormalizeCultureName(string? cultureName)
        {
            if (string.IsNullOrWhiteSpace(cultureName))
            {
                return DefaultCulture;
            }

            cultureName = cultureName.Trim();
            cultureName = cultureName.Replace('_', '-');

            if (_aliases.TryGetValue(cultureName, out var normalized))
            {
                return normalized;
            }

            return cultureName;
        }

        public static string GetHtmlLanguageTag(CultureInfo culture)
        {
            if (_htmlLanguageTags.TryGetValue(culture.Name, out var tag))
            {
                return tag;
            }

            return culture.Name;
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
