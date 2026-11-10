
using System.ComponentModel.DataAnnotations;

namespace DemirbasZimmet.API.DTOs;

public class GirisDto
{
    [Required]
    [EmailAddress]
    public string Eposta { get; set; } = string.Empty;

    [Required]
    public string Parola { get; set; } = string.Empty;
}
