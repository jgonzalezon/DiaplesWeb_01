using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using DiaplesWeb.Localization;

namespace DiaplesWeb.Controllers
{
    public class LocalizationController : Controller
    {
        private readonly HashSet<string> _supportedCultureNames;
        private readonly CultureInfo _defaultCulture;

        public LocalizationController(IOptions<RequestLocalizationOptions> localizationOptions)
        {
            var options = localizationOptions.Value;

            _supportedCultureNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (options.SupportedCultures is { Count: > 0 })
            {
                foreach (var culture in options.SupportedCultures)
                {
                    _supportedCultureNames.Add(LocalizationSettings.NormalizeCultureName(culture.Name));
                }
            }

            if (options.SupportedUICultures is { Count: > 0 })
            {
                foreach (var culture in options.SupportedUICultures)
                {
                    _supportedCultureNames.Add(LocalizationSettings.NormalizeCultureName(culture.Name));
                }
            }

            foreach (var alias in LocalizationSettings.CultureAliases)
            {
                _supportedCultureNames.Add(alias.Key);
                _supportedCultureNames.Add(alias.Value);
            }

            _defaultCulture = options.DefaultRequestCulture?.UICulture
                ?? CultureInfo.GetCultureInfo(LocalizationSettings.DefaultCulture);

            _supportedCultureNames.Add(_defaultCulture.Name);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetLanguage(string? culture, string? returnUrl)
        {
            var cultureInfo = ResolveCultureInfo(culture);

            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(cultureInfo)),
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1)
                });

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        private CultureInfo ResolveCultureInfo(string? culture)
        {
            var normalizedRequest = LocalizationSettings.NormalizeCultureName(culture);

            CultureInfo candidateCulture;

            try
            {
                candidateCulture = string.IsNullOrWhiteSpace(normalizedRequest)
                    ? _defaultCulture
                    : new CultureInfo(normalizedRequest);
            }
            catch (CultureNotFoundException)
            {
                candidateCulture = _defaultCulture;
            }

            var normalizedCandidate = LocalizationSettings.NormalizeCultureName(candidateCulture.Name);
            if (!string.Equals(normalizedCandidate, candidateCulture.Name, StringComparison.OrdinalIgnoreCase))
            {
                candidateCulture = new CultureInfo(normalizedCandidate);
            }

            if (_supportedCultureNames.Contains(candidateCulture.Name))
            {
                return candidateCulture;
            }

            var parentCulture = candidateCulture.Parent;
            if (parentCulture != null && parentCulture != CultureInfo.InvariantCulture)
            {
                var normalizedParent = LocalizationSettings.NormalizeCultureName(parentCulture.Name);
                if (_supportedCultureNames.Contains(normalizedParent))
                {
                    return new CultureInfo(normalizedParent);
                }
            }

            return _defaultCulture;
        }
    }
}
