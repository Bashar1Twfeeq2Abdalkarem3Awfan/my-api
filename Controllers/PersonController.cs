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
    public class PersonController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PersonController(AppDbContext context)
        {
            _context = context;
        }

        private async Task<bool> UserHasPermissionAsync(string requiredPermission)
        {
            var userIdHeader = HttpContext.Request.Headers["X-User-Id"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(userIdHeader)) return false;
            if (!long.TryParse(userIdHeader, out var userId)) return false;

            var user = await _context.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role).ThenInclude(r => r!.RolePermissions).ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return false;

            var permissions = user.UserRoles?
                .Where(ur => ur.Role != null && ur.Role.IsActive)
                .SelectMany(ur => ur.Role!.RolePermissions ?? Enumerable.Empty<RolePermission>())
                .Where(rp => rp.Permission != null && rp.Permission.IsActive)
                .Select(rp => rp.Permission!.PermissionName)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            return permissions.Contains(requiredPermission);
        }

        // GET: api/Person
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PersonDto>>> GetPersons()
        {
            bool viewCustomers = await UserHasPermissionAsync("view_customers");
            bool viewSuppliers = await UserHasPermissionAsync("view_suppliers");

            if (!viewCustomers && !viewSuppliers) return Forbid();

            var query = _context.Persons.AsQueryable();

            if (!viewCustomers) query = query.Where(p => p.PersonType != "Customer");
            if (!viewSuppliers) query = query.Where(p => p.PersonType != "Supplier");

            var persons = await query
                .Select(p => new PersonDto
                {
                    Id = p.Id,
                    FirstName = p.FirstName,
                    SecondName = p.SecondName,
                    ThirdWithLastname = p.ThirdWithLastname,
                    Email = p.Email,
                    PhoneNumber = p.PhoneNumber,
                    Address = p.Address,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    IsActive = p.IsActive,
                    PersonType = p.PersonType
                })
                .ToListAsync();

            return Ok(persons);
        }

        // GET: api/Person/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PersonDto>> GetPerson(long id)
        {
            var person = await _context.Persons
                .Where(p => p.Id == id)
                .Select(p => new PersonDto
                {
                    Id = p.Id,
                    FirstName = p.FirstName,
                    SecondName = p.SecondName,
                    ThirdWithLastname = p.ThirdWithLastname,
                    Email = p.Email,
                    PhoneNumber = p.PhoneNumber,
                    Address = p.Address,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    IsActive = p.IsActive,
                    PersonType = p.PersonType
                })
                .FirstOrDefaultAsync();

            if (person == null)
                return NotFound();

            if (person.PersonType == "Customer" && !await UserHasPermissionAsync("view_customers")) return Forbid();
            if (person.PersonType == "Supplier" && !await UserHasPermissionAsync("view_suppliers")) return Forbid();

            return Ok(person);
        }

        // POST: api/Person
        [HttpPost]
        public async Task<ActionResult<PersonDto>> PostPerson(CreatePersonDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.PersonType == "Customer" && !await UserHasPermissionAsync("create_customer")) return Forbid();
            if (dto.PersonType == "Supplier" && !await UserHasPermissionAsync("create_supplier")) return Forbid();

            var person = new Person
            {
                FirstName = dto.FirstName,
                SecondName = dto.SecondName,
                ThirdWithLastname = dto.ThirdWithLastname,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Address = dto.Address,
                IsActive = dto.IsActive,
                PersonType = dto.PersonType,
                CreatedAt = DateTime.UtcNow
            };

            _context.Persons.Add(person);
            await _context.SaveChangesAsync();

            var result = new PersonDto
            {
                Id = person.Id,
                FirstName = person.FirstName,
                SecondName = person.SecondName,
                ThirdWithLastname = person.ThirdWithLastname,
                Email = person.Email,
                PhoneNumber = person.PhoneNumber,
                Address = person.Address,
                CreatedAt = person.CreatedAt,
                UpdatedAt = person.UpdatedAt,
                IsActive = person.IsActive,
                PersonType = person.PersonType
            };

            return CreatedAtAction(nameof(GetPerson), new { id = person.Id }, result);
        }

        // PUT: api/Person/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPerson(long id, UpdatePersonDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingPerson = await _context.Persons.FindAsync(id);
            if (existingPerson == null)
                return NotFound();

            if (existingPerson.PersonType == "Customer" && !await UserHasPermissionAsync("edit_customer")) return Forbid();
            if (existingPerson.PersonType == "Supplier" && !await UserHasPermissionAsync("edit_supplier")) return Forbid();
            
            // تحقق من النوع الجديد أيضاً إذا تغير
            if (dto.PersonType != existingPerson.PersonType)
            {
                if (dto.PersonType == "Customer" && !await UserHasPermissionAsync("create_customer")) return Forbid(); // تحويل لمورد لعميل يتطلب إنشاء عميل
                if (dto.PersonType == "Supplier" && !await UserHasPermissionAsync("create_supplier")) return Forbid();
            }

            existingPerson.FirstName = dto.FirstName;
            existingPerson.SecondName = dto.SecondName;
            existingPerson.ThirdWithLastname = dto.ThirdWithLastname;
            existingPerson.Email = dto.Email;
            existingPerson.PhoneNumber = dto.PhoneNumber;
            existingPerson.Address = dto.Address;
            existingPerson.IsActive = dto.IsActive;
            existingPerson.PersonType = dto.PersonType;
            existingPerson.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Persons.Any(e => e.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/Person/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePerson(long id)
        {
            var person = await _context.Persons.FindAsync(id);
            if (person == null)
                return NotFound();

            if (person.PersonType == "Customer" && !await UserHasPermissionAsync("delete_customer")) return Forbid();
            if (person.PersonType == "Supplier" && !await UserHasPermissionAsync("delete_supplier")) return Forbid();

            _context.Persons.Remove(person);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/Person/UpdateContact/{id}
        // تحديث معلومات الاتصال (هاتف، بريد)
        // Update contact information (phone, email)
        [HttpPut("UpdateContact/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateContact(long id, [FromBody] UpdateContactDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var person = await _context.Persons.FindAsync(id);
            if (person == null)
                return NotFound(new { message = "الشخص غير موجود" });

            // تحديث معلومات الاتصال
            // Update contact info
            person.PhoneNumber = dto.PhoneNumber;
            person.Email = dto.Email;
            person.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Persons.Any(e => e.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return Ok(new { message = "تم تحديث معلومات الاتصال بنجاح" });
        }
    }
}




