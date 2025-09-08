using System.ComponentModel.DataAnnotations;

namespace AppointmentsApi.Models
{
    public class Customer
    {
        public Guid Id { get; set; }
        [Required, MaxLength(120)] public string Name { get; set; } = "";
        [EmailAddress] public string? Email { get; set; }
    }
}
