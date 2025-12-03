using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("new_details")]
    public class NewDetail
    {
        [Key]
        public int Id { get; set; }

        public int NewId { get; set; }
        [ForeignKey("NewId")]
        public virtual News News { get; set; }

        public int? OrderDetailId { get; set; }
        [ForeignKey("OrderDetailId")]
        public virtual OrderDetail OrderDetail { get; set; }

        public string DescriptionNews { get; set; }
        public string Proposal { get; set; }
        public string Solution { get; set; }

        public int? UserIdSolution { get; set; }
        [ForeignKey("UserIdSolution")]
        public virtual User UserSolution { get; set; }

        public int StateId { get; set; } // Estado de la incidencia
    }
}
