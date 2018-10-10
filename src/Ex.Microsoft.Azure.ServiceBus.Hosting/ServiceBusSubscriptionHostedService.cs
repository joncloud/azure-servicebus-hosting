using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.ServiceBus.Hosting
{
    public class ServiceBusSubscriptionHostedService : IHostedService
    {
        readonly SubscriptionClient _subscriptionClient;
        readonly IMessageHandlerService _messageHandlerService;

        public ServiceBusSubscriptionHostedService(IOptions<ServiceBusSubscriptionOptions> options, IMessageHandlerService messageHandlerService)
        {
            _subscriptionClient = options.Value.ToSubscriptionClient();
            _messageHandlerService = messageHandlerService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _messageHandlerService.RegisterHandler(_subscriptionClient);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) =>
            _subscriptionClient.CloseAsync();
    }
}
