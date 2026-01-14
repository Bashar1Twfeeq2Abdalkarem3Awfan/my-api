using System;

namespace MyAPIv3.DTOs
{
    public class PermissionDto
    {
        public long Id { get; set; }
        public string PermissionName { get; set; } = null!;
        public string? Category { get; set; }
        public string? Description { get; set; }
        public string? Module { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class CreatePermissionDto
    {
        public string PermissionName { get; set; } = null!;
        public string? Category { get; set; }
        public string? Description { get; set; }
        public string? Module { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdatePermissionDto
    {
        public string PermissionName { get; set; } = null!;
        public string? Category { get; set; }
        public string? Description { get; set; }
        public string? Module { get; set; }
        public bool IsActive { get; set; }
    }
}

