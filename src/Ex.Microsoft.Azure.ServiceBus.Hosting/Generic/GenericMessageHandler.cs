using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.ServiceBus.Hosting.Generic
{
    public class GenericMessageHandler : IMessageHandler
    {
        readonly GenericMessageHandlerOptions _options;
        readonly IServiceProvider _serviceProvider;
        readonly ILogger<GenericMessageHandler> _logger;
        public GenericMessageHandler(
            IOptions<GenericMessageHandlerOptions> options, IServiceProvider serviceProvider,
            ILogger<GenericMessageHandler> logger)
        {
            _options = options.Value;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }


        readonly ConcurrentDictionary<Type, Func<object, CancellationToken, Task>> _handlers =
            new ConcurrentDictionary<Type, Func<object, CancellationToken, Task>>();
        public async Task HandleTypedMessageAsync<T>(T message, CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var handler = scope.ServiceProvider.GetRequiredService<IMessageHandler<T>>();
                await handler.HandleAsync(message, cancellationToken);
            }
        }

        public async Task HandleAsync(Message message, CancellationToken cancellationToken)
        {
            var deserializedMessage = _options.MessageDeserializer(message);
            if (deserializedMessage == null)
            {
                // TODO handle
            }
            var messageType = deserializedMessage.GetType();

            // TODO calculate using ticks ala asp.net core
            var watch = Stopwatch.StartNew();

            var handler = _handlers.GetOrAdd(messageType,
                key =>
                {
                    // Compile to call (rawMessage, token) => HandleTypedMessageAsync<{type}>(({type})rawMessage, token)
                    var param = Expression.Parameter(typeof(object), "msg");
                    var param2 = Expression.Parameter(typeof(CancellationToken), "cancellationToken");
                    var cast = Expression.Convert(param, key);
                    var self = Expression.Constant(this, typeof(GenericMessageHandler));
                    var method = typeof(GenericMessageHandler).GetMethod(nameof(HandleTypedMessageAsync))
                        .MakeGenericMethod(key);
                    var call = Expression.Call(self, method, cast, param2);
                    return Expression.Lambda<Func<object, CancellationToken, Task>>(call, param, param2)
                        .Compile();
                });

            await handler(deserializedMessage, cancellationToken);

            watch.Stop();
            _logger.LogInformation("Handled message type {messageType} in {elapsed}", messageType, watch.Elapsed);
        }
    }
}
