using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace MenStyle.Web.Services;

public sealed class SmtpEmailSender : IEmailSender
{
    private readonly EmailSettings _settings;

    public SmtpEmailSender(IOptions<EmailSettings> options)
    {
        _settings = options.Value;
    }

    public async Task SendHtmlAsync(
        string recipientEmail,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        ValidateConfiguration();
        cancellationToken.ThrowIfCancellationRequested();

        var senderEmail = string.IsNullOrWhiteSpace(_settings.SenderEmail)
            ? _settings.Username.Trim()
            : _settings.SenderEmail.Trim();

        using var message = new MailMessage
        {
            From = new MailAddress(senderEmail, _settings.SenderName, Encoding.UTF8),
            Subject = subject,
            SubjectEncoding = Encoding.UTF8,
            Body = htmlBody,
            BodyEncoding = Encoding.UTF8,
            IsBodyHtml = true
        };

        message.To.Add(new MailAddress(recipientEmail));

        using var client = new SmtpClient(_settings.SmtpHost.Trim(), _settings.SmtpPort)
        {
            EnableSsl = _settings.EnableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(
                _settings.Username.Trim(),
                _settings.Password)
        };

        await client.SendMailAsync(message);
    }

    private void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_settings.SmtpHost)
            || _settings.SmtpPort <= 0
            || string.IsNullOrWhiteSpace(_settings.Username)
            || string.IsNullOrWhiteSpace(_settings.Password))
        {
            throw new InvalidOperationException(
                "Chưa cấu hình đầy đủ EmailSettings để gửi email SMTP.");
        }

        if (string.IsNullOrWhiteSpace(_settings.SenderEmail)
            && string.IsNullOrWhiteSpace(_settings.Username))
        {
            throw new InvalidOperationException(
                "Chưa cấu hình địa chỉ email người gửi.");
        }
    }
}
