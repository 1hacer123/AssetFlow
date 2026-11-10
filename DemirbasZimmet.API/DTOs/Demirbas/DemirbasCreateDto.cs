using System.ComponentModel.DataAnnotations;

namespace DemirbasZimmet.API.DTOs.Demirbas;

public class DemirbasCreateDto
{
    [Required(ErrorMessage = "Demirbaş kodu zorunludur.")]
    [StringLength(50)]
    public string DemirbasKodu { get; set; } = string.Empty;

    [Required(ErrorMessage = "Demirbaş adı zorunludur.")]
    [StringLength(100)]
    public string Ad { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kategori zorunludur.")]
    [StringLength(50)]
    public string Kategori { get; set; } = string.Empty;

    [StringLength(100)]
    public string Marka { get; set; } = string.Empty;

    [StringLength(100)]
    public string Model { get; set; } = string.Empty;

    [StringLength(100)]
    public string SeriNo { get; set; } = string.Empty;

    public DateTime? SatinAlmaTarihi { get; set; }

    [Range(0, 999999999)]
    public decimal? SatinAlmaFiyati { get; set; }
}