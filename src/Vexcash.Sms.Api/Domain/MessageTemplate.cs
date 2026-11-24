namespace Vexcash.Sms.Api.Domain;

public sealed class MessageTemplate
{
    private MessageTemplate() { }

    public MessageTemplate(string name, TemplateKind kind, string body)
    {
        Id = Guid.NewGuid();
        Update(name, kind, body);
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public TemplateKind Kind { get; private set; }
    public string Body { get; private set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; private set; }
    public ICollection<SmsMessage> Messages { get; } = new List<SmsMessage>();

    public void Update(string name, TemplateKind kind, string body)
    {
        Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("Template name is required.") : name.Trim();
        Body = string.IsNullOrWhiteSpace(body) ? throw new ArgumentException("Template body is required.") : body.Trim();
        if (Name.Length > 120 || Body.Length > 2_000) throw new ArgumentException("Template exceeds the allowed length.");
        Kind = kind;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
