using Common;
using EventService_Application.DTOs;

namespace EventService_Application.Interfaces;

public interface IEventTemplateService
{
    Task<ApiResponse<EventTemplateDto>> CreateAsync(CreateEventTemplateDto dto);
    Task<ApiResponse<EventTemplateDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<IEnumerable<EventTemplateDto>>> GetByCreatorAsync(Guid createdBy);
    Task<ApiResponse<EventTemplateDto>> UpdateAsync(Guid id, UpdateEventTemplateDto dto);
    Task<ApiResponse<bool>> DeleteAsync(Guid id);
}
