using PROGPOE.Models;

namespace PROGPOE.Factory
{
    public class Maintenance
    {
        public ServiceRequest Create(Dictionary<string, object> d)
        {
            var r = new MaintenanceRequest
            {
                RequestId = $"MT-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}",
                RequestDate = DateTime.Now,
                Origin = d["Origin"].ToString()!,
                Description = d["Description"].ToString()!,
                EquipmentType = d["EquipmentType"].ToString(),
                EstimatedHours = Convert.ToInt32(d["EstimatedHours"]),
                Status = ServiceRequestStatus.Pending
            };
            r.CalculateCost();
            return r;
        }
    }
}