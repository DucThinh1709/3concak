namespace MenStyle.Web.Services;

public interface IEmailSender
{
    Task SendHtmlAsync(
        string recipientEmail,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default);
}
