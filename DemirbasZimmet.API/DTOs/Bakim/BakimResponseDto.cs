namespace DemirbasZimmet.API.DTOs.Bakim;

public class BakimResponseDto
{
    public int BakimKaydiId { get; set; }

    public int DemirbasId { get; set; }

    public string DemirbasAdi { get; set; } = string.Empty;

    public string DemirbasKodu { get; set; } = string.Empty;

    public DateTime BaslangicTarihi { get; set; }

    public DateTime? BitisTarihi { get; set; }

    public string ArizaAciklamasi { get; set; } = string.Empty;

    public string? YapilanIslem { get; set; }

    public string? BakimiYapan { get; set; }

    public decimal? Maliyet { get; set; }

    public string Durum { get; set; } = string.Empty;
}