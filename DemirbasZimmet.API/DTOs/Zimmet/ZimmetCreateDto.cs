using System.ComponentModel.DataAnnotations;

namespace DemirbasZimmet.API.DTOs.Zimmet;

public class ZimmetCreateDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Geçerli bir personel seçilmelidir.")]
    public int PersonelId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Geçerli bir demirbaş seçilmelidir.")]
    public int DemirbasId { get; set; }

    [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
    public string? Aciklama { get; set; }

    [StringLength(100)]
    public string? TeslimEden { get; set; }

    [StringLength(100)]
    public string? TeslimAlan { get; set; }
}