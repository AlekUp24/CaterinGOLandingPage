using CaterinGO.Configuration;
using CaterinGO.Components;
using CaterinGO.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("The PostgreSQL connection string is missing. Configure ConnectionStrings:DefaultConnection.");

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
