
using DemirbasZimmet.API.Data;
using DemirbasZimmet.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DemirbasZimmet.API.Services
{
    public class BildirimService
    {
        private readonly AppDbContext _context;

        public BildirimService(AppDbContext context)
        {
            _context = context;
        }

        public async Task TumAktifKullanicilaraGonderAsync(
            string baslik,
            string mesaj,
            string tur = "Bilgi",
            string? ilgiliKayitTuru = null,
            int? ilgiliKayitId = null)
        {
            var kullanicilar = await _context.Kullanicilar
                .Where(k => k.AktifMi)
                .Select(k => k.KullaniciId)
                .ToListAsync();

            var bildirimler = kullanicilar.Select(id => new Bildirim
            {
                KullaniciId = id,
                Baslik = baslik,
                Mesaj = mesaj,
                Tur = tur,
                OkunduMu = false,
                OlusturmaTarihi = DateTime.Now,
                IlgiliKayitTuru = ilgiliKayitTuru,
                IlgiliKayitId = ilgiliKayitId
            }).ToList();

            if (bildirimler.Count == 0)
                return;

            await _context.Bildirimler.AddRangeAsync(bildirimler);
            await _context.SaveChangesAsync();
        }
    }
}
