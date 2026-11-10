namespace DemirbasZimmet.API.DTOs.Zimmet;

public class ZimmetResponseDto
{
    public int ZimmetId { get; set; }

    public int PersonelId { get; set; }

    public string PersonelAdSoyad { get; set; } = string.Empty;

    public int DemirbasId { get; set; }

    public string DemirbasAdi { get; set; } = string.Empty;

    public DateTime TeslimTarihi { get; set; }

    public DateTime? IadeTarihi { get; set; }

    public string? Aciklama { get; set; }

    public string? TeslimEden { get; set; }

    public string? TeslimAlan { get; set; }

    public string Durum { get; set; } = string.Empty;
}