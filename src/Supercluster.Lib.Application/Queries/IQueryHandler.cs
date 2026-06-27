using Supercluster.Lib.Primitives;

namespace Supercluster.Lib.Application.Queries;

public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
{
    Task<Result<TResult>> HandleAsync(TQuery query, CancellationToken cancellationToken);
}
