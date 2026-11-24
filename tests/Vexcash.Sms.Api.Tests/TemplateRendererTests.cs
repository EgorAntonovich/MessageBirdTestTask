using Vexcash.Sms.Api.Application;
using Vexcash.Sms.Api.Domain;
using Xunit;

namespace Vexcash.Sms.Api.Tests;

public sealed class TemplateRendererTests
{
    [Fact]
    public void Render_ReplacesEverySupportedPlaceholder()
    {
        var customer = new Customer("+31612345678", "Amelia Hart", "VC-10482", 480m, new DateOnly(2026, 11, 28));
        var renderer = new TemplateRenderer();

        var result = renderer.Render("{CreditNumber}|{Name}|{Amount}|{DueDate}", customer);

        Assert.Equal("VC-10482|Amelia Hart|€480.00|28 November 2026", result);
    }

    [Fact]
    public void Render_RejectsUnsupportedPlaceholder()
    {
        var customer = new Customer("+31612345678", "Amelia Hart", "VC-10482", 480m, new DateOnly(2026, 11, 28));
        var renderer = new TemplateRenderer();

        var exception = Assert.Throws<InvalidOperationException>(() => renderer.Render("Hello {FirstName}", customer));

        Assert.Contains("FirstName", exception.Message);
    }
}
