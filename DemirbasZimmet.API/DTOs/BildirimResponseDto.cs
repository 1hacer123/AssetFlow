
namespace DemirbasZimmet.API.DTOs
{
    public class BildirimResponseDto
    {
        public int BildirimId { get; set; }
        public string Baslik { get; set; } = string.Empty;
        public string Mesaj { get; set; } = string.Empty;
        public string Tur { get; set; } = string.Empty;
        public bool OkunduMu { get; set; }
        public DateTime OlusturmaTarihi { get; set; }

        public string? IlgiliKayitTuru { get; set; }
        public int? IlgiliKayitId { get; set; }
    }
}
