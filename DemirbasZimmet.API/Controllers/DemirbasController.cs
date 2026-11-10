
using DemirbasZimmet.API.DTOs.Demirbas;
using DemirbasZimmet.API.Services.Interfaces;
using DemirbasZimmet.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DemirbasZimmet.API.Models;

namespace DemirbasZimmet.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DemirbasController : ControllerBase
{
    private readonly IDemirbasService _demirbasService;
    private readonly BildirimService _bildirimService;

    public DemirbasController(
        IDemirbasService demirbasService,
        BildirimService bildirimService)
    {
        _demirbasService = demirbasService;
        _bildirimService = bildirimService;
    }

    // 1. Tüm demirbaşları listele
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var demirbaslar = await _demirbasService.GetAllAsync();
        return Ok(demirbaslar);
    }

    // 2. ID'ye göre demirbaş getir
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var demirbas = await _demirbasService.GetByIdAsync(id);

        if (demirbas == null)
            return NotFound("Demirbaş bulunamadı.");

        return Ok(demirbas);
    }


    // 3. Yeni demirbaş ekle
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] DemirbasCreateDto dto)
    {
        // Demirbaşı veritabanına kaydet
        var demirbas = await _demirbasService.CreateAsync(dto);

        // Yeni demirbaş için aktif kullanıcılara bildirim gönder
        await _bildirimService.TumAktifKullanicilaraGonderAsync(
            "Yeni Demirbaş Eklendi",
            $"{demirbas.Ad} ({demirbas.DemirbasKodu}) sisteme eklendi.",
            "Basari",
            "Demirbas",
            demirbas.DemirbasId
        );

        return CreatedAtAction(
            nameof(GetById),
            new { id = demirbas.DemirbasId },
            demirbas
        );
    }


    // 4. Demirbaş bilgilerini güncelle
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] DemirbasUpdateDto dto)
    {
        var demirbas = await _demirbasService.UpdateAsync(id, dto);

        if (demirbas == null)
            return NotFound("Demirbaş bulunamadı.");

        return Ok(demirbas);
    }

    // 5. Demirbaş durumunu güncelle
    [HttpPatch("{id}/durum")]
    public async Task<IActionResult> UpdateStatus(
        int id,
        [FromBody] DemirbasStatusUpdateDto dto)
    {
        var demirbas = await _demirbasService.UpdateStatusAsync(id, dto);

        if (demirbas == null)
            return NotFound("Demirbaş bulunamadı.");

        return Ok(demirbas);
    }

    // 6. Demirbaşı sil
    [Authorize(Roles = KullaniciRolleri.Yonetici)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        bool silindi = await _demirbasService.DeleteAsync(id);

        if (!silindi)
            return NotFound("Demirbaş bulunamadı.");

        return Ok(new { message = "Demirbaş silindi." });
    }
}
