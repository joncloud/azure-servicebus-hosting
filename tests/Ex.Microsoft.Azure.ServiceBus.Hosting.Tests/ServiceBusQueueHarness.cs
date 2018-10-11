using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace Microsoft.Azure.ServiceBus.Hosting.Tests
{
    public static class ServiceBusQueueHarness
    {
        public static ServiceBusQueueHarness<ThrowingMessageHandler> Throwing(int countdown) =>
            new ServiceBusQueueHarness<ThrowingMessageHandler>(
                new ThrowingMessageHandler(),
                new MemoryExceptionHandler(countdown)
            );

        public static ServiceBusQueueHarness<MemoryMessageHandler> InMemory(int countdown) =>
            new ServiceBusQueueHarness<MemoryMessageHandler>(
                new MemoryMessageHandler(countdown),
                new MemoryExceptionHandler(0)
            );
    }

    public class ServiceBusQueueHarness<T>
        where T : IMessageHandler
    {
        readonly T _messageHandler;
        readonly MemoryExceptionHandler _exceptionHandler;
        public ServiceBusQueueHarness(T messageHandler, MemoryExceptionHandler exceptionHandler)
        {
            _messageHandler = messageHandler;
            _exceptionHandler = exceptionHandler;
        }

        readonly static string _connectionString;
        static ServiceBusQueueHarness()
        {
            _connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__ServiceBus");
            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                throw new Exception("No environment variable specified for ConnectionStrings__ServiceBus");
            }
        }

        public async Task RunAsync(Func<QueueClient, T, MemoryExceptionHandler, Task> fn)
        {
            var queueName = Guid.NewGuid().ToString("N");
            var connectionString = _connectionString;

            var namespaceManager = Microsoft.ServiceBus.NamespaceManager.CreateFromConnectionString(connectionString);
            await namespaceManager.CreateQueueAsync(queueName);

            connectionString += $";EntityPath={queueName}";

            try
            {
                var hostBuilder = new HostBuilder()
                    .ConfigureServiceBusQueue(connectionString, context =>
                    {
                        context.Services.AddSingleton<IExceptionHandler>(_exceptionHandler);
                        context.Services.AddSingleton<IMessageHandler>(_messageHandler);
                    });

                var host = hostBuilder.Start();

                var queueClient = new QueueClient(
                    new ServiceBusConnectionStringBuilder(connectionString)
                );
                await fn(queueClient, _messageHandler, _exceptionHandler);

                await host.StopAsync();
            }
            finally
            {
                await namespaceManager.DeleteQueueAsync(queueName);
            }
        }
    }
}
