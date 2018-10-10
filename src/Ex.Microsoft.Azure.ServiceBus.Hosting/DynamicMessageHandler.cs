using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Azure.ServiceBus.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using System.Text;

namespace Microsoft.Azure.ServiceBus.Hosting
{
    public class MyExceptionHandler : IExceptionHandler
    {
        public Task HandleExceptionAsync(ExceptionReceivedEventArgs eventArgs) =>
          throw new NotImplementedException();
    }

    public class MyInt32Handler : IMessageHandler<int>
    {
        public Task HandleAsync(int message, CancellationToken cancellationToken) => throw new NotImplementedException();
    }

    public class MyStringHandler : IMessageHandler<string>
    {
        public Task HandleAsync(string message, CancellationToken cancellationToken) => throw new NotImplementedException();
    }

    public static class Program
    {
        static string GetConnectionString(IConfiguration configuration) =>
          configuration.GetSection("ConnectionStrings")["ServiceBusConnection"];

        public static Task Main(string[] args) =>
            new HostBuilder().ConfigureServiceBusSubscription(
                (hostBuilder, options) =>
                {
                    options.ConnectionString = GetConnectionString(hostBuilder.Configuration);
                    options.SubscriptionName = hostBuilder.Configuration["SubscriptionName"];
                },
                context => context.ExceptionHandler<MyExceptionHandler>()
                    .DynamicMessageHandler(msg =>
                    {
                        switch (msg.ContentType)
                        {
                            case "int":
                                return BitConverter.ToInt32(msg.Body, 0);
                            case "string":
                                return Encoding.UTF8.GetString(msg.Body);
                            default:
                                throw new Exception("Invalid message content type");
                        }
                    })
                    .Scoped<int, MyInt32Handler>()
                    .Scoped<string, MyStringHandler>()
            )
            .RunConsoleAsync();
    }

    public class DynamicMessageHandler : IMessageHandler
    {
        readonly DynamicMessageHandlerOptions _options;
        readonly IServiceProvider _serviceProvider;
        readonly ILogger<DynamicMessageHandler> _logger;
        public DynamicMessageHandler(
            IOptions<DynamicMessageHandlerOptions> options, IServiceProvider serviceProvider,
            ILogger<DynamicMessageHandler> logger)
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
                    var self = Expression.Constant(this, typeof(DynamicMessageHandler));
                    var method = typeof(DynamicMessageHandler).GetMethod(nameof(HandleTypedMessageAsync))
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
