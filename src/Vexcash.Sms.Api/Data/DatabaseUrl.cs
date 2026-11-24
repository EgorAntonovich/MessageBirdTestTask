using Npgsql;

namespace Vexcash.Sms.Api.Data;

public static class DatabaseUrl
{
    public static string Resolve(IConfiguration configuration)
    {
        var databaseUrl = configuration["DATABASE_URL"];
        if (!string.IsNullOrWhiteSpace(databaseUrl)) return FromUrl(databaseUrl);

        var connectionString = configuration.GetConnectionString("Postgres");
        if (!string.IsNullOrWhiteSpace(connectionString)) return connectionString;

        throw new InvalidOperationException("Configure DATABASE_URL or ConnectionStrings:Postgres.");
    }

    private static string FromUrl(string databaseUrl)
    {
        var uri = new Uri(databaseUrl);
        var credentials = uri.UserInfo.Split(':', 2);
        return new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Database = uri.AbsolutePath.TrimStart('/'),
            Username = Uri.UnescapeDataString(credentials[0]),
            Password = credentials.Length > 1 ? Uri.UnescapeDataString(credentials[1]) : string.Empty,
            SslMode = SslMode.Prefer
        }.ConnectionString;
    }
}
