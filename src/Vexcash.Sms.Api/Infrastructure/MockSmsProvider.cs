using System.Text.Json;
using Vexcash.Sms.Api.Application;

namespace Vexcash.Sms.Api.Infrastructure;

public sealed class MockSmsProvider : ISmsProvider
{
    public Task<SmsProviderResult> SendAsync(string recipient, string body, string reference, CancellationToken cancellationToken)
    {
        var id = $"mock_{Guid.NewGuid():N}";
        var response = JsonSerializer.Serialize(new
        {
            id,
            direction = "mt",
            type = "sms",
            originator = "Vexcash",
            body,
            reference,
            createdDatetime = DateTimeOffset.UtcNow,
            recipients = new { totalCount = 1, totalSentCount = 1, items = new[] { new { recipient, status = "sent" } } }
        });
        return Task.FromResult(new SmsProviderResult(id, "sent", 201, response));
    }
}
