namespace Message.Splitter.Store
{
    public static class ApplicationStore
    {
        public static bool IsEnabled { get; set; } = true;
        public static DateTime ExpirationTime { get; set; } = DateTime.Now.AddMinutes(10);
        public static int NumberOfMaximumActiveClients { get; set; } = 1;
        public static List<ProcessClients> ProcessClientsList { get; set; } = [];
    }

    public class ProcessClients
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public DateTime LastTransactionTime { get; set; }
        public bool IsEnabled { get; set; }
    }
}
