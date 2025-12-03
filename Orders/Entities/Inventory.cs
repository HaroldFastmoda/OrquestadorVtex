using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("inventory")]
    public class Inventory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Sku { get; set; }

        [MaxLength(50)]
        public string Ean { get; set; }

        [MaxLength(200)]
        public string Name { get; set; }

        public int Units { get; set; }
        public decimal Price { get; set; }
        public int Reservation { get; set; }

        public int StateId { get; set; }
        [ForeignKey("StateId")]
        public virtual State State { get; set; }

        public int WarehouseId { get; set; }
        [ForeignKey("WarehouseId")]
        public virtual Warehouse Warehouse { get; set; }
    }
}
