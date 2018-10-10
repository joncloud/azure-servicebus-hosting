using System.Threading.Tasks;

namespace Microsoft.Azure.ServiceBus.Hosting
{
    public interface IExceptionHandler
    {
        Task HandleExceptionAsync(ExceptionReceivedEventArgs eventArgs);
    }
}
