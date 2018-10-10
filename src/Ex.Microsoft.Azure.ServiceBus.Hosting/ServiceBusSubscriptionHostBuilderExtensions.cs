using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Microsoft.Azure.ServiceBus.Hosting
{
    public static class ServiceBusSubscriptionHostBuilderExtensions
    {
        public static IHostBuilder ConfigureServiceBusSubscription(this IHostBuilder hostBuilder, Action<HostBuilderContext, ServiceBusSubscriptionOptions> configure, Action<ServiceBusContext> contextHandler) =>
            hostBuilder.ConfigureServices((context, services) =>
            {
                services.Configure<ServiceBusSubscriptionOptions>(options =>
                {
                    configure(context, options);
                });
                services.AddHostedService<ServiceBusSubscriptionHostedService>();

                contextHandler(new ServiceBusContext(services));
            });

        public static IHostBuilder ConfigureServiceBusSubscription(this IHostBuilder hostBuilder, string connectionString, string subscriptionName, Action<ServiceBusContext> contextHandler) =>
            hostBuilder.ConfigureServices(services =>
            {
                services.Configure<ServiceBusSubscriptionOptions>(options =>
                {
                    options.ConnectionString = connectionString;
                    options.SubscriptionName = subscriptionName;
                });
                services.AddHostedService<ServiceBusSubscriptionHostedService>();
                contextHandler(new ServiceBusContext(services));
            });

        public static IHostBuilder ConfigureServiceBusSubscription(this IHostBuilder hostBuilder, string connectionString, string subscriptionName, ReceiveMode receiveMode, Action<ServiceBusContext> contextHandler) =>
            hostBuilder.ConfigureServices(services =>
            {
                services.Configure<ServiceBusSubscriptionOptions>(options =>
                {
                    options.ConnectionString = connectionString;
                    options.ReceiveMode = receiveMode;
                    options.SubscriptionName = subscriptionName;
                });
                services.AddHostedService<ServiceBusSubscriptionHostedService>();
                contextHandler(new ServiceBusContext(services));
            });

        public static IHostBuilder ConfigureServiceBusSubscription(this IHostBuilder hostBuilder, string connectionString, string subscriptionName, ReceiveMode receiveMode, RetryPolicy retryPolicy, Action<ServiceBusContext> contextHandler) =>
            hostBuilder.ConfigureServices(services =>
            {
                services.Configure<ServiceBusSubscriptionOptions>(options =>
                {
                    options.ConnectionString = connectionString;
                    options.ReceiveMode = receiveMode;
                    options.RetryPolicy = retryPolicy;
                    options.SubscriptionName = subscriptionName;
                });
                services.AddHostedService<ServiceBusSubscriptionHostedService>();
                contextHandler(new ServiceBusContext(services));
            });
    }
}
