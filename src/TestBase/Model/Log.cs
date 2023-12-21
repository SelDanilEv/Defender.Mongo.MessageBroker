namespace TestBase.Model
{
    public static class Log
    {
        public static List<string> Messages { get; } = new List<string>();

        public static void AddLog(string message)
        {
            Messages.Add(message);
        }
    }
}