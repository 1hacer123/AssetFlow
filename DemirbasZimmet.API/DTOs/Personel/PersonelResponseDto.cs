namespace DemirbasZimmet.API.DTOs.Personel;

public class PersonelResponseDto
{
    public int PersonelId { get; set; }

    public string Ad { get; set; } = string.Empty;

    public string Soyad { get; set; } = string.Empty;

    public string Departman { get; set; } = string.Empty;

    public string Eposta { get; set; } = string.Empty;

    public bool AktifMi { get; set; }
}