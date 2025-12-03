using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("customers")]
    public class Customer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Names { get; set; }

        [Required]
        [MaxLength(100)]
        public string Email { get; set; }

        [MaxLength(20)]
        public string Phone { get; set; }

        [MaxLength(250)]
        public string Address { get; set; }

        [MaxLength(100)]
        public string City { get; set; }

        [MaxLength(20)]
        public string CodeZipcode { get; set; }

        // Si este ID viene de VTEX, a veces es string, confirma si es int
        public string IdCustomerVtex { get; set; }

        public int StateId { get; set; }
        [ForeignKey("StateId")]
        public virtual State State { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
    }
}
