using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("states")]
    public class State
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string StateName { get; set; } // Renombrado de 'state' para evitar confusión

        [MaxLength(50)]
        public string Type { get; set; }

        // Relaciones inversas (para navegación)
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        public virtual ICollection<Inventory> Inventories { get; set; }
    }
}
