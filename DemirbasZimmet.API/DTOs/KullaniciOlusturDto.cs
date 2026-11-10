
using System.ComponentModel.DataAnnotations;

namespace DemirbasZimmet.API.DTOs;

public class KullaniciOlusturDto
{
    [Required]
    [MaxLength(100)]
    public string AdSoyad { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Eposta { get; set; } = string.Empty;

    [Required]
    [MinLength(12)]
    public string Parola { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(Yonetici|BilgiIslem)$",
        ErrorMessage = "Rol Yonetici veya BilgiIslem olmalıdır.")]
    public string Rol { get; set; } = string.Empty;
}
