namespace Microsoft.Azure.ServiceBus.Hosting
{
    public class ServiceBusQueueOptions
    {
        public ServiceBusConnectionStringBuilder ConnectionStringBuilder { get; private set; } = new ServiceBusConnectionStringBuilder();
        public string ConnectionString
        {
            get => ConnectionStringBuilder.ToString();
            set => ConnectionStringBuilder = new ServiceBusConnectionStringBuilder(value ?? "");
        }
        public ReceiveMode ReceiveMode { get; set; } = ReceiveMode.PeekLock;
        public RetryPolicy RetryPolicy { get; set; }

        public QueueClient ToQueueClient() =>
            new QueueClient(ConnectionStringBuilder, ReceiveMode, RetryPolicy);
    }
}
