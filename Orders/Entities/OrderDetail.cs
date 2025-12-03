using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("orders_details")]
    public class OrderDetail
    {
        [Key]
        public int Id { get; set; }

        public int OrderId { get; set; }
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }

        [Required]
        [MaxLength(50)]
        public string Sku { get; set; }

        public int Units { get; set; }
        public decimal FinalPrice { get; set; } // Cambiado a decimal por ser dinero

        public int WarehouseId { get; set; }
        [ForeignKey("WarehouseId")]
        public virtual Warehouse Warehouse { get; set; }

        public int StateId { get; set; }
        [ForeignKey("StateId")]
        public virtual State State { get; set; }

        public virtual ICollection<Packing> Packings { get; set; }
    }
}
