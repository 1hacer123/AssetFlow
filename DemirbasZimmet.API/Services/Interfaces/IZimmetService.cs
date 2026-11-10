using DemirbasZimmet.API.DTOs.Zimmet;

namespace DemirbasZimmet.API.Services.Interfaces;

public interface IZimmetService
{
    Task<List<ZimmetResponseDto>> GetAllAsync();

    Task<ZimmetResponseDto?> GetByIdAsync(int id);

    Task<ZimmetResponseDto> CreateAsync(ZimmetCreateDto dto);

    Task<ZimmetResponseDto?> ReturnAssetAsync(int id, ZimmetReturnDto dto);
    Task<List<ZimmetResponseDto>> GetActiveAsync();
}