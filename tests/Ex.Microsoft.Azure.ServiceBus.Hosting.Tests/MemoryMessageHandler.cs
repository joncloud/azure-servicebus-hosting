using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.ServiceBus.Hosting.Tests
{
    public class MemoryMessageHandler : IMessageHandler
    {
        public MemoryMessageHandler(int countdown) =>
            _countdownEvent = new CountdownEvent(countdown);
        readonly CountdownEvent _countdownEvent;
        public WaitHandle Handled => _countdownEvent.WaitHandle;

        readonly ConcurrentBag<Message> _messages =
            new ConcurrentBag<Message>();
        public IEnumerable<Message> GetMessages() =>
            _messages.AsEnumerable();

        public Task HandleAsync(Message message, CancellationToken cancellationToken)
        {
            _messages.Add(message);
            _countdownEvent.Signal();
            return Task.CompletedTask;
        }
    }
}
