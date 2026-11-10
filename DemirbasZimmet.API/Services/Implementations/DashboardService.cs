using DemirbasZimmet.API.Data;
using DemirbasZimmet.API.DTOs.Dashboard;
using DemirbasZimmet.API.Models;
using DemirbasZimmet.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DemirbasZimmet.API.Services.Implementations;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;

    public DashboardService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardResponseDto> GetSummaryAsync()
    {
        var sonuc = new DashboardResponseDto
        {
            ToplamDemirbas = await _context.Demirbaslar.CountAsync(),

            KullanilabilirDemirbas = await _context.Demirbaslar
                .CountAsync(d => d.Durum == DemirbasDurum.Kullanilabilir),

            ZimmetliDemirbas = await _context.Demirbaslar
                .CountAsync(d => d.Durum == DemirbasDurum.Zimmetli),

            ArizaliDemirbas = await _context.Demirbaslar
                .CountAsync(d => d.Durum == DemirbasDurum.Arizali),

            HurdaDemirbas = await _context.Demirbaslar
                .CountAsync(d => d.Durum == DemirbasDurum.Hurda),

            AktifPersonel = await _context.Personeller
                .CountAsync(p => p.AktifMi),

            DevamEdenBakim = await _context.BakimKayitlari
                .CountAsync(b => b.Durum == BakimDurum.DevamEdiyor),

            SonZimmetler = await _context.Zimmetler
                .AsNoTracking()
                .OrderByDescending(z => z.TeslimTarihi)
                .Take(5)
                .Select(z => new SonZimmetDto
                {
                    ZimmetId = z.ZimmetId,
                    PersonelAdi = z.Personel!.Ad + " " + z.Personel.Soyad,
                    DemirbasAdi = z.Demirbas!.Ad,
                    DemirbasKodu = z.Demirbas.DemirbasKodu,
                    TeslimTarihi = z.TeslimTarihi,
                    IadeTarihi = z.IadeTarihi,
                    Durum = z.IadeTarihi == null
                        ? "Aktif"
                        : "İade Edildi"
                })
                .ToListAsync()
        };

        return sonuc;
    }
}