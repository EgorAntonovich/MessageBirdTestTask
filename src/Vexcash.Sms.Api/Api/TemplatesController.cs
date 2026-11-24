using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vexcash.Sms.Api.Application;
using Vexcash.Sms.Api.Data;
using Vexcash.Sms.Api.Domain;

namespace Vexcash.Sms.Api.Api;

[ApiController]
[Route("api/templates")]
[Produces("application/json")]
public sealed class TemplatesController(AppDbContext dbContext, ITemplateRenderer renderer) : ControllerBase
{
    /// <summary>Lists all message templates.</summary>
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken) => Ok(await dbContext.MessageTemplates
        .AsNoTracking().OrderBy(x => x.Kind).Select(x => new { x.Id, x.Name, x.Kind, x.Body, x.UpdatedAt }).ToListAsync(cancellationToken));

    /// <summary>Creates a reusable SMS template.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(UpsertTemplateRequest request, CancellationToken cancellationToken)
    {
        var unsupported = renderer.FindUnsupportedPlaceholders(request.Body);
        if (unsupported.Count > 0) return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { ["body"] = [$"Unsupported placeholder(s): {string.Join(", ", unsupported)}."] }));
        var template = new MessageTemplate(request.Name, request.Kind, request.Body);
        dbContext.MessageTemplates.Add(template);
        await dbContext.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = template.Id }, new { template.Id, template.Name, template.Kind, template.Body, template.UpdatedAt });
    }

    /// <summary>Returns one message template.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var template = await dbContext.MessageTemplates.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        return template is null ? NotFound() : Ok(new { template.Id, template.Name, template.Kind, template.Body, template.UpdatedAt });
    }

    /// <summary>Updates a message template.</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpsertTemplateRequest request, CancellationToken cancellationToken)
    {
        var template = await dbContext.MessageTemplates.FindAsync([id], cancellationToken);
        if (template is null) return NotFound();
        var unsupported = renderer.FindUnsupportedPlaceholders(request.Body);
        if (unsupported.Count > 0) return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { ["body"] = [$"Unsupported placeholder(s): {string.Join(", ", unsupported)}."] }));
        template.Update(request.Name, request.Kind, request.Body);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(new { template.Id, template.Name, template.Kind, template.Body, template.UpdatedAt });
    }
}
