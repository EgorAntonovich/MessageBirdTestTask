namespace Vexcash.Sms.Api.Domain;

public sealed class SmsMessage
{
    private SmsMessage() { }

    public SmsMessage(Customer customer, MessageTemplate template, string recipient, string body)
    {
        Id = Guid.NewGuid();
        Customer = customer;
        CustomerId = customer.Id;
        Template = template;
        TemplateId = template.Id;
        Recipient = recipient;
        Body = body;
        Status = MessageStatus.Pending;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public Customer Customer { get; private set; } = null!;
    public Guid TemplateId { get; private set; }
    public MessageTemplate Template { get; private set; } = null!;
    public string Recipient { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public MessageStatus Status { get; private set; }
    public string? ProviderMessageId { get; private set; }
    public string? ProviderStatus { get; private set; }
    public int? ProviderHttpStatus { get; private set; }
    public string? ProviderResponseJson { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    public void MarkAccepted(string providerMessageId, string providerStatus, int httpStatus, string responseJson)
    {
        ProviderMessageId = providerMessageId;
        ProviderStatus = providerStatus;
        ProviderHttpStatus = httpStatus;
        ProviderResponseJson = responseJson;
        Status = MessageStatus.Accepted;
        CompletedAt = DateTimeOffset.UtcNow;
    }

    public void MarkFailed(int? httpStatus, string? responseJson, string errorMessage)
    {
        ProviderHttpStatus = httpStatus;
        ProviderResponseJson = responseJson;
        ErrorMessage = errorMessage.Length > 1_000 ? errorMessage[..1_000] : errorMessage;
        Status = MessageStatus.Failed;
        CompletedAt = DateTimeOffset.UtcNow;
    }
}
