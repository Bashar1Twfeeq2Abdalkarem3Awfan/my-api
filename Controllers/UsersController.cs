using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyAPIv3.Data;
using MyAPIv3.Models;
using MyAPIv3.DTOs;
using MyAPIv3.Helpers;
using MyAPIv3.Attributes;
using MyAPIv3.Services;
using Microsoft.AspNetCore.Authorization;

namespace MyAPIv3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwtService;

        public UsersController(AppDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        // ============================================================
        // Login - نقطة نهاية جديدة لتسجيل الدخول
        // Login - New endpoint for user authentication
        // ============================================================
        
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponseDto>> Login(LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // البحث عن المستخدم مع الصلاحيات
            // Find user by username with permissions
            var user = await _context.Users
                .Include(u => u.Person)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.Username == dto.Username);

            if (user == null)
            {
                return Ok(new LoginResponseDto
                {
                    Success = false,
Message = "اسم المستخدم أو كلمة المرور غير صحيحة"
                });
            }

            // التحقق من كلمة المرور
            // Verify password
            if (!PasswordHelper.VerifyPassword(dto.Password, user.PasswordHash))
            {
                return Ok(new LoginResponseDto
                {
                    Success = false,
                    Message = "اسم المستخدم أو كلمة المرور غير صحيحة"
                });
            }

            // تحديث آخر تسجيل دخول
            // Update last login
            user.LastLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // الحصول على جميع الصلاحيات
            // Get all user permissions
            var permissions = user.UserRoles?
                .SelectMany(ur => ur.Role?.RolePermissions?
                    .Select(rp => rp.Permission?.PermissionName ?? string.Empty) ?? Enumerable.Empty<string>())
                .Distinct()
                .ToList() ?? new List<string>();

            // توليد JWT Token
            // Generate JWT Token
            var token = _jwtService.GenerateToken(user.Id, user.Username, permissions);

            // إرجاع بيانات المستخدم مع Token
            // Return user data with token
            return Ok(new LoginResponseDto
            {
                Success = true,
                Message = "تم تسجيل الدخول بنجاح",
                Token = token,
                User = new UserDto
                {
                    Id = user.Id,
                    PersonId = user.PersonId,
                    Username = user.Username,
                    LoginName = user.LoginName,
                    LastLogin = user.LastLogin,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                    Person = user.Person != null ? new PersonDto
                    {
                        Id = user.Person.Id,
                        FirstName = user.Person.FirstName,
                        SecondName = user.Person.SecondName,
                        ThirdWithLastname = user.Person.ThirdWithLastname,
                        Email = user.Person.Email,
                        PhoneNumber = user.Person.PhoneNumber,
                        IsActive = user.Person.IsActive,
                        PersonType = user.Person.PersonType
                    } : null
                }
            });
        }

        // GET: api/Users
        [HttpGet]
        [RequirePermission("view_users")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _context.Users
                .Include(u => u.Person)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    PersonId = u.PersonId,
                    Username = u.Username,
                    LoginName = u.LoginName,
                    LastLogin = u.LastLogin,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt,
                    Person = u.Person != null ? new PersonDto
                    {
                        Id = u.Person.Id,
                        FirstName = u.Person.FirstName,
                        SecondName = u.Person.SecondName,
                        ThirdWithLastname = u.Person.ThirdWithLastname,
                        Email = u.Person.Email,
                        PhoneNumber = u.Person.PhoneNumber,
                        Address = u.Person.Address,
                        CreatedAt = u.Person.CreatedAt,
                        UpdatedAt = u.Person.UpdatedAt,
                        IsActive = u.Person.IsActive,
                        PersonType = u.Person.PersonType
                    } : null
                })
                .ToListAsync();

            return Ok(users);
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        [RequirePermission("view_users")]
        public async Task<ActionResult<UserDto>> GetUser(long id)
        {
            var user = await _context.Users
                .Include(u => u.Person)
                .Where(u => u.Id == id)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    PersonId = u.PersonId,
                    Username = u.Username,
                    LoginName = u.LoginName,
                    LastLogin = u.LastLogin,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt,
                    Person = u.Person != null ? new PersonDto
                    {
                        Id = u.Person.Id,
                        FirstName = u.Person.FirstName,
                        SecondName = u.Person.SecondName,
                        ThirdWithLastname = u.Person.ThirdWithLastname,
                        Email = u.Person.Email,
                        PhoneNumber = u.Person.PhoneNumber,
                        Address = u.Person.Address,
                        CreatedAt = u.Person.CreatedAt,
                        UpdatedAt = u.Person.UpdatedAt,
                        IsActive = u.Person.IsActive,
                        PersonType = u.Person.PersonType
                    } : null
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        // POST: api/Users
        // Modified: 2025-12-15 - Added permission check
        // ملاحظة 18/12/2025:
        // تم توحيد اسم الصلاحية هنا مع PermissionName في جدول Permissions
        // ومع ثوابت Flutter (permission_constants.dart) لضمان تطابق كامل.
        [HttpPost]
        [RequirePermission("create_user")] // ← Requires 'create_user' permission
        public async Task<ActionResult<UserDto>> PostUser(CreateUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var personExists = await _context.Persons.AnyAsync(p => p.Id == dto.PersonId);
            if (!personExists)
                return BadRequest("Associated person does not exist.");

            // التحقق من عدم تكرار اسم المستخدم
            // Check username uniqueness
            var usernameExists = await _context.Users.AnyAsync(u => u.Username == dto.Username);
            if (usernameExists)
                return BadRequest("اسم المستخدم موجود مسبقاً");

            // ← تشفير كلمة المرور
            // Hash password
            var passwordHash = PasswordHelper.HashPassword(dto.Password);

            var user = new User
            {
                PersonId = dto.PersonId,
                Username = dto.Username,
                PasswordHash = passwordHash, // ← استخدام الكلمة المشفرة
                LoginName = dto.LoginName,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = await _context.Users
                .Include(u => u.Person)
                .Where(u => u.Id == user.Id)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    PersonId = u.PersonId,
                    Username = u.Username,
                    LoginName = u.LoginName,
                    LastLogin = u.LastLogin,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt,
                    Person = u.Person != null ? new PersonDto
                    {
                        Id = u.Person.Id,
                        FirstName = u.Person.FirstName,
                        SecondName = u.Person.SecondName,
                        ThirdWithLastname = u.Person.ThirdWithLastname,
                        Email = u.Person.Email,
                        PhoneNumber = u.Person.PhoneNumber,
                        Address = u.Person.Address,
                        CreatedAt = u.Person.CreatedAt,
                        UpdatedAt = u.Person.UpdatedAt,
                        IsActive = u.Person.IsActive,
                        PersonType = u.Person.PersonType
                    } : null
                })
                .FirstOrDefaultAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, result);
        }

        // PUT: api/Users/5
        // Modified: 2025-12-15 - Added permission check
        // ملاحظة 18/12/2025:
        // تم توحيد اسم الصلاحية مع PermissionName = 'edit_user'.
        [HttpPut("{id}")]
        [RequirePermission("edit_user")] // ← Requires 'edit_user' permission
        public async Task<IActionResult> PutUser(long id, UpdateUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
                return NotFound();

            existingUser.Username = dto.Username;
            existingUser.LoginName = dto.LoginName;
            
            // ← تشفير كلمة المرور الجديدة إذا تم تغييرها
            // Hash new password if changed
            if (!string.IsNullOrEmpty(dto.Password))
                existingUser.PasswordHash = PasswordHelper.HashPassword(dto.Password);
            
            existingUser.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Users.Any(e => e.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }


        // GET: api/Users/5/permissions
        // Get all permissions for a specific user
        [HttpGet("{id}/permissions")]
        [AllowAnonymous] // Allow without auth for now
        public async Task<ActionResult<UserPermissionsDto>> GetUserPermissions(long id)
        {
            var user = await _context.Users
                .Include(u => u.Person)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r!.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound();

            // Build the response
            var result = new UserPermissionsDto
            {
                UserId = (int)user.Id,
                Username = user.Username,
                FullName = user.Person != null ? $"{user.Person.FirstName} {user.Person.SecondName} {user.Person.ThirdWithLastname}" : user.LoginName,
                Roles = new List<RoleWithPermissionsDto>(),
                AllPermissions = new List<string>()
            };

            // Process each role
            var allPermissions = new HashSet<string>();
            
            foreach (var userRole in user.UserRoles ?? Enumerable.Empty<UserRole>())
            {
                if (userRole.Role == null) continue;

                var roleDto = new RoleWithPermissionsDto
                {
                    RoleId = (int)userRole.Role.Id,
                    RoleName = userRole.Role.RoleName,
                    Permissions = new List<string>()
                };

                // Get all permissions for this role
                foreach (var rolePerm in userRole.Role.RolePermissions ?? Enumerable.Empty<RolePermission>())
                {
                    if (rolePerm.Permission != null && rolePerm.Permission.IsActive)
                    {
                        roleDto.Permissions.Add(rolePerm.Permission.PermissionName);
                        allPermissions.Add(rolePerm.Permission.PermissionName);
                    }
                }

                result.Roles.Add(roleDto);
            }

            result.AllPermissions = allPermissions.ToList();

            return Ok(result);
        }

        // DELETE: api/Users/5
        // Modified: 2025-12-15 - Added permission check
        // ملاحظة 18/12/2025:
        // تم توحيد اسم الصلاحية مع PermissionName = 'delete_user'.
        [HttpDelete("{id}")]
        [RequirePermission("delete_user")] // ← Requires 'delete_user' permission
        public async Task<IActionResult> DeleteUser(long id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Users/ChangePassword
        // تغيير كلمة المرور للمستخدم الحالي
        // Change password for current user
        [HttpPost("ChangePassword")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // الحصول على معرف المستخدم من Token
            // Get user ID from token
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long userId))
                return Unauthorized(new { message = "غير مصرح" });

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound(new { message = "المستخدم غير موجود" });

            // التحقق من كلمة المرور القديمة
            // Verify old password
            if (!PasswordHelper.VerifyPassword(dto.OldPassword, user.PasswordHash))
                return BadRequest(new { message = "كلمة المرور القديمة غير صحيحة" });

            // تحديث كلمة المرور
            // Update password
            user.PasswordHash = PasswordHelper.HashPassword(dto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "تم تغيير كلمة المرور بنجاح" });
        }
    }
}
