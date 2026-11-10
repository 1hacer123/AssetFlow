using System.ComponentModel.DataAnnotations;

namespace DemirbasZimmet.API.DTOs.Personel;

public class PersonelCreateDto
{
    [Required(ErrorMessage = "Ad alanı zorunludur.")]
    [StringLength(50, MinimumLength = 2,
        ErrorMessage = "Ad 2-50 karakter arasında olmalıdır.")]
    public string Ad { get; set; } = string.Empty;

    [Required(ErrorMessage = "Soyad alanı zorunludur.")]
    [StringLength(50, MinimumLength = 2,
        ErrorMessage = "Soyad 2-50 karakter arasında olmalıdır.")]
    public string Soyad { get; set; } = string.Empty;

    [Required(ErrorMessage = "Departman alanı zorunludur.")]
    [StringLength(100, ErrorMessage = "Departman en fazla 100 karakter olabilir.")]
    public string Departman { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-posta alanı zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [StringLength(150, ErrorMessage = "E-posta en fazla 150 karakter olabilir.")]
    public string Eposta { get; set; } = string.Empty;
}