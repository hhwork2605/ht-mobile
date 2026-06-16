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
        // Dev sender: log cả nội dung để thấy link (reset mật khẩu…) — KHÔNG gửi thật.
        _logger.LogInformation("[Email:DEV] To={To} Subject={Subject}\n{Body}", to, subject, htmlBody);
        return Task.CompletedTask;
    }
}
