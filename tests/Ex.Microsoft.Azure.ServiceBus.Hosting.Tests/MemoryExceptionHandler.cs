using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.ServiceBus.Hosting.Tests
{
    public class MemoryExceptionHandler : IExceptionHandler
    {
        readonly ManualResetEvent _manualResetEvent = new ManualResetEvent(false);
        public WaitHandle Handled => _manualResetEvent;

        readonly ConcurrentBag<ExceptionReceivedEventArgs> _eventArgs =
            new ConcurrentBag<ExceptionReceivedEventArgs>();

        public IEnumerable<ExceptionReceivedEventArgs> GetEventsArgs() =>
            _eventArgs.AsEnumerable();

        public Task HandleExceptionAsync(ExceptionReceivedEventArgs eventArgs)
        {
            _eventArgs.Add(eventArgs);
            _manualResetEvent.Set();
            return Task.CompletedTask;
        }
    }
}
