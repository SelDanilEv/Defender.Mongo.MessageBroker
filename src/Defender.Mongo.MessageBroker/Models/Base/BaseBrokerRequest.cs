namespace Defender.Mongo.MessageBroker.Models.Base
{
    internal abstract class BaseBrokerRequest
    {
        protected string? Name { get; set; }
        public string? MessageType { get; set; }
        public long? MaxDocuments { get; set; } = null;
        public long? MaxByteSize { get; set; } = null;

        public string? GetName => Name;
        public void SetName(string name)
        {
            Name = name;
        }
    }
}
