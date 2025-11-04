// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using DiaplesWeb.Services.Email; // para el cast a SmtpEmailSender

namespace DiaplesWeb.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;

        public ForgotPasswordModel(UserManager<IdentityUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                // No reveles si el email existe o está confirmado
                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            // Generar token y URL de reseteo
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var callbackUrl = Url.Page(
                "/Account/ResetPassword",
                pageHandler: null,
                values: new { area = "Identity", code },
                protocol: Request.Scheme);

            // Enviar con TU plantilla (botón + imágenes CID)
            if (_emailSender is SmtpEmailSender senderEx)
            {
                await senderEx.SendTemplatedAsync(
                    toEmail: Input.Email,
                    subject: "Reestablecer contraseña · Os Diaples",
                    title: "¿Necesitas reestablecer tu contraseña?",
                    intro: "Hemos recibido una solicitud para cambiar la contraseña de tu cuenta.",
                    body: "Si no fuiste tú, puedes ignorar este mensaje. Si fuiste tú, pulsa el botón para crear una nueva contraseña.",
                    buttonText: "Crear contraseña nueva",
                    buttonUrl: callbackUrl,
                    // Usa una imagen que tengas en /wwwroot/img/Galeria/
                    heroRel: "/img/Galeria/Diables-lonyar_Diaples_pilares2016-5066.jpg"
                );
            }
            else
            {
                // Fallback simple en HTML
                await _emailSender.SendEmailAsync(
                    Input.Email,
                    "Reestablecer contraseña · Os Diaples",
                    $"Para reestablecer tu contraseña haz clic en <a href='{callbackUrl}'>este enlace</a>."
                );
            }

            return RedirectToPage("./ForgotPasswordConfirmation");
        }
    }
}
