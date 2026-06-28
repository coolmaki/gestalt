using Supercluster.Lib.Application.Queries;
using Supercluster.Lib.Primitives;
using Passport.Core.Application.ReadModels;

namespace Passport.Core.Application.Queries;

public sealed record FindUserQuery(string Email) : IQuery<UserReadModel>;