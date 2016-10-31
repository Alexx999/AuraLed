namespace AuraLedHelper.Core
{
    public class ServiceMessage
    {
        public ServiceMessage(ServiceCommand command)
        {
            Command = command;
        }

        public ServiceMessage()
        {  
        }

        public ServiceCommand Command { get; set; }
        public int MessageId { get; set; }
    }

    public class ServiceMessage<T> : ServiceMessage
    {
        public ServiceMessage()
        {
        }

        public ServiceMessage(ServiceCommand command, T payload) : base(command)
        {
            Payload = payload;
        }

        public T Payload { get; set; }
    }
}
