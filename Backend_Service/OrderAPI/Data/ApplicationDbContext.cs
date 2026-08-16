using Microsoft.EntityFrameworkCore;
using OrderAPI.Models;

namespace OrderAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Order> Orders { get; set; } = null!;
    }
}
