using Supercluster.Lib.Application.Commands;
using Supercluster.Lib.Application.Queries;
using Supercluster.Lib.Primitives;

namespace Supercluster.Lib.Application.Mediator;

public interface ISender
{
    Task<Result<TResult>> SendAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken);

    Task<Result<TResult>> SendAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken);
}