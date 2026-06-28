using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Supercluster.Lib.Application.Commands;
using Supercluster.Lib.Application.Queries;
using Supercluster.Lib.Primitives;

namespace Supercluster.Lib.Application.Mediator;

internal sealed class Sender(IServiceProvider serviceProvider) : ISender
{
    private static readonly ConcurrentDictionary<Type, Type> CommandHandlerCache = new();
    private static readonly ConcurrentDictionary<Type, Type> QueryHandlerCache = new();

    public async Task<Result<TResult>> SendAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken)
    {
        var commandType = command.GetType();

        var handlerType = CommandHandlerCache.GetOrAdd(commandType, ct =>
        {
            var handlerInterface = typeof(ICommandHandler<,>).MakeGenericType(commandType, typeof(TResult));
            return handlerInterface;
        });

        dynamic handler = serviceProvider.GetRequiredService(handlerType);
        return await handler.HandleAsync((dynamic)command, cancellationToken);
    }

    public async Task<Result<TResult>> SendAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken)
    {
        var queryType = query.GetType();

        var handlerType = QueryHandlerCache.GetOrAdd(queryType, qt =>
        {
            var handlerInterface = typeof(IQueryHandler<,>).MakeGenericType(queryType, typeof(TResult));
            return handlerInterface;
        });

        dynamic handler = serviceProvider.GetRequiredService(handlerType);
        return await handler.HandleAsync((dynamic)query, cancellationToken);
    }
}