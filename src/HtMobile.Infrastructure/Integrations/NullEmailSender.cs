using HtMobile.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace HtMobile.Infrastructure.Integrations;

/// <summary>
/// Email sender mặc định (chỉ log) cho dev. Thay bằng SMTP/SendGrid ở phase Dịch vụ.
/// </summary>
public class NullEmailSender : IEmailSender
{
    private readonly ILogger<NullEmailSender> _logger;

    public NullEmailSender(ILogger<NullEmailSender> logger) => _logger = logger;

    public Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        _logger.LogInformation("[Email:DEV] To={To} Subject={Subject}", to, subject);
        return Task.CompletedTask;
    }
}
