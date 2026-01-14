using System;

namespace MyAPIv3.DTOs
{
    public class UserRoleDto
    {
        public long UserId { get; set; }
        public long RoleId { get; set; }
        public DateTime? AssignedAt { get; set; }
        // Navigation
        public UserDto? User { get; set; }
        public RoleDto? Role { get; set; }
    }

    public class CreateUserRoleDto
    {
        public long UserId { get; set; }
        public long RoleId { get; set; }
    }
}

