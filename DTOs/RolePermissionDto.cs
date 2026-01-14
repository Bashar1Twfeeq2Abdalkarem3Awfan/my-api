using System;

namespace MyAPIv3.DTOs
{
    public class RolePermissionDto
    {
        public long RoleId { get; set; }
        public long PermissionId { get; set; }
        public DateTime? GrantedAt { get; set; }
        // Navigation
        public RoleDto? Role { get; set; }
        public PermissionDto? Permission { get; set; }
    }

    public class CreateRolePermissionDto
    {
        public long RoleId { get; set; }
        public long PermissionId { get; set; }
    }
}

