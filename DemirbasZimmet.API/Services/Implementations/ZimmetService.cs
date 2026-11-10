using DemirbasZimmet.API.Data;
using DemirbasZimmet.API.DTOs.Zimmet;
using DemirbasZimmet.API.Models;
using DemirbasZimmet.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using DemirbasZimmet.API.Exceptions;

namespace DemirbasZimmet.API.Services.Implementations;

public class ZimmetService : IZimmetService
{
    private readonly AppDbContext _context;

    public ZimmetService(AppDbContext context)
    {
        _context = context;
    }

    // Tüm zimmet geçmişini listele
    public async Task<List<ZimmetResponseDto>> GetAllAsync()
    {
        return await _context.Zimmetler
            .AsNoTracking()
            .Select(z => new ZimmetResponseDto
            {
                ZimmetId = z.ZimmetId,
                PersonelId = z.PersonelId,
                PersonelAdSoyad = z.Personel!.Ad + " " + z.Personel.Soyad,
                DemirbasId = z.DemirbasId,
                DemirbasAdi = z.Demirbas!.Ad,
                TeslimTarihi = z.TeslimTarihi,
                IadeTarihi = z.IadeTarihi,
                Aciklama = z.Aciklama,
                TeslimEden = z.TeslimEden,
                TeslimAlan = z.TeslimAlan,
                Durum = z.IadeTarihi == null ? "Aktif" : "İade Edildi"
            })
            .ToListAsync();
    }

    // ID'ye göre zimmet getir
    public async Task<ZimmetResponseDto?> GetByIdAsync(int id)
    {
        return await _context.Zimmetler
            .AsNoTracking()
            .Where(z => z.ZimmetId == id)
            .Select(z => new ZimmetResponseDto
            {
                ZimmetId = z.ZimmetId,
                PersonelId = z.PersonelId,
                PersonelAdSoyad = z.Personel!.Ad + " " + z.Personel.Soyad,
                DemirbasId = z.DemirbasId,
                DemirbasAdi = z.Demirbas!.Ad,
                TeslimTarihi = z.TeslimTarihi,
                IadeTarihi = z.IadeTarihi,
                Aciklama = z.Aciklama,
                TeslimEden = z.TeslimEden,
                TeslimAlan = z.TeslimAlan,
                Durum = z.IadeTarihi == null ? "Aktif" : "İade Edildi"
            })
            .FirstOrDefaultAsync();
    }

    // Yeni zimmet oluştur
    public async Task<ZimmetResponseDto> CreateAsync(ZimmetCreateDto dto)
    {
        var personel = await _context.Personeller
            .FirstOrDefaultAsync(p =>
                p.PersonelId == dto.PersonelId && p.AktifMi);

        if (personel == null)
            throw new ZimmetIliskiBulunamadiException("Personel bulunamadı veya aktif değil.");

        var demirbas = await _context.Demirbaslar
            .FirstOrDefaultAsync(d => d.DemirbasId == dto.DemirbasId);

        if (demirbas == null)
            throw new ZimmetIliskiBulunamadiException("Demirbaş bulunamadı.");
        if (demirbas.Durum != DemirbasDurum.Kullanilabilir)
            throw new ZimmetIslemException("Bu demirbaş şu anda zimmetlenebilir durumda değil.");
        bool aktifZimmetVarMi = await _context.Zimmetler
            .AnyAsync(z =>
                z.DemirbasId == dto.DemirbasId &&
                z.IadeTarihi == null);

        if (aktifZimmetVarMi)
            throw new ZimmetIslemException("Bu demirbaş zaten başka bir personele zimmetlenmiş.");
        var zimmet = new Zimmet
        {
            PersonelId = dto.PersonelId,
            DemirbasId = dto.DemirbasId,
            TeslimTarihi = DateTime.Now,
            IadeTarihi = null,
            Aciklama = dto.Aciklama?.Trim(),
            TeslimEden = dto.TeslimEden?.Trim(),
            TeslimAlan = dto.TeslimAlan?.Trim()
        };

        demirbas.Durum = DemirbasDurum.Zimmetli;

        _context.Zimmetler.Add(zimmet);

        await _context.SaveChangesAsync();

        return await GetByIdAsync(zimmet.ZimmetId)
            ?? throw new Exception("Oluşturulan zimmet kaydı getirilemedi.");
    }

    // Demirbaş iadesi
    public async Task<ZimmetResponseDto?> ReturnAssetAsync(
     int id,
     ZimmetReturnDto dto)
    {
        var zimmet = await _context.Zimmetler
            .Include(z => z.Demirbas)
            .FirstOrDefaultAsync(z => z.ZimmetId == id);

        if (zimmet == null)
            return null;

        if (zimmet.IadeTarihi != null)
            throw new ZimmetIslemException(
                "Bu demirbaş zaten iade edilmiş.");

        // İade sonrası durum yalnızca Kullanilabilir, Arizali veya Hurda olabilir.
        if (dto.IadeSonrasiDurum == DemirbasDurum.Zimmetli ||
            !Enum.IsDefined(typeof(DemirbasDurum), dto.IadeSonrasiDurum))
        {
            throw new ZimmetIslemException(
                "İade sonrası durum Kullanılabilir, Arızalı veya Hurda olmalıdır.");
        }

        zimmet.IadeTarihi = DateTime.Now;

        zimmet.Demirbas!.Durum = dto.IadeSonrasiDurum;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    // Sadece aktif zimmetleri listele
    public async Task<List<ZimmetResponseDto>> GetActiveAsync()
    {
        return await _context.Zimmetler
            .AsNoTracking()
            .Where(z => z.IadeTarihi == null)
            .Select(z => new ZimmetResponseDto
            {
                ZimmetId = z.ZimmetId,
                PersonelId = z.PersonelId,
                PersonelAdSoyad = z.Personel!.Ad + " " + z.Personel.Soyad,
                DemirbasId = z.DemirbasId,
                DemirbasAdi = z.Demirbas!.Ad,
                TeslimTarihi = z.TeslimTarihi,
                IadeTarihi = z.IadeTarihi,
                Aciklama = z.Aciklama,
                TeslimEden = z.TeslimEden,
                TeslimAlan = z.TeslimAlan,
                Durum = "Aktif"
            })
            .ToListAsync();
    }
}