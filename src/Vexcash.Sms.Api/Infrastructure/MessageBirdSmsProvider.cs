using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Vexcash.Sms.Api.Application;

namespace Vexcash.Sms.Api.Infrastructure;

public sealed class MessageBirdSmsProvider(HttpClient httpClient, IOptions<MessageBirdOptions> options) : ISmsProvider
{
    private readonly MessageBirdOptions _options = options.Value;

    public async Task<SmsProviderResult> SendAsync(string recipient, string body, string reference, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.AccessKey))
            throw new SmsProviderException(null, null, "MessageBird access key is not configured.");

        using var request = new HttpRequestMessage(HttpMethod.Post, "messages");
        request.Headers.Authorization = new AuthenticationHeaderValue("AccessKey", _options.AccessKey);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["recipients"] = recipient,
            ["originator"] = _options.Originator,
            ["body"] = body,
            ["reference"] = reference,
            ["datacoding"] = "auto"
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new SmsProviderException((int)response.StatusCode, json, $"MessageBird rejected the message with HTTP {(int)response.StatusCode}.");

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var id = root.GetProperty("id").GetString() ?? throw new SmsProviderException((int)response.StatusCode, json, "MessageBird response did not contain a message ID.");
        var status = root.TryGetProperty("recipients", out var recipients)
            && recipients.TryGetProperty("items", out var items)
            && items.GetArrayLength() > 0
            && items[0].TryGetProperty("status", out var statusNode)
                ? statusNode.GetString() ?? "accepted"
                : "accepted";

        return new SmsProviderResult(id, status, (int)response.StatusCode, json);
    }
}
