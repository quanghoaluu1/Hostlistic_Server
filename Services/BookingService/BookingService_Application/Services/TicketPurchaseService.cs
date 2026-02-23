using Common;
using BookingService_Application.DTOs;
using BookingService_Application.Interfaces;
using BookingService_Domain.Enum;
using System.Net.Http.Json;
using System.Text.Json;

namespace BookingService_Application.Services;

public class TicketPurchaseService : ITicketPurchaseService
{
    private readonly IOrderService _orderService;
    private readonly ITicketService _ticketService;
    private readonly IPaymentService _paymentService;
    private readonly IInventoryService _inventoryService;
    private readonly IQrCodeService _qrCodeService;
    private readonly IHttpClientFactory _httpClientFactory;

    public TicketPurchaseService(IOrderService orderService, ITicketService ticketService, IPaymentService paymentService,
    IInventoryService inventoryService, IQrCodeService qrCodeService, IHttpClientFactory httpClientFactory)
    {
        _orderService = orderService;
        _ticketService = ticketService;
        _paymentService = paymentService;
        _inventoryService = inventoryService;
        _qrCodeService = qrCodeService;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<ApiResponse<InventoryCheckResponse>> CheckTicketAvailabilityAsync(InventoryCheckRequest request)
    {
        return await _inventoryService.CheckAvailabilityAsync(request.TicketItems);
    }

    public async Task<ApiResponse<PurchaseTicketResponse>> PurchaseTicketsAsync(PurchaseTicketRequest request)
    {
        try
        {
            // 1. Validate ticket availability
            var availabilityCheck = await _inventoryService.CheckAvailabilityAsync(request.TicketItems);
            if (!availabilityCheck.IsSuccess || !availabilityCheck.Data!.IsAvailable)
            {
                return ApiResponse<PurchaseTicketResponse>.Fail(400,
                    availabilityCheck.Data?.Message ?? "Tickets not available");
            }

            // 2. Reserve inventory temporarily
            var reservationId = await _inventoryService.ReserveInventoryAsync(request.TicketItems);

            try
            {
                // 3. Calculate total amount
                var totalAmount = request.TicketItems.Sum(x => x.UnitPrice * x.Quantity);

                // 4. Create order
                var orderRequest = new CreateOrderRequest
                {
                    EventId = request.EventId,
                    UserId = request.UserId,
                    Notes = request.Notes,
                    OrderDetails = request.TicketItems.Select(item => new CreateOrderDetailRequest
                    {
                        TicketTypeId = item.TicketTypeId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    }).ToList()
                };

                var orderResult = await _orderService.CreateOrderAsync(orderRequest);
                if (!orderResult.IsSuccess)
                {
                    await _inventoryService.ReleaseReservationAsync(reservationId);
                    return ApiResponse<PurchaseTicketResponse>.Fail(400, orderResult.Message);
                }

                // 5. Process payment
                var paymentResult = await _paymentService.CreatePaymentAsync(new CreatePaymentRequest
                {
                    OrderId = orderResult.Data!.Id,
                    PaymentMethodId = request.PaymentMethodId,
                    Amount = totalAmount,
                    Gateway = request.PaymentGateway
                });

                if (!paymentResult.IsSuccess)
                {
                    await _orderService.UpdateOrderAsync(orderResult.Data.Id, new UpdateOrderRequest
                    {
                        Status = OrderStatus.Cancelled,
                        Notes = "Payment failed"
                    });
                    await _inventoryService.ReleaseReservationAsync(reservationId);
                    return ApiResponse<PurchaseTicketResponse>.Fail(400, paymentResult.Message);
                }

                // 6. Real payment not integrated yet
                // For now, payment will be marked as successful
                await _paymentService.UpdatePaymentAsync(paymentResult.Data!.Id, new UpdatePaymentRequest
                {
                    Status = PaymentStatus.Completed,
                    TransactionId = $"TXN-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..8]}"
                });

                // 7. Confirm inventory reduction
                await _inventoryService.ConfirmReservationAsync(reservationId);

                // 8. Generate tickets with QR codes
                var tickets = new List<TicketDto>();
                foreach (var item in request.TicketItems)
                {
                    for (int i = 0; i < item.Quantity; i++)
                    {
                        var ticketResult = await _ticketService.CreateTicketAsync(new CreateTicketRequest
                        {
                            OrderId = orderResult.Data.Id,
                            TicketTypeId = item.TicketTypeId
                        });

                        if (ticketResult.IsSuccess)
                        {
                            // Generate QR code
                            var qrCodeUrl = await _qrCodeService.GenerateQrCodeAsync(ticketResult.Data!.TicketCode);

                            if (!string.IsNullOrEmpty(qrCodeUrl))
                            {
                                await _ticketService.UpdateTicketAsync(ticketResult.Data.Id, new UpdateTicketRequest
                                {
                                    QrCodeUrl = qrCodeUrl,
                                    IsUsed = false
                                });

                                // Update the DTO with QR code URL
                                ticketResult.Data.QrCodeUrl = qrCodeUrl;
                            }

                            tickets.Add(ticketResult.Data);
                        }
                    }
                }

                // 9. Update order status
                await _orderService.UpdateOrderAsync(orderResult.Data.Id, new UpdateOrderRequest
                {
                    Status = OrderStatus.Confirmed,
                    Notes = "Payment completed and tickets generated"
                });

                // 10. Get event and user information for email
                Console.WriteLine($"[DEBUG] Getting event info for EventId: {request.EventId}");
                var eventInfo = await GetEventInfoAsync(request.EventId);
                Console.WriteLine($"[DEBUG] Event info retrieved: {(eventInfo != null ? eventInfo.Name : "NULL")}");
                
                Console.WriteLine($"[DEBUG] Getting user info for UserId: {request.UserId}");
                var userInfo = await GetUserInfoAsync(request.UserId);
                Console.WriteLine($"[DEBUG] User info retrieved: {(userInfo != null ? userInfo.Email : "NULL")}");

                // 11. Send confirmation email with tickets and QR codes
                var emailSent = await SendTicketPurchaseConfirmationAsync(new PurchaseConfirmationRequest
                {
                    UserId = request.UserId,
                    OrderId = orderResult.Data.Id,
                    Tickets = tickets,
                    TotalAmount = totalAmount,
                    EventName = eventInfo?.Name ?? "Unknown Event",
                    EventDate = eventInfo?.StartDate ?? DateTime.Now,
                    EventLocation = eventInfo?.Location ?? "TBD",
                    CustomerName = userInfo?.FullName ?? "Valued Customer",
                    CustomerEmail = userInfo?.Email ?? ""
                });

                var responseMessage = emailSent 
                    ? "Purchase completed successfully. Confirmation email with tickets and QR codes sent."
                    : "Purchase completed successfully. Email sending failed - please check your email settings.";

                return ApiResponse<PurchaseTicketResponse>.Success(200, "Tickets purchased successfully", new PurchaseTicketResponse
                {
                    OrderId = orderResult.Data.Id,
                    Tickets = tickets,
                    PaymentId = paymentResult.Data.Id,
                    TotalAmount = totalAmount,
                    Message = responseMessage
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Purchase failed: {ex.Message}");
                Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
                // Rollback on any error
                await _inventoryService.ReleaseReservationAsync(reservationId);
                return ApiResponse<PurchaseTicketResponse>.Fail(500, $"Purchase failed: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Purchase failed: {ex.Message}");
            return ApiResponse<PurchaseTicketResponse>.Fail(500, $"Purchase failed: {ex.Message}");
        }
    }

    private async Task<bool> SendTicketPurchaseConfirmationAsync(PurchaseConfirmationRequest request)
    {
        try
        {
            Console.WriteLine($"[DEBUG] Starting email send process for user: {request.CustomerEmail}");
            
            if (string.IsNullOrEmpty(request.CustomerEmail))
            {
                Console.WriteLine("[WARNING] No email provided for user - skipping email send");
                return false;
            }

            var httpClient = _httpClientFactory.CreateClient();
            Console.WriteLine("[DEBUG] HTTP client created");

            // Prepare ticket information for email
            var ticketEmailInfos = new List<TicketEmailInfo>();
            Console.WriteLine($"[DEBUG] Processing {request.Tickets.Count} tickets for email");
            
            foreach (var ticket in request.Tickets)
            {
                Console.WriteLine($"[DEBUG] Processing ticket: {ticket.TicketCode}");
                // Get ticket type name from EventService
                var ticketTypeInfo = await GetTicketTypeInfoAsync(ticket.TicketTypeId);
                
                ticketEmailInfos.Add(new TicketEmailInfo
                {
                    TicketCode = ticket.TicketCode,
                    QrCodeUrl = ticket.QrCodeUrl ?? "",
                    TicketTypeName = ticketTypeInfo?.Name ?? "Standard Ticket",
                    Price = ticketTypeInfo?.Price ?? 0
                });
                
                Console.WriteLine($"[DEBUG] Added ticket info - Code: {ticket.TicketCode}, QR: {(!string.IsNullOrEmpty(ticket.QrCodeUrl) ? "Present" : "Missing")}");
            }

            var emailRequest = new TicketPurchaseEmailRequest
            {
                Email = request.CustomerEmail,
                CustomerName = request.CustomerName,
                EventName = request.EventName,
                EventDate = request.EventDate,
                EventLocation = request.EventLocation,
                OrderId = request.OrderId,
                TotalAmount = request.TotalAmount,
                PurchaseDate = DateTime.UtcNow,
                Tickets = ticketEmailInfos
            };

            Console.WriteLine($"[DEBUG] Email request prepared for: {emailRequest.Email}");
            Console.WriteLine($"[DEBUG] Event: {emailRequest.EventName}");
            Console.WriteLine($"[DEBUG] Tickets count: {emailRequest.Tickets.Count}");

            // Send to NotificationService
            var notificationServiceUrl = "http://localhost:5097/api/Email/send-ticket-confirmation";
            Console.WriteLine($"[DEBUG] Sending email to: {notificationServiceUrl}");
            
            var response = await httpClient.PostAsJsonAsync(notificationServiceUrl, emailRequest);
            
            Console.WriteLine($"[DEBUG] Email service response status: {response.StatusCode}");
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[ERROR] Failed to send ticket confirmation email: Status={response.StatusCode}, Error={error}");
                return false;
            }

            Console.WriteLine("[SUCCESS] Email sent successfully!");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Exception in SendTicketPurchaseConfirmationAsync: {ex.Message}");
            Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
            return false;
        }
    }

    private async Task<EventInfoDto?> GetEventInfoAsync(Guid eventId)
    {
        try
        {
            Console.WriteLine($"[DEBUG] Fetching event info from EventService for ID: {eventId}");
            var httpClient = _httpClientFactory.CreateClient();
            var eventServiceUrl = $"http://localhost:5001/api/Events/{eventId}";
            Console.WriteLine($"[DEBUG] Event service URL: {eventServiceUrl}");
            
            var response = await httpClient.GetAsync(eventServiceUrl);
            Console.WriteLine($"[DEBUG] Event service response status: {response.StatusCode}");
            
            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[DEBUG] Event service raw response: {jsonContent}");
                
                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<EventInfoDto>>();
                Console.WriteLine($"[DEBUG] Event service parsed response: {(apiResponse?.Data != null ? "SUCCESS" : "NULL")}");
                return apiResponse?.Data;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[WARNING] Event service failed: Status={response.StatusCode}, Error={error}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Exception getting event info: {ex.Message}");
        }
        
        return null;
    }

    private async Task<UserInfoDto?> GetUserInfoAsync(Guid userId)
    {
        try
        {
            Console.WriteLine($"[DEBUG] Fetching user info from IdentityService for ID: {userId}");
            var httpClient = _httpClientFactory.CreateClient();
            var userServiceUrl = $"http://localhost:5000/api/Users/{userId}";
            Console.WriteLine($"[DEBUG] User service URL: {userServiceUrl}");
            
            var response = await httpClient.GetAsync(userServiceUrl);
            Console.WriteLine($"[DEBUG] User service response status: {response.StatusCode}");
            
            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[DEBUG] User service raw response: {jsonContent}");
                
                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<UserInfoDto>>();
                Console.WriteLine($"[DEBUG] User service parsed response: {(apiResponse?.Data != null ? "SUCCESS" : "NULL")}");
                return apiResponse?.Data;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[WARNING] User service failed: Status={response.StatusCode}, Error={error}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Exception getting user info: {ex.Message}");
        }
        
        return null;
    }

    private async Task<TicketTypeInfoDto?> GetTicketTypeInfoAsync(Guid ticketTypeId)
    {
        try
        {
            Console.WriteLine($"[DEBUG] Fetching ticket type info from EventService for ID: {ticketTypeId}");
            var httpClient = _httpClientFactory.CreateClient();
            var ticketTypeServiceUrl = $"http://localhost:5001/api/TicketTypes/{ticketTypeId}";
            Console.WriteLine($"[DEBUG] Ticket type service URL: {ticketTypeServiceUrl}");
            
            var response = await httpClient.GetAsync(ticketTypeServiceUrl);
            Console.WriteLine($"[DEBUG] Ticket type service response status: {response.StatusCode}");
            
            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[DEBUG] Ticket type service raw response: {jsonContent}");
                
                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<TicketTypeInfoDto>>();
                Console.WriteLine($"[DEBUG] Ticket type service parsed response: {(apiResponse?.Data != null ? "SUCCESS" : "NULL")}");
                return apiResponse?.Data;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[WARNING] Ticket type service failed: Status={response.StatusCode}, Error={error}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Exception getting ticket type info: {ex.Message}");
        }
        
        return null;
    }
}

// DTOs for external service calls
public class EventInfoDto
{
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public string Location { get; set; } = string.Empty;
}

public class UserInfoDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class TicketTypeInfoDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
