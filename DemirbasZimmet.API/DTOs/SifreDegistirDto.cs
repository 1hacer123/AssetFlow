using System.ComponentModel.DataAnnotations;

namespace DemirbasZimmet.API.DTOs;

public class SifreDegistirDto
{
    [Required(ErrorMessage = "Mevcut şifre zorunludur.")]
    public string MevcutSifre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Yeni şifre zorunludur.")]
    [MinLength(8, ErrorMessage = "Yeni şifre en az 8 karakter olmalıdır.")]
    public string YeniSifre { get; set; } = string.Empty;
}