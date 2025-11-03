using System.IO;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;

namespace DiaplesWeb.Services.Email
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly IConfiguration _cfg;
        private readonly IWebHostEnvironment _env;

        public SmtpEmailSender(IConfiguration cfg, IWebHostEnvironment env)
        {
            _cfg = cfg;
            _env = env;
        }

        // Implementación básica requerida por Identity (sin plantilla)
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
            => SendAsync(email, subject, htmlMessage);

        // Envío con plantilla + imágenes inline
        public async Task SendTemplatedAsync(string toEmail, string subject,
            string title, string intro, string body,
            string? buttonText = null, string? buttonUrl = null,
            string logoRel = "/img/Inicio/wwwdiapleses.png",
            string heroRel = "/img/Inicio/logo_diaples.jpg") 
        {
            // 1) Carga la plantilla
            var contentRoot = _env.ContentRootPath; // raíz del proyecto
            var templatePath = Path.Combine(contentRoot, "Services", "Email", "Templates", "Basic.html");
            var html = await File.ReadAllTextAsync(templatePath);

            // 2) Rellena placeholders simples
            html = html.Replace("{{Subject}}", subject)
                       .Replace("{{Title}}", title)
                       .Replace("{{Intro}}", intro)
                       .Replace("{{Body}}", body)
                       .Replace("{{Year}}", System.DateTime.Now.Year.ToString());

            // 3) Manejo del bloque botón (muy simple)
            if (!string.IsNullOrWhiteSpace(buttonText) && !string.IsNullOrWhiteSpace(buttonUrl))
            {
                html = html.Replace("{{#Button}}", "")
                           .Replace("{{ButtonText}}", buttonText)
                           .Replace("{{ButtonUrl}}", buttonUrl)
                           .Replace("{{/Button}}", "");
            }
            else
            {
                // quita el bloque completo si no hay botón
                var start = html.IndexOf("{{#Button}}");
                var end   = html.IndexOf("{{/Button}}") + "{{/Button}}".Length;
                if (start >= 0 && end > start) html = html.Remove(start, end - start);
            }

            // 4) Construye mensaje + imágenes inline
            var msg = new MimeMessage();
            msg.From.Add(new MailboxAddress(_cfg["Email:FromName"], _cfg["Email:FromAddress"]));
            msg.To.Add(MailboxAddress.Parse(toEmail));
            msg.Subject = subject;

            var bodyBuilder = new BodyBuilder();

            // Logo inline
            var wwwroot = _env.WebRootPath; // apunta a /wwwroot
            var logoPath = Path.Combine(wwwroot, logoRel.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(logoPath))
            {
                var logo = bodyBuilder.LinkedResources.Add(logoPath);
                logo.ContentId = "logo";
            }
            // Hero inline
            var heroPath = Path.Combine(wwwroot, heroRel.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(heroPath))
            {
                var hero = bodyBuilder.LinkedResources.Add(heroPath);
                hero.ContentId = "hero";
            }

            bodyBuilder.HtmlBody = html;
            msg.Body = bodyBuilder.ToMessageBody();

            // 5) SMTP Gmail (app password)
            using var smtp = new SmtpClient();
            var host = _cfg["Email:Smtp:Host"];
            var port = int.Parse(_cfg["Email:Smtp:Port"] ?? "587");
            var user = _cfg["Email:Smtp:User"];
            var pass = _cfg["Email:Smtp:Pass"];
            var useStartTls = bool.Parse(_cfg["Email:Smtp:UseStartTls"] ?? "true");

            await smtp.ConnectAsync(host, port, useStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);
            await smtp.AuthenticateAsync(user, pass);
            await smtp.SendAsync(msg);
            await smtp.DisconnectAsync(true);
        }

        private async Task SendAsync(string toEmail, string subject, string html)
        {
            var msg = new MimeMessage();
            msg.From.Add(new MailboxAddress(_cfg["Email:FromName"], _cfg["Email:FromAddress"]));
            msg.To.Add(MailboxAddress.Parse(toEmail));
            msg.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = html };
            msg.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            var host = _cfg["Email:Smtp:Host"];
            var port = int.Parse(_cfg["Email:Smtp:Port"] ?? "587");
            var user = _cfg["Email:Smtp:User"];
            var pass = _cfg["Email:Smtp:Pass"];
            var useStartTls = bool.Parse(_cfg["Email:Smtp:UseStartTls"] ?? "true");

            await smtp.ConnectAsync(host, port, useStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);
            await smtp.AuthenticateAsync(user, pass);
            await smtp.SendAsync(msg);
            await smtp.DisconnectAsync(true);
        }
    }
}
