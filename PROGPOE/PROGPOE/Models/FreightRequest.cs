using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROGPOE.Models
{
    public class FreightRequest : ServiceRequest
    {
        [MaxLength(200)] public string? Destination { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal WeightKg { get; set; }
        [MaxLength(50)] public string? TrackingNumber { get; set; }

        public FreightRequest() => RequestType = ServiceRequestType.Freight;
        public override void CalculateCost() => Cost = WeightKg * 5.50m;
    }
}