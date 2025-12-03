using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("orders")]
    public class Order
    {
        [Key]
        public int Id { get; set; }

        // IDs Externos (VTEX)
        public string IdOrderVtex { get; set; }
        public string IdCustomerVtex { get; set; }

        // Estado original en texto de VTEX (ej: "ready-for-handling")
        [MaxLength(50)]
        public string StateVtex { get; set; }

        // Estado interno en TU base de datos (Orquestación)
        public int StateId { get; set; }
        [ForeignKey("StateId")]
        public virtual State CurrentState { get; set; }

        // Relación con tu tabla de clientes
        public int? CustomerId { get; set; } // Puede ser nulo si llega antes el pedido que el cliente
        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
        public virtual ICollection<News> NewsReports { get; set; }
    }
}
