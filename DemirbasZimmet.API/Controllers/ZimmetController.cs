
using DemirbasZimmet.API.DTOs.Zimmet;
using DemirbasZimmet.API.Services;
using DemirbasZimmet.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace DemirbasZimmet.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ZimmetController : ControllerBase
{
    private readonly IZimmetService _zimmetService;
    private readonly BildirimService _bildirimService;

    public ZimmetController(
        IZimmetService zimmetService,
        BildirimService bildirimService)
    {
        _zimmetService = zimmetService;
        _bildirimService = bildirimService;
    }

    // Tüm zimmetleri getir
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var zimmetler = await _zimmetService.GetAllAsync();
        return Ok(zimmetler);
    }

    // ID'ye göre zimmet getir
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var zimmet = await _zimmetService.GetByIdAsync(id);

        if (zimmet == null)
            return NotFound(new { message = "Zimmet kaydı bulunamadı." });

        return Ok(zimmet);
    }

    // Yeni zimmet oluştur
    [HttpPost]
    public async Task<IActionResult> Create(ZimmetCreateDto dto)
    {
        var zimmet = await _zimmetService.CreateAsync(dto);

        await _bildirimService.TumAktifKullanicilaraGonderAsync(
            "Yeni Zimmet Oluşturuldu",
            "Sistemde yeni bir demirbaş zimmet kaydı oluşturuldu.",
            "Bilgi"
        );

        return CreatedAtAction(
            nameof(GetById),
            new { id = zimmet.ZimmetId },
            zimmet
        );
    }


    // Demirbaş iadesi
    [HttpPut("{id}/iade")]
    public async Task<IActionResult> ReturnAsset(
        int id,
        ZimmetReturnDto dto)
    {
        var zimmet = await _zimmetService.ReturnAssetAsync(id, dto);

        if (zimmet == null)
            return NotFound(new { message = "Zimmet kaydı bulunamadı." });

        await _bildirimService.TumAktifKullanicilaraGonderAsync(
            "Demirbaş İade Edildi",
            "Bir demirbaşın zimmet iade işlemi tamamlandı.",
            "Bilgi"
        );

        return Ok(zimmet);
    }


    // Aktif zimmetleri getir
    [HttpGet("aktif")]
    public async Task<IActionResult> GetActive()
    {
        var zimmetler = await _zimmetService.GetActiveAsync();
        return Ok(zimmetler);
    }
}
