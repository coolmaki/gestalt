using Gestalt.Lib.Application.Queries;

namespace Passport.Core.Application.Queries;

public sealed record FindUserQuery(string Email) : IQuery<FindUserResult>;