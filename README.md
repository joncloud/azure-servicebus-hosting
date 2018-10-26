# Ex.Microsoft.Azure.ServiceBus.Hosting
[![NuGet](https://img.shields.io/nuget/v/Ex.Microsoft.Azure.ServiceBus.Hosting.svg)](https://www.nuget.org/packages/Ex.Microsoft.Azure.ServiceBus.Hosting/)

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

### Connecting to a Service Bus Queue
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

### Connecting to a Service Bus Subscription
Use the `ConfigureServiceBusSubscription` extension method to connect to a Service Bus Subscription.

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

### Handling messages generically
You can also handle messages using `IMessageHandler<T>`, which creates a contract with a generic type.

```csharp
public class MyInt32Handler : IMessageHandler<int> {
  public Task HandleAsync(int message, CancellationToken cancellationToken) =>
    throw new NotImplementedException();
}

public class MyStringHandler : IMessageHandler<string> {
  public Task HandleAsync(string message, CancellationToken cancellationToken) =>
    throw new NotImplementedException();
}

public static class Program {
  static string GetConnectionString(IConfiguration configuration) =>
    configuration.GetSection("ConnectionStrings")["ServiceBusConnection"];

  public static Task Main(string[] args) =>
    new HostBuilder().ConfigureServiceBusQueue(
      (hostBuilder, options) =>
        options.ConnectionString = GetConnectionString(hostBuilder.Configuration),
      context => context.ExceptionHandler<MyExceptionHandler>()
        .GenericMessageHandler(msg =>
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
        })
        .Scoped<int, MyInt32Handler>()
        .Scoped<string, MyStringHandler>()
    )
    .RunConsoleAsync();
}
```
