using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.ServiceBus.Hosting
{
    public class MessageHandlerService : IMessageHandlerService
    {
        readonly IServiceProvider _serviceProvider;
        readonly ServiceBus.MessageHandlerOptions _options;
        public MessageHandlerService(IOptions<MessageHandlerOptions> options, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            Func<ExceptionReceivedEventArgs, Task> exceptionHandler =
                options.Value.ExceptionReceivedHandler ??
                serviceProvider.GetRequiredService<IExceptionHandler>().HandleExceptionAsync;

            _options = new ServiceBus.MessageHandlerOptions(exceptionHandler)
            {
                AutoComplete = options.Value.AutoComplete,
                MaxAutoRenewDuration = options.Value.MaxAutoRenewDuration,
                MaxConcurrentCalls = options.Value.MaxConcurrentCalls
            };
        }

        public async Task HandleMessageAsync(Message message, CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var handler = scope.ServiceProvider.GetRequiredService<IMessageHandler>();
                await handler.HandleAsync(message, cancellationToken);
            }
        }

        public void RegisterHandler(QueueClient queueClient) =>
            queueClient.RegisterMessageHandler(HandleMessageAsync, _options);

        public void RegisterHandler(SubscriptionClient subscriptionClient) =>
            subscriptionClient.RegisterMessageHandler(HandleMessageAsync, _options);
    }
}
