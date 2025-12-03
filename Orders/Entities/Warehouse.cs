using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("warehouse")]
    public class Warehouse
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Store { get; set; }

        public int WarehouseIdExt { get; set; } // ID externo de la bodega

        public virtual ICollection<Inventory> Inventories { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
