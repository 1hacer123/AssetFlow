using DemirbasZimmet.API.DTOs.Personel;

namespace DemirbasZimmet.API.Services.Interfaces;

public interface IPersonelService
{
    Task<List<PersonelResponseDto>> GetAllAsync();

    Task<PersonelResponseDto?> GetByIdAsync(int id);

    Task<PersonelResponseDto> CreateAsync(PersonelCreateDto dto);

    Task<PersonelResponseDto?> UpdateAsync(int id, PersonelUpdateDto dto);

    Task<bool> DeleteAsync(int id);
}
/*
 
 Bu kod ne yapıyor?
interface, servisimizin hangi işlemleri sunacağını belirleyen bir sözleşmedir.
Metot	Görevi
GetAllAsync	Aktif personelleri getirir.
GetByIdAsync	ID ile personel bulur.
CreateAsync	Yeni personel oluşturur.
UpdateAsync	Personel bilgilerini günceller.
DeleteAsync	Personeli pasife alır.*/