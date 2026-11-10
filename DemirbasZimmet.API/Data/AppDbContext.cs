
using DemirbasZimmet.API.Models;
using Microsoft.EntityFrameworkCore;
namespace DemirbasZimmet.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Personel> Personeller { get; set; }

    public DbSet<Demirbas> Demirbaslar { get; set; }

    public DbSet<Zimmet> Zimmetler { get; set; }

    public DbSet<BakimKaydi> BakimKayitlari { get; set; }
    public DbSet<Kullanici> Kullanicilar { get; set; }

    public DbSet<Bildirim> Bildirimler { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<BakimKaydi>()
            .Property(b => b.Maliyet)
            .HasColumnType("decimal(18,2)");
        modelBuilder.Entity<BakimKaydi>()
             .HasIndex(b => b.DemirbasId)
             .IsUnique()
             .HasFilter("[Durum] = 1");

        modelBuilder.Entity<BakimKaydi>()
            .HasOne(b => b.Demirbas)
            .WithMany()
            .HasForeignKey(b => b.DemirbasId)
            .OnDelete(DeleteBehavior.Restrict);
        // Kullanıcı e-posta adresi benzersiz olmalı
        modelBuilder.Entity<Kullanici>()
            .HasIndex(k => k.Eposta)
            .IsUnique();

        // Personel e-posta adresi benzersiz olmalı
        modelBuilder.Entity<Personel>()
            .HasIndex(p => p.Eposta)
            .IsUnique();
        modelBuilder.Entity<Demirbas>()
            .Property(d => d.SatinAlmaFiyati)
            .HasColumnType("decimal(18,2)");
        // Demirbaş kodu benzersiz olmalı
        modelBuilder.Entity<Demirbas>()
            .HasIndex(d => d.DemirbasKodu)
            .IsUnique();
        // Bir demirbaşın aynı anda yalnızca bir aktif zimmeti olabilir
        modelBuilder.Entity<Zimmet>()
            .HasIndex(z => z.DemirbasId)
            .IsUnique()
            .HasFilter("[IadeTarihi] IS NULL");

        // Personel - Zimmet ilişkisi
        modelBuilder.Entity<Zimmet>()
            .HasOne(z => z.Personel)
            .WithMany(p => p.Zimmetler)
            .HasForeignKey(z => z.PersonelId)
            .OnDelete(DeleteBehavior.Restrict);

        // Demirbaş - Zimmet ilişkisi
        modelBuilder.Entity<Zimmet>()
            .HasOne(z => z.Demirbas)
            .WithMany(d => d.Zimmetler)
            .HasForeignKey(z => z.DemirbasId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Bildirim>()
            .HasOne(b => b.Kullanici)
            .WithMany()
            .HasForeignKey(b => b.KullaniciId)
            .OnDelete(DeleteBehavior.Cascade);

    }
}
