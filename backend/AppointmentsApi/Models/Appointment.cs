using System.ComponentModel.DataAnnotations;

namespace AppointmentsApi.Models
{
    public class Appointment
    {
        public Guid Id { get; set; }
        [Required] public Guid CustomerId { get; set; }
        [Required] public DateTime DateTime { get; set; }
        [Required]
        [RegularExpression("scheduled|done|cancelled")]
        public string Status { get; set; } = "scheduled";
        public Customer? Customer { get; set; }
    }
}
