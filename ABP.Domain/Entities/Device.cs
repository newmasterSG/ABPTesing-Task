using System.ComponentModel.DataAnnotations;

namespace ABP.Domain.Entities
{
    public class Device 
    {
        [Key]
        public string? DeviceToken { get; set; }
        public string? Experiment { get; set; }
        public string? ReceivedValue { get; set; }
        public DateTime FirstRequest { get; set; }
    }
}
