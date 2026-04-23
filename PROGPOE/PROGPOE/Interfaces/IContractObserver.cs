using PROGPOE.Models;

namespace PROGPOE.Interfaces
{
    public interface IContractObserver
    {
        void OnContractStatusChanged(Contract contract);
    }
}