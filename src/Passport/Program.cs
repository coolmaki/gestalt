using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using Passport.Core.Application.Configuration;
using Passport.Core.Application.Extensions;
using Passport.Infrastructure.Configuration;
using Passport.Infrastructure.Extensions;
using Passport.Presentation.Http.Extensions;

using Supercluster.Lib.Application.Extensions;
using Supercluster.Lib.Infrastructure.Extensions;
using Supercluster.Lib.Presentation.Http.Extensions;

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

app.UseAuthentication();
app.UseAuthorization();

app.MapEndpoints();

app.Run();