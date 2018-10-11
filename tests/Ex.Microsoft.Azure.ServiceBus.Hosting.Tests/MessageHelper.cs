using Microsoft.Azure.ServiceBus;
using System;
using System.Linq;

namespace Microsoft.Azure.ServiceBus.Hosting.Tests
{
    public static class MessageHelper
    {
        public static Message ForInt32(string id, int value)
        {
            var body = BitConverter.GetBytes(value);
            return new Message(body)
            {
                MessageId = id
            };
        }

        public static int ToInt32(this Message message) =>
            BitConverter.ToInt32(message.Body, 0);
    }
}
