using AppointmentsApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AppointmentsApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> opt) : base(opt) { }
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Appointment> Appointments => Set<Appointment>();
        protected override void OnModelCreating(ModelBuilder mb)
        {
            mb.Entity<Appointment>()
              .HasIndex(a => new { a.CustomerId, a.DateTime })
              .IsUnique(); // avoid double booking
        }
    }
}
