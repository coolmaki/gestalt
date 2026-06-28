using Passport.Presentation.Http.Controllers;

namespace Passport.Presentation.Http.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication MapPassportEndpoints(this WebApplication app)
    {
        app.MapAuthEndpoints();
        app.MapRecoveryEndpoints();
        app.MapCredentialsEndpoints();

        return app;
    }
}