using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vexcash.Sms.Api.Application;
using Vexcash.Sms.Api.Data;

namespace Vexcash.Sms.Api.Api;

[ApiController]
[Route("api/messages")]
[Produces("application/json")]
public sealed class MessagesController(AppDbContext dbContext, ISmsDispatchService dispatchService) : ControllerBase
{
    /// <summary>Resolves a template for a customer without sending it.</summary>
    [HttpPost("preview")]
    public async Task<IActionResult> Preview(SendMessageRequest request, CancellationToken cancellationToken) =>
        Ok(await dispatchService.PreviewAsync(request.CreditNumber, request.TemplateId, cancellationToken));

    /// <summary>Sends one SMS using a credit number and template, then stores the provider response.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Send(SendMessageRequest request, CancellationToken cancellationToken)
    {
        var result = await dispatchService.SendAsync(request.CreditNumber, request.TemplateId, cancellationToken);
        if (result.Status == "Failed")
        {
            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status502BadGateway,
                Title = "Message provider rejected the request",
                Detail = result.ErrorMessage ?? "MessageBird did not accept the message."
            };
            problem.Extensions["messageId"] = result.Id;
            return StatusCode(StatusCodes.Status502BadGateway, problem);
        }
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    /// <summary>Returns paged send history, newest first.</summary>
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? creditNumber, [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = dbContext.SmsMessages.AsNoTracking().Include(x => x.Customer).Include(x => x.Template).AsQueryable();
        if (!string.IsNullOrWhiteSpace(creditNumber)) query = query.Where(x => x.Customer.CreditNumber == creditNumber);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(x => x.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new MessageResult(x.Id, x.Customer.CreditNumber, x.Customer.Name, x.Template.Name, x.Recipient, x.Body, x.Status.ToString(), x.ProviderMessageId, x.ProviderStatus, x.CreatedAt, x.ErrorMessage))
            .ToListAsync(cancellationToken);
        return Ok(new { page, pageSize, total, items });
    }

    /// <summary>Returns a stored send record including the provider response.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var message = await dbContext.SmsMessages.AsNoTracking().Include(x => x.Customer).Include(x => x.Template).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        return message is null ? NotFound() : Ok(new
        {
            message.Id,
            message.CustomerId,
            message.Customer.CreditNumber,
            CustomerName = message.Customer.Name,
            message.TemplateId,
            TemplateName = message.Template.Name,
            message.Recipient,
            message.Body,
            message.Status,
            message.ProviderMessageId,
            message.ProviderStatus,
            message.ProviderHttpStatus,
            message.ProviderResponseJson,
            message.ErrorMessage,
            message.CreatedAt,
            message.CompletedAt
        });
    }
}
