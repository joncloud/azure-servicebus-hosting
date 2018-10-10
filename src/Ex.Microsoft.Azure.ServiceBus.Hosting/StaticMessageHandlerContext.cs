using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Azure.ServiceBus.Hosting
{
    public class StaticMessageHandlerContext
    {
        readonly IServiceCollection _services;
        public StaticMessageHandlerContext(IServiceCollection services) =>
            _services = services;

        public StaticMessageHandlerContext Singleton<T>() where T : class, IMessageHandler
        {
            _services.AddSingleton<IMessageHandler, T>();
            return this;
        }

        public StaticMessageHandlerContext Scoped<T>() where T : class, IMessageHandler
        {
            _services.AddScoped<IMessageHandler, T>();
            return this;
        }

        public StaticMessageHandlerContext Transient<T>() where T : class, IMessageHandler
        {
            _services.AddTransient<IMessageHandler, T>();
            return this;
        }
    }
}
