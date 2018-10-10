using System;
using System.Threading.Tasks;

namespace Microsoft.Azure.ServiceBus.Hosting
{
    public class MessageHandlerOptions
    {
        public Func<ExceptionReceivedEventArgs, Task> ExceptionReceivedHandler { get; set; }
        public int MaxConcurrentCalls { get; set; } = 1;
        public bool AutoComplete { get; set; } = true;
        public TimeSpan MaxAutoRenewDuration { get; set; } = TimeSpan.FromMinutes(5);
    }
}
