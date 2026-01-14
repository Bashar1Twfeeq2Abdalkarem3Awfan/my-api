using System.Collections.Generic;

namespace MyAPIv3.DTOs
{
    public class UserPermissionsDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public List<RoleWithPermissionsDto> Roles { get; set; } = new();
        public List<string> AllPermissions { get; set; } = new();
    }

    public class RoleWithPermissionsDto
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public List<string> Permissions { get; set; } = new();
    }
}
