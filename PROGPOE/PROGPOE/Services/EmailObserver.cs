using PROGPOE.Interfaces;
using PROGPOE.Models;

namespace PROGPOE.Services
{
    public class EmailObserver : IContractObserver
    {
        public void OnContractStatusChanged(Contract contract)
        {
            var email = contract.Client?.Email ?? "unknown@client.com";
            Console.WriteLine($"[EMAIL] To: {email} | Contract {contract.ContractNumber}: {contract.Status}");
        }
    }
}