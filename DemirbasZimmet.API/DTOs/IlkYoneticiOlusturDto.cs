using System.ComponentModel.DataAnnotations;

namespace DemirbasZimmet.API.DTOs;

public class IlkYoneticiOlusturDto
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
    [Compare(nameof(Parola), ErrorMessage = "Parolalar eşleşmiyor.")]
    public string ParolaTekrar { get; set; } = string.Empty;
}