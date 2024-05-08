namespace Defender.Mongo.MessageBroker.Models.Base
{
    internal abstract class BaseBrokerRequestBuilder
    {
        public string MessageType { get; set; } = string.Empty;
        private MessageType MessageTypeEnum { get; set; } = Models.MessageType.Default;

        protected BaseBrokerRequest Build<T>(BaseBrokerRequest request)
        {
            request.MessageType = MessageTypeEnum switch
            {
                Models.MessageType.NoType => string.Empty,
                Models.MessageType.ClassName => typeof(T).Name,
                Models.MessageType.Custom => MessageType,
                Models.MessageType.Default or
                _ => string.Empty,
            };

            return request;
        }

        public void SetMessageType(string messageType)
        {
            MessageType = messageType;
            MessageTypeEnum = Models.MessageType.Custom;
        }

        public void SetMessageType(MessageType messageType)
        {
            MessageTypeEnum = messageType;
        }
    }
}
