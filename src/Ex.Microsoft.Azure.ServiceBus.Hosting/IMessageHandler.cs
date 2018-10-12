using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.ServiceBus.Hosting
{
    public interface IMessageHandler
    {
        Task HandleAsync(Message message, CancellationToken cancellationToken);
    }

    public interface IMessageHandler<T>
    {
        Task HandleAsync(T message, CancellationToken cancellationToken);
    }
}
