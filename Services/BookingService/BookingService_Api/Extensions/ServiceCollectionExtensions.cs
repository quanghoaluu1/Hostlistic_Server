using BookingService_Application.Interfaces;
using BookingService_Application.Services;
using BookingService_Api.Hubs;
using BookingService_Api.Services;
using BookingService_Domain.Interfaces;
using BookingService_Infrastructure.Repositories;
using BookingService_Infrastructure.ServiceClients;
using BookingService_Infrastructure.Services;

namespace BookingService_Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<ISessionSnapshotRepository, SessionSnapshotRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderDetailRepository, OrderDetailRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<IPayoutRequestRepository, PayoutRequestRepository>();
        services.AddScoped<IInventoryReservationRepository, InventoryReservationRepository>();
        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<IEventSettlementRepository, EventSettlementRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();

        // Services
        services.AddScoped<ITicketPurchaseService, TicketPurchaseService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IQrCodeService, QrCodeService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IOrderDetailService, OrderDetailService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IPaymentMethodService, PaymentMethodService>();
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<IPayoutRequestService, PayoutRequestService>();
        services.AddScoped<IPhotoService, PhotoService>();
        services.AddScoped<IWalletService, WalletService>();
        services.AddScoped<IEventServiceClient, EventServiceClient>();
        services.AddScoped<IUserServiceClient, UserServiceClient>();
        services.AddScoped<INotificationServiceClient, NotificationServiceClient>();
        services.AddScoped<IUserPlanServiceClient, UserPlanServiceClient>();
        services.AddScoped<IPayOsService, PayOsService>();
        services.AddScoped<IPayOsWebhookHandler, PayOsWebhookHandler>();
        services.AddScoped<IPaymentNotifier, SignalRPaymentNotifier>();
        services.AddScoped<ISettlementService, SettlementService>();
        services.AddScoped<ISubscriptionPurchaseService, SubscriptionPurchaseService>();
        services.AddScoped<ICheckInService, CheckInService>();

        return services;
    }
}
