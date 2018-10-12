using Microsoft.Azure.ServiceBus.Hosting.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Azure.ServiceBus.Hosting.Tests.Generic
{
    public class GenericMessageHandlerTests
    {
        [Fact]
        public async Task HandleAsync_ShouldThrowNullMessageContentException_GivenNullDeserializedMessage()
        {
            var options = Options.Create(new GenericMessageHandlerOptions
            {
                MessageDeserializer = _ => null
            });
            var serviceProvider = default(IServiceProvider);
            var logger = default(ILogger<GenericMessageHandler>);

            var target = new GenericMessageHandler(
                options,
                serviceProvider,
                logger
            );

            var message = new Message
            {
                MessageId = Guid.NewGuid().ToString()
            };

            var ex = await Assert.ThrowsAsync<NullMessageContentException>(
                () => target.HandleAsync(message, default(CancellationToken))
            );

            Assert.Equal(message.MessageId, ex.MessageId);
        }
         
        [Fact]
        public async Task HandleAsync_ShouldCreateNewScopePerMessage()
        {
            var num = 0;
            var target = CreateMessageHandler(services =>
                services.AddScoped<IMessageHandler<int>>(_ =>
                {
                    num++;
                    return new DumbMessageHandler<int>();
                }),
                _ => 123
            );

            await target.HandleAsync(new Message(), default(CancellationToken));
            Assert.Equal(1, num);

            await target.HandleAsync(new Message(), default(CancellationToken));
            Assert.Equal(2, num);
        }
        
        [Fact]
        public async Task HandleAsync_ShouldCreateGenericHandler_GivenDeserializedMessage()
        {
            var stringHandler = new DumbMessageHandler<string>();
            var int32Handler = new DumbMessageHandler<int>();

            var target = CreateMessageHandler(services =>
                services.AddScoped<IMessageHandler<int>>(_ => int32Handler)
                    .AddScoped<IMessageHandler<string>>(_ => stringHandler),
                    msg =>
                    {
                        switch (msg.ContentType)
                        {
                            case "int":
                                return BitConverter.ToInt32(msg.Body, 0);
                            case "string":
                                return Encoding.UTF8.GetString(msg.Body);
                            default:
                                throw new Exception("Invalid message content type");
                        }
                    }
            );

            var int32Messages = new[]
            {
                CreateInt32Message(1),
                CreateInt32Message(2),
                CreateInt32Message(3)
            };

            await Task.WhenAll(int32Messages.Select(msg => target.HandleAsync(msg, default(CancellationToken))));

            var stringMessages = new[]
            {
                CreateStringMessage("a"),
                CreateStringMessage("b"),
                CreateStringMessage("c"),
                CreateStringMessage("d")
            };

            await Task.WhenAll(stringMessages.Select(msg => target.HandleAsync(msg, default(CancellationToken))));

            Assert.Equal(
                new[] { 1, 2, 3 },
                int32Handler.GetMessages().OrderBy(msg => msg)
            );
            Assert.Equal(
                new[] { "a", "b", "c", "d" },
                stringHandler.GetMessages().OrderBy(msg => msg)
            );

            Message CreateInt32Message(int num) =>
                new Message(BitConverter.GetBytes(num))
                {
                    ContentType = "int"
                };

            Message CreateStringMessage(string s) =>
                new Message(Encoding.UTF8.GetBytes(s))
                {
                    ContentType = "string"
                };
        }

        class DumbMessageHandler<T> : IMessageHandler<T>
        {
            readonly ConcurrentBag<T> _messages = new ConcurrentBag<T>();
            public IEnumerable<T> GetMessages() => 
                _messages.AsEnumerable();
            public Task HandleAsync(T message, CancellationToken cancellationToken)
            {
                _messages.Add(message);
                return Task.CompletedTask;
            }
        }

        static GenericMessageHandler CreateMessageHandler(Action<IServiceCollection> fn, Func<Message, object> messageDeserializer)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddOptions();
            services.PostConfigure<GenericMessageHandlerOptions>(
                options => options.MessageDeserializer = messageDeserializer
            );
            services.AddSingleton<GenericMessageHandler>();
            fn(services);

            return services.BuildServiceProvider()
                .GetRequiredService<GenericMessageHandler>();
        }
    }
}
