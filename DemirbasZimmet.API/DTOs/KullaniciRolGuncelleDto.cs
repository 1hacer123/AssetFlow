
using System.ComponentModel.DataAnnotations;
using DemirbasZimmet.API.Models;

namespace DemirbasZimmet.API.DTOs;

public class KullaniciRolGuncelleDto
{
    [Required]
    [RegularExpression("^(Yonetici|BilgiIslem)$",
        ErrorMessage = "Rol Yonetici veya BilgiIslem olmalıdır.")]
    public string Rol { get; set; } = KullaniciRolleri.BilgiIslem;
}

