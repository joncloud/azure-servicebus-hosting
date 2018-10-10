using System;
using System.Threading;

namespace Microsoft.Azure.ServiceBus.Hosting
{
    public class DynamicMessageHandlerOptions
    {
        public Func<Message, object> MessageDeserializer { get; set; }
    }
}
