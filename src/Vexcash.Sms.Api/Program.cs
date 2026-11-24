using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Vexcash.Sms.Api.Api;
using Vexcash.Sms.Api.Application;
using Vexcash.Sms.Api.Data;
using Vexcash.Sms.Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

if (int.TryParse(Environment.GetEnvironmentVariable("PORT"), out var port))
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Vexcash SMS API",
        Version = "v1",
        Description = "Manual customer SMS dispatch using reusable templates and MessageBird."
    });
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));
});

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(DatabaseUrl.Resolve(builder.Configuration)));
builder.Services.Configure<MessageBirdOptions>(builder.Configuration.GetSection(MessageBirdOptions.SectionName));
builder.Services.AddHttpClient<MessageBirdSmsProvider>((services, client) =>
{
    var settings = services.GetRequiredService<Microsoft.Extensions.Options.IOptions<MessageBirdOptions>>().Value;
    client.BaseAddress = new Uri(settings.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(15);
});
builder.Services.AddScoped<ITemplateRenderer, TemplateRenderer>();
builder.Services.AddScoped<ISmsDispatchService, SmsDispatchService>();
builder.Services.AddScoped<ISmsProvider>(services =>
{
    var settings = services.GetRequiredService<Microsoft.Extensions.Options.IOptions<MessageBirdOptions>>().Value;
    return settings.UseMock ? services.GetRequiredService<MockSmsProvider>() : services.GetRequiredService<MessageBirdSmsProvider>();
});
builder.Services.AddScoped<MockSmsProvider>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Vexcash SMS API v1");
    options.RoutePrefix = "swagger";
    options.DocumentTitle = "Vexcash SMS API documentation";
});
app.MapControllers();
app.MapHealthChecks("/health");
app.MapFallbackToFile("index.html");

if (!app.Environment.IsEnvironment("Testing"))
{
    await using var scope = app.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
}

await app.RunAsync();

public partial class Program;
