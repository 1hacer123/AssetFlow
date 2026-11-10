using DemirbasZimmet.API.DTOs.Bakim;

namespace DemirbasZimmet.API.Services.Interfaces;

public interface IBakimService
{
    Task<List<BakimResponseDto>> GetAllAsync();
    Task<BakimResponseDto?> GetByIdAsync(int id);
    Task<List<BakimResponseDto>> GetByDemirbasIdAsync(int demirbasId);
    Task<BakimResponseDto> CreateAsync(BakimCreateDto dto);
    Task<BakimResponseDto?> CompleteAsync(int id, BakimTamamlaDto dto);
}