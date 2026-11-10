using DemirbasZimmet.API.Models;

namespace DemirbasZimmet.API.DTOs.Demirbas;

public class DemirbasResponseDto
{
    public int DemirbasId { get; set; }

    public string DemirbasKodu { get; set; } = string.Empty;

    public string Ad { get; set; } = string.Empty;

    public string Kategori { get; set; } = string.Empty;

    public string Marka { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public string SeriNo { get; set; } = string.Empty;

    public DateTime? SatinAlmaTarihi { get; set; }

    public decimal? SatinAlmaFiyati { get; set; }

    public DemirbasDurum Durum { get; set; }
}