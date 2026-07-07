using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using AuthService.Application.Interfaces;

namespace AuthService.Application.Services;

public class EmailService(IConfiguration configuration, ILogger<EmailService> logger) : IEmailService
{

    public async Task SendEmailVerificationAsync(string email, string username, string token)
    {
        var subject = "Verifica tu correo - Bite&Go";
        var verificationUrl = $"{configuration["AppSettings:FrontendUrl"]}/verify-email?token={token}";

        var body = $@"<!DOCTYPE html>
<html lang='es'>
<head><meta charset='utf-8'><meta name='viewport' content='width=device-width,initial-scale=1'></head>
<body style='margin:0;padding:0;background-color:#F5EFE6;font-family:Arial,Helvetica,sans-serif;'>
<table width='100%' cellpadding='0' cellspacing='0' style='background-color:#F5EFE6;padding:40px 0;'>
<tr><td align='center'>
<table width='500' cellpadding='0' cellspacing='0' style='background:#fff;border-radius:16px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,0.08);'>
  <tr><td style='background:linear-gradient(135deg,#E67E22,#D35400);padding:32px 24px;text-align:center;'>
    <h1 style='color:#fff;margin:0;font-size:28px;font-weight:800;font-family:Arial,sans-serif;'>Bite &amp; Go</h1>
    <p style='color:rgba(255,255,255,0.85);margin:8px 0 0;font-size:14px;'>Verificación de correo electrónico</p>
  </td></tr>
  <tr><td style='padding:32px 24px;text-align:center;'>
    <table width='64' height='64' cellpadding='0' cellspacing='0' style='margin:0 auto 20px;background:#F5EFE6;border-radius:50%;'>
    <tr><td align='center' valign='middle' style='font-size:28px;'>✉</td></tr>
    </table>
    <h2 style='color:#2B2B2B;font-size:20px;margin:0 0 12px;font-family:Arial,sans-serif;'>¡Hola {username}!</h2>
    <p style='color:#6B6B6B;font-size:14px;line-height:1.6;margin:0 0 24px;'>Para completar tu registro, verifica tu correo electrónico haciendo clic en el botón de abajo.</p>
    <table cellpadding='0' cellspacing='0' style='margin:0 auto;'><tr>
      <td align='center' style='background:#E67E22;border-radius:10px;'>
        <a href='{verificationUrl}' target='_blank' style='display:inline-block;color:#fff;padding:14px 32px;text-decoration:none;font-weight:700;font-size:15px;font-family:Arial,sans-serif;'>Verificar Mi Correo</a>
      </td>
    </tr></table>
    <p style='color:#6B6B6B;font-size:12px;margin:24px 0 0;line-height:1.5;'>Si no puedes hacer clic, copia y pega este enlace en tu navegador:</p>
    <p style='color:#E67E22;font-size:12px;word-break:break-all;margin:8px 0 0;'>{verificationUrl}</p>
  </td></tr>
  <tr><td style='background:#F5EFE6;padding:16px 24px;text-align:center;'>
    <p style='color:#8a7a72;font-size:11px;margin:0;'>Este enlace expira en 24 horas.</p>
    <p style='color:#8a7a72;font-size:11px;margin:4px 0 0;'>Si no creaste una cuenta, ignora este correo.</p>
  </td></tr>
</table>
</td></tr></table>
</body></html>";

        await SendEmailAsync(email, subject, body);
    }

