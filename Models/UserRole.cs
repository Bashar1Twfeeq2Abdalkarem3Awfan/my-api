using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable enable

namespace MyAPIv3.Models
{
    [Table("user_role")]
    public class UserRole
    {
        [Column("user_id")]
        public long UserId { get; set; }

        [Column("role_id")]
        public long RoleId { get; set; }

        [Column("assigned_at")]
        public DateTime? AssignedAt { get; set; }

        [ForeignKey(nameof(UserId))]
        [InverseProperty(nameof(User.UserRoles))]
        public User? User { get; set; }

        [ForeignKey(nameof(RoleId))]
        [InverseProperty(nameof(Role.UserRoles))]
        public Role? Role { get; set; }
    }
}