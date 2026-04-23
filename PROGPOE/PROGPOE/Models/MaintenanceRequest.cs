using System.ComponentModel.DataAnnotations;

namespace PROGPOE.Models
{
    public class MaintenanceRequest : ServiceRequest
    {
        [MaxLength(100)] public string? EquipmentType { get; set; }
        public int EstimatedHours { get; set; }
        [MaxLength(100)] public string? TechnicianName { get; set; }
        public DateTime? ScheduledDate { get; set; }

        public MaintenanceRequest() => RequestType = ServiceRequestType.Maintenance;
        public override void CalculateCost() => Cost = EstimatedHours * 120m;
    }
}