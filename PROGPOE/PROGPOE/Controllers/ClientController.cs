using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using PROGPOE.Data;
using PROGPOE.Factory;
using PROGPOE.Models;

namespace PROGPOE.Controllers
{
    [ApiController, Route("api/[controller]"), Authorize(Roles = "Client")]
    public class ClientController : ControllerBase
    {
        private readonly AppDbContext _db;
        public ClientController(AppDbContext db) => _db = db;
        private string Id => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard() => Ok(new
        {
            Contracts = await _db.Contracts.CountAsync(c => c.ClientId == Id),
            Active = await _db.Contracts.CountAsync(c => c.ClientId == Id && c.Status == ContractStatus.Active),
            Pending = await _db.ServiceRequests.CountAsync(r => r.Contract!.ClientId == Id && r.Status == ServiceRequestStatus.Pending)
        });

        [HttpGet("contracts")]
        public async Task<IActionResult> Contracts() => Ok(await _db.Contracts.Where(c => c.ClientId == Id).Select(c => new { c.ContractId, c.ContractNumber, c.StartDate, c.EndDate, Status = c.Status.ToString(), c.ServiceLevel }).ToListAsync());

        [HttpPost("contracts/{id}/sign")]
        public async Task<IActionResult> Sign(int id)
        {
            var c = await _db.Contracts.FirstOrDefaultAsync(c => c.ContractId == id && c.ClientId == Id);
            if (c == null) return NotFound();
            c.ChangeStatus(ContractStatus.Active);
            await _db.SaveChangesAsync();
            return Ok(new { Message = "Signed" });
        }

        [HttpPost("service-requests")]
        public async Task<IActionResult> CreateRequest([FromBody] ReqDto d)
        {
            var c = await _db.Contracts.FirstOrDefaultAsync(c => c.ContractId == d.ContractId && c.ClientId == Id);
            if (c == null) return NotFound();
            if (c.Status == ContractStatus.Expired || c.Status == ContractStatus.OnHold) return BadRequest("Cannot create request");
            var data = new Dictionary<string, object> { ["Origin"] = d.Origin, ["Description"] = d.Desc };
            ServiceRequest r;
            if (d.Type == 0) { data["Destination"] = d.Dest!; data["WeightKg"] = d.Weight!; r = new FreightRequestFactory().Create(data); }
            else { data["EquipmentType"] = d.Equip!; data["EstimatedHours"] = d.Hours!; r = new Maintenance().Create(data); }
            r.ContractId = d.ContractId;
            _db.ServiceRequests.Add(r);
            await _db.SaveChangesAsync();
            return Ok(new { r.RequestId, r.Cost });
        }

        [HttpGet("service-requests")]
        public async Task<IActionResult> Requests() => Ok(await _db.ServiceRequests.Where(r => r.Contract!.ClientId == Id).Select(r => new { r.ServiceRequestId, r.RequestId, Type = r.RequestType.ToString(), r.Origin, r.Cost, Status = r.Status.ToString(), ContractNumber = r.Contract!.ContractNumber, r.AdminNotes }).ToListAsync());
    }
    public class ReqDto { public int ContractId { get; set; } public int Type { get; set; } public string Origin { get; set; } = ""; public string Desc { get; set; } = ""; public string? Dest { get; set; } public decimal? Weight { get; set; } public string? Equip { get; set; } public int? Hours { get; set; } }
}