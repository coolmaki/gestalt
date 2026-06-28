using Passport.Core.Application.Configuration;
using Passport.Core.Application.Extensions;
using Passport.Infrastructure.Configuration;
using Passport.Infrastructure.Extensions;
using Passport.Presentation.Http.Extensions;
using Supercluster.Lib.Application.Extensions;
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
// Services
// ------------------------------------------------------------

builder.Services.AddMediator();
builder.Services.AddPassportCommandsAndQueries();
builder.Services.AddPassportInfrastructure(infraConfig.Persistence);
builder.Services.AddPassportEndpoints();

// ------------------------------------------------------------
// App
// ------------------------------------------------------------

var app = builder.Build();

app.MapEndpoints();

app.Run();