using System.Data.Common;
using Dapper;
using Gestalt.Lib.Primitives;
using Passport.Core.Application.ReadModels;
using Passport.Core.Application.Repositories;

namespace Passport.Infrastructure.Repositories;

internal sealed class UserQueryRepository(DbConnection connection) : IUserQueryRepository
{
    public async Task<Option<UserReadModel>> FindByEmailAsync(string email, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT "Email", "EmailVerified", "CreatedAt", "UpdatedAt"
            FROM "Users"
            WHERE "Email" = @Email
            """;

        var user = await connection.QueryFirstOrDefaultAsync<UserReadModel>(sql, new { Email = email });

        return user is null ? Option<UserReadModel>.None : user;
    }

    public async Task<IReadOnlyList<CredentialInfo>> GetCredentialsAsync(string email, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT p."CredentialId", p."DeviceName", p."CreatedAt"
            FROM "PasskeyCredential" p
            INNER JOIN "Users" u ON u."Id" = p."UserId"
            WHERE u."Email" = @Email
            ORDER BY p."CreatedAt" DESC
            """;

        var credentials = await connection.QueryAsync<CredentialInfo>(sql, new { Email = email });

        return credentials.AsList();
    }
}