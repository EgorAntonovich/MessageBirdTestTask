using Vexcash.Sms.Api.Domain;
using Xunit;

namespace Vexcash.Sms.Api.Tests;

public sealed class CustomerTests
{
    [Fact]
    public void Constructor_AcceptsInternationalPhoneNumber()
    {
        var customer = new Customer(" +491701234567 ", "Test Customer", "TEST-10001", 125.50m, new DateOnly(2026, 12, 15));

        Assert.Equal("+491701234567", customer.PhoneNumber);
    }

    [Theory]
    [InlineData("01701234567")]
    [InlineData("+0123456789")]
    [InlineData("+49 170 1234567")]
    public void Constructor_RejectsPhoneNumberOutsideE164Format(string phoneNumber)
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new Customer(phoneNumber, "Test Customer", "TEST-10001", 125.50m, new DateOnly(2026, 12, 15)));

        Assert.Contains("E.164", exception.Message);
    }
}
