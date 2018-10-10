using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Microsoft.Azure.ServiceBus.Hosting
{
    public static class ServiceBusQueueHostBuilderExtensions
    {
        public static IHostBuilder ConfigureServiceBusQueue(this IHostBuilder hostBuilder, Action<HostBuilderContext, ServiceBusQueueOptions> configure, Action<ServiceBusContext> contextHandler) =>
            hostBuilder.ConfigureServices((context, services) =>
            {
                services.Configure<ServiceBusQueueOptions>(options =>
                {
                    configure(context, options);
                });
                services.AddHostedService<ServiceBusQueueHostedService>();

                contextHandler(new ServiceBusContext(services));
            });

        public static IHostBuilder ConfigureServiceBusQueue(this IHostBuilder hostBuilder, string connectionString, Action<ServiceBusContext> contextHandler) =>
            hostBuilder.ConfigureServices(services =>
            {
                services.Configure<ServiceBusQueueOptions>(options =>
                {
                    options.ConnectionString = connectionString;
                });
                services.AddHostedService<ServiceBusQueueHostedService>();

                contextHandler(new ServiceBusContext(services));
            });

        public static IHostBuilder ConfigureServiceBusQueue(this IHostBuilder hostBuilder, string connectionString, ReceiveMode receiveMode, Action<ServiceBusContext> contextHandler) =>
            hostBuilder.ConfigureServices(services =>
            {
                services.Configure<ServiceBusQueueOptions>(options =>
                {
                    options.ConnectionString = connectionString;
                    options.ReceiveMode = receiveMode;
                });
                services.AddHostedService<ServiceBusQueueHostedService>();
                contextHandler(new ServiceBusContext(services));
            });

        public static IHostBuilder ConfigureServiceBusQueue(this IHostBuilder hostBuilder, string connectionString, ReceiveMode receiveMode, RetryPolicy retryPolicy, Action<ServiceBusContext> contextHandler) =>
            hostBuilder.ConfigureServices(services =>
            {
                services.Configure<ServiceBusQueueOptions>(options =>
                {
                    options.ConnectionString = connectionString;
                    options.ReceiveMode = receiveMode;
                    options.RetryPolicy = retryPolicy;
                });
                services.AddHostedService<ServiceBusQueueHostedService>();
                contextHandler(new ServiceBusContext(services));
            });
    }
}
