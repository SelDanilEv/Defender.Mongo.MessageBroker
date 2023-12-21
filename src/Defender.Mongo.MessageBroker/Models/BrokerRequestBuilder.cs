using Defender.Mongo.MessageBroker.Models.TopicMessage;

namespace Defender.Mongo.MessageBroker.Models
{
    internal class BrokerRequestBuilder
    {
        public string TopicName { get; set; } = "default";
        public string MessageType { get; set; } = String.Empty;
        private MessageType MessageTypeEnum { get; set; } = TopicMessage.MessageType.Default;

        public BrokerRequest Build<T>()
        {
            var messageTypeString = MessageTypeEnum switch
            {
                TopicMessage.MessageType.NoType => string.Empty,
                TopicMessage.MessageType.ClassName => typeof(T).Name,
                TopicMessage.MessageType.Custom => MessageType,
                TopicMessage.MessageType.Default or
                _ => string.Empty,
            };

            return new BrokerRequest { TopicName = TopicName, MessageType = messageTypeString };
        }

        public void SetMessageType(string messageType)
        {
            this.MessageType = messageType;
            this.MessageTypeEnum = TopicMessage.MessageType.Custom;
        }

        public void SetMessageType(MessageType messageType)
        {
            this.MessageTypeEnum = messageType;
        }

        public void SetTopic(string topicName)
        {
            this.TopicName = topicName;
        }
    }
}
