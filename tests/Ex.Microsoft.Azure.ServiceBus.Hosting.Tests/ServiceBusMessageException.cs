using System;

namespace Microsoft.Azure.ServiceBus.Hosting.Tests
{
    public class ServiceBusMessageException : Exception
    {
        public Message ServiceBusMessage { get; }
        public ServiceBusMessageException(Message serviceBusMessage) =>
            ServiceBusMessage = serviceBusMessage;
    }
}
