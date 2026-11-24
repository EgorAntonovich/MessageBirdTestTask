using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vexcash.Sms.Api.Data;

namespace Vexcash.Sms.Api.Api;

[ApiController]
[Route("api/customers")]
[Produces("application/json")]
public sealed class CustomersController(AppDbContext dbContext) : ControllerBase
{
    /// <summary>Lists customers available for manual SMS dispatch.</summary>
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken) => Ok(await dbContext.Customers
        .AsNoTracking()
        .OrderBy(x => x.Name)
        .Select(x => new { x.Id, x.CreditNumber, x.Name, x.PhoneNumber, x.Amount, x.DueDate })
        .ToListAsync(cancellationToken));

    /// <summary>Returns one customer by credit number.</summary>
    [HttpGet("{creditNumber}")]
    public async Task<IActionResult> Get(string creditNumber, CancellationToken cancellationToken)
    {
        var customer = await dbContext.Customers.AsNoTracking().SingleOrDefaultAsync(x => x.CreditNumber == creditNumber, cancellationToken);
        return customer is null ? NotFound() : Ok(new { customer.Id, customer.CreditNumber, customer.Name, customer.PhoneNumber, customer.Amount, customer.DueDate });
    }
}
