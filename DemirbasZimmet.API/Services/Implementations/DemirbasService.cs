using DemirbasZimmet.API.Data;
using DemirbasZimmet.API.DTOs.Demirbas;
using DemirbasZimmet.API.Exceptions;
using DemirbasZimmet.API.Models;
using DemirbasZimmet.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DemirbasZimmet.API.Services.Implementations;

public class DemirbasService : IDemirbasService
{
    private readonly AppDbContext _context;

    public DemirbasService(AppDbContext context)
    {
        _context = context;
    }

    // Demirbaşları listele
    public async Task<List<DemirbasResponseDto>> GetAllAsync()
    {
        return await _context.Demirbaslar
            .AsNoTracking()
            .Select(d => new DemirbasResponseDto
            {
                DemirbasId = d.DemirbasId,
                DemirbasKodu = d.DemirbasKodu,
                Ad = d.Ad,
                Kategori = d.Kategori,
                Marka = d.Marka,
                Model = d.Model,
                SeriNo = d.SeriNo,
                SatinAlmaTarihi = d.SatinAlmaTarihi,
                SatinAlmaFiyati = d.SatinAlmaFiyati,
                Durum = d.Durum
            })
            .ToListAsync();
    }

    // ID'ye göre demirbaş getir
    public async Task<DemirbasResponseDto?> GetByIdAsync(int id)
    {
        return await _context.Demirbaslar
            .AsNoTracking()
            .Where(d => d.DemirbasId == id)
            .Select(d => new DemirbasResponseDto
            {
                DemirbasId = d.DemirbasId,
                DemirbasKodu = d.DemirbasKodu,
                Ad = d.Ad,
                Kategori = d.Kategori,
                Marka = d.Marka,
                Model = d.Model,
                SeriNo = d.SeriNo,
                SatinAlmaTarihi = d.SatinAlmaTarihi,
                SatinAlmaFiyati = d.SatinAlmaFiyati,
                Durum = d.Durum
            })
            .FirstOrDefaultAsync();
    }

    // Yeni demirbaş oluştur
    public async Task<DemirbasResponseDto> CreateAsync(DemirbasCreateDto dto)
    {
        string kod = dto.DemirbasKodu.Trim();

        bool kodVarMi = await _context.Demirbaslar
            .AnyAsync(d => d.DemirbasKodu == kod);

        if (kodVarMi)
            throw new DuplicateAssetCodeException(
                "Bu demirbaş kodu zaten kullanılıyor.");

        var demirbas = new Demirbas
        {
            DemirbasKodu = kod,
            Ad = dto.Ad.Trim(),
            Kategori = dto.Kategori.Trim(),
            Marka = dto.Marka.Trim(),
            Model = dto.Model.Trim(),
            SeriNo = dto.SeriNo.Trim(),
            SatinAlmaTarihi = dto.SatinAlmaTarihi,
            SatinAlmaFiyati = dto.SatinAlmaFiyati,
            Durum = DemirbasDurum.Kullanilabilir
        };

        _context.Demirbaslar.Add(demirbas);
        await _context.SaveChangesAsync();

        return MapToDto(demirbas);
    }

    // Demirbaş bilgilerini güncelle
    public async Task<DemirbasResponseDto?> UpdateAsync(
        int id, DemirbasUpdateDto dto)
    {
        var demirbas = await _context.Demirbaslar
            .FirstOrDefaultAsync(d => d.DemirbasId == id);

        if (demirbas == null)
            return null;

        string kod = dto.DemirbasKodu.Trim();

        bool kodVarMi = await _context.Demirbaslar
            .AnyAsync(d => d.DemirbasKodu == kod
                        && d.DemirbasId != id);

        if (kodVarMi)
            throw new DuplicateAssetCodeException(
                "Bu demirbaş kodu başka bir demirbaş tarafından kullanılıyor.");

        demirbas.DemirbasKodu = kod;
        demirbas.Ad = dto.Ad.Trim();
        demirbas.Kategori = dto.Kategori.Trim();
        demirbas.Marka = dto.Marka.Trim();
        demirbas.Model = dto.Model.Trim();
        demirbas.SeriNo = dto.SeriNo.Trim();
        demirbas.SatinAlmaTarihi = dto.SatinAlmaTarihi;
        demirbas.SatinAlmaFiyati = dto.SatinAlmaFiyati;

        await _context.SaveChangesAsync();

        return MapToDto(demirbas);
    }

    // Demirbaş durumunu güncelle
    public async Task<DemirbasResponseDto?> UpdateStatusAsync(
        int id, DemirbasStatusUpdateDto dto)
    {
        var demirbas = await _context.Demirbaslar
            .FirstOrDefaultAsync(d => d.DemirbasId == id);

        if (demirbas == null)
            return null;

        if (!Enum.IsDefined(typeof(DemirbasDurum), dto.Durum))
            throw new InvalidAssetStatusException(
                "Geçersiz demirbaş durumu.");

        bool aktifZimmetVarMi = await _context.Zimmetler
            .AnyAsync(z => z.DemirbasId == id && z.IadeTarihi == null);

        if (aktifZimmetVarMi)
            throw new AssetHasActiveAssignmentException(
                "Aktif zimmeti bulunan demirbaşın durumu değiştirilemez.");

        // Zimmetli durumu yalnızca zimmet oluşturma işlemiyle verilebilir.
        if (dto.Durum == DemirbasDurum.Zimmetli)
            throw new InvalidAssetStatusException(
                "Demirbaşın zimmetli durumu yalnızca zimmet oluşturulurken belirlenebilir.");

        demirbas.Durum = dto.Durum;

        await _context.SaveChangesAsync();

        return MapToDto(demirbas);
    }

    // Demirbaşı sil
    public async Task<bool> DeleteAsync(int id)
    {
        var demirbas = await _context.Demirbaslar
            .FirstOrDefaultAsync(d => d.DemirbasId == id);

        if (demirbas == null)
            return false;

        bool zimmetGecmisiVarMi = await _context.Zimmetler
            .AnyAsync(z => z.DemirbasId == id);

        if (zimmetGecmisiVarMi)
            throw new AssetHasAssignmentHistoryException(
                "Zimmet geçmişi bulunan demirbaş silinemez.");

        _context.Demirbaslar.Remove(demirbas);
        await _context.SaveChangesAsync();

        return true;
    }

    // Entity -> Response DTO dönüşümü
    private static DemirbasResponseDto MapToDto(Demirbas d)
    {
        return new DemirbasResponseDto
        {
            DemirbasId = d.DemirbasId,
            DemirbasKodu = d.DemirbasKodu,
            Ad = d.Ad,
            Kategori = d.Kategori,
            Marka = d.Marka,
            Model = d.Model,
            SeriNo = d.SeriNo,
            SatinAlmaTarihi = d.SatinAlmaTarihi,
            SatinAlmaFiyati = d.SatinAlmaFiyati,
            Durum = d.Durum
        };
    }
}