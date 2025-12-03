using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("roles")]
    public class Role
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string RoleName { get; set; }

        public virtual ICollection<User> Users { get; set; }
        public virtual ICollection<RoleAsPermission> RolePermissions { get; set; }
    }
}
