namespace Microsoft.Azure.ServiceBus.Hosting
{
    public class ServiceBusSubscriptionOptions : ServiceBusQueueOptions
    {
        public string SubscriptionName { get; set; }

        public SubscriptionClient ToSubscriptionClient() =>
            new SubscriptionClient(ConnectionStringBuilder, SubscriptionName, ReceiveMode, RetryPolicy);
    }
}
