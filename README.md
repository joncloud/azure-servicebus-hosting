# Ex.Microsoft.Azure.ServiceBus.Hosting

## Description
Ex.Microsoft.Azure.ServiceBus.Hosting implements the ability to host an application process messages from an Azure Service Bus Queue or Subscription.

## Licensing
Released under the MIT License. See the [LICENSE][] File for further details.

[license]: LICENSE.md

## Usage
Use the `IMessageHandler` and `IExceptionHandler` interfaces in order to implement functionality for processing. The `IMessageHandler` interface is used for business logic that would normally get processed through the `RegisterMessageHandler` method. Likewise the `IExceptionHandler` interface is used for handling any uncaught exceptions.

### Processing messages
This sample shows a simple implementation of what `IMessageHandler` requires.

```csharp
using Microsoft.Azure.ServiceBus.Hosting;
using System.Threading;
using System.Threading.Tasks;

public class MyMessageHandler : IMessageHandler {
  public Task HandleAsync(Message message, CancellationToken cancellationToken) =>
    throw new NotImplementedException();
}
```

### Processing exceptions
This sample shows a simple implementation of what `IExceptionHandler` requires.

```csharp
using Microsoft.Azure.ServiceBus.Hosting;
using System.Threading;
using System.Threading.Tasks;

public class MyExceptionHandler : IExceptionHandler {
  public Task HandleExceptionAsync(ExceptionReceivedEventArgs eventArgs) =>
    throw new NotImplementedException();
}
```

### Conecting to a Service Bus Queue
Use the `ConfigureServiceBusQueue` extension method to connect to a Service Bus Queue.

```csharp
using Microsoft.Azure.ServiceBus.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

public static class Program {
  static string GetConnectionString(IConfiguration configuration) =>
    configuration.GetSection("ConnectionStrings")["ServiceBusConnection"];

  public static Task Main(string[] args) =>
    new HostBuilder().ConfigureServiceBusQueue(
      (hostBuilder, options) => 
        options.ConnectionString = GetConnectionString(hostBuilder.Configuration),
      context => context.ExceptionHandler<MyExceptionHandler>()
        .StaticMessageHandler()
        .Scoped<MyMessageHandler>()
    )
    .RunConsoleAsync();
}
```

### Conecting to a Service Bus Subscription
Use the `ConfigureServiceBusQueue` extension method to connect to a Service Bus Subscription.

```csharp
using Microsoft.Azure.ServiceBus.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

public static class Program {
  static string GetConnectionString(IConfiguration configuration) =>
    configuration.GetSection("ConnectionStrings")["ServiceBusConnection"];

  public static Task Main(string[] args) =>
    new HostBuilder().ConfigureServiceBusSubscription(
      (hostBuilder, options) =>
      {
        options.ConnectionString = GetConnectionString(hostBuilder.Configuration);
        options.SubscriptionName = hostBuilder.Configuration["SubscriptionName"];
      },
      context => context.ExceptionHandler<MyExceptionHandler>()
        .StaticMessageHandler()
        .Scoped<MyMessageHandler>()
    )
    .RunConsoleAsync();
}
```
