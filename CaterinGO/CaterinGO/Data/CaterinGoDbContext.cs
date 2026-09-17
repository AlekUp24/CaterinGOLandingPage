using Microsoft.EntityFrameworkCore;

namespace CaterinGO.Data;

public sealed class CaterinGoDbContext(DbContextOptions<CaterinGoDbContext> options) : DbContext(options)
{
    public DbSet<WaitingListEntry> WaitingListEntries => Set<WaitingListEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<WaitingListEntry>();

        entity.ToTable("waiting_list");
        entity.HasKey(entry => entry.Id);
        entity.Property(entry => entry.Email)
            .HasMaxLength(320)
            .IsRequired();
        entity.Property(entry => entry.DateOfEntry)
            .HasColumnType("timestamp with time zone")
            .IsRequired();
        entity.HasIndex(entry => entry.Email)
            .IsUnique();
    }
}
