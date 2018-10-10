using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Azure.ServiceBus.Hosting
{
    public class DynamicMessageHandlerContext
    {
        readonly IServiceCollection _services;
        public DynamicMessageHandlerContext(IServiceCollection services)
        {
            _services = services;
            _services.AddSingleton<IMessageHandler, DynamicMessageHandler>();
        }

        public DynamicMessageHandlerContext Singleton<TMessage, THandler>() where THandler : class, IMessageHandler<TMessage>
        {
            _services.AddSingleton<IMessageHandler<TMessage>, THandler>();
            return this;
        }

        public DynamicMessageHandlerContext Scoped<TMessage, THandler>() where THandler : class, IMessageHandler<TMessage>
        {
            _services.AddScoped<IMessageHandler<TMessage>, THandler>();
            return this;
        }

        public DynamicMessageHandlerContext Transient<TMessage, THandler>() where THandler : class, IMessageHandler<TMessage>
        {
            _services.AddTransient<IMessageHandler<TMessage>, THandler>();
            return this;
        }
    }
}
