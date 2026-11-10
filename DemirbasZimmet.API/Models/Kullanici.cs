using System.ComponentModel.DataAnnotations;

namespace DemirbasZimmet.API.Models;

public class Kullanici
{
    public int KullaniciId { get; set; }

    [Required]
    [MaxLength(100)]
    public string AdSoyad { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    [EmailAddress]
    public string Eposta { get; set; } = string.Empty;

    [Required]
    public string ParolaHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string Rol { get; set; } = string.Empty;

    public bool AktifMi { get; set; } = true;

    public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
}