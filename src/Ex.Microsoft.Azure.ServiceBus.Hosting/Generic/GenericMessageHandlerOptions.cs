using System;

namespace Microsoft.Azure.ServiceBus.Hosting.Generic
{
    public class GenericMessageHandlerOptions
    {
        public Func<Message, object> MessageDeserializer { get; set; }
    }
}
