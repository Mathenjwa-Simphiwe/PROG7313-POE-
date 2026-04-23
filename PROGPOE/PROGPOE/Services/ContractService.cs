using Microsoft.EntityFrameworkCore;
using PROGPOE.Data;
using PROGPOE.Models;

namespace PROGPOE.Services
{
    public class ContractService
    {
        private readonly AppDbContext _db;
        public ContractService(AppDbContext db, Billing b, EmailObserver e) => _db = db;

        public async Task<Contract> CreateAsync(Contract c)
        {
            c.ContractNumber = $"CT-{DateTime.Now:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";
            c.Status = ContractStatus.Draft;
            _db.Contracts.Add(c);
            await _db.SaveChangesAsync();
            return c;
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