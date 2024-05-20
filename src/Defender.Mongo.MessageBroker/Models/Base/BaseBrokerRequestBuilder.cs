namespace Defender.Mongo.MessageBroker.Models.Base
{
    internal abstract class BaseBrokerRequestBuilder
    {
        public string MessageType { get; set; } = string.Empty;
        public long? MaxDocuments { get; set; } = null;
        public long? MaxByteSize { get; set; } = null;
        private MessageType MessageTypeEnum { get; set; } = Models.MessageType.Default;

        protected BaseBrokerRequest Build(BaseBrokerRequest request)
        {
            request.MessageType = MessageTypeEnum switch
            {
                Models.MessageType.NoType => string.Empty,
                Models.MessageType.Custom => MessageType,
                Models.MessageType.Default or
                _ => string.Empty,
            };

            request.MaxDocuments = MaxDocuments;
            request.MaxByteSize = MaxByteSize;

            return request;
        }

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

            request.MaxDocuments = MaxDocuments;
            request.MaxByteSize = MaxByteSize;

            return request;
        }

        public BaseBrokerRequestBuilder SetMessageType(string messageType)
        {
            MessageType = messageType;
            MessageTypeEnum = Models.MessageType.Custom;

            return this;
        }

        public BaseBrokerRequestBuilder SetMessageType(MessageType messageType)
        {
            MessageTypeEnum = messageType;

            return this;
        }

        public BaseBrokerRequestBuilder SetMaxDocuments(long maxDocuments)
        {
            MaxDocuments = maxDocuments;

            return this;
        }

        public BaseBrokerRequestBuilder SetMaxByteSize(long maxByteSize)
        {
            MaxByteSize = maxByteSize;

            return this;
        }
    }
}
