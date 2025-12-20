using System;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using AppBiblioteca.Models;
using Microsoft.Extensions.Options;

namespace AppBiblioteca.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task<bool> SendEmailAsync(EmailMessage message)
        {
            try
            {
                using var smtpClient = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
                {
                    EnableSsl = _emailSettings.EnableSsl,
                    Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                    Subject = message.Subject,
                    Body = message.Body,
                    IsBodyHtml = message.IsHtml
                };

                mailMessage.To.Add(new MailAddress(message.ToEmail, message.ToName));

                await smtpClient.SendMailAsync(mailMessage);
                return true;
            }
            catch (Exception)
            {
                // Log error aquí si tienes un sistema de logging
                return false;
            }
        }

        public async Task<bool> SendReservaConfirmationAsync(string userEmail, string userName, int idPrestamo, string[] materiales)
        {
            var materialesList = string.Join("", Array.ConvertAll(materiales, m => $"<li>{m}</li>"));

            var body = GetEmailTemplate(
                "Confirmación de Reserva",
                $"Hola <strong>{userName}</strong>,",
                $@"
                <p>Tu reserva ha sido registrada exitosamente en el sistema.</p>
                <p><strong>Número de reserva:</strong> #{idPrestamo}</p>
                <p><strong>Materiales reservados:</strong></p>
                <ul>
                    {materialesList}
                </ul>
                <p>Un bibliotecario revisará tu solicitud y recibirás una notificación cuando sea aprobada.</p>
                ",
                "info"
            );

            var message = new EmailMessage
            {
                ToEmail = userEmail,
                ToName = userName,
                Subject = $"Confirmación de Reserva #{idPrestamo}",
                Body = body,
                IsHtml = true
            };

            return await SendEmailAsync(message);
        }

        public async Task<bool> SendReservaAprobadaAsync(string userEmail, string userName, int idPrestamo, string[] materiales, DateTime fechaDevolucion)
        {
            var materialesList = string.Join("", Array.ConvertAll(materiales, m => $"<li>{m}</li>"));

            var body = GetEmailTemplate(
                "Préstamo Aprobado",
                $"Hola <strong>{userName}</strong>,",
                $@"
                <p>¡Buenas noticias! Tu reserva ha sido aprobada.</p>
                <p><strong>Número de préstamo:</strong> #{idPrestamo}</p>
                <p><strong>Materiales aprobados:</strong></p>
                <ul>
                    {materialesList}
                </ul>
                <p><strong>Fecha de devolución:</strong> {fechaDevolucion:dd/MM/yyyy}</p>
                <p>Puedes pasar a recoger los materiales en la biblioteca durante el horario de atención.</p>
                <p style='color: #dc3545; font-weight: 500;'>Recuerda devolver los materiales antes de la fecha indicada para evitar sanciones.</p>
                ",
                "success"
            );

            var message = new EmailMessage
            {
                ToEmail = userEmail,
                ToName = userName,
                Subject = $"Préstamo Aprobado #{idPrestamo}",
                Body = body,
                IsHtml = true
            };

            return await SendEmailAsync(message);
        }

        public async Task<bool> SendRecordatorioVencimientoAsync(string userEmail, string userName, int idPrestamo, string[] materiales, DateTime fechaDevolucion)
        {
            var materialesList = string.Join("", Array.ConvertAll(materiales, m => $"<li>{m}</li>"));
            var diasRestantes = (fechaDevolucion - DateTime.Now).Days;

            var body = GetEmailTemplate(
                "Recordatorio de Devolución",
                $"Hola <strong>{userName}</strong>,",
                $@"
                <p>Este es un recordatorio amigable sobre tu préstamo.</p>
                <p><strong>Número de préstamo:</strong> #{idPrestamo}</p>
                <p><strong>Materiales prestados:</strong></p>
                <ul>
                    {materialesList}
                </ul>
                <p><strong>Fecha de devolución:</strong> {fechaDevolucion:dd/MM/yyyy}</p>
                <p style='color: #ffc107; font-weight: 500;'>Te quedan {diasRestantes} día(s) para devolver estos materiales.</p>
                <p>Por favor, asegúrate de devolverlos a tiempo para evitar sanciones.</p>
                ",
                "warning"
            );

            var message = new EmailMessage
            {
                ToEmail = userEmail,
                ToName = userName,
                Subject = $"Recordatorio: Devolución Próxima - Préstamo #{idPrestamo}",
                Body = body,
                IsHtml = true
            };

            return await SendEmailAsync(message);
        }

        public async Task<bool> SendNotificacionAtrasoAsync(string userEmail, string userName, int idPrestamo, string[] materiales, int diasAtraso)
        {
            var materialesList = string.Join("", Array.ConvertAll(materiales, m => $"<li>{m}</li>"));

            var body = GetEmailTemplate(
                "Préstamo Atrasado",
                $"Hola <strong>{userName}</strong>,",
                $@"
                <p style='color: #dc3545; font-weight: 500;'>Tu préstamo está atrasado.</p>
                <p><strong>Número de préstamo:</strong> #{idPrestamo}</p>
                <p><strong>Materiales prestados:</strong></p>
                <ul>
                    {materialesList}
                </ul>
                <p><strong>Días de atraso:</strong> {diasAtraso} día(s)</p>
                <p>Por favor, devuelve los materiales lo antes posible para evitar sanciones adicionales.</p>
                <p>Si tienes algún inconveniente, contacta con la biblioteca inmediatamente.</p>
                ",
                "danger"
            );

            var message = new EmailMessage
            {
                ToEmail = userEmail,
                ToName = userName,
                Subject = $"URGENTE: Préstamo Atrasado #{idPrestamo}",
                Body = body,
                IsHtml = true
            };

            return await SendEmailAsync(message);
        }

        public async Task<bool> SendDevolucionConfirmadaAsync(string userEmail, string userName, int idPrestamo, string[] materiales)
        {
            var materialesList = string.Join("", Array.ConvertAll(materiales, m => $"<li>{m}</li>"));

            var body = GetEmailTemplate(
                "Devolución Confirmada",
                $"Hola <strong>{userName}</strong>,",
                $@"
                <p>Hemos recibido la devolución de tus materiales.</p>
                <p><strong>Número de préstamo:</strong> #{idPrestamo}</p>
                <p><strong>Materiales devueltos:</strong></p>
                <ul>
                    {materialesList}
                </ul>
                <p>¡Gracias por usar nuestros servicios!</p>
                <p>Esperamos verte pronto en la biblioteca.</p>
                ",
                "success"
            );

            var message = new EmailMessage
            {
                ToEmail = userEmail,
                ToName = userName,
                Subject = $"Devolución Confirmada - Préstamo #{idPrestamo}",
                Body = body,
                IsHtml = true
            };

            return await SendEmailAsync(message);
        }

        public async Task<bool> SendReservaCanceladaAsync(string userEmail, string userName, int idPrestamo, string motivo)
        {
            var body = GetEmailTemplate(
                "Reserva Cancelada",
                $"Hola <strong>{userName}</strong>,",
                $@"
                <p>Tu reserva ha sido cancelada.</p>
                <p><strong>Número de reserva:</strong> #{idPrestamo}</p>
                <p><strong>Motivo:</strong> {motivo}</p>
                <p>Si tienes alguna pregunta, no dudes en contactar con la biblioteca.</p>
                ",
                "danger"
            );

            var message = new EmailMessage
            {
                ToEmail = userEmail,
                ToName = userName,
                Subject = $"Reserva Cancelada #{idPrestamo}",
                Body = body,
                IsHtml = true
            };

            return await SendEmailAsync(message);
        }

        private string GetEmailTemplate(string title, string greeting, string content, string type = "info")
        {
            var colorMap = new Dictionary<string, string>
            {
                { "info", "#0d6efd" },
                { "success", "#198754" },
                { "warning", "#ffc107" },
                { "danger", "#dc3545" }
            };

            var headerColor = colorMap.ContainsKey(type) ? colorMap[type] : colorMap["info"];

            return $@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>{title}</title>
</head>
<body style='margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;'>
    <table role='presentation' style='width: 100%; border-collapse: collapse;'>
        <tr>
            <td align='center' style='padding: 40px 0;'>
                <table role='presentation' style='width: 600px; border-collapse: collapse; background-color: #ffffff; box-shadow: 0 4px 6px rgba(0,0,0,0.1);'>
                    <!-- Header -->
                    <tr>
                        <td style='background-color: {headerColor}; padding: 30px; text-align: center;'>
                            <h1 style='color: #ffffff; margin: 0; font-size: 28px;'>📚 Sistema de Biblioteca</h1>
                        </td>
                    </tr>

                    <!-- Content -->
                    <tr>
                        <td style='padding: 40px 30px;'>
                            <h2 style='color: #333333; margin-top: 0; font-size: 24px;'>{title}</h2>
                            <p style='color: #666666; font-size: 16px; line-height: 1.6; margin: 20px 0;'>
                                {greeting}
                            </p>
                            <div style='color: #666666; font-size: 16px; line-height: 1.6;'>
                                {content}
                            </div>
                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td style='background-color: #f8f9fa; padding: 20px 30px; text-align: center; border-top: 3px solid {headerColor};'>
                            <p style='color: #6c757d; font-size: 14px; margin: 0;'>
                                Este es un mensaje automático del Sistema de Biblioteca.
                            </p>
                            <p style='color: #6c757d; font-size: 14px; margin: 10px 0 0 0;'>
                                Por favor, no respondas a este correo.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
        }
    }
}