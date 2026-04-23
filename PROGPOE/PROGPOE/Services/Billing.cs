using PROGPOE.Interfaces;
using PROGPOE.Models;

namespace PROGPOE.Services
{
    public class Billing : IContractObserver
    {
        public void OnContractStatusChanged(Contract contract)
        {
            Console.WriteLine($"[BILLING] Contract {contract.ContractNumber} status: {contract.Status}");
        }
    }
}