﻿using Microsoft.Azure.ServiceBus.Hosting.Generic;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Microsoft.Azure.ServiceBus.Hosting
{
    public class ServiceBusContext
    {
        public IServiceCollection Services { get; }

        public ServiceBusContext(IServiceCollection services)
        {
            Services = services;
            Services.AddSingleton<IMessageHandlerService, MessageHandlerService>();
        }

        public ServiceBusContext ExceptionHandler<T>() where T : class, IExceptionHandler
        {
            Services.AddSingleton<IExceptionHandler, T>();
            return this;
        }

        public ServiceBusContext MessageHandler(Action<MessageHandlerOptions> config)
        {
            Services.Configure(config);
            return this;
        }

        public GenericMessageHandlerContext GenericMessageHandler(Func<Message, object> messageDeserializer)
        {
            Services.Configure<GenericMessageHandlerOptions>(options =>
            {
                options.MessageDeserializer = messageDeserializer; 
            });
            return new GenericMessageHandlerContext(Services);
        }

        public StaticMessageHandlerContext StaticMessageHandler() =>
            new StaticMessageHandlerContext(Services);
    }
}
