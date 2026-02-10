using Common;
using Mapster;
using NotificationService_Application.DTOs;
using NotificationService_Application.Interfaces;
using NotificationService_Domain.Entities;
using NotificationService_Domain.Interfaces;

namespace NotificationService_Application.Services;

public class NotificationCrudService(INotificationRepository notificationRepository) : INotificationCrudService
{
    public async Task<ApiResponse<NotificationDto>> GetByIdAsync(Guid id)
    {
        var notification = await notificationRepository.GetByIdAsync(id);
        if (notification is null)
            return ApiResponse<NotificationDto>.Fail(404, "Notification not found");

        var dto = notification.Adapt<NotificationDto>();
        return ApiResponse<NotificationDto>.Success(200, "Notification retrieved successfully", dto);
    }

    public async Task<ApiResponse<List<NotificationDto>>> GetAllAsync()
    {
        var notifications = await notificationRepository.GetAllAsync();
        var dtos = notifications.Adapt<List<NotificationDto>>();
        return ApiResponse<List<NotificationDto>>.Success(200, "Notifications retrieved successfully", dtos);
    }

    public async Task<ApiResponse<NotificationDto>> CreateAsync(CreateNotificationRequest request)
    {
        var notification = request.Adapt<Notification>();
        notification.Id = Guid.NewGuid();
        notification.SentAt = DateTime.UtcNow;

        await notificationRepository.AddAsync(notification);
        await notificationRepository.SaveChangesAsync();

        var dto = notification.Adapt<NotificationDto>();
        return ApiResponse<NotificationDto>.Success(201, "Notification created successfully", dto);
    }

    public async Task<ApiResponse<NotificationDto>> UpdateAsync(Guid id, UpdateNotificationRequest request)
    {
        var notification = await notificationRepository.GetByIdAsync(id);
        if (notification is null)
            return ApiResponse<NotificationDto>.Fail(404, "Notification not found");

        notification.Title = request.Title;
        notification.Content = request.Content;
        notification.Type = request.Type;
        notification.RecipientType = request.RecipientType;
        notification.ScheduledDate = request.ScheduledDate;
        notification.Status = request.Status;
        notification.TargetData = request.TargetData?.Adapt<NotificationTargetData>();

        await notificationRepository.UpdateAsync(notification);
        await notificationRepository.SaveChangesAsync();

        var dto = notification.Adapt<NotificationDto>();
        return ApiResponse<NotificationDto>.Success(200, "Notification updated successfully", dto);
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id)
    {
        var notification = await notificationRepository.GetByIdAsync(id);
        if (notification is null)
            return ApiResponse<bool>.Fail(404, "Notification not found");

        await notificationRepository.DeleteAsync(notification);
        await notificationRepository.SaveChangesAsync();

        return ApiResponse<bool>.Success(200, "Notification deleted successfully", true);
    }
}
