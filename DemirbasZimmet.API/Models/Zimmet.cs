
namespace DemirbasZimmet.API.Models;

public class Zimmet
{
    public int ZimmetId { get; set; }

    public int PersonelId { get; set; }

    public int DemirbasId { get; set; }

    public DateTime TeslimTarihi { get; set; }
        = DateTime.Now;

    public DateTime? IadeTarihi { get; set; }

    public string? Aciklama { get; set; }

    public string? TeslimEden { get; set; }

    public string? TeslimAlan { get; set; }

    // İlişkili personel
    public Personel? Personel { get; set; }

    // İlişkili demirbaş
    public Demirbas? Demirbas { get; set; }
}
