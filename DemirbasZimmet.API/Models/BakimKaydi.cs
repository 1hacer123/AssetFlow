using System.ComponentModel.DataAnnotations;

namespace DemirbasZimmet.API.Models;

public class BakimKaydi
{
    public int BakimKaydiId { get; set; }

    public int DemirbasId { get; set; }

    public DateTime BaslangicTarihi { get; set; } = DateTime.Now;

    public DateTime? BitisTarihi { get; set; }

    [Required]
    [MaxLength(500)]
    public string ArizaAciklamasi { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? YapilanIslem { get; set; }

    [MaxLength(150)]
    public string? BakimiYapan { get; set; }

    public decimal? Maliyet { get; set; }

    public BakimDurum Durum { get; set; } = BakimDurum.DevamEdiyor;

    public Demirbas? Demirbas { get; set; }
}