using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DiaplesWeb.Controllers
{
    public class LocalizationController : Controller
    {
        private readonly HashSet<string> _supportedCultureNames;
        private readonly CultureInfo _defaultCulture;

        public LocalizationController(IOptions<RequestLocalizationOptions> localizationOptions)
        {
            var options = localizationOptions.Value;

            _supportedCultureNames = options.SupportedUICultures?
                .Select(culture => culture.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase)
                ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            _defaultCulture = options.DefaultRequestCulture?.UICulture ?? CultureInfo.GetCultureInfo("es");

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
            CultureInfo candidateCulture;

            try
            {
                candidateCulture = string.IsNullOrWhiteSpace(culture)
                    ? _defaultCulture
                    : new CultureInfo(culture);
            }
            catch (CultureNotFoundException)
            {
                candidateCulture = _defaultCulture;
            }

            if (_supportedCultureNames.Contains(candidateCulture.Name))
            {
                return candidateCulture;
            }

            var parentCulture = candidateCulture.Parent;
            if (parentCulture != null
                && parentCulture != CultureInfo.InvariantCulture
                && _supportedCultureNames.Contains(parentCulture.Name))
            {
                return parentCulture;
            }

            return _defaultCulture;
        }
    }
}
