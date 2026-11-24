using Microsoft.EntityFrameworkCore;
using Vexcash.Sms.Api.Data;
using Vexcash.Sms.Api.Domain;

namespace Vexcash.Sms.Api.Application;

public sealed record MessagePreview(Guid CustomerId, Guid TemplateId, string CreditNumber, string CustomerName, string Recipient, decimal Amount, DateOnly DueDate, string Body);
public sealed record MessageResult(Guid Id, string CreditNumber, string CustomerName, string TemplateName, string Recipient, string Body, string Status, string? ProviderMessageId, string? ProviderStatus, DateTimeOffset CreatedAt, string? ErrorMessage);

public interface ISmsDispatchService
{
    Task<MessagePreview> PreviewAsync(string creditNumber, Guid templateId, CancellationToken cancellationToken);
    Task<MessageResult> SendAsync(string creditNumber, Guid templateId, CancellationToken cancellationToken);
}

public sealed class SmsDispatchService(AppDbContext dbContext, ITemplateRenderer renderer, ISmsProvider provider) : ISmsDispatchService
{
    public async Task<MessagePreview> PreviewAsync(string creditNumber, Guid templateId, CancellationToken cancellationToken)
    {
        var customer = await dbContext.Customers.SingleOrDefaultAsync(x => x.CreditNumber == creditNumber, cancellationToken)
            ?? throw new KeyNotFoundException($"Customer with credit number '{creditNumber}' was not found.");
        var template = await dbContext.MessageTemplates.FindAsync([templateId], cancellationToken)
            ?? throw new KeyNotFoundException($"Template '{templateId}' was not found.");
        return new MessagePreview(customer.Id, template.Id, customer.CreditNumber, customer.Name, customer.PhoneNumber, customer.Amount, customer.DueDate, renderer.Render(template.Body, customer));
    }

    public async Task<MessageResult> SendAsync(string creditNumber, Guid templateId, CancellationToken cancellationToken)
    {
        var customer = await dbContext.Customers.SingleOrDefaultAsync(x => x.CreditNumber == creditNumber, cancellationToken)
            ?? throw new KeyNotFoundException($"Customer with credit number '{creditNumber}' was not found.");
        var template = await dbContext.MessageTemplates.SingleOrDefaultAsync(x => x.Id == templateId, cancellationToken)
            ?? throw new KeyNotFoundException($"Template '{templateId}' was not found.");

        var message = new SmsMessage(customer, template, customer.PhoneNumber, renderer.Render(template.Body, customer));
        dbContext.SmsMessages.Add(message);
        await dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            var result = await provider.SendAsync(message.Recipient, message.Body, message.Id.ToString(), cancellationToken);
            message.MarkAccepted(result.MessageId, result.Status, result.HttpStatus, result.ResponseJson);
        }
        catch (SmsProviderException exception)
        {
            message.MarkFailed(exception.HttpStatus, exception.ResponseJson, exception.Message);
        }
        catch (HttpRequestException exception)
        {
            message.MarkFailed(null, null, $"MessageBird could not be reached: {exception.Message}");
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(message, customer.CreditNumber, customer.Name, template.Name);
    }

    public static MessageResult Map(SmsMessage message, string creditNumber, string customerName, string templateName) =>
        new(message.Id, creditNumber, customerName, templateName, message.Recipient, message.Body, message.Status.ToString(), message.ProviderMessageId, message.ProviderStatus, message.CreatedAt, message.ErrorMessage);
}
