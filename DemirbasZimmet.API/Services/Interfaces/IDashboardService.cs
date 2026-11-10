using DemirbasZimmet.API.DTOs.Dashboard;

namespace DemirbasZimmet.API.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardResponseDto> GetSummaryAsync();
}