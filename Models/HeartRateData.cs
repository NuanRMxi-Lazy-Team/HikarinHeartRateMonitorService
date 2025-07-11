namespace HikarinHeartRateMonitorService.Models
{
    public class HeartRateData
    {
        public int HeartRate { get; set; }
        public DateTime Timestamp { get; set; }
        public string DeviceName { get; set; }
        public readonly string Token = File.ReadAllText("token.txt");
    }

    public class HeartRateResponse
    {
        public int HeartRate { get; set; }
        public DateTime Timestamp { get; set; }
        public string DeviceName { get; set; }
    }
}
