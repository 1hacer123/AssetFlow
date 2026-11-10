
namespace DemirbasZimmet.API.Models;

public class Personel
{
    public int PersonelId { get; set; }

    public string Ad { get; set; } = string.Empty;

    public string Soyad { get; set; } = string.Empty;

    public string Departman { get; set; } = string.Empty;

    public string Eposta { get; set; } = string.Empty;

    public bool AktifMi { get; set; } = true;

    // Personelin zimmet geçmişi
    public ICollection<Zimmet> Zimmetler { get; set; }
        = new List<Zimmet>();
}