    public async Task SendPasswordResetAsync(string email, string username, string token)
    {
        var subject = "Restablecer contraseña - Bite&Go";
        var resetUrl = $"{configuration["AppSettings:FrontendUrl"]}/reset-password?token={token}";

        var body = $@"<!DOCTYPE html>
<html lang='es'>
<head><meta charset='utf-8'><meta name='viewport' content='width=device-width,initial-scale=1'></head>
<body style='margin:0;padding:0;background-color:#F5EFE6;font-family:Arial,Helvetica,sans-serif;'>
<table width='100%' cellpadding='0' cellspacing='0' style='background-color:#F5EFE6;padding:40px 0;'>
<tr><td align='center'>
<table width='500' cellpadding='0' cellspacing='0' style='background:#fff;border-radius:16px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,0.08);'>
  <tr><td style='background:linear-gradient(135deg,#E67E22,#D35400);padding:32px 24px;text-align:center;'>
    <h1 style='color:#fff;margin:0;font-size:28px;font-weight:800;font-family:Arial,sans-serif;'>Bite &amp; Go</h1>
    <p style='color:rgba(255,255,255,0.85);margin:8px 0 0;font-size:14px;'>Restablecer contraseña</p>
  </td></tr>
  <tr><td style='padding:32px 24px;text-align:center;'>
    <table width='64' height='64' cellpadding='0' cellspacing='0' style='margin:0 auto 20px;background:#F5EFE6;border-radius:50%;'>
    <tr><td align='center' valign='middle' style='font-size:28px;'>🔒</td></tr>
    </table>
    <h2 style='color:#2B2B2B;font-size:20px;margin:0 0 12px;font-family:Arial,sans-serif;'>Hola {username}</h2>
    <p style='color:#6B6B6B;font-size:14px;line-height:1.6;margin:0 0 24px;'>Recibimos una solicitud para restablecer tu contraseña. Haz clic en el botón de abajo.</p>
    <table cellpadding='0' cellspacing='0' style='margin:0 auto;'><tr>
      <td align='center' style='background:#E67E22;border-radius:10px;'>
        <a href='{resetUrl}' target='_blank' style='display:inline-block;color:#fff;padding:14px 32px;text-decoration:none;font-weight:700;font-size:15px;font-family:Arial,sans-serif;'>Restablecer Contraseña</a>
      </td>
    </tr></table>
    <p style='color:#6B6B6B;font-size:12px;margin:24px 0 0;line-height:1.5;'>Si no puedes hacer clic, copia y pega este enlace:</p>
    <p style='color:#E67E22;font-size:12px;word-break:break-all;margin:8px 0 0;'>{resetUrl}</p>
  </td></tr>
  <tr><td style='background:#F5EFE6;padding:16px 24px;text-align:center;'>
    <p style='color:#8a7a72;font-size:11px;margin:0;'>Este enlace expira en 1 hora.</p>
    <p style='color:#8a7a72;font-size:11px;margin:4px 0 0;'>Si no solicitaste esto, ignora este correo.</p>
  </td></tr>
</table>
</td></tr></table>
</body></html>";

        await SendEmailAsync(email, subject, body);
    }

    public async Task SendWelcomeEmailAsync(string email, string username)
    {
        var subject = "¡Bienvenido a Bite&Go!";
        var frontendUrl = configuration["AppSettings:FrontendUrl"];

        var body = $@"<!DOCTYPE html>
<html lang='es'>
<head><meta charset='utf-8'><meta name='viewport' content='width=device-width,initial-scale=1'></head>
<body style='margin:0;padding:0;background-color:#F5EFE6;font-family:Arial,Helvetica,sans-serif;'>
<table width='100%' cellpadding='0' cellspacing='0' style='background-color:#F5EFE6;padding:40px 0;'>
<tr><td align='center'>
<table width='500' cellpadding='0' cellspacing='0' style='background:#fff;border-radius:16px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,0.08);'>
  <tr><td style='background:linear-gradient(135deg,#E67E22,#D35400);padding:32px 24px;text-align:center;'>
    <h1 style='color:#fff;margin:0;font-size:28px;font-weight:800;font-family:Arial,sans-serif;'>Bite &amp; Go</h1>
    <p style='color:rgba(255,255,255,0.85);margin:8px 0 0;font-size:14px;'>¡Cuenta activada!</p>
  </td></tr>
  <tr><td style='padding:32px 24px;text-align:center;'>
    <table width='64' height='64' cellpadding='0' cellspacing='0' style='margin:0 auto 20px;background:#F5EFE6;border-radius:50%;'>
    <tr><td align='center' valign='middle' style='font-size:28px;'>✓</td></tr>
    </table>
    <h2 style='color:#2B2B2B;font-size:20px;margin:0 0 12px;font-family:Arial,sans-serif;'>¡Bienvenido, {username}!</h2>
    <p style='color:#6B6B6B;font-size:14px;line-height:1.6;margin:0 0 24px;'>Tu cuenta ha sido verificada exitosamente. Ya puedes disfrutar de todos los restaurantes que Bite&amp;Go tiene para ti.</p>
    <table cellpadding='0' cellspacing='0' style='margin:0 auto;'><tr>
      <td align='center' style='background:#E67E22;border-radius:10px;'>
        <a href='{frontendUrl}/auth' target='_blank' style='display:inline-block;color:#fff;padding:14px 32px;text-decoration:none;font-weight:700;font-size:15px;font-family:Arial,sans-serif;'>Ir a Bite&amp;Go</a>
      </td>
    </tr></table>
  </td></tr>
  <tr><td style='background:#F5EFE6;padding:16px 24px;text-align:center;'>
    <p style='color:#8a7a72;font-size:11px;margin:0;'>Gracias por confiar en nosotros.</p>
  </td></tr>
</table>
</td></tr></table>
</body></html>";

        await SendEmailAsync(email, subject, body);
    }

