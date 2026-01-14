using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable enable

namespace MyAPIv3.Models
{
    [Table("role_permission")]
    public class RolePermission
    {
        [Column("role_id")]
        public long RoleId { get; set; }

        [Column("permission_id")]
        public long PermissionId { get; set; }

        [Column("granted_at")]
        public DateTime? GrantedAt { get; set; }

        [ForeignKey(nameof(RoleId))]
        [InverseProperty(nameof(Role.RolePermissions))]
        public Role? Role { get; set; }

        [ForeignKey(nameof(PermissionId))]
        [InverseProperty(nameof(Permission.RolePermissions))]
        public Permission? Permission { get; set; }
    }
}