using DotNetCoreSqlDb.Data;
using DotNetCoreSqlDb.Domain.Entities;
using DotNetCoreSqlDb.Features.Account;
using DotNetCoreSqlDb.Features.Auth;
using DotNetCoreSqlDb.Infrastructure.Email;
using DotNetCoreSqlDb.Infrastructure.Http;
using DotNetCoreSqlDb.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Environment.IsDevelopment()
    ? DevelopmentConnectionStringSelector.GetDevelopmentConnectionString(builder.Configuration)
    : builder.Configuration.GetConnectionString("AZURE_SQL_CONNECTIONSTRING")
        ?? throw new InvalidOperationException("Missing connection string 'AZURE_SQL_CONNECTIONSTRING'.");

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddControllers();
builder.Services.AddAuthorization();

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<ITokenGenerator, TokenGenerator>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IEmailSender, LoggingEmailSender>();
builder.Services.AddSingleton(TimeProvider.System);

builder.Services
    .AddAuthentication(AuthConstants.CookieScheme)
    .AddCookie(AuthConstants.CookieScheme, options =>
    {
        options.Cookie.Name = builder.Environment.IsDevelopment()
            ? "ordomilo-auth"
            : "__Host-ordomilo-auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
        options.SlidingExpiration = false;
        options.Events = new CookieAuthenticationEvents
        {
            OnValidatePrincipal = SessionCookieValidator.ValidateAsync,
            OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            },
            OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            }
        };
    });

builder.Logging.AddAzureWebAppDiagnostics();

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        var (statusCode, title) = exception switch
        {
            ApiException apiException => (apiException.StatusCode, apiException.Message),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized."),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = statusCode,
            Title = title
        });
    });
});

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    await ProductionDatabaseMigrator.MigrateAsync(app.Services, app.Logger);
}

if (app.Environment.IsDevelopment())
{
    await DevelopmentDatabaseInitializer.InitializeAsync(app.Services, app.Configuration, app.Logger);
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api", () => Results.Ok(new
{
    service = "Ordomilo Account API",
    version = "v1"
}));

app.MapGet("/api/health", () => Results.Ok(new
{
    status = "ok"
}));

app.MapGet("/", () => Results.Redirect("/api", permanent: false));

app.MapGet("/health", () => Results.Redirect("/api/health", permanent: false));

app.MapControllers();

await app.RunAsync();
