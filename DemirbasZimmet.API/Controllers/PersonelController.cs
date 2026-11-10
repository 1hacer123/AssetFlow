using DemirbasZimmet.API.DTOs.Personel;
using DemirbasZimmet.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DemirbasZimmet.API.Models;

namespace DemirbasZimmet.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PersonelController : ControllerBase
{
    private readonly IPersonelService _personelService;

    public PersonelController(IPersonelService personelService)
    {
        _personelService = personelService;
    }

    // 1. Tüm aktif personelleri listele
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var personeller = await _personelService.GetAllAsync();

        return Ok(personeller);
    }

    // 2. ID'ye göre personel getir
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var personel = await _personelService.GetByIdAsync(id);

        if (personel == null)
            return NotFound("Personel bulunamadı.");

        return Ok(personel);
    }

    // 3. Yeni personel ekle
    [HttpPost]
    public async Task<IActionResult> Create(PersonelCreateDto dto)
    {
        

        var yeniPersonel = await _personelService.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = yeniPersonel.PersonelId },
            yeniPersonel
        );
    }

    // 4. Personel bilgilerini güncelle
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id, PersonelUpdateDto dto)
    {
      

        var guncelPersonel = await _personelService.UpdateAsync(id, dto);

        if (guncelPersonel == null)
            return NotFound("Personel bulunamadı.");

        return Ok(guncelPersonel);
    }

    // 5. Personeli pasife al
    [Authorize(Roles = KullaniciRolleri.Yonetici)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        bool sonuc = await _personelService.DeleteAsync(id);

        if (!sonuc)
            return NotFound("Personel bulunamadı.");

        return Ok("Personel pasife alındı.");
    }
}