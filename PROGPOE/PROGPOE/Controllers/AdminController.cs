using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROGPOE.Data;
using PROGPOE.Models;
using PROGPOE.Services;

namespace PROGPOE.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class ApiAdminController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ContractService _cs;
        private readonly FileStorageService _fs;

        public ApiAdminController(AppDbContext db, ContractService cs, FileStorageService fs)
        {
            _db = db;
            _cs = cs;
            _fs = fs;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            return Ok(new
            {
                TotalContracts = await _db.Contracts.CountAsync(),
                ActiveContracts = await _db.Contracts.CountAsync(c => c.Status == ContractStatus.Active),
                PendingRequests = await _db.ServiceRequests.CountAsync(r => r.Status == ServiceRequestStatus.Pending),
                TotalClients = await _db.Clients.CountAsync()
            });
        }

        [HttpGet("contracts")]
        public async Task<IActionResult> GetContracts(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] ContractStatus? status)
        {
            var contracts = await _cs.SearchAsync(startDate, endDate, status);

            var result = contracts.Select(c => new
            {
                c.ContractId,
                c.ContractNumber,
                c.StartDate,
                c.EndDate,
                Status = c.Status.ToString(),
                ServiceLevel = c.ServiceLevel.ToString(),
                ClientName = c.Client?.FullName,
                HasAgreement = !string.IsNullOrEmpty(c.SignedAgreementPath)
            });

            return Ok(result);
        }

        [HttpPost("contracts")]
        public async Task<IActionResult> CreateContract([FromForm] CreateContractRequest request)
        {
            if (!DateTime.TryParse(request.StartDate, out var startDate))
                return BadRequest(new { Message = "Invalid start date format." });
            if (!DateTime.TryParse(request.EndDate, out var endDate))
                return BadRequest(new { Message = "Invalid end date format." });
            if (endDate <= startDate)
                return BadRequest(new { Message = "End date must be after start date." });

            var contract = new Contract
            {
                ClientId = request.ClientId,
                StartDate = startDate,
                EndDate = endDate,
                ServiceLevel = (ServiceLevel)request.ServiceLevel
            };

            var created = await _cs.CreateAsync(contract);

            if (request.Agreement != null)
            {
                created.SignedAgreementPath = await _fs.SaveFileAsync(created.ContractId, request.Agreement);
                await _db.SaveChangesAsync();
            }

            return Ok(new
            {
                created.ContractId,
                created.ContractNumber,
                Message = "Contract created successfully"
            });
        }

        [HttpPut("contracts/{id}/status")]
        public async Task<IActionResult> ChangeContractStatus(int id, [FromBody] ChangeStatusRequest request)
        {
            var contract = await _db.Contracts.FindAsync(id);
            if (contract == null)
                return NotFound(new { Message = "Contract not found" });

            contract.ChangeStatus(request.Status);
            await _db.SaveChangesAsync();

            return Ok(new { Message = $"Contract status changed to {request.Status}" });
        }

        [HttpGet("service-requests")]
        public async Task<IActionResult> GetServiceRequests()
        {
            var requests = await _db.ServiceRequests
                .Include(r => r.Contract)
                .ThenInclude(c => c!.Client)
                .OrderByDescending(r => r.RequestDate)
                .Select(r => new
                {
                    r.ServiceRequestId,
                    r.RequestId,
                    Type = r.RequestType.ToString(),
                    r.Origin,
                    r.Description,
                    r.Cost,
                    Status = r.Status.ToString(),
                    ContractNumber = r.Contract!.ContractNumber,
                    ClientName = r.Contract.Client!.FullName
                })
                .ToListAsync();

            return Ok(requests);
        }

        [HttpPut("service-requests/{id}/approve")]
        public async Task<IActionResult> ApproveRequest(int id, [FromBody] DecisionRequest request)
        {
            var serviceRequest = await _db.ServiceRequests.FindAsync(id);
            if (serviceRequest == null)
                return NotFound(new { Message = "Request not found" });

            serviceRequest.Status = ServiceRequestStatus.Approved;
            serviceRequest.AdminNotes = request.Notes;
            serviceRequest.DecisionDate = DateTime.Now;
            await _db.SaveChangesAsync();

            return Ok(new { Message = "Request approved" });
        }

        [HttpPut("service-requests/{id}/deny")]
        public async Task<IActionResult> DenyRequest(int id, [FromBody] DecisionRequest request)
        {
            var serviceRequest = await _db.ServiceRequests.FindAsync(id);
            if (serviceRequest == null)
                return NotFound(new { Message = "Request not found" });

            serviceRequest.Status = ServiceRequestStatus.Denied;
            serviceRequest.AdminNotes = request.Notes;
            serviceRequest.DecisionDate = DateTime.Now;
            await _db.SaveChangesAsync();

            return Ok(new { Message = "Request denied" });
        }

        [HttpGet("contracts/{id}/download")]
        public async Task<IActionResult> DownloadAgreement(int id)
        {
            var contract = await _db.Contracts.FindAsync(id);
            if (contract?.SignedAgreementPath == null)
                return NotFound(new { Message = "Agreement not found" });

            var stream = _fs.GetFile(contract.SignedAgreementPath);
            if (stream == null)
                return NotFound(new { Message = "File not found on server" });

            return File(stream, "application/pdf", $"Contract_{contract.ContractNumber}.pdf");
        }

        [HttpGet("clients")]
        public async Task<IActionResult> GetClients()
        {
            var clients = await _db.Clients
                .Select(c => new { c.Id, c.FullName, c.Email, c.Region })
                .ToListAsync();
            return Ok(clients);
        }

        [HttpGet("currency/rate")]
        public IActionResult GetExchangeRate([FromQuery] string from = "USD", [FromQuery] string to = "EUR")
        {
            var converter = new CurrencyConverter();

            return Ok(new
            {
                From = from,
                To = to,
                Rate = converter.GetExchangeRate(from, to),
                Timestamp = DateTime.Now
            });
        }
    }

    // DTOs
    public class CreateContractRequest
    {
        public string ClientId { get; set; } = string.Empty;
        public string StartDate { get; set; } = string.Empty;
        public string EndDate { get; set; } = string.Empty;
        public int ServiceLevel { get; set; }
        public IFormFile? Agreement { get; set; }
    }

    public class ChangeStatusRequest
    {
        public ContractStatus Status { get; set; }
    }

    public class DecisionRequest
    {
        public string? Notes { get; set; }
    }
}