using PROGPOE.Models;

namespace PROGPOE.Interfaces
{
    public interface IServiceRequestFactory
    {
        ServiceRequest CreateRequest(Dictionary<string, object> data);
        bool ValidateData(Dictionary<string, object> data);
    }
}