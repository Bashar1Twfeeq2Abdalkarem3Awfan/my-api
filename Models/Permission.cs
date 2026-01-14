using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable enable

namespace MyAPIv3.Models
{
    [Table("permission")]
    public class Permission
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("permission_name")]
        [MaxLength(150)]
        public string PermissionName { get; set; } = null!;

        [Column("category")]
        [MaxLength(100)]
        public string? Category { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("module")]
        [MaxLength(100)]
        public string? Module { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [InverseProperty(nameof(RolePermission.Permission))]
        public ICollection<RolePermission>? RolePermissions { get; set; }
    }
}