namespace Defender.Mongo.MessageBroker.Models.ProcessedEvent
{
    public interface IProcessedEvent
    {
        public DateTime ProcessedDateTime { get; set; }
    }
}
