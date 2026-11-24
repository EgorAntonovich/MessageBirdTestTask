namespace Vexcash.Sms.Api.Domain;

public sealed class Customer
{
    private Customer() { }

    public Customer(string phoneNumber, string name, string creditNumber, decimal amount, DateOnly dueDate)
    {
        Id = Guid.NewGuid();
        PhoneNumber = Required(phoneNumber, 32);
        Name = Required(name, 160);
        CreditNumber = Required(creditNumber, 64);
        SetPayment(amount, dueDate);
    }

    public Guid Id { get; private set; }
    public string PhoneNumber { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string CreditNumber { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public DateOnly DueDate { get; private set; }
    public ICollection<SmsMessage> Messages { get; } = new List<SmsMessage>();

    public void SetPayment(decimal amount, DateOnly dueDate)
    {
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
        Amount = amount;
        DueDate = dueDate;
    }

    private static string Required(string value, int maxLength)
    {
        var normalized = value.Trim();
        if (normalized.Length is 0 || normalized.Length > maxLength) throw new ArgumentException("Value is required and must fit the configured length.");
        return normalized;
    }
}
