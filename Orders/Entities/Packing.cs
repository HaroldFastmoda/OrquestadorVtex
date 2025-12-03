using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("packing")]
    public class Packing
    {
        [Key]
        public int Id { get; set; }

        public int OrderDetailId { get; set; }
        [ForeignKey("OrderDetailId")]
        public virtual OrderDetail OrderDetail { get; set; }

        public int Units { get; set; }
    }
}
