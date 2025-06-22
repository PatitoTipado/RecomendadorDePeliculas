using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace RecomendadorDePeliculas.Web.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public void EnviarEmail(string destinatario, string asunto, string mensajeHtml)
        {
            var emailConfig = _config.GetSection("EmailSettings");
            var smtpClient = new SmtpClient(emailConfig["Host"])
            {
                Port = int.Parse(emailConfig["Port"]),
                Credentials = new NetworkCredential(emailConfig["UserName"], emailConfig["Password"]),
                EnableSsl = bool.Parse(emailConfig["EnableSsl"])
            };

            var mensaje = new MailMessage
            {
                From = new MailAddress(emailConfig["UserName"]),
                Subject = asunto,
                Body = mensajeHtml,
                IsBodyHtml = true
            };
            mensaje.To.Add(destinatario);

            smtpClient.Send(mensaje);
        }
    }
}