    private async Task SendEmailAsync(string to, string subject, string body)
    {
        var smtpSettings = configuration.GetSection("SmtpSettings");

        try
        {
            // Verificar si el email está habilitado
            var enabled = bool.Parse(smtpSettings["Enabled"] ?? "true");
            if (!enabled)
            {
                logger.LogInformation("Email disabled in configuration. Skipping send");
                return;
            }

            // Validar configuración
            var host = smtpSettings["Host"];
            var portString = smtpSettings["Port"];
            var username = smtpSettings["Username"];
            var password = smtpSettings["Password"];
            var fromEmail = smtpSettings["FromEmail"];
            var fromName = smtpSettings["FromName"];

            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                logger.LogError("SMTP settings are not properly configured");
                throw new InvalidOperationException("SMTP settings are not properly configured");
            }

            var port = int.Parse(portString ?? "587");

            using var client = new SmtpClient();

            // Configurar timeout
            var timeoutMs = int.Parse(smtpSettings["Timeout"] ?? "30000");
            client.Timeout = timeoutMs;

            try
            {

                // Verificar configuración de SSL implícito
                var useImplicitSsl = bool.Parse(smtpSettings["UseImplicitSsl"] ?? "false");

                // Configuración específica por puerto y SSL
                if (useImplicitSsl || port == 465)
                {
                    await client.ConnectAsync(host, port, SecureSocketOptions.SslOnConnect);
                }
                else if (port == 587)
                {
                    await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);
                }
                else
                {
                    await client.ConnectAsync(host, port, SecureSocketOptions.Auto);
                }

                // Autenticación
                await client.AuthenticateAsync(username, password);

                // Crear mensaje con MimeKit
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(fromName ?? "Bite&Go", fromEmail ?? "noreply@bitego.com"));
                message.To.Add(new MailboxAddress("", to));
                message.Subject = subject;
                message.Body = new TextPart("html") { Text = body };

                // Enviar
                await client.SendAsync(message);
                logger.LogInformation("Email sent successfully");

                await client.DisconnectAsync(true);
                logger.LogInformation("Email pipeline completed");
            }
            catch (MailKit.Security.AuthenticationException authEx)
            {
                logger.LogError(authEx, "Gmail authentication failed. Check app password.");
                throw new InvalidOperationException($"Gmail authentication failed: {authEx.Message}. Please check your app password.", authEx);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send email");
                throw;
            }
            logger.LogInformation("Email processed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email");

            // Verificar si usar fallback
            var useFallback = bool.Parse(smtpSettings["UseFallback"] ?? "false");
            if (useFallback)
            {
                logger.LogWarning("Using email fallback");
                return;
            }

            throw new InvalidOperationException($"Failed to send email: {ex.Message}", ex);
        }
    }
}