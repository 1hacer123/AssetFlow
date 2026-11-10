using DemirbasZimmet.API.Data;
using DemirbasZimmet.API.DTOs.Bakim;
using DemirbasZimmet.API.Exceptions;
using DemirbasZimmet.API.Models;
using DemirbasZimmet.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DemirbasZimmet.API.Services.Implementations;

public class BakimService : IBakimService
{
    private readonly AppDbContext _context;

    public BakimService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<BakimResponseDto>> GetAllAsync()
    {
        return await _context.BakimKayitlari
            .AsNoTracking()
            .OrderByDescending(b => b.BaslangicTarihi)
            .Select(b => new BakimResponseDto
            {
                BakimKaydiId = b.BakimKaydiId,
                DemirbasId = b.DemirbasId,
                DemirbasAdi = b.Demirbas!.Ad,
                DemirbasKodu = b.Demirbas.DemirbasKodu,
                BaslangicTarihi = b.BaslangicTarihi,
                BitisTarihi = b.BitisTarihi,
                ArizaAciklamasi = b.ArizaAciklamasi,
                YapilanIslem = b.YapilanIslem,
                BakimiYapan = b.BakimiYapan,
                Maliyet = b.Maliyet,
                Durum = b.Durum.ToString()
            })
            .ToListAsync();
    }

    public async Task<BakimResponseDto?> GetByIdAsync(int id)
    {
        return await _context.BakimKayitlari
            .AsNoTracking()
            .Where(b => b.BakimKaydiId == id)
            .Select(b => new BakimResponseDto
            {
                BakimKaydiId = b.BakimKaydiId,
                DemirbasId = b.DemirbasId,
                DemirbasAdi = b.Demirbas!.Ad,
                DemirbasKodu = b.Demirbas.DemirbasKodu,
                BaslangicTarihi = b.BaslangicTarihi,
                BitisTarihi = b.BitisTarihi,
                ArizaAciklamasi = b.ArizaAciklamasi,
                YapilanIslem = b.YapilanIslem,
                BakimiYapan = b.BakimiYapan,
                Maliyet = b.Maliyet,
                Durum = b.Durum.ToString()
            })
            .FirstOrDefaultAsync();
    }
    public async Task<List<BakimResponseDto>> GetByDemirbasIdAsync(
    int demirbasId)
    {
        return await _context.BakimKayitlari
            .AsNoTracking()
            .Where(b => b.DemirbasId == demirbasId)
            .OrderByDescending(b => b.BaslangicTarihi)
            .Select(b => new BakimResponseDto
            {
                BakimKaydiId = b.BakimKaydiId,
                DemirbasId = b.DemirbasId,
                DemirbasAdi = b.Demirbas!.Ad,
                DemirbasKodu = b.Demirbas.DemirbasKodu,
                BaslangicTarihi = b.BaslangicTarihi,
                BitisTarihi = b.BitisTarihi,
                ArizaAciklamasi = b.ArizaAciklamasi,
                YapilanIslem = b.YapilanIslem,
                BakimiYapan = b.BakimiYapan,
                Maliyet = b.Maliyet,
                Durum = b.Durum.ToString()
            })
            .ToListAsync();
    }

    public async Task<BakimResponseDto> CreateAsync(BakimCreateDto dto)
    {
        var demirbas = await _context.Demirbaslar
            .FirstOrDefaultAsync(d => d.DemirbasId == dto.DemirbasId);

        if (demirbas == null)
            throw new ZimmetIliskiBulunamadiException(
                "Bakım yapılacak demirbaş bulunamadı.");

        if (demirbas.Durum == DemirbasDurum.Zimmetli ||
            demirbas.Durum == DemirbasDurum.Hurda)
        {
            throw new ZimmetIslemException(
                "Zimmetli veya hurda demirbaş için bakım kaydı açılamaz.");
        }

        bool aktifBakimVar = await _context.BakimKayitlari
            .AnyAsync(b =>
                b.DemirbasId == dto.DemirbasId &&
                b.Durum == BakimDurum.DevamEdiyor);

        if (aktifBakimVar)
        {
            throw new ZimmetIslemException(
                "Bu demirbaşın devam eden bir bakım kaydı zaten var.");
        }

        if (dto.Maliyet < 0)
            throw new InvalidAssetStatusException(
                "Bakım maliyeti negatif olamaz.");

        var bakim = new BakimKaydi
        {
            DemirbasId = dto.DemirbasId,
            ArizaAciklamasi = dto.ArizaAciklamasi.Trim(),
            BakimiYapan = dto.BakimiYapan?.Trim(),
            Maliyet = dto.Maliyet,
            Durum = BakimDurum.DevamEdiyor
        };

        demirbas.Durum = DemirbasDurum.Arizali;

        _context.BakimKayitlari.Add(bakim);

        await _context.SaveChangesAsync();

        return (await GetByIdAsync(bakim.BakimKaydiId))!;
    }

    public async Task<BakimResponseDto?> CompleteAsync(
        int id,
        BakimTamamlaDto dto)
    {
        var bakim = await _context.BakimKayitlari
            .Include(b => b.Demirbas)
            .FirstOrDefaultAsync(b => b.BakimKaydiId == id);

        if (bakim == null)
            return null;

        if (bakim.Durum == BakimDurum.Tamamlandi)
        {
            throw new ZimmetIslemException(
                "Bu bakım kaydı zaten tamamlanmış.");
        }

        if (string.IsNullOrWhiteSpace(dto.YapilanIslem))
        {
            throw new ZimmetIslemException(
                "Yapılan işlem açıklaması boş olamaz.");
        }

        if (dto.Maliyet < 0)
        {
            throw new InvalidAssetStatusException(
                "Bakım maliyeti negatif olamaz.");
        }

        if (dto.TamamlanmaSonrasiDurum != DemirbasDurum.Kullanilabilir &&
            dto.TamamlanmaSonrasiDurum != DemirbasDurum.Hurda)
        {
            throw new InvalidAssetStatusException(
                "Bakım sonrasında demirbaş yalnızca Kullanılabilir veya Hurda olabilir.");
        }

        bakim.YapilanIslem = dto.YapilanIslem.Trim();
        bakim.Maliyet = dto.Maliyet;
        bakim.BitisTarihi = DateTime.Now;
        bakim.Durum = BakimDurum.Tamamlandi;

        bakim.Demirbas!.Durum = dto.TamamlanmaSonrasiDurum;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(id);
    }
}