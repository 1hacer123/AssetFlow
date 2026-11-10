using DemirbasZimmet.API.DTOs.Demirbas;

namespace DemirbasZimmet.API.Services.Interfaces;

public interface IDemirbasService
{
    Task<List<DemirbasResponseDto>> GetAllAsync();

    Task<DemirbasResponseDto?> GetByIdAsync(int id);

    Task<DemirbasResponseDto> CreateAsync(DemirbasCreateDto dto);

    Task<DemirbasResponseDto?> UpdateAsync(int id, DemirbasUpdateDto dto);

    Task<DemirbasResponseDto?> UpdateStatusAsync(int id, DemirbasStatusUpdateDto dto);

    Task<bool> DeleteAsync(int id);
}