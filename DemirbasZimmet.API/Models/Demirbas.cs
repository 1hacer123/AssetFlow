
namespace DemirbasZimmet.API.Models;

public enum DemirbasDurum
{
    Kullanilabilir = 1,
    Zimmetli = 2,
    Arizali = 3,
    Hurda = 4
}

public class Demirbas
{
    public int DemirbasId { get; set; }

    public string DemirbasKodu { get; set; } = string.Empty;

    public string Ad { get; set; } = string.Empty;

    public string Kategori { get; set; } = string.Empty;

    public string Marka { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public string SeriNo { get; set; } = string.Empty;

    public DateTime? SatinAlmaTarihi { get; set; }

    public decimal? SatinAlmaFiyati { get; set; }

    public DemirbasDurum Durum { get; set; }
        = DemirbasDurum.Kullanilabilir;

    // Demirbaşın zimmet geçmişi
    public ICollection<Zimmet> Zimmetler { get; set; }
        = new List<Zimmet>();
}
