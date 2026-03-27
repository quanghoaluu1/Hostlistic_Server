using Common;
using Mapster;
using NotificationService_Application.DTOs;
using NotificationService_Application.Interfaces;
using NotificationService_Domain.Entities;
using NotificationService_Domain.Interfaces;

namespace NotificationService_Application.Services;

public class UserNotificationService(IUserNotificationRepository userNotificationRepository) : IUserNotificationService
{
    public async Task<ApiResponse<UserNotificationDto>> GetByIdAsync(Guid id)
    {
        var userNotification = await userNotificationRepository.GetByIdAsync(id);
        if (userNotification is null)
            return ApiResponse<UserNotificationDto>.Fail(404, "User notification not found");

        var dto = userNotification.Adapt<UserNotificationDto>();
        return ApiResponse<UserNotificationDto>.Success(200, "User notification retrieved successfully", dto);
    }

    public async Task<ApiResponse<List<UserNotificationDto>>> GetAllAsync()
    {
        var userNotifications = await userNotificationRepository.GetAllAsync();
        var dtos = userNotifications.Adapt<List<UserNotificationDto>>();
        return ApiResponse<List<UserNotificationDto>>.Success(200, "User notifications retrieved successfully", dtos);
    }

    public async Task<ApiResponse<List<UserNotificationDto>>> GetByUserIdAsync(Guid userId)
    {
        var userNotifications = await userNotificationRepository.GetByUserIdAsync(userId);
        var dtos = userNotifications.Adapt<List<UserNotificationDto>>();
        return ApiResponse<List<UserNotificationDto>>.Success(200, "User notifications retrieved successfully", dtos);
    }

    public async Task<ApiResponse<List<UserNotificationDto>>> GetUnreadByUserIdAsync(Guid userId)
    {
        var userNotifications = await userNotificationRepository.GetUnreadByUserIdAsync(userId);
        var dtos = userNotifications.Adapt<List<UserNotificationDto>>();
        return ApiResponse<List<UserNotificationDto>>.Success(200, "Unread notifications retrieved successfully", dtos);
    }

    public async Task<ApiResponse<UserNotificationDto>> MarkAsReadAsync(Guid id, Guid userId)
    {
        var userNotification = await userNotificationRepository.GetByIdAsync(id);
        if (userNotification is null || userNotification.UserId != userId)
            return ApiResponse<UserNotificationDto>.Fail(404, "Notification not found");

        if (!userNotification.IsRead)
        {
            userNotification.IsRead = true;
            userNotification.ReadAt = DateTime.UtcNow;
            await userNotificationRepository.UpdateAsync(userNotification);
            await userNotificationRepository.SaveChangesAsync();
        }

        var dto = userNotification.Adapt<UserNotificationDto>();
        return ApiResponse<UserNotificationDto>.Success(200, "Notification marked as read", dto);
    }

    public async Task<ApiResponse<List<UserNotificationDto>>> GetByNotificationIdAsync(Guid notificationId)
    {
        var userNotifications = await userNotificationRepository.GetByNotificationIdAsync(notificationId);
        var dtos = userNotifications.Adapt<List<UserNotificationDto>>();
        return ApiResponse<List<UserNotificationDto>>.Success(200, "User notifications retrieved successfully", dtos);
    }

    public async Task<ApiResponse<UserNotificationDto>> CreateAsync(CreateUserNotificationRequest request)
    {
        var userNotification = request.Adapt<UserNotification>();
        userNotification.Id = Guid.NewGuid();

        await userNotificationRepository.AddAsync(userNotification);
        await userNotificationRepository.SaveChangesAsync();

        var dto = userNotification.Adapt<UserNotificationDto>();
        return ApiResponse<UserNotificationDto>.Success(201, "User notification created successfully", dto);
    }

    public async Task<ApiResponse<UserNotificationDto>> UpdateAsync(Guid id, UpdateUserNotificationRequest request)
    {
        var userNotification = await userNotificationRepository.GetByIdAsync(id);
        if (userNotification is null)
            return ApiResponse<UserNotificationDto>.Fail(404, "User notification not found");

        userNotification.IsRead = request.IsRead;
        userNotification.DeliveryStatus = request.DeliveryStatus;
        userNotification.DeliveryError = request.DeliveryError;

        if (request.IsRead && userNotification.ReadAt == default)
            userNotification.ReadAt = DateTime.UtcNow;

        await userNotificationRepository.UpdateAsync(userNotification);
        await userNotificationRepository.SaveChangesAsync();

        var dto = userNotification.Adapt<UserNotificationDto>();
        return ApiResponse<UserNotificationDto>.Success(200, "User notification updated successfully", dto);
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id)
    {
        var userNotification = await userNotificationRepository.GetByIdAsync(id);
        if (userNotification is null)
            return ApiResponse<bool>.Fail(404, "User notification not found");

        await userNotificationRepository.DeleteAsync(userNotification);
        await userNotificationRepository.SaveChangesAsync();

        return ApiResponse<bool>.Success(200, "User notification deleted successfully", true);
    }
}
