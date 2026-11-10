
using System.Security.Claims;

using DemirbasZimmet.API.Data;
using DemirbasZimmet.API.DTOs;
using DemirbasZimmet.API.Models;
using DemirbasZimmet.API.Services.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DemirbasZimmet.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = KullaniciRolleri.Yonetici)]
public class KullanicilarController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IParolaService _parolaService;

    public KullanicilarController(
        AppDbContext context,
        IParolaService parolaService)
    {
        _context = context;
        _parolaService = parolaService;
    }

    // GİRİŞ YAPMIŞ YÖNETİCİNİN ID'Sİ
    private int GirisYapanKullaniciId
    {
        get
        {
            int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out int id);

            return id;
        }
    }

    // 1. TÜM KULLANICILARI LİSTELE
    [HttpGet]
    public async Task<IActionResult> KullanicilariGetir()
    {
        var kullanicilar = await _context.Kullanicilar
            .AsNoTracking()
            .OrderBy(x => x.KullaniciId)
            .Select(x => new
            {
                x.KullaniciId,
                x.AdSoyad,
                x.Eposta,
                x.Rol,
                x.AktifMi,
                x.OlusturmaTarihi
            })
            .ToListAsync();

        return Ok(kullanicilar);
    }

    // 2. YENİ KULLANICI OLUŞTUR
    [HttpPost]
    public async Task<IActionResult> KullaniciOlustur(
        [FromBody] KullaniciOlusturDto dto)
    {
        string eposta = dto.Eposta.Trim();

        bool epostaVarMi = await _context.Kullanicilar
            .AnyAsync(x => x.Eposta == eposta);

        if (epostaVarMi)
        {
            return Conflict(new
            {
                mesaj = "Bu e-posta adresi zaten kayıtlı."
            });
        }

        var kullanici = new Kullanici
        {
            AdSoyad = dto.AdSoyad.Trim(),
            Eposta = eposta,
            Rol = dto.Rol,
            AktifMi = true,
            OlusturmaTarihi = DateTime.Now
        };

        kullanici.ParolaHash =
            _parolaService.Hashle(kullanici, dto.Parola);

        _context.Kullanicilar.Add(kullanici);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            mesaj = "Kullanıcı başarıyla oluşturuldu.",
            kullanici = new
            {
                kullanici.KullaniciId,
                kullanici.AdSoyad,
                kullanici.Eposta,
                kullanici.Rol,
                kullanici.AktifMi,
                kullanici.OlusturmaTarihi
            }
        });
    }

    // 3. KULLANICIYI AKTİF / PASİF YAP
    [HttpPatch("{id}/durum")]
    public async Task<IActionResult> KullaniciDurumGuncelle(
        int id,
        [FromBody] KullaniciDurumGuncelleDto dto)
    {
        var kullanici = await _context.Kullanicilar
            .FirstOrDefaultAsync(x => x.KullaniciId == id);

        if (kullanici == null)
        {
            return NotFound(new
            {
                mesaj = "Kullanıcı bulunamadı."
            });
        }

        // Yönetici kendi hesabını pasifleştiremez.
        if (id == GirisYapanKullaniciId && !dto.AktifMi)
        {
            return BadRequest(new
            {
                mesaj = "Kendi hesabınızı pasifleştiremezsiniz."
            });
        }

        // Son aktif yönetici pasifleştirilemez.
        if (!dto.AktifMi &&
            kullanici.AktifMi &&
            kullanici.Rol == KullaniciRolleri.Yonetici)
        {
            int aktifYoneticiSayisi =
                await _context.Kullanicilar.CountAsync(x =>
                    x.AktifMi &&
                    x.Rol == KullaniciRolleri.Yonetici);

            if (aktifYoneticiSayisi <= 1)
            {
                return BadRequest(new
                {
                    mesaj = "Sistemde en az bir aktif yönetici bulunmalıdır."
                });
            }
        }

        kullanici.AktifMi = dto.AktifMi;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            mesaj = dto.AktifMi
                ? "Kullanıcı hesabı aktifleştirildi."
                : "Kullanıcı hesabı pasifleştirildi.",
            kullaniciId = kullanici.KullaniciId,
            kullanici.AktifMi
        });
    }

    // 4. KULLANICI ROLÜNÜ DEĞİŞTİR
    [HttpPatch("{id}/rol")]
    public async Task<IActionResult> KullaniciRolGuncelle(
        int id,
        [FromBody] KullaniciRolGuncelleDto dto)
    {
        var kullanici = await _context.Kullanicilar
            .FirstOrDefaultAsync(x => x.KullaniciId == id);

        if (kullanici == null)
        {
            return NotFound(new
            {
                mesaj = "Kullanıcı bulunamadı."
            });
        }

        // Yönetici kendi rolünü değiştiremez.
        if (id == GirisYapanKullaniciId)
        {
            return BadRequest(new
            {
                mesaj = "Kendi rolünüzü değiştiremezsiniz."
            });
        }

        // Son aktif yöneticinin rolü kaldırılamaz.
        if (kullanici.AktifMi &&
            kullanici.Rol == KullaniciRolleri.Yonetici &&
            dto.Rol != KullaniciRolleri.Yonetici)
        {
            int aktifYoneticiSayisi =
                await _context.Kullanicilar.CountAsync(x =>
                    x.AktifMi &&
                    x.Rol == KullaniciRolleri.Yonetici);

            if (aktifYoneticiSayisi <= 1)
            {
                return BadRequest(new
                {
                    mesaj = "Sistemde en az bir aktif yönetici bulunmalıdır."
                });
            }
        }

        kullanici.Rol = dto.Rol;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            mesaj = "Kullanıcı rolü başarıyla güncellendi.",
            kullaniciId = kullanici.KullaniciId,
            kullanici.Rol
        });
    }
}
