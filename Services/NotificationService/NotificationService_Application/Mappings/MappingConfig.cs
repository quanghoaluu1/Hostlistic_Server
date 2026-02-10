using Mapster;
using NotificationService_Application.DTOs;
using NotificationService_Domain.Entities;

namespace NotificationService_Application.Mappings;

public class MappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Notification mappings
        config.NewConfig<CreateNotificationRequest, Notification>()
            .Map(dest => dest.TargetData, src => src.TargetData == null ? null : src.TargetData.Adapt<NotificationTargetData>());

        config.NewConfig<Notification, NotificationDto>()
            .Map(dest => dest.TargetData, src => src.TargetData == null ? null : src.TargetData.Adapt<NotificationTargetDataDto>());

        config.NewConfig<NotificationTargetDataDto, NotificationTargetData>();
        config.NewConfig<NotificationTargetData, NotificationTargetDataDto>();

        // UserNotification mappings
        config.NewConfig<CreateUserNotificationRequest, UserNotification>();
        config.NewConfig<UserNotification, UserNotificationDto>();

        // EmailCampaign mappings
        config.NewConfig<CreateEmailCampaignRequest, EmailCampaign>()
            .Map(dest => dest.TargetFilter, src => src.TargetFilter == null ? null : src.TargetFilter.Adapt<EmailTargetFilter>());

        config.NewConfig<EmailCampaign, EmailCampaignDto>()
            .Map(dest => dest.TargetFilter, src => src.TargetFilter == null ? null : src.TargetFilter.Adapt<EmailTargetFilterDto>());

        config.NewConfig<EmailTargetFilterDto, EmailTargetFilter>();
        config.NewConfig<EmailTargetFilter, EmailTargetFilterDto>();

        // EmailLog mappings
        config.NewConfig<CreateEmailLogRequest, EmailLog>();
        config.NewConfig<EmailLog, EmailLogDto>();
    }
}
