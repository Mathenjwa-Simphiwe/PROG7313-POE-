using Microsoft.EntityFrameworkCore;
using PROGPOE.Data;
using PROGPOE.Models;

namespace PROGPOE.Services
{
    public class ContractService
    {
        private readonly AppDbContext _db;
        private readonly Billing _billing;
        private readonly EmailObserver _email;

        // Both observers are injected so the Observer Pattern is fully wired:
        // when a contract changes status, Billing and EmailObserver are notified.
        public ContractService(AppDbContext db, Billing billing, EmailObserver email)
        {
            _db      = db;
            _billing = billing;
            _email   = email;
        }

        public async Task<Contract> CreateAsync(Contract contract)
        {
            contract.ContractNumber = $"CT-{DateTime.Now:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";
            contract.Status = ContractStatus.Draft;

            // Attach both observers BEFORE saving so any status change during creation notifies them
            contract.AttachObserver(_billing);
            contract.AttachObserver(_email);

            _db.Contracts.Add(contract);
            await _db.SaveChangesAsync();
            return contract;
        }

        public async Task<List<Contract>> SearchAsync(DateTime? sd, DateTime? ed, ContractStatus? st)
        {
            var q = _db.Contracts.Include(c => c.Client).AsQueryable();
            if (sd.HasValue) q = q.Where(c => c.StartDate >= sd.Value);
            if (ed.HasValue) q = q.Where(c => c.EndDate <= ed.Value);
            if (st.HasValue) q = q.Where(c => c.Status == st.Value);
            return await q.OrderByDescending(c => c.StartDate).ToListAsync();
        }
    }
}