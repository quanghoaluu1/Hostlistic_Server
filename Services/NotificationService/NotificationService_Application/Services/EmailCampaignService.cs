using Common;
using Mapster;
using NotificationService_Application.DTOs;
using NotificationService_Application.Interfaces;
using NotificationService_Domain.Entities;
using NotificationService_Domain.Enums;
using NotificationService_Domain.Interfaces;

namespace NotificationService_Application.Services;

public class EmailCampaignService(IEmailCampaignRepository emailCampaignRepository) : IEmailCampaignService
{
    public async Task<ApiResponse<EmailCampaignDto>> GetByIdAsync(Guid id)
    {
        var campaign = await emailCampaignRepository.GetByIdAsync(id);
        if (campaign is null)
            return ApiResponse<EmailCampaignDto>.Fail(404, "Email campaign not found");

        var dto = campaign.Adapt<EmailCampaignDto>();
        return ApiResponse<EmailCampaignDto>.Success(200, "Email campaign retrieved successfully", dto);
    }

    public async Task<ApiResponse<List<EmailCampaignDto>>> GetAllAsync()
    {
        var campaigns = await emailCampaignRepository.GetAllAsync();
        var dtos = campaigns.Adapt<List<EmailCampaignDto>>();
        return ApiResponse<List<EmailCampaignDto>>.Success(200, "Email campaigns retrieved successfully", dtos);
    }

    public async Task<ApiResponse<EmailCampaignDto>> CreateAsync(CreateEmailCampaignRequest request)
    {
        var campaign = request.Adapt<EmailCampaign>();
        campaign.Id = Guid.NewGuid();
        campaign.Status = EmailCampaignStatus.Draft;
        campaign.CreatedAt = DateTime.UtcNow;
        campaign.UpdatedAt = DateTime.UtcNow;

        await emailCampaignRepository.AddAsync(campaign);
        await emailCampaignRepository.SaveChangesAsync();

        var dto = campaign.Adapt<EmailCampaignDto>();
        return ApiResponse<EmailCampaignDto>.Success(201, "Email campaign created successfully", dto);
    }

    public async Task<ApiResponse<EmailCampaignDto>> UpdateAsync(Guid id, UpdateEmailCampaignRequest request)
    {
        var campaign = await emailCampaignRepository.GetByIdAsync(id);
        if (campaign is null)
            return ApiResponse<EmailCampaignDto>.Fail(404, "Email campaign not found");

        campaign.Name = request.Name;
        campaign.Content = request.Content;
        campaign.ScheduledDate = request.ScheduledDate;
        campaign.Status = request.Status;
        campaign.RecipientGroup = request.RecipientGroup;
        campaign.TargetFilter = request.TargetFilter?.Adapt<EmailTargetFilter>();
        campaign.UpdatedAt = DateTime.UtcNow;

        await emailCampaignRepository.UpdateAsync(campaign);
        await emailCampaignRepository.SaveChangesAsync();

        var dto = campaign.Adapt<EmailCampaignDto>();
        return ApiResponse<EmailCampaignDto>.Success(200, "Email campaign updated successfully", dto);
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id)
    {
        var campaign = await emailCampaignRepository.GetByIdAsync(id);
        if (campaign is null)
            return ApiResponse<bool>.Fail(404, "Email campaign not found");

        await emailCampaignRepository.DeleteAsync(campaign);
        await emailCampaignRepository.SaveChangesAsync();

        return ApiResponse<bool>.Success(200, "Email campaign deleted successfully", true);
    }
}
