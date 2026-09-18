using CaterinGO.Configuration;
using CaterinGO.Components;
using CaterinGO.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to listen on port 8080 for Railway
var httpPort = Environment.GetEnvironmentVariable("ASPNETCORE_HTTP_PORTS")
    ?? Environment.GetEnvironmentVariable("PORT")
    ?? "8080";

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(int.Parse(httpPort));
});

var configuredConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("DATABASE_URL");
var connectionString = ConvertPostgresUrlToConnectionString(configuredConnectionString);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();
builder.Services.AddDbContextFactory<CaterinGoDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddScoped<IWaitingListService, WaitingListService>();
builder.Services.Configure<CaterinGoOptions>(builder.Configuration.GetSection("CaterinGo"));
builder.Services.AddScoped<LanguageState>();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<CaterinGoDbContext>>();
    await using var dbContext = await dbContextFactory.CreateDbContextAsync();
    await dbContext.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(CaterinGO.Client._Imports).Assembly);

app.Run();

static string ConvertPostgresUrlToConnectionString(string? configuredConnectionString)
{
    if (string.IsNullOrWhiteSpace(configuredConnectionString))
    {
        throw new InvalidOperationException(
            "The PostgreSQL connection string is missing. Configure ConnectionStrings__DefaultConnection on Railway.");
    }

    if (!Uri.TryCreate(configuredConnectionString, UriKind.Absolute, out var databaseUri)
        || (databaseUri.Scheme != Uri.UriSchemeHttp
            && databaseUri.Scheme != Uri.UriSchemeHttps
            && databaseUri.Scheme != "postgres"
            && databaseUri.Scheme != "postgresql"))
    {
        return configuredConnectionString;
    }

    if (databaseUri.Scheme is "http" or "https")
    {
        throw new InvalidOperationException(
            "The PostgreSQL connection value is an HTTP URL. Use Railway's PostgreSQL DATABASE_URL instead.");
    }

    var userInfo = databaseUri.UserInfo.Split(':', 2);
    if (userInfo.Length != 2 || string.IsNullOrWhiteSpace(databaseUri.Host))
    {
        throw new InvalidOperationException(
            "The PostgreSQL URL is invalid. Use Railway's PostgreSQL DATABASE_URL value.");
    }

    var connectionBuilder = new NpgsqlConnectionStringBuilder
    {
        Host = databaseUri.Host,
        Port = databaseUri.Port > 0 ? databaseUri.Port : 5432,
        Database = Uri.UnescapeDataString(databaseUri.AbsolutePath.Trim('/')),
        Username = Uri.UnescapeDataString(userInfo[0]),
        Password = Uri.UnescapeDataString(userInfo[1])
    };

    var query = databaseUri.Query.TrimStart('?')
        .Split('&', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    foreach (var parameter in query)
    {
        var parts = parameter.Split('=', 2);
        if (parts.Length == 2 && parts[0].Equals("sslmode", StringComparison.OrdinalIgnoreCase))
        {
            connectionBuilder.SslMode = Enum.Parse<SslMode>(Uri.UnescapeDataString(parts[1]), true);
        }
    }

    return connectionBuilder.ConnectionString;
}