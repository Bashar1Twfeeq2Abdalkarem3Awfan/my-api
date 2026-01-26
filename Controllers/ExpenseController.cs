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
    public class ExpenseController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ExpenseController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Expense
        [HttpGet]
        [RequirePermission("view_expenses")]
        public async Task<ActionResult<IEnumerable<ExpenseDto>>> GetExpenses()
        {
            var expenses = await _context.Expenses
                .Include(e => e.CreatedByPerson)
                .Select(e => new ExpenseDto
                {
                    Id = e.Id,
                    Description = e.Description,
                    Amount = e.Amount,
                    Currency = e.Currency,
                    ExpenseDate = e.ExpenseDate,
                    Notes = e.Notes,
                    CreatedBy = e.CreatedBy,
                    CreatedAt = e.CreatedAt,
                    CreatedByPerson = e.CreatedByPerson != null ? new PersonDto
                    {
                        Id = e.CreatedByPerson.Id,
                        FirstName = e.CreatedByPerson.FirstName,
                        SecondName = e.CreatedByPerson.SecondName,
                        ThirdWithLastname = e.CreatedByPerson.ThirdWithLastname,
                        Email = e.CreatedByPerson.Email,
                        PhoneNumber = e.CreatedByPerson.PhoneNumber,
                        Address = e.CreatedByPerson.Address,
                        CreatedAt = e.CreatedByPerson.CreatedAt,
                        UpdatedAt = e.CreatedByPerson.UpdatedAt,
                        IsActive = e.CreatedByPerson.IsActive,
                        PersonType = e.CreatedByPerson.PersonType
                    } : null
                })
                .ToListAsync();

            return Ok(expenses);
        }

        // GET: api/Expense/5
        [HttpGet("{id}")]
        [RequirePermission("view_expenses")]
        public async Task<ActionResult<ExpenseDto>> GetExpense(long id)
        {
            var expense = await _context.Expenses
                .Include(e => e.CreatedByPerson)
                .Where(e => e.Id == id)
                .Select(e => new ExpenseDto
                {
                    Id = e.Id,
                    Description = e.Description,
                    Amount = e.Amount,
                    Currency = e.Currency,
                    ExpenseDate = e.ExpenseDate,
                    Notes = e.Notes,
                    CreatedBy = e.CreatedBy,
                    CreatedAt = e.CreatedAt,
                    CreatedByPerson = e.CreatedByPerson != null ? new PersonDto
                    {
                        Id = e.CreatedByPerson.Id,
                        FirstName = e.CreatedByPerson.FirstName,
                        SecondName = e.CreatedByPerson.SecondName,
                        ThirdWithLastname = e.CreatedByPerson.ThirdWithLastname,
                        Email = e.CreatedByPerson.Email,
                        PhoneNumber = e.CreatedByPerson.PhoneNumber,
                        Address = e.CreatedByPerson.Address,
                        CreatedAt = e.CreatedByPerson.CreatedAt,
                        UpdatedAt = e.CreatedByPerson.UpdatedAt,
                        IsActive = e.CreatedByPerson.IsActive,
                        PersonType = e.CreatedByPerson.PersonType
                    } : null
                })
                .FirstOrDefaultAsync();

            if (expense == null)
                return NotFound();

            return Ok(expense);
        }

        // POST: api/Expense
        [HttpPost]
        [RequirePermission("create_expense")]
        public async Task<ActionResult<ExpenseDto>> PostExpense(CreateExpenseDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var expense = new Expense
            {
                Description = dto.Description,
                Amount = dto.Amount,
                Currency = dto.Currency ?? "YER",
                ExpenseDate = dto.ExpenseDate,
                Notes = dto.Notes,
                CreatedBy = dto.CreatedBy,
                CreatedAt = DateTime.UtcNow
            };

            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();

            var result = await _context.Expenses
                .Include(e => e.CreatedByPerson)
                .Where(e => e.Id == expense.Id)
                .Select(e => new ExpenseDto
                {
                    Id = e.Id,
                    Description = e.Description,
                    Amount = e.Amount,
                    Currency = e.Currency,
                    ExpenseDate = e.ExpenseDate,
                    Notes = e.Notes,
                    CreatedBy = e.CreatedBy,
                    CreatedAt = e.CreatedAt,
                    CreatedByPerson = e.CreatedByPerson != null ? new PersonDto
                    {
                        Id = e.CreatedByPerson.Id,
                        FirstName = e.CreatedByPerson.FirstName,
                        SecondName = e.CreatedByPerson.SecondName,
                        ThirdWithLastname = e.CreatedByPerson.ThirdWithLastname,
                        Email = e.CreatedByPerson.Email,
                        PhoneNumber = e.CreatedByPerson.PhoneNumber,
                        Address = e.CreatedByPerson.Address,
                        CreatedAt = e.CreatedByPerson.CreatedAt,
                        UpdatedAt = e.CreatedByPerson.UpdatedAt,
                        IsActive = e.CreatedByPerson.IsActive,
                        PersonType = e.CreatedByPerson.PersonType
                    } : null
                })
                .FirstOrDefaultAsync();

            return CreatedAtAction(nameof(GetExpense), new { id = expense.Id }, result);
        }

        // PUT: api/Expense/5
        [HttpPut("{id}")]
        [RequirePermission("edit_expense")]
        public async Task<IActionResult> PutExpense(long id, UpdateExpenseDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null)
                return NotFound();

            expense.Description = dto.Description;
            expense.Amount = dto.Amount;
            expense.Currency = dto.Currency;
            expense.ExpenseDate = dto.ExpenseDate;
            expense.Notes = dto.Notes;
            expense.CreatedBy = dto.CreatedBy;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ExpenseExists(id))
                    return NotFound();
                else
                    throw;
            }

            //return NoContent();
            return Ok(dto);
        }

        // DELETE: api/Expense/5
        [HttpDelete("{id}")]
        [RequirePermission("delete_expense")]
        public async Task<IActionResult> DeleteExpense(long id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null)
                return NotFound();

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ExpenseExists(long id) => _context.Expenses.Any(e => e.Id == id);
    }
}



