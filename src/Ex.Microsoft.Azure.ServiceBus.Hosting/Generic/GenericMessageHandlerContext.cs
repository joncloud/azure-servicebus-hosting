using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Azure.ServiceBus.Hosting.Generic
{
    public class GenericMessageHandlerContext
    {
        readonly IServiceCollection _services;
        public GenericMessageHandlerContext(IServiceCollection services)
        {
            _services = services;
            _services.AddSingleton<IMessageHandler, GenericMessageHandler>();
        }

        public GenericMessageHandlerContext Singleton<TMessage, THandler>() where THandler : class, IMessageHandler<TMessage>
        {
            _services.AddSingleton<IMessageHandler<TMessage>, THandler>();
            return this;
        }

        public GenericMessageHandlerContext Scoped<TMessage, THandler>() where THandler : class, IMessageHandler<TMessage>
        {
            _services.AddScoped<IMessageHandler<TMessage>, THandler>();
            return this;
        }

        public GenericMessageHandlerContext Transient<TMessage, THandler>() where THandler : class, IMessageHandler<TMessage>
        {
            _services.AddTransient<IMessageHandler<TMessage>, THandler>();
            return this;
        }
    }
}
