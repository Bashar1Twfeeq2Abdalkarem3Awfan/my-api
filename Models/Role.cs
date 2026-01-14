using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable enable

namespace MyAPIv3.Models
{
    [Table("role")]
    public class Role
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("role_name")]
        [MaxLength(100)]
        public string RoleName { get; set; } = null!;

        [Column("description")]
        public string? Description { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [InverseProperty(nameof(UserRole.Role))]
        public ICollection<UserRole>? UserRoles { get; set; }

        [InverseProperty(nameof(RolePermission.Role))]
        public ICollection<RolePermission>? RolePermissions { get; set; }
    }
}