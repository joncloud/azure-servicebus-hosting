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

        Func<object, CancellationToken, Task> CompileHandlerDelegate(Type type)
        {
            // Compile to call:
            // (rawMessage, token) => HandleTypedMessageAsync<{type}>(({type})rawMessage, token)
            var param = Expression.Parameter(typeof(object), "msg");
            var param2 = Expression.Parameter(typeof(CancellationToken), "cancellationToken");
            var cast = Expression.Convert(param, type);
            var self = Expression.Constant(this, typeof(GenericMessageHandler));
            var method = typeof(GenericMessageHandler).GetMethod(nameof(HandleTypedMessageAsync))
                .MakeGenericMethod(type);
            var call = Expression.Call(self, method, cast, param2);
            return Expression.Lambda<Func<object, CancellationToken, Task>>(call, param, param2)
                .Compile();
        }

        private static readonly double TimestampToTicks = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;

        public async Task HandleAsync(Message message, CancellationToken cancellationToken)
        {
            var deserializedMessage = _options.MessageDeserializer(message);
            if (deserializedMessage == null)
            {
                throw new NullMessageContentException(message.MessageId);
            }
            var messageType = deserializedMessage.GetType();

            var start = Stopwatch.GetTimestamp();

            var handler = _handlers.GetOrAdd(
                messageType,
                CompileHandlerDelegate
            );

            try
            {
                await handler(deserializedMessage, cancellationToken);
            }
            finally
            {
                var stop = Stopwatch.GetTimestamp();

                _logger.LogInformation(
                    "Handled message {messageId} type {messageType} in {messageDuration}ms",
                    message.MessageId,
                    messageType,
                    new TimeSpan((long)(TimestampToTicks * (stop - start))).TotalMilliseconds
                );
            }
        }
    }
}
