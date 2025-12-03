using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("notifications")]
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }

        public int? UserId { get; set; } // Usuario que disparó o sistema
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public int OrderId { get; set; }
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }

        [MaxLength(50)]
        public string Type { get; set; } // WhatsApp, Email

        [Required]
        public string Message { get; set; }

        public string Response { get; set; } // JSON de respuesta de Twilio/Meta

        [MaxLength(50)]
        public string State { get; set; } // Enviado, Fallido
    }
}
