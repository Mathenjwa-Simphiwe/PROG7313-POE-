using PROGPOE.Models;

namespace PROGPOE.Factory
{
    public class FreightRequestFactory
    {
        public ServiceRequest Create(Dictionary<string, object> d)
        {
            var r = new FreightRequest
            {
                RequestId = $"FR-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}",
                RequestDate = DateTime.Now,
                Origin = d["Origin"].ToString()!,
                Description = d["Description"].ToString()!,
                Destination = d["Destination"].ToString(),
                WeightKg = Convert.ToDecimal(d["WeightKg"]),
                Status = ServiceRequestStatus.Pending
            };
            r.CalculateCost();
            return r;
        }
    }
}