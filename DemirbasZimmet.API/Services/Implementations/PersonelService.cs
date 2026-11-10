using DemirbasZimmet.API.Data;
using DemirbasZimmet.API.DTOs.Personel;
using DemirbasZimmet.API.Models;
using DemirbasZimmet.API.Services.Interfaces;
using DemirbasZimmet.API.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace DemirbasZimmet.API.Services.Implementations;

public class PersonelService : IPersonelService
{
    private readonly AppDbContext _context;

    public PersonelService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<PersonelResponseDto>> GetAllAsync()
    {
        return await _context.Personeller
            .Where(p => p.AktifMi)
            .Select(p => new PersonelResponseDto
            {
                PersonelId = p.PersonelId,
                Ad = p.Ad,
                Soyad = p.Soyad,
                Departman = p.Departman,
                Eposta = p.Eposta,
                AktifMi = p.AktifMi
            })
            .ToListAsync();
    }

    public async Task<PersonelResponseDto?> GetByIdAsync(int id)
    {
        return await _context.Personeller
            .Where(p => p.PersonelId == id && p.AktifMi)
            .Select(p => new PersonelResponseDto
            {
                PersonelId = p.PersonelId,
                Ad = p.Ad,
                Soyad = p.Soyad,
                Departman = p.Departman,
                Eposta = p.Eposta,
                AktifMi = p.AktifMi
            })
            .FirstOrDefaultAsync();
    }

    public async Task<PersonelResponseDto> CreateAsync(PersonelCreateDto dto)
    {
        string eposta = dto.Eposta.Trim();

        // Aynı e-posta adresine sahip personeli bul
        var mevcutPersonel = await _context.Personeller
            .FirstOrDefaultAsync(p => p.Eposta == eposta);

        // Kayıt varsa
        if (mevcutPersonel != null)
        {
            // Zaten aktifse tekrar eklenmesine izin verme
            if (mevcutPersonel.AktifMi)
            {
                throw new DuplicateEmailException(
                    "Bu e-posta adresi zaten kayıtlı.");
            }

            // Pasifse mevcut kaydı yeniden aktifleştir
            mevcutPersonel.Ad = dto.Ad.Trim();
            mevcutPersonel.Soyad = dto.Soyad.Trim();
            mevcutPersonel.Departman = dto.Departman.Trim();
            mevcutPersonel.AktifMi = true;

            await _context.SaveChangesAsync();

            return new PersonelResponseDto
            {
                PersonelId = mevcutPersonel.PersonelId,
                Ad = mevcutPersonel.Ad,
                Soyad = mevcutPersonel.Soyad,
                Departman = mevcutPersonel.Departman,
                Eposta = mevcutPersonel.Eposta,
                AktifMi = mevcutPersonel.AktifMi
            };
        }

        // Hiç kayıt yoksa yeni personel oluştur
        var personel = new Personel
        {
            Ad = dto.Ad.Trim(),
            Soyad = dto.Soyad.Trim(),
            Departman = dto.Departman.Trim(),
            Eposta = eposta,
            AktifMi = true
        };

        _context.Personeller.Add(personel);

        await _context.SaveChangesAsync();

        return new PersonelResponseDto
        {
            PersonelId = personel.PersonelId,
            Ad = personel.Ad,
            Soyad = personel.Soyad,
            Departman = personel.Departman,
            Eposta = personel.Eposta,
            AktifMi = personel.AktifMi
        };
    }

    public async Task<PersonelResponseDto?> UpdateAsync(
        int id, PersonelUpdateDto dto)
    {
        var personel = await _context.Personeller
            .FirstOrDefaultAsync(p => p.PersonelId == id && p.AktifMi);

        if (personel == null)
            return null;
        bool epostaVarMi = await _context.Personeller
            .AnyAsync(p => p.Eposta == dto.Eposta.Trim()
                && p.PersonelId != id);

        if (epostaVarMi)
            throw new DuplicateEmailException(
              "Bu e-posta adresi başka bir personel tarafından kullanılıyor.");

        personel.Ad = dto.Ad.Trim();
        personel.Soyad = dto.Soyad.Trim();
        personel.Departman = dto.Departman.Trim();
        personel.Eposta = dto.Eposta.Trim();

        await _context.SaveChangesAsync();

        return new PersonelResponseDto
        {
            PersonelId = personel.PersonelId,
            Ad = personel.Ad,
            Soyad = personel.Soyad,
            Departman = personel.Departman,
            Eposta = personel.Eposta,
            AktifMi = personel.AktifMi
        };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var personel = await _context.Personeller
            .FirstOrDefaultAsync(p => p.PersonelId == id && p.AktifMi);

        if (personel == null)
            return false;

        // Personelin aktif zimmeti var mı kontrol et
        bool aktifZimmetiVarMi = await _context.Zimmetler
            .AnyAsync(z => z.PersonelId == id && z.IadeTarihi == null);

        if (aktifZimmetiVarMi)
            throw new PersonelAktifZimmetException(
                "Aktif zimmeti bulunan personel pasife alınamaz. Önce zimmet iade edilmelidir.");

        // Aktif zimmet yoksa personeli pasife al
        personel.AktifMi = false;

        await _context.SaveChangesAsync();

        return true;
    }
}