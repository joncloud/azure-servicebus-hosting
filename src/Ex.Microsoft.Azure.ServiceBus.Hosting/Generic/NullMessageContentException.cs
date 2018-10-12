using System;

namespace Microsoft.Azure.ServiceBus.Hosting.Generic
{
    public class NullMessageContentException : Exception
    {
        public string MessageId { get; }

        public NullMessageContentException(string messageId)
            : base($"Message {messageId} deserialized into a null value")
        {
            MessageId = messageId;
        }

        public NullMessageContentException(string messageId, Exception innerException)
            : base($"Message {messageId} deserialized into a null value", innerException)
        {
            MessageId = messageId;
        }
    }
}
