using System.Globalization;
using System.Text.RegularExpressions;
using Vexcash.Sms.Api.Domain;

namespace Vexcash.Sms.Api.Application;

public interface ITemplateRenderer
{
    string Render(string template, Customer customer);
    IReadOnlyCollection<string> FindUnsupportedPlaceholders(string template);
}

public sealed partial class TemplateRenderer : ITemplateRenderer
{
    private static readonly HashSet<string> Supported = new(StringComparer.OrdinalIgnoreCase)
    {
        "CreditNumber", "Name", "Amount", "DueDate"
    };

    public string Render(string template, Customer customer)
    {
        var unsupported = FindUnsupportedPlaceholders(template);
        if (unsupported.Count > 0)
            throw new InvalidOperationException($"Unsupported placeholder(s): {string.Join(", ", unsupported)}.");

        var amount = customer.Amount.ToString("C", CultureInfo.GetCultureInfo("en-IE"));
        return template
            .Replace("{CreditNumber}", customer.CreditNumber, StringComparison.OrdinalIgnoreCase)
            .Replace("{Name}", customer.Name, StringComparison.OrdinalIgnoreCase)
            .Replace("{Amount}", amount, StringComparison.OrdinalIgnoreCase)
            .Replace("{DueDate}", customer.DueDate.ToString("d MMMM yyyy", CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase);
    }

    public IReadOnlyCollection<string> FindUnsupportedPlaceholders(string template) => PlaceholderRegex()
        .Matches(template)
        .Select(match => match.Groups[1].Value)
        .Where(value => !Supported.Contains(value))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();

    [GeneratedRegex(@"\{([A-Za-z][A-Za-z0-9]*)\}")]
    private static partial Regex PlaceholderRegex();
}
