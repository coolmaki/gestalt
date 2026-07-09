using System.Data.Common;
using Dapper;
using Supercluster.Lib.Primitives;
using Passport.Core.Application.Repositories;

namespace Passport.Infrastructure.Repositories;

internal sealed class RefreshTokenQueryRepository(DbConnection connection) : IRefreshTokenQueryRepository
{
    public async Task<Option<string>> FindEmailByHashAsync(string tokenHash, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT u."Email"
            FROM "RefreshToken" rt
            INNER JOIN "Users" u ON u."Id" = rt."UserId"
            WHERE rt."TokenHash" = @TokenHash AND rt."RevokedAt" IS NULL
            """;

        var email = await connection.QueryFirstOrDefaultAsync<string>(
            new CommandDefinition(sql, new { TokenHash = tokenHash }, cancellationToken: cancellationToken));

        return email is null ? Option<string>.None : email;
    }
}