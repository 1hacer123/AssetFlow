namespace DemirbasZimmet.API.DTOs.Dashboard;

public class SonZimmetDto
{
    public int ZimmetId { get; set; }
    public string PersonelAdi { get; set; } = string.Empty;
    public string DemirbasAdi { get; set; } = string.Empty;
    public string DemirbasKodu { get; set; } = string.Empty;
    public DateTime TeslimTarihi { get; set; }
    public DateTime? IadeTarihi { get; set; }
    public string Durum { get; set; } = string.Empty;
}