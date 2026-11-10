using System.ComponentModel.DataAnnotations;
using DemirbasZimmet.API.Models;

namespace DemirbasZimmet.API.DTOs.Demirbas;

public class DemirbasStatusUpdateDto
{
    [Required]
    [EnumDataType(typeof(DemirbasDurum),
        ErrorMessage = "Geçersiz demirbaş durumu.")]
    public DemirbasDurum Durum { get; set; }
}