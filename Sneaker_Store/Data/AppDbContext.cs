using Microsoft.EntityFrameworkCore;
using Sneaker_Store.Model;

namespace Sneaker_Store.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Sko> Sko => Set<Sko>();
    public DbSet<Kunde> Kunder => Set<Kunde>();
    public DbSet<Ordre> Ordrer => Set<Ordre>();
    public DbSet<Kvittering> Kvitteringer => Set<Kvittering>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Sko>(b =>
        {
            b.HasKey(s => s.SkoId);
            b.Property(s => s.SkoId).ValueGeneratedOnAdd();
            b.Property(s => s.Maerke).IsRequired().HasMaxLength(100);
            b.Property(s => s.Model).IsRequired().HasMaxLength(100);
            b.Property(s => s.Billede).HasMaxLength(500);

            b.HasData(
                new Sko(1, "Nike", "Air Max", 44, 999, 12, "https://picsum.photos/seed/nike-air-max/400/300"),
                new Sko(2, "Asics", "Gel-1130", 38, 850, 5, "https://picsum.photos/seed/asics-gel-1130/400/300"),
                new Sko(3, "Adidas", "Campus", 42, 700, 1, "https://picsum.photos/seed/adidas-campus/400/300"),
                new Sko(4, "Asics", "Gel-Kayano", 44, 999, 0, "https://picsum.photos/seed/asics-gel-kayano/400/300"),
                new Sko(5, "New Balance", "530", 40, 799, 20, "https://picsum.photos/seed/new-balance-530/400/300"),
                new Sko(6, "Puma", "Suede Classic", 43, 599, 8, "https://picsum.photos/seed/puma-suede-classic/400/300")
            );
        });

        modelBuilder.Entity<Kunde>(b =>
        {
            b.HasKey(k => k.KundeId);
            b.Property(k => k.KundeId).ValueGeneratedOnAdd();
            b.Property(k => k.Email).IsRequired().HasMaxLength(256);
            b.HasIndex(k => k.Email).IsUnique();
            b.Property(k => k.Navn).IsRequired().HasMaxLength(100);
            b.Property(k => k.Efternavn).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<Ordre>(b =>
        {
            b.HasKey(o => o.OrdreId);
            b.Property(o => o.OrdreId).ValueGeneratedOnAdd();
            b.HasOne<Kunde>().WithMany().HasForeignKey(o => o.KundeId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne<Sko>().WithMany().HasForeignKey(o => o.SkoId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Kvittering>(b =>
        {
            b.HasKey(k => k.Id);
            b.Property(k => k.Id).ValueGeneratedOnAdd();
            b.Property(k => k.Beskrivelse).HasMaxLength(500);
            b.HasOne<Kunde>().WithMany().HasForeignKey(k => k.KundeId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
