using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using Passport.Core.Application.Configuration;
using Passport.Core.Application.Extensions;
using Passport.Infrastructure.Configuration;
using Passport.Infrastructure.Extensions;
using Passport.Presentation.Http.Extensions;

using Gestalt.Lib.Application.Extensions;
using Gestalt.Lib.Infrastructure.Extensions;
using Gestalt.Lib.Presentation.Http.Extensions;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------
// Configuration
// ------------------------------------------------------------

var appConfig = builder.Configuration
    .GetSection(ApplicationConfiguration.SectionName)
    .Get<ApplicationConfiguration>()!;

var infraConfig = builder.Configuration
    .GetSection(InfrastructureConfiguration.SectionName)
    .Get<InfrastructureConfiguration>()!;

#if DEBUG
if (builder.Environment.IsDevelopment() && infraConfig.Persistence.Provider == PersistenceProvider.Sqlite)
{
    var dbDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".gestalt", "passport");
    Directory.CreateDirectory(dbDir);
    infraConfig.Persistence.ConnectionString = $"Data Source={Path.Combine(dbDir, "passport.db")}";
}
#endif

builder.Services.AddSingleton(appConfig);

// ------------------------------------------------------------
// Authentication
// ------------------------------------------------------------

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<ApplicationConfiguration, ECDsaSecurityKey>((options, appCfg, securityKey) =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = appCfg.AccessToken.Issuer,
            ValidateAudience = true,
            ValidAudience = appCfg.AccessToken.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = securityKey,
            NameClaimType = "sub",
        };
    });

builder.Services.AddAuthorization();

// ------------------------------------------------------------
// Services
// ------------------------------------------------------------

builder.Services.AddMediator();
builder.Services.AddProviders();
builder.Services.AddPassportCommandsAndQueries();
builder.Services.AddPassportInfrastructure(infraConfig.Persistence, appConfig.SigningKey);
builder.Services.AddPassportEndpoints();

// ------------------------------------------------------------
// App
// ------------------------------------------------------------

var app = builder.Build();

await app.Services.InitializeDatabaseAsync();

app.UseAuthentication();
app.UseAuthorization();

app.MapEndpoints();

#if DEBUG
if (app.Environment.IsDevelopment())
{
    var spaProxyUrl = builder.Configuration["SpaProxyServerUrl"]!;

    app.MapWhen(
        ctx => !ctx.Request.Path.StartsWithSegments("/api") &&
               !ctx.Request.Path.StartsWithSegments("/.well-known"),
        spaApp =>
        {
            spaApp.UseSpa(spa =>
            {
                spa.UseProxyToSpaDevelopmentServer(spaProxyUrl);
            });
        });
}
#else
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapFallbackToFile("index.html");
#endif

app.Run();