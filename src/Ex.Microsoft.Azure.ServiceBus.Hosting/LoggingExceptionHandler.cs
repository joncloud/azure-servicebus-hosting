using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Microsoft.Azure.ServiceBus.Hosting
{
    public class LoggingExceptionHandler : IExceptionHandler
    {
        readonly ILogger<LoggingExceptionHandler> _logger;
        public LoggingExceptionHandler(ILogger<LoggingExceptionHandler> logger) =>
            _logger = logger;

        public Task HandleExceptionAsync(ExceptionReceivedEventArgs eventArgs)
        {
            _logger.LogError(
                eventArgs.Exception,
                "Unexpected exception occurred during {action} with client {clientId} connected to {endpoint}/{entityPath}",
                eventArgs.ExceptionReceivedContext.Action,
                eventArgs.ExceptionReceivedContext.ClientId,
                eventArgs.ExceptionReceivedContext.Endpoint,
                eventArgs.ExceptionReceivedContext.EntityPath
            );
            return Task.CompletedTask;
        }
    }
}
