namespace TestBase.Consts;


internal static class StaticConsts
{
    public const string MessageBrokerROConnectionString = "SharedROConnectionString";
    public const string MessageBrokerAdminConnectionString = "SharedAdminConnectionString";
    public const string DBName = "local_shared";

    public const int TopicDocuments = 200;
    public const int QueueDocuments = 100;

    public const int TopicMaxByteSize = 1000000;
    public const int QueueMaxByteSize = int.MaxValue;
}
