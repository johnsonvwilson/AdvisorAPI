using AdvisorAPI.Model;
using Microsoft.EntityFrameworkCore;

public class AdvisorDbContext : DbContext
{
    public DbSet<Advisor> Advisors { get; set; }

    public AdvisorDbContext(DbContextOptions<AdvisorDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Optionally configure model properties here
    }
}
