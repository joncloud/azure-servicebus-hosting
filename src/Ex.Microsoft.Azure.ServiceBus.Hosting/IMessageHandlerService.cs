using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.ServiceBus.Hosting
{
    public interface IMessageHandlerService
    {
        Task HandleMessageAsync(Message message, CancellationToken cancellationToken);
        void RegisterHandler(QueueClient queueClient);
        void RegisterHandler(SubscriptionClient subscriptionClient);
    }
}
