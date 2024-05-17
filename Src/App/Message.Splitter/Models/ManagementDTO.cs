namespace Message.Splitter.Models
{

    public class HealthCheckInfoDTO
    {
        public string Id { get; set; }
        public DateTime SystemTime { get; set; }
        public int NumberOfConnectedClients { get; set; }
    }

    public class ManagementResponseDTO
    {
        public bool IsEnabled { get; set; }
        public int NumberOfActiveClients { get; set; }
        public DateTime ExpirationTime { get; set; }
    }

}
