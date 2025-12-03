using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("audit")]
    public class Audit
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(100)]
        public string TableName { get; set; } // 'table' es palabra reservada en SQL

        public int IdTable { get; set; }

        [MaxLength(50)]
        public string Action { get; set; } // INSERT, UPDATE

        public DateTime Datetime { get; set; }

        public int? UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public string Payload { get; set; } // JSON con los datos cambiados
    }
}
