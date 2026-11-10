using System.Security.Claims;
using DemirbasZimmet.API.Data;
using DemirbasZimmet.API.DTOs;
using DemirbasZimmet.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DemirbasZimmet.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HesapController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IParolaService _parolaService;

    public HesapController(
        AppDbContext context,
        IParolaService parolaService)
    {
        _context = context;
        _parolaService = parolaService;
    }

    [HttpPatch("sifre-degistir")]
    public async Task<IActionResult> SifreDegistir(
        [FromBody] SifreDegistirDto dto)
    {
        var kullaniciId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(kullaniciId, out int id))
        {
            return Unauthorized(new
            {
                mesaj = "Kullanıcı kimliği doğrulanamadı."
            });
        }

        var kullanici = await _context.Kullanicilar
            .FirstOrDefaultAsync(x =>
                x.KullaniciId == id && x.AktifMi);

        if (kullanici == null)
        {
            return Unauthorized(new
            {
                mesaj = "Aktif kullanıcı bulunamadı."
            });
        }

        if (!_parolaService.Dogrula(
            kullanici, dto.MevcutSifre))
        {
            return BadRequest(new
            {
                mesaj = "Mevcut şifreniz hatalı."
            });
        }

        if (_parolaService.Dogrula(
            kullanici, dto.YeniSifre))
        {
            return BadRequest(new
            {
                mesaj = "Yeni şifreniz mevcut şifrenizden farklı olmalıdır."
            });
        }

        kullanici.ParolaHash =
            _parolaService.Hashle(kullanici, dto.YeniSifre);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            mesaj = "Şifreniz başarıyla değiştirildi."
        });
    }
}