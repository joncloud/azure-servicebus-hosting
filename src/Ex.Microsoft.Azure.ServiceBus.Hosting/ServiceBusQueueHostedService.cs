using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.ServiceBus.Hosting
{
    public class ServiceBusQueueHostedService : IHostedService
    {
        readonly QueueClient _queueClient;
        readonly IMessageHandlerService _messageHandlerService;
        public ServiceBusQueueHostedService(IOptions<ServiceBusQueueOptions> options, IMessageHandlerService messageHandlerService, IServiceProvider serviceProvider)
        {
            _queueClient = options.Value.ToQueueClient();
            _messageHandlerService = messageHandlerService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _messageHandlerService.RegisterHandler(_queueClient);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) =>
            _queueClient.CloseAsync();
    }
}
