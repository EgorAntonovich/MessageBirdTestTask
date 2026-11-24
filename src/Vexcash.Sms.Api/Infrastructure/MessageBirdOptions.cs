namespace Vexcash.Sms.Api.Infrastructure;

public sealed class MessageBirdOptions
{
    public const string SectionName = "MessageBird";
    public string BaseUrl { get; init; } = "https://rest.messagebird.com/";
    public string? AccessKey { get; init; }
    public string Originator { get; init; } = "Vexcash";
    public bool UseMock { get; init; } = true;
}
