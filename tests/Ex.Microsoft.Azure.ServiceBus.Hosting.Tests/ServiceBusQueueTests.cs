using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Azure.ServiceBus.Hosting.Tests
{
    public class ServiceBusQueueTests
    {
        [Fact]
        public async Task ExceptionHandler_ShouldHandleExceptions()
        {
            var harness = ServiceBusQueueHarness.Throwing();

            await harness.RunAsync(AssertAsync);

            async Task AssertAsync(QueueClient queueClient, ThrowingMessageHandler _, MemoryExceptionHandler exceptionHandler)
            {
                var messages = new List<Message>
                {
                    MessageHelper.ForInt32("d", 4),
                    MessageHelper.ForInt32("e", 5),
                    MessageHelper.ForInt32("f", 6)
                };
                await queueClient.SendAsync(messages);

                if (!exceptionHandler.Handled.WaitOne(TimeSpan.FromMilliseconds(500)))
                {
                    throw new Exception("No exceptions received");
                }
                
                var expected = messages
                    .Select(msg => new { Id = msg.MessageId, Value = msg.ToInt32() })
                    .ToList();

                var actual = exceptionHandler.GetEventsArgs()
                    .Select(args => args.Exception)
                    .OfType<ServiceBusMessageException>()
                    .Select(ex => ex.ServiceBusMessage)
                    .Select(msg => new { Id = msg.MessageId, Value = msg.ToInt32() })
                    .OrderBy(x => x.Id)
                    .ToList();

                await Task.Delay(0);
            }

        }

        [Fact]
        public async Task MessageHandler_ShouldHandleMessages()
        {
            var harness = ServiceBusQueueHarness.InMemory(3);

            await harness.RunAsync(AssertAsync);

            async Task AssertAsync(QueueClient queueClient, MemoryMessageHandler messageHandler, MemoryExceptionHandler _)
            {
                var messages = new List<Message>
                {
                    MessageHelper.ForInt32("a", 1),
                    MessageHelper.ForInt32("b", 2),
                    MessageHelper.ForInt32("c", 3)
                };
                await queueClient.SendAsync(messages);

                if (!messageHandler.Handled.WaitOne(TimeSpan.FromMilliseconds(500)))
                {
                    throw new Exception("No messages received");
                }

                var expected = messages
                    .Select(msg => new { Id = msg.MessageId, Value = msg.ToInt32() })
                    .ToList();

                var actual = messageHandler.GetMessages()
                    .Select(msg => new { Id = msg.MessageId, Value = msg.ToInt32() })
                    .OrderBy(x => x.Id)
                    .ToList();

                Assert.Equal(expected, actual);
            }
        }
    }
}
