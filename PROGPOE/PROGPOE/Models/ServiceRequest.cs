using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROGPOE.Models
{
    public enum ServiceRequestStatus { Pending, Approved, InProgress, Completed, Denied }
    public enum ServiceRequestType { Freight, Maintenance }

    public abstract class ServiceRequest
    {
        [Key]
        public int ServiceRequestId { get; set; }
        [Required, MaxLength(50)] public string RequestId { get; set; } = string.Empty;
        [Required] public DateTime RequestDate { get; set; } = DateTime.Now;
        [Required, MaxLength(200)] public string Origin { get; set; } = string.Empty;
        [Required, MaxLength(500)] public string Description { get; set; } = string.Empty;

        [Required, Column(TypeName = "decimal(18,2)")] public decimal Cost { get; protected set; }

        /// <summary>USD amount entered by the client for currency conversion</summary>
        [Column(TypeName = "decimal(18,2)")] public decimal? UsdAmount { get; set; }

        /// <summary>ZAR equivalent calculated via live ExchangeRateService at submission time</summary>
        [Column(TypeName = "decimal(18,2)")] public decimal? LocalCostZar { get; set; }

        /// <summary>The USD/ZAR rate fetched from open.er-api.com at submission time</summary>
        [Column(TypeName = "decimal(18,4)")] public decimal? ExchangeRateUsed { get; set; }

        [Required] public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.Pending;
        [Required] public ServiceRequestType RequestType { get; protected set; }
        public int ContractId { get; set; }
        [ForeignKey(nameof(ContractId))] public Contract? Contract { get; set; }
        public string? AdminNotes { get; set; }
        public DateTime? DecisionDate { get; set; }
        public abstract void CalculateCost();
    }
}