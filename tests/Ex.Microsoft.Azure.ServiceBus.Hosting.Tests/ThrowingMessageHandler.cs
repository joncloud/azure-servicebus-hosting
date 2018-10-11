using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.ServiceBus.Hosting.Tests
{
    public class ThrowingMessageHandler : IMessageHandler
    {
        public Task HandleAsync(Message message, CancellationToken cancellationToken) =>
            throw new ServiceBusMessageException(message);
    }
}
