using ApiSample.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiSample
{
  public sealed class ApplicationDbContext : DbContext
  {
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
      : base(options) { }

    public DbSet<Summary> Summaries => Set<Summary>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      ArgumentNullException.ThrowIfNull(modelBuilder);

      var summaryBuilder = modelBuilder.Entity<Summary>();

      summaryBuilder.HasKey(s => s.Id);

      summaryBuilder
        .Property(s => s.Id)
        .ValueGeneratedOnAdd()
        .HasDefaultValueSql("NEWSEQUENTIALID()");

      summaryBuilder.Property(s => s.Value).IsRequired().HasMaxLength(20);
      summaryBuilder.HasIndex(s => s.Value).IsUnique();
    }
  }
}
