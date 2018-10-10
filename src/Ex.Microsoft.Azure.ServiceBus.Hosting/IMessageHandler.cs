using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.ServiceBus.Hosting
{
    public interface IMessageHandler
    {
        Task HandleAsync(Message message, CancellationToken cancellationToken);
    }
}
