using Microsoft.Extensions.Configuration;
using Npgsql;
using Vexcash.Sms.Api.Data;
using Xunit;

namespace Vexcash.Sms.Api.Tests;

public sealed class DatabaseUrlTests
{
    [Fact]
    public void Resolve_PrefersRailwayDatabaseUrlOverLocalFallback()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["DATABASE_URL"] = "postgresql://railway:secret%40value@postgres.railway.internal:5432/railway",
            ["ConnectionStrings:Postgres"] = "Host=localhost;Port=5432;Database=vexcash_sms;Username=postgres;Password=postgres"
        });

        var result = new NpgsqlConnectionStringBuilder(DatabaseUrl.Resolve(configuration));

        Assert.Equal("postgres.railway.internal", result.Host);
        Assert.Equal("railway", result.Database);
        Assert.Equal("railway", result.Username);
        Assert.Equal("secret@value", result.Password);
    }

    [Fact]
    public void Resolve_UsesStandardConnectionStringWhenDatabaseUrlIsAbsent()
    {
        const string local = "Host=postgres;Port=5432;Database=vexcash_sms;Username=postgres;Password=postgres";
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Postgres"] = local
        });

        Assert.Equal(local, DatabaseUrl.Resolve(configuration));
    }

    private static IConfiguration BuildConfiguration(Dictionary<string, string?> values) =>
        new ConfigurationBuilder().AddInMemoryCollection(values).Build();
}
