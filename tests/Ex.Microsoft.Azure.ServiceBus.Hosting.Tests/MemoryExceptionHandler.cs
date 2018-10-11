using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.ServiceBus.Hosting.Tests
{
    public class MemoryExceptionHandler : IExceptionHandler
    {
        readonly CountdownEvent _countdownEvent;
        public WaitHandle Handled => _countdownEvent.WaitHandle;
        public MemoryExceptionHandler(int countdown) =>
            _countdownEvent = new CountdownEvent(countdown);

        readonly ConcurrentBag<ExceptionReceivedEventArgs> _eventArgs =
            new ConcurrentBag<ExceptionReceivedEventArgs>();

        public IEnumerable<ExceptionReceivedEventArgs> GetEventsArgs() =>
            _eventArgs.AsEnumerable();

        public Task HandleExceptionAsync(ExceptionReceivedEventArgs eventArgs)
        {
            _eventArgs.Add(eventArgs);
            _countdownEvent.Signal();
            return Task.CompletedTask;
        }
    }
}
