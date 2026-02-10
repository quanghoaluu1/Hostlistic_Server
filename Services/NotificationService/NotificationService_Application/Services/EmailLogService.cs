using Common;
using Mapster;
using NotificationService_Application.DTOs;
using NotificationService_Application.Interfaces;
using NotificationService_Domain.Entities;
using NotificationService_Domain.Interfaces;

namespace NotificationService_Application.Services;

public class EmailLogService(IEmailLogRepository emailLogRepository) : IEmailLogService
{
    public async Task<ApiResponse<EmailLogDto>> GetByIdAsync(Guid id)
    {
        var emailLog = await emailLogRepository.GetByIdAsync(id);
        if (emailLog is null)
            return ApiResponse<EmailLogDto>.Fail(404, "Email log not found");

        var dto = emailLog.Adapt<EmailLogDto>();
        return ApiResponse<EmailLogDto>.Success(200, "Email log retrieved successfully", dto);
    }

    public async Task<ApiResponse<List<EmailLogDto>>> GetAllAsync()
    {
        var emailLogs = await emailLogRepository.GetAllAsync();
        var dtos = emailLogs.Adapt<List<EmailLogDto>>();
        return ApiResponse<List<EmailLogDto>>.Success(200, "Email logs retrieved successfully", dtos);
    }

    public async Task<ApiResponse<List<EmailLogDto>>> GetByCampaignIdAsync(Guid campaignId)
    {
        var emailLogs = await emailLogRepository.GetByCampaignIdAsync(campaignId);
        var dtos = emailLogs.Adapt<List<EmailLogDto>>();
        return ApiResponse<List<EmailLogDto>>.Success(200, "Email logs retrieved successfully", dtos);
    }

    public async Task<ApiResponse<EmailLogDto>> CreateAsync(CreateEmailLogRequest request)
    {
        var emailLog = request.Adapt<EmailLog>();
        emailLog.Id = Guid.NewGuid();
        emailLog.SentAt = DateTime.UtcNow;

        await emailLogRepository.AddAsync(emailLog);
        await emailLogRepository.SaveChangesAsync();

        var dto = emailLog.Adapt<EmailLogDto>();
        return ApiResponse<EmailLogDto>.Success(201, "Email log created successfully", dto);
    }

    public async Task<ApiResponse<EmailLogDto>> UpdateAsync(Guid id, UpdateEmailLogRequest request)
    {
        var emailLog = await emailLogRepository.GetByIdAsync(id);
        if (emailLog is null)
            return ApiResponse<EmailLogDto>.Fail(404, "Email log not found");

        emailLog.Status = request.Status;
        emailLog.ErrorMessage = request.ErrorMessage;
        emailLog.IsOpened = request.IsOpened;
        emailLog.IsClicked = request.IsClicked;
        emailLog.OpenedAt = request.OpenedAt;
        emailLog.ClickedAt = request.ClickedAt;

        await emailLogRepository.UpdateAsync(emailLog);
        await emailLogRepository.SaveChangesAsync();

        var dto = emailLog.Adapt<EmailLogDto>();
        return ApiResponse<EmailLogDto>.Success(200, "Email log updated successfully", dto);
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id)
    {
        var emailLog = await emailLogRepository.GetByIdAsync(id);
        if (emailLog is null)
            return ApiResponse<bool>.Fail(404, "Email log not found");

        await emailLogRepository.DeleteAsync(emailLog);
        await emailLogRepository.SaveChangesAsync();

        return ApiResponse<bool>.Success(200, "Email log deleted successfully", true);
    }
}
