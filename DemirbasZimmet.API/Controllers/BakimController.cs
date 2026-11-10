
using DemirbasZimmet.API.DTOs.Bakim;
using DemirbasZimmet.API.Services;
using DemirbasZimmet.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace DemirbasZimmet.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BakimController : ControllerBase
{
    private readonly IBakimService _bakimService;
    private readonly BildirimService _bildirimService;

    public BakimController(
        IBakimService bakimService,
        BildirimService bildirimService)
    {
        _bakimService = bakimService;
        _bildirimService = bildirimService;
    }

    // Tüm bakım kayıtlarını getir
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var bakimlar = await _bakimService.GetAllAsync();
        return Ok(bakimlar);
    }

    // ID'ye göre bakım kaydı getir
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var bakim = await _bakimService.GetByIdAsync(id);

        if (bakim == null)
            return NotFound(new
            {
                message = "Bakım kaydı bulunamadı."
            });

        return Ok(bakim);
    }

    // Yeni bakım kaydı oluştur
    [HttpPost]
    public async Task<IActionResult> Create(BakimCreateDto dto)
    {
        var bakim = await _bakimService.CreateAsync(dto);

        await _bildirimService.TumAktifKullanicilaraGonderAsync(
            "Yeni Bakım Kaydı Oluşturuldu",
            "Bir demirbaş için yeni bakım kaydı oluşturuldu.",
            "Bilgi"
        );

        return CreatedAtAction(
            nameof(GetById),
            new { id = bakim.BakimKaydiId },
            bakim
        );
    }

    // Bakım işlemini tamamla
    [HttpPut("{id}/tamamla")]
    public async Task<IActionResult> Complete(
        int id,
        BakimTamamlaDto dto)
    {
        var bakim = await _bakimService.CompleteAsync(id, dto);

        if (bakim == null)
            return NotFound(new
            {
                message = "Bakım kaydı bulunamadı."
            });

        await _bildirimService.TumAktifKullanicilaraGonderAsync(
            "Bakım İşlemi Tamamlandı",
            "Bir demirbaşın bakım işlemi başarıyla tamamlandı.",
            "Basari"
        );

        return Ok(bakim);
    }

    // Belirli bir demirbaşın bakım kayıtlarını getir
    [HttpGet("demirbas/{demirbasId}")]
    public async Task<IActionResult> GetByDemirbasId(int demirbasId)
    {
        if (demirbasId <= 0)
            return BadRequest(new
            {
                message = "Geçerli bir demirbaş ID girilmelidir."
            });

        var bakimlar = await _bakimService.GetByDemirbasIdAsync(demirbasId);

        return Ok(bakimlar);
    }
}
