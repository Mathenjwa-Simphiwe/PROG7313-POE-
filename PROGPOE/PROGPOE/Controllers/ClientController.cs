using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROGPOE.Data;
using PROGPOE.Factory;
using PROGPOE.Models;
using PROGPOE.Services;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace PROGPOE.Controllers
{
    [ApiController, Route("api/client"), Authorize(Roles = "Client")]
    public class ApiClientController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ExchangeRateService _fx;

        public ApiClientController(AppDbContext db, ExchangeRateService fx)
        {
            _db = db;
            _fx = fx;
        }

        private string Id => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard() => Ok(new
        {
            totalContracts = await _db.Contracts.CountAsync(c => c.ClientId == Id),
            activeContracts = await _db.Contracts.CountAsync(c => c.ClientId == Id && c.Status == ContractStatus.Active),
            pendingRequests = await _db.ServiceRequests.CountAsync(r => r.Contract!.ClientId == Id && r.Status == ServiceRequestStatus.Pending)
        });

        [HttpGet("contracts")]
        public async Task<IActionResult> Contracts() => Ok(await _db.Contracts
            .Where(c => c.ClientId == Id)
            .Select(c => new
            {
                c.ContractId,
                c.ContractNumber,
                c.StartDate,
                c.EndDate,
                Status = c.Status.ToString(),
                ServiceLevel = c.ServiceLevel.ToString()
            })
            .ToListAsync());

        [HttpPost("contracts/{id}/sign")]
        public async Task<IActionResult> Sign(int id)
        {
            var c = await _db.Contracts.FirstOrDefaultAsync(c => c.ContractId == id && c.ClientId == Id);
            if (c == null) return NotFound(new { Message = "Contract not found" });
            c.ChangeStatus(ContractStatus.Active);
            await _db.SaveChangesAsync();
            return Ok(new { Message = "Contract signed successfully" });
        }

        /// <summary>
        /// GET /api/client/currency/rate
        /// Returns the live USD→ZAR rate from open.er-api.com via HttpClient.
        /// The client UI calls this when the dollar amount field changes to show a live preview.
        /// </summary>
        [HttpGet("currency/rate")]
        public async Task<IActionResult> GetRate()
        {
            var rate = await _fx.GetUsdToZarRateAsync();
            return Ok(new { rate, timestamp = DateTime.UtcNow });
        }

        /// <summary>
        /// POST /api/client/service-requests
        /// Creates a FreightRequest or MaintenanceRequest via the Factory Pattern.
        /// Calls ExchangeRateService (HttpClient → open.er-api.com) to get live USD/ZAR rate,
        /// calculates LocalCostZar from the UsdAmount the client entered, and persists all
        /// currency fields to the database alongside the request.
        /// </summary>
        [HttpPost("service-requests")]
        public async Task<IActionResult> CreateRequest([FromBody] ReqDto d)
        {
            // 🔥 Validate incoming data FIRST
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (d.ContractId <= 0)
                return BadRequest(new { Message = "Invalid contract selected." });

            // 1. Verify contract ownership
            var contract = await _db.Contracts
                .FirstOrDefaultAsync(c => c.ContractId == d.ContractId && c.ClientId == Id);

            if (contract == null)
                return NotFound(new { Message = "Contract not found or does not belong to you." });

            if (contract.Status == ContractStatus.Expired || contract.Status == ContractStatus.OnHold)
                return BadRequest(new { Message = "Cannot create request for an expired or on-hold contract." });

            // 2. Build data dictionary
            var data = new Dictionary<string, object>
            {
                ["Origin"] = d.Origin.Trim(),
                ["Description"] = d.Desc.Trim()
            };

            ServiceRequest request;

            // 3. Factory logic
            if (d.Type == 0) // Freight
            {
                if (string.IsNullOrWhiteSpace(d.Dest) || d.Weight == null)
                    return BadRequest(new { Message = "Freight requests require Destination and Weight." });

                data["Destination"] = d.Dest.Trim();
                data["WeightKg"] = d.Weight.Value;

                request = new FreightRequestFactory().Create(data);
            }
            else // Maintenance
            {
                if (string.IsNullOrWhiteSpace(d.Equip) || d.Hours == null)
                    return BadRequest(new { Message = "Maintenance requests require EquipmentType and EstimatedHours." });

                data["EquipmentType"] = d.Equip.Trim();
                data["EstimatedHours"] = d.Hours.Value;

                request = new Maintenance().Create(data);
            }

            // 4. Currency conversion
            if (d.UsdAmount.HasValue && d.UsdAmount.Value > 0)
            {
                var (zarAmount, rateUsed) = await _fx.ConvertUsdToZarAsync(d.UsdAmount.Value);
                request.UsdAmount = d.UsdAmount.Value;
                request.LocalCostZar = zarAmount;
                request.ExchangeRateUsed = rateUsed;
            }

            // 5. Save
            request.ContractId = d.ContractId;
            _db.ServiceRequests.Add(request);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                request.RequestId,
                request.Cost,
                request.UsdAmount,
                request.LocalCostZar,
                request.ExchangeRateUsed,
                Message = "Service request created successfully."
            });
        }

        [HttpGet("service-requests")]
        public async Task<IActionResult> Requests() => Ok(await _db.ServiceRequests
            .Where(r => r.Contract!.ClientId == Id)
            .Select(r => new
            {
                r.ServiceRequestId,
                r.RequestId,
                Type            = r.RequestType.ToString(),
                r.Origin,
                r.Description,
                r.Cost,
                r.UsdAmount,
                r.LocalCostZar,
                r.ExchangeRateUsed,
                Status          = r.Status.ToString(),
                ContractNumber  = r.Contract!.ContractNumber,
                r.AdminNotes,
                r.RequestDate
            })
            .OrderByDescending(r => r.RequestDate)
            .ToListAsync());
    }

    public class ReqDto
    {
        [Required]
        public int ContractId { get; set; }

        [Required]
        public int Type { get; set; }

        [Required]
        public string Origin { get; set; } = "";

        [Required]
        public string Desc { get; set; } = "";

        // Freight
        public string? Dest { get; set; }
        public decimal? Weight { get; set; }

        // Maintenance
        public string? Equip { get; set; }
        public int? Hours { get; set; }

        public decimal? UsdAmount { get; set; }
    }
}
