using System.ComponentModel.DataAnnotations;
using DemirbasZimmet.API.Models;

namespace DemirbasZimmet.API.DTOs.Bakim;

public class BakimTamamlaDto
{
    [Required(ErrorMessage = "Yapılan işlem açıklaması zorunludur.")]
    [StringLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir.")]
    public string YapilanIslem { get; set; } = string.Empty;

    [Range(0, 999999999, ErrorMessage = "Maliyet negatif olamaz.")]
    public decimal? Maliyet { get; set; }

    [EnumDataType(typeof(DemirbasDurum))]
    public DemirbasDurum TamamlanmaSonrasiDurum { get; set; }
}