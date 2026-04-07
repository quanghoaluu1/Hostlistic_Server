using EventService_Application.Interfaces;
using EventService_Application.Services;
using EventService_Domain.Interfaces;
using EventService_Infrastructure.Repositories;
using EventService_Infrastructure.ServiceClients;

namespace EventService_Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<ISessionBookingRepository, SessionBookingRepository>();
        services.AddScoped<ITrackRepository, TrackRepository>();
        services.AddScoped<IEventTypeRepository, EventTypeRepository>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<ITicketTypeRepository, TicketTypeRepository>();
        services.AddScoped<ITalentRepository, TalentRepository>();
        services.AddScoped<ILineupRepository, LineupRepository>();
        services.AddScoped<ICheckInRepository, CheckInRepository>();
        services.AddScoped<IEventTemplateRepository, EventTemplateRepository>();
        services.AddScoped<ISponsorRepository, SponsorRepository>();
        services.AddScoped<ISponsorTierRepository, SponsorTierRepository>();
        services.AddScoped<ISponsorInteractionRepository, SponsorInteractionRepository>();
        services.AddScoped<IEventTeamMemberRepository, EventTeamMemberRepository>();
        services.AddScoped<IFeedbackRepository, FeedbackRepository>();
        services.AddScoped<IVenueRepository, VenueRepository>();
        services.AddScoped<IAgendaRepository, AgendaRepository>();
        services.AddScoped<IEventDayRepository, EventDayRepository>();

        // Services
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<ISessionBookingService, SessionBookingService>();
        services.AddScoped<ITrackService, TrackService>();
        services.AddScoped<IEventTypeService, EventTypeService>();
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<ITicketTypeService, TicketTypeService>();
        services.AddScoped<IPhotoService, PhotoService>();
        services.AddScoped<ITalentService, TalentService>();
        services.AddScoped<ILineupService, LineupService>();
        services.AddScoped<ICheckInService, CheckInService>();
        services.AddScoped<IEventTemplateService, EventTemplateService>();
        services.AddScoped<ISponsorService, SponsorService>();
        services.AddScoped<ISponsorTierService, SponsorTierService>();
        services.AddScoped<ISponsorInteractionService, SponsorInteractionService>();
        services.AddScoped<IFeedbackService, FeedbackService>();
        services.AddScoped<IAgendaService, AgendaService>();
        services.AddScoped<IEventDayService, EventDayService>();
        services.AddScoped<IVenueService, VenueService>();
        services.AddScoped<IUserPlanServiceClient, UserPlanServiceClient>();
        services.AddScoped<ITeamMemberService, TeamMemberService>();

        return services;
    }
}
