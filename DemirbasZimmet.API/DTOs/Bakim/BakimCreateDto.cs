using System.ComponentModel.DataAnnotations;

namespace DemirbasZimmet.API.DTOs.Bakim;

public class BakimCreateDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Geçerli bir demirbaş seçilmelidir.")]
    public int DemirbasId { get; set; }

    [Required(ErrorMessage = "Arıza açıklaması zorunludur.")]
    [StringLength(500, ErrorMessage = "Arıza açıklaması en fazla 500 karakter olabilir.")]
    public string ArizaAciklamasi { get; set; } = string.Empty;

    [StringLength(150)]
    public string? BakimiYapan { get; set; }

    public decimal? Maliyet { get; set; }
}