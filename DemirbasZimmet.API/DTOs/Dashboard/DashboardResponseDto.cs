namespace DemirbasZimmet.API.DTOs.Dashboard;

public class DashboardResponseDto
{
    public int ToplamDemirbas { get; set; }
    public int KullanilabilirDemirbas { get; set; }
    public int ZimmetliDemirbas { get; set; }
    public int ArizaliDemirbas { get; set; }
    public int HurdaDemirbas { get; set; }

    public int AktifPersonel { get; set; }
    public int DevamEdenBakim { get; set; }

    public List<SonZimmetDto> SonZimmetler { get; set; } = new();
}