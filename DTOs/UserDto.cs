using System;

namespace MyAPIv3.DTOs
{
    public class UserDto
    {
        public long Id { get; set; }
        public long PersonId { get; set; }
        public string Username { get; set; } = null!;
        public string? LoginName { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public PersonDto? Person { get; set; }
    }

    public class CreateUserDto
    {
        public long PersonId { get; set; }
        public string Username { get; set; } = null!;
        
        // ← تغيير: الآن نستلم password نصية،  سنشفرها في Controller
        // Changed: Now receiving plain password, will hash in Controller
        public string Password { get; set; } = null!;
        
        public string? LoginName { get; set; }
    }

    public class UpdateUserDto
    {
        public string Username { get; set; } = null!;
        public string? LoginName { get; set; }
        
        // ← إضافة جديدة: تغيير كلمة المرور (اختياري)
        // New: Change password (optional)
        public string? Password { get; set; }
    }

    // ← DTO جديد: Login
    // New DTO: Login
    public class LoginDto
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    // ← DTO جديد: Login Response
    // New DTO: Login Response
    public class LoginResponseDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public UserDto? User { get; set; }
        public string? Token { get; set; } // للمستقبل (JWT)
    }

    // ← DTO جديد: Change Password
    // New DTO: Change Password
    public class ChangePasswordDto
    {
        public string OldPassword { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }
}
