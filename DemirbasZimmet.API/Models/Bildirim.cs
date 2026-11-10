
using System.ComponentModel.DataAnnotations;

namespace DemirbasZimmet.API.Models
{
    public class Bildirim
    {
        [Key]
        public int BildirimId { get; set; }

        [Required]
        public int KullaniciId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Baslik { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Mesaj { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Tur { get; set; } = "Bilgi";

        public bool OkunduMu { get; set; } = false;

        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

        // Bildirimin ilişkili olduğu kayıt
        [MaxLength(50)]
        public string? IlgiliKayitTuru { get; set; }

        public int? IlgiliKayitId { get; set; }

        public Kullanici? Kullanici { get; set; }
    }
}
