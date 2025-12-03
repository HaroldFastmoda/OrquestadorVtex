using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("news")]
    public class News
    {
        [Key]
        public int Id { get; set; }

        public int OrderId { get; set; }
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }

        public int StateId { get; set; }
        [ForeignKey("StateId")]
        public virtual State State { get; set; }

        public int? UserIdReport { get; set; } // Usuario que reportó o NULL si fue sistema
        [ForeignKey("UserIdReport")]
        public virtual User UserReport { get; set; }

        public virtual ICollection<NewDetail> NewDetails { get; set; }
    }
}
