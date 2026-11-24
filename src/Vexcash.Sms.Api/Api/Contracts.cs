using System.ComponentModel.DataAnnotations;
using Vexcash.Sms.Api.Domain;

namespace Vexcash.Sms.Api.Api;

public sealed record SendMessageRequest(
    [Required, MaxLength(64)] string CreditNumber,
    Guid TemplateId);

public sealed record UpsertTemplateRequest(
    [Required, MaxLength(120)] string Name,
    TemplateKind Kind,
    [Required, MaxLength(2000)] string Body);
