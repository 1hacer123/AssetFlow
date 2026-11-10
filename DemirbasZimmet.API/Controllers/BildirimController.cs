
using System.Security.Claims;
using DemirbasZimmet.API.Data;
using DemirbasZimmet.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DemirbasZimmet.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BildirimController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BildirimController(AppDbContext context)
        {
            _context = context;
        }

        private int? GirisYapanKullaniciId()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(id, out int kullaniciId))
                return kullaniciId;

            return null;
        }

        // GET: api/Bildirim
        [HttpGet]
        public async Task<IActionResult> GetBildirimler()
        {
            var kullaniciId = GirisYapanKullaniciId();

            if (kullaniciId == null)
                return Unauthorized();

            var bildirimler = await _context.Bildirimler
                .Where(b => b.KullaniciId == kullaniciId.Value)
                .OrderByDescending(b => b.OlusturmaTarihi)
                .Select(b => new BildirimResponseDto
                {
                    BildirimId = b.BildirimId,
                    Baslik = b.Baslik,
                    Mesaj = b.Mesaj,
                    Tur = b.Tur,
                    OkunduMu = b.OkunduMu,
                    OlusturmaTarihi = b.OlusturmaTarihi,

                    // İlgili kayıt bilgileri
                    IlgiliKayitTuru = b.IlgiliKayitTuru,
                    IlgiliKayitId = b.IlgiliKayitId
                })
                .ToListAsync();

            return Ok(bildirimler);
        }

        // GET: api/Bildirim/okunmamis-sayisi
        [HttpGet("okunmamis-sayisi")]
        public async Task<IActionResult> OkunmamisSayisi()
        {
            var kullaniciId = GirisYapanKullaniciId();

            if (kullaniciId == null)
                return Unauthorized();

            var sayi = await _context.Bildirimler
                .CountAsync(b =>
                    b.KullaniciId == kullaniciId.Value &&
                    !b.OkunduMu);

            return Ok(new { okunmamisSayisi = sayi });
        }

        // PATCH: api/Bildirim/5/okundu
        [HttpPatch("{id}/okundu")]
        public async Task<IActionResult> OkunduIsaretle(int id)
        {
            var kullaniciId = GirisYapanKullaniciId();

            if (kullaniciId == null)
                return Unauthorized();

            var bildirim = await _context.Bildirimler
                .FirstOrDefaultAsync(b =>
                    b.BildirimId == id &&
                    b.KullaniciId == kullaniciId.Value);

            if (bildirim == null)
                return NotFound(new { message = "Bildirim bulunamadı." });

            bildirim.OkunduMu = true;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Bildirim okundu olarak işaretlendi." });
        }

        // PATCH: api/Bildirim/okundu-tumu
        [HttpPatch("okundu-tumu")]
        public async Task<IActionResult> TumunuOkunduIsaretle()
        {
            var kullaniciId = GirisYapanKullaniciId();

            if (kullaniciId == null)
                return Unauthorized();

            var okunmamislar = await _context.Bildirimler
                .Where(b =>
                    b.KullaniciId == kullaniciId.Value &&
                    !b.OkunduMu)
                .ToListAsync();

            foreach (var bildirim in okunmamislar)
            {
                bildirim.OkunduMu = true;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Tüm bildirimler okundu olarak işaretlendi.",
                guncellenenSayisi = okunmamislar.Count
            });
        }
    }
}
