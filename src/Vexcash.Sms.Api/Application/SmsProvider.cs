namespace Vexcash.Sms.Api.Application;

public sealed record SmsProviderResult(string MessageId, string Status, int HttpStatus, string ResponseJson);

public sealed class SmsProviderException(int? httpStatus, string? responseJson, string message) : Exception(message)
{
    public int? HttpStatus { get; } = httpStatus;
    public string? ResponseJson { get; } = responseJson;
}

public interface ISmsProvider
{
    Task<SmsProviderResult> SendAsync(string recipient, string body, string reference, CancellationToken cancellationToken);
}
