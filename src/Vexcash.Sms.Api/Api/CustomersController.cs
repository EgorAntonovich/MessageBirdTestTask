using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vexcash.Sms.Api.Data;
using Vexcash.Sms.Api.Domain;

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

    /// <summary>Creates a customer who can receive messages.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        var creditNumber = request.CreditNumber.Trim();
        if (request.DueDate == default)
        {
            ModelState.AddModelError(nameof(request.DueDate), "Due date is required.");
            return ValidationProblem(ModelState);
        }

        if (await dbContext.Customers.AnyAsync(x => x.CreditNumber == creditNumber, cancellationToken))
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Credit number already exists",
                Detail = $"A customer with credit number '{creditNumber}' already exists."
            });
        }

        var customer = new Customer(request.PhoneNumber, request.Name, creditNumber, request.Amount, request.DueDate);
        dbContext.Customers.Add(customer);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(Get), new { creditNumber = customer.CreditNumber }, new
        {
            customer.Id,
            customer.CreditNumber,
            customer.Name,
            customer.PhoneNumber,
            customer.Amount,
            customer.DueDate
        });
    }
}
