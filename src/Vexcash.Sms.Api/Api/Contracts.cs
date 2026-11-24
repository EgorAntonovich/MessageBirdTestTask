using System.ComponentModel.DataAnnotations;
using Vexcash.Sms.Api.Domain;

namespace Vexcash.Sms.Api.Api;

public sealed record SendMessageRequest(
    [Required, MaxLength(64)] string CreditNumber,
    Guid TemplateId);

public sealed record CreateCustomerRequest(
    [Required, MaxLength(32), RegularExpression(@"^\+[1-9]\d{7,14}$", ErrorMessage = "Use international E.164 format, for example +491701234567.")] string PhoneNumber,
    [Required, MaxLength(160)] string Name,
    [Required, MaxLength(64)] string CreditNumber,
    [Range(typeof(decimal), "0", "9999999999999999.99")] decimal Amount,
    DateOnly DueDate);

public sealed record UpsertTemplateRequest(
    [Required, MaxLength(120)] string Name,
    TemplateKind Kind,
    [Required, MaxLength(2000)] string Body);
