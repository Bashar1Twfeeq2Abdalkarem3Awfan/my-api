using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MyAPIv3.Data;
using MyAPIv3.Models;
using MyAPIv3.DTOs;
using MyAPIv3.Attributes;

namespace MyAPIv3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DebtController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DebtController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Debt
        [HttpGet]
        [RequirePermission("view_debts")]
        public async Task<ActionResult<IEnumerable<DebtDto>>> GetDebts(
            [FromQuery] long? clientId,
            [FromQuery] long? supplierId)
        {
            var query = _context.Debts.AsQueryable();

            if (clientId.HasValue)
            {
                query = query.Where(d => d.ClientId == clientId.Value);
            }

            if (supplierId.HasValue)
            {
                query = query.Where(d => d.SupplierId == supplierId.Value);
            }

            var debts = await query
                .Include(d => d.ClientPerson)
                .Include(d => d.SupplierPerson)
                .Include(d => d.CreatedByPerson)
                .Select(d => new DebtDto
                {
                    Id = d.Id,
                    AccountName = d.AccountName,
                    AccountType = d.AccountType,
                    Debit = d.Debit,
                    Credit = d.Credit,
                    Paid = d.Paid,
                    Remaining = d.Remaining,
                    LastPaymentDate = d.LastPaymentDate,
                    Notes = d.Notes,
                    ClientId = d.ClientId,
                    SupplierId = d.SupplierId,
                    CreatedBy = d.CreatedBy,
                    CreatedAt = d.CreatedAt,
                    ClientPerson = d.ClientPerson != null ? new PersonDto
                    {
                        Id = d.ClientPerson.Id,
                        FirstName = d.ClientPerson.FirstName,
                        SecondName = d.ClientPerson.SecondName,
                        ThirdWithLastname = d.ClientPerson.ThirdWithLastname,
                        Email = d.ClientPerson.Email,
                        PhoneNumber = d.ClientPerson.PhoneNumber,
                        Address = d.ClientPerson.Address,
                        CreatedAt = d.ClientPerson.CreatedAt,
                        UpdatedAt = d.ClientPerson.UpdatedAt,
                        IsActive = d.ClientPerson.IsActive,
                        PersonType = d.ClientPerson.PersonType
                    } : null,
                    SupplierPerson = d.SupplierPerson != null ? new PersonDto
                    {
                        Id = d.SupplierPerson.Id,
                        FirstName = d.SupplierPerson.FirstName,
                        SecondName = d.SupplierPerson.SecondName,
                        ThirdWithLastname = d.SupplierPerson.ThirdWithLastname,
                        Email = d.SupplierPerson.Email,
                        PhoneNumber = d.SupplierPerson.PhoneNumber,
                        Address = d.SupplierPerson.Address,
                        CreatedAt = d.SupplierPerson.CreatedAt,
                        UpdatedAt = d.SupplierPerson.UpdatedAt,
                        IsActive = d.SupplierPerson.IsActive,
                        PersonType = d.SupplierPerson.PersonType
                    } : null,
                    CreatedByPerson = d.CreatedByPerson != null ? new PersonDto
                    {
                        Id = d.CreatedByPerson.Id,
                        FirstName = d.CreatedByPerson.FirstName,
                        SecondName = d.CreatedByPerson.SecondName,
                        ThirdWithLastname = d.CreatedByPerson.ThirdWithLastname,
                        Email = d.CreatedByPerson.Email,
                        PhoneNumber = d.CreatedByPerson.PhoneNumber,
                        Address = d.CreatedByPerson.Address,
                        CreatedAt = d.CreatedByPerson.CreatedAt,
                        UpdatedAt = d.CreatedByPerson.UpdatedAt,
                        IsActive = d.CreatedByPerson.IsActive,
                        PersonType = d.CreatedByPerson.PersonType
                    } : null
                })
                .ToListAsync();

            return Ok(debts);
        }

        // GET: api/Debt/ByPerson/5
        [HttpGet("ByPerson/{personId}")]
        [RequirePermission("view_debts")]
        public async Task<ActionResult<IEnumerable<DebtDto>>> GetDebtsByPerson(long personId)
        {
            var debts = await _context.Debts
                .Where(d => d.ClientId == personId || d.SupplierId == personId)
                .Select(d => new DebtDto
                {
                    Id = d.Id,
                    AccountName = d.AccountName,
                    AccountType = d.AccountType,
                    Debit = d.Debit,
                    Credit = d.Credit,
                    Paid = d.Paid,
                    Remaining = d.Remaining,
                    LastPaymentDate = d.LastPaymentDate,
                    Notes = d.Notes,
                    ClientId = d.ClientId,
                    SupplierId = d.SupplierId,
                    CreatedBy = d.CreatedBy,
                    CreatedAt = d.CreatedAt,
                    ClientPerson = d.ClientPerson != null ? new PersonDto
                    {
                        Id = d.ClientPerson.Id,
                        FirstName = d.ClientPerson.FirstName,
                        SecondName = d.ClientPerson.SecondName,
                        ThirdWithLastname = d.ClientPerson.ThirdWithLastname,
                        Email = d.ClientPerson.Email,
                        PhoneNumber = d.ClientPerson.PhoneNumber,
                        Address = d.ClientPerson.Address,
                        CreatedAt = d.ClientPerson.CreatedAt,
                        UpdatedAt = d.ClientPerson.UpdatedAt,
                        IsActive = d.ClientPerson.IsActive,
                        PersonType = d.ClientPerson.PersonType
                    } : null,
                    SupplierPerson = d.SupplierPerson != null ? new PersonDto
                    {
                        Id = d.SupplierPerson.Id,
                        FirstName = d.SupplierPerson.FirstName,
                        SecondName = d.SupplierPerson.SecondName,
                        ThirdWithLastname = d.SupplierPerson.ThirdWithLastname,
                        Email = d.SupplierPerson.Email,
                        PhoneNumber = d.SupplierPerson.PhoneNumber,
                        Address = d.SupplierPerson.Address,
                        CreatedAt = d.SupplierPerson.CreatedAt,
                        UpdatedAt = d.SupplierPerson.UpdatedAt,
                        IsActive = d.SupplierPerson.IsActive,
                        PersonType = d.SupplierPerson.PersonType
                    } : null,
                    CreatedByPerson = d.CreatedByPerson != null ? new PersonDto
                    {
                        Id = d.CreatedByPerson.Id,
                        FirstName = d.CreatedByPerson.FirstName,
                        SecondName = d.CreatedByPerson.SecondName,
                        ThirdWithLastname = d.CreatedByPerson.ThirdWithLastname,
                        Email = d.CreatedByPerson.Email,
                        PhoneNumber = d.CreatedByPerson.PhoneNumber,
                        Address = d.CreatedByPerson.Address,
                        CreatedAt = d.CreatedByPerson.CreatedAt,
                        UpdatedAt = d.CreatedByPerson.UpdatedAt,
                        IsActive = d.CreatedByPerson.IsActive,
                        PersonType = d.CreatedByPerson.PersonType
                    } : null
                })
                .ToListAsync();

            return Ok(debts);
        }

        // GET: api/Debt/5
        [HttpGet("{id}")]
        [RequirePermission("view_debts")]
        public async Task<ActionResult<DebtDto>> GetDebt(long id)
        {
            var debt = await _context.Debts
                .Include(d => d.ClientPerson)
                .Include(d => d.SupplierPerson)
                .Include(d => d.CreatedByPerson)
                .Where(d => d.Id == id)
                .Select(d => new DebtDto
                {
                    Id = d.Id,
                    AccountName = d.AccountName,
                    AccountType = d.AccountType,
                    Debit = d.Debit,
                    Credit = d.Credit,
                    Paid = d.Paid,
                    Remaining = d.Remaining,
                    LastPaymentDate = d.LastPaymentDate,
                    Notes = d.Notes,
                    ClientId = d.ClientId,
                    SupplierId = d.SupplierId,
                    CreatedBy = d.CreatedBy,
                    CreatedAt = d.CreatedAt,
                    ClientPerson = d.ClientPerson != null ? new PersonDto
                    {
                        Id = d.ClientPerson.Id,
                        FirstName = d.ClientPerson.FirstName,
                        SecondName = d.ClientPerson.SecondName,
                        ThirdWithLastname = d.ClientPerson.ThirdWithLastname,
                        Email = d.ClientPerson.Email,
                        PhoneNumber = d.ClientPerson.PhoneNumber,
                        Address = d.ClientPerson.Address,
                        CreatedAt = d.ClientPerson.CreatedAt,
                        UpdatedAt = d.ClientPerson.UpdatedAt,
                        IsActive = d.ClientPerson.IsActive,
                        PersonType = d.ClientPerson.PersonType
                    } : null,
                    SupplierPerson = d.SupplierPerson != null ? new PersonDto
                    {
                        Id = d.SupplierPerson.Id,
                        FirstName = d.SupplierPerson.FirstName,
                        SecondName = d.SupplierPerson.SecondName,
                        ThirdWithLastname = d.SupplierPerson.ThirdWithLastname,
                        Email = d.SupplierPerson.Email,
                        PhoneNumber = d.SupplierPerson.PhoneNumber,
                        Address = d.SupplierPerson.Address,
                        CreatedAt = d.SupplierPerson.CreatedAt,
                        UpdatedAt = d.SupplierPerson.UpdatedAt,
                        IsActive = d.SupplierPerson.IsActive,
                        PersonType = d.SupplierPerson.PersonType
                    } : null,
                    CreatedByPerson = d.CreatedByPerson != null ? new PersonDto
                    {
                        Id = d.CreatedByPerson.Id,
                        FirstName = d.CreatedByPerson.FirstName,
                        SecondName = d.CreatedByPerson.SecondName,
                        ThirdWithLastname = d.CreatedByPerson.ThirdWithLastname,
                        Email = d.CreatedByPerson.Email,
                        PhoneNumber = d.CreatedByPerson.PhoneNumber,
                        Address = d.CreatedByPerson.Address,
                        CreatedAt = d.CreatedByPerson.CreatedAt,
                        UpdatedAt = d.CreatedByPerson.UpdatedAt,
                        IsActive = d.CreatedByPerson.IsActive,
                        PersonType = d.CreatedByPerson.PersonType
                    } : null
                })
                .FirstOrDefaultAsync();

            if (debt == null)
                return NotFound();

            return Ok(debt);
        }

        // POST: api/Debt
        [HttpPost]
        [RequirePermission("create_debt")]
        public async Task<ActionResult<DebtDto>> PostDebt(CreateDebtDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var debt = new Debt
            {
                AccountName = dto.AccountName,
                AccountType = dto.AccountType,
                Debit = dto.Debit,
                Credit = dto.Credit,
                Paid = dto.Paid,
                Remaining = dto.Remaining,
                LastPaymentDate = dto.LastPaymentDate,
                Notes = dto.Notes,
                ClientId = dto.ClientId,
                SupplierId = dto.SupplierId,
                CreatedBy = dto.CreatedBy,
                CreatedAt = DateTime.UtcNow
            };

            _context.Debts.Add(debt);
            await _context.SaveChangesAsync();

            var result = await _context.Debts
                .Include(d => d.ClientPerson)
                .Include(d => d.SupplierPerson)
                .Include(d => d.CreatedByPerson)
                .Where(d => d.Id == debt.Id)
                .Select(d => new DebtDto
                {
                    Id = d.Id,
                    AccountName = d.AccountName,
                    AccountType = d.AccountType,
                    Debit = d.Debit,
                    Credit = d.Credit,
                    Paid = d.Paid,
                    Remaining = d.Remaining,
                    LastPaymentDate = d.LastPaymentDate,
                    Notes = d.Notes,
                    ClientId = d.ClientId,
                    SupplierId = d.SupplierId,
                    CreatedBy = d.CreatedBy,
                    CreatedAt = d.CreatedAt,
                    ClientPerson = d.ClientPerson != null ? new PersonDto
                    {
                        Id = d.ClientPerson.Id,
                        FirstName = d.ClientPerson.FirstName,
                        SecondName = d.ClientPerson.SecondName,
                        ThirdWithLastname = d.ClientPerson.ThirdWithLastname,
                        Email = d.ClientPerson.Email,
                        PhoneNumber = d.ClientPerson.PhoneNumber,
                        Address = d.ClientPerson.Address,
                        CreatedAt = d.ClientPerson.CreatedAt,
                        UpdatedAt = d.ClientPerson.UpdatedAt,
                        IsActive = d.ClientPerson.IsActive,
                        PersonType = d.ClientPerson.PersonType
                    } : null,
                    SupplierPerson = d.SupplierPerson != null ? new PersonDto
                    {
                        Id = d.SupplierPerson.Id,
                        FirstName = d.SupplierPerson.FirstName,
                        SecondName = d.SupplierPerson.SecondName,
                        ThirdWithLastname = d.SupplierPerson.ThirdWithLastname,
                        Email = d.SupplierPerson.Email,
                        PhoneNumber = d.SupplierPerson.PhoneNumber,
                        Address = d.SupplierPerson.Address,
                        CreatedAt = d.SupplierPerson.CreatedAt,
                        UpdatedAt = d.SupplierPerson.UpdatedAt,
                        IsActive = d.SupplierPerson.IsActive,
                        PersonType = d.SupplierPerson.PersonType
                    } : null,
                    CreatedByPerson = d.CreatedByPerson != null ? new PersonDto
                    {
                        Id = d.CreatedByPerson.Id,
                        FirstName = d.CreatedByPerson.FirstName,
                        SecondName = d.CreatedByPerson.SecondName,
                        ThirdWithLastname = d.CreatedByPerson.ThirdWithLastname,
                        Email = d.CreatedByPerson.Email,
                        PhoneNumber = d.CreatedByPerson.PhoneNumber,
                        Address = d.CreatedByPerson.Address,
                        CreatedAt = d.CreatedByPerson.CreatedAt,
                        UpdatedAt = d.CreatedByPerson.UpdatedAt,
                        IsActive = d.CreatedByPerson.IsActive,
                        PersonType = d.CreatedByPerson.PersonType
                    } : null
                })
                .FirstOrDefaultAsync();

            return CreatedAtAction(nameof(GetDebt), new { id = debt.Id }, result);
        }

        // PUT: api/Debt/5
        [HttpPut("{id}")]
        [RequirePermission("edit_debt")]
        public async Task<IActionResult> PutDebt(long id, UpdateDebtDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var debt = await _context.Debts.FindAsync(id);
            if (debt == null)
                return NotFound();

            debt.AccountName = dto.AccountName;
            debt.AccountType = dto.AccountType;
            debt.Debit = dto.Debit;
            debt.Credit = dto.Credit;
            debt.Paid = dto.Paid;
            debt.Remaining = dto.Remaining;
            debt.LastPaymentDate = dto.LastPaymentDate;
            debt.Notes = dto.Notes;
            debt.ClientId = dto.ClientId;
            debt.SupplierId = dto.SupplierId;
            debt.CreatedBy = dto.CreatedBy;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DebtExists(id))
                    return NotFound();
                else
                    throw;
            }

            //return NoContent();
            return Ok(dto);
        }

        // DELETE: api/Debt/5
        [HttpDelete("{id}")]
        [RequirePermission("delete_debt")]
        public async Task<IActionResult> DeleteDebt(long id)
        {
            var debt = await _context.Debts.FindAsync(id);
            if (debt == null)
                return NotFound();

            _context.Debts.Remove(debt);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DebtExists(long id) => _context.Debts.Any(d => d.Id == id);
    }
}



