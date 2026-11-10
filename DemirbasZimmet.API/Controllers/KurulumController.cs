using System.Data;
using DemirbasZimmet.API.Data;
using DemirbasZimmet.API.DTOs;
using DemirbasZimmet.API.Models;
using DemirbasZimmet.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DemirbasZimmet.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class KurulumController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IParolaService _parolaService;

    public KurulumController(
        AppDbContext context,
        IParolaService parolaService)
    {
        _context = context;
        _parolaService = parolaService;
    }

    [HttpPost("ilk-yonetici")]
    public async Task<IActionResult> IlkYoneticiOlustur(
        [FromBody] IlkYoneticiOlusturDto dto)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        bool kullaniciVarMi = await _context.Kullanicilar.AnyAsync();

        if (kullaniciVarMi)
        {
            return Conflict(new
            {
                mesaj = "Sistemde zaten bir kullanıcı bulunuyor. İlk yönetici oluşturma işlemi kapalı."
            });
        }

        var yonetici = new Kullanici
        {
            AdSoyad = dto.AdSoyad,
            Eposta = dto.Eposta,
            Rol = KullaniciRolleri.Yonetici,
            AktifMi = true,
            OlusturmaTarihi = DateTime.Now
        };

        yonetici.ParolaHash =
            _parolaService.Hashle(yonetici, dto.Parola);

        _context.Kullanicilar.Add(yonetici);

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return Ok(new
        {
            mesaj = "İlk yönetici hesabı başarıyla oluşturuldu.",
            kullaniciId = yonetici.KullaniciId,
            adSoyad = yonetici.AdSoyad,
            eposta = yonetici.Eposta,
            rol = yonetici.Rol
        });
    }
}