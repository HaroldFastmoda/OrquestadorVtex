using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("permissions")]
    public class Permission
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string PermissionName { get; set; }

        public virtual ICollection<RoleAsPermission> RolePermissions { get; set; }
    }
}
