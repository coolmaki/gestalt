using Microsoft.Extensions.DependencyInjection;
using Supercluster.Lib.Application.Queries;

namespace Passport.Core.Application.Queries;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPassportQueries(this IServiceCollection services)
    {
        services.AddScoped<IQueryHandler<FindUserQuery, FindUserResult>, FindUserQueryHandler>();
        services.AddScoped<IQueryHandler<GetCredentialsQuery, GetCredentialsResult>, GetCredentialsQueryHandler>();

        return services;
    }
}