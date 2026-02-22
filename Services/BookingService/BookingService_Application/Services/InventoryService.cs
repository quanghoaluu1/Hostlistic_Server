using BookingService_Application.DTOs;
using BookingService_Application.Interfaces;
using BookingService_Domain.Entities;
using BookingService_Domain.Interfaces;
using Common;
using System.Net.Http.Json;
using System.Text.Json;

namespace BookingService_Application.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryReservationRepository _inventoryReservationRepository;
        private readonly IHttpClientFactory _httpClientFactory;

        public InventoryService(IInventoryReservationRepository inventoryReservationRepository, IHttpClientFactory httpClientFactory)
        {
            _inventoryReservationRepository = inventoryReservationRepository;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<ApiResponse<InventoryCheckResponse>> CheckAvailabilityAsync(List<TicketItemRequest> items)
        {
            var response = new InventoryCheckResponse
            {
                IsAvailable = true,
                TicketAvailability = new List<TicketAvailabilityInfo>()
            };

            try
            {
                // Call EventService to get ticket type information
                var httpClient = _httpClientFactory.CreateClient("EventService");

                foreach (var item in items)
                {
                    try
                    {
                        var requestUrl = $"/api/tickettypes/{item.TicketTypeId}";
                        Console.WriteLine($"Making request to EventService: {httpClient.BaseAddress}{requestUrl}");
                        
                        var ticketTypeResponse = await httpClient.GetAsync(requestUrl);
                        
                        Console.WriteLine($"EventService Response Status: {ticketTypeResponse.StatusCode}");
                        
                        if (!ticketTypeResponse.IsSuccessStatusCode)
                        {
                            var errorContent = await ticketTypeResponse.Content.ReadAsStringAsync();
                            Console.WriteLine($"EventService Error Response: {errorContent}");
                            
                            response.IsAvailable = false;
                            response.Message = $"Ticket type {item.TicketTypeId} not found (Status: {ticketTypeResponse.StatusCode})";
                            continue;
                        }

                        var ticketTypeJson = await ticketTypeResponse.Content.ReadAsStringAsync();
                        Console.WriteLine($"EventService JSON Response: {ticketTypeJson}");

                        var options = new JsonSerializerOptions 
                        { 
                            PropertyNameCaseInsensitive = true,
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        };
                        
                        var ticketTypeApiResponse = JsonSerializer.Deserialize<ApiResponse<TicketTypeDto>>(ticketTypeJson, options);

                        if (ticketTypeApiResponse?.Data == null)
                        {
                            Console.WriteLine("Failed to deserialize EventService response or Data is null");
                            response.IsAvailable = false;
                            response.Message = $"Unable to retrieve ticket type {item.TicketTypeId}";
                            continue;
                        }

                        var ticketType = ticketTypeApiResponse.Data;
                        var availableQuantity = ticketType.QuantityAvailable - ticketType.QuantitySold;

                        var availabilityInfo = new TicketAvailabilityInfo
                        {
                            TicketTypeId = item.TicketTypeId,
                            TicketTypeName = ticketType.Name,
                            AvailableQuantity = availableQuantity,
                            RequestedQuantity = item.Quantity,
                            IsValid = true
                        };

                        // Validate availability
                        if (availableQuantity < item.Quantity)
                        {
                            availabilityInfo.IsValid = false;
                            availabilityInfo.ErrorMessage = $"Not enough tickets available. Available: {availableQuantity}, Requested: {item.Quantity}";
                            response.IsAvailable = false;
                        }

                        // Check sale period
                        if (DateTime.UtcNow < ticketType.SaleStartDate || DateTime.UtcNow > ticketType.SaleEndTime)
                        {
                            availabilityInfo.IsValid = false;
                            availabilityInfo.ErrorMessage = $"Ticket type {ticketType.Name} is not currently on sale";
                            response.IsAvailable = false;
                        }

                        // Check min/max per order
                        if (item.Quantity < ticketType.MinPerOrder || item.Quantity > ticketType.MaxPerOrder)
                        {
                            availabilityInfo.IsValid = false;
                            availabilityInfo.ErrorMessage = $"Invalid quantity for {ticketType.Name}. Min: {ticketType.MinPerOrder}, Max: {ticketType.MaxPerOrder}";
                            response.IsAvailable = false;
                        }

                        response.TicketAvailability.Add(availabilityInfo);
                    }
                    catch (Exception itemEx)
                    {
                        Console.WriteLine($"Error processing ticket type {item.TicketTypeId}: {itemEx.Message}");
                        response.IsAvailable = false;
                        response.Message = $"Error processing ticket type {item.TicketTypeId}: {itemEx.Message}";
                    }
                }

                if (!response.IsAvailable && string.IsNullOrEmpty(response.Message))
                {
                    response.Message = "Some tickets are not available for purchase";
                }

                return ApiResponse<InventoryCheckResponse>.Success(200, response.IsAvailable ? "All tickets available" : response.Message, response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"InventoryService.CheckAvailabilityAsync Error: {ex.Message}");
                return ApiResponse<InventoryCheckResponse>.Fail(500, $"Error checking availability: {ex.Message}");
            }
        }

        public async Task ConfirmReservationAsync(Guid reservationId)
        {
            var reservations = await _inventoryReservationRepository.GetReservationsByIdAsync(reservationId);

            // Call EventService to process bulk ticket purchase
            var httpClient = _httpClientFactory.CreateClient("EventService");

            try
            {
                // Prepare bulk purchase request
                var bulkPurchaseRequest = new
                {
                    Items = reservations.Select(r => new
                    {
                        TicketTypeId = r.TicketTypeId,
                        Quantity = r.Quantity
                    }).ToList()
                };

                var response = await httpClient.PostAsJsonAsync("/api/tickettypes/bulk-purchase", bulkPurchaseRequest);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Failed to process bulk ticket purchase: {errorContent}");
                    throw new InvalidOperationException($"Failed to update ticket inventory: {errorContent}");
                }

                Console.WriteLine("Bulk ticket purchase processed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing bulk ticket purchase: {ex.Message}");
                throw;
            }

            // Remove reservations after successful inventory update
            await _inventoryReservationRepository.DeleteReservationsAsync(reservationId);
            await _inventoryReservationRepository.SaveChangesAsync();
        }

        public async Task ReleaseReservationAsync(Guid reservationId)
        {
            await _inventoryReservationRepository.DeleteReservationsAsync(reservationId);
            await _inventoryReservationRepository.SaveChangesAsync();
        }

        public async Task<Guid> ReserveInventoryAsync(List<TicketItemRequest> items)
        {
            var reservationId = Guid.NewGuid();

            foreach (var item in items)
            {
                await _inventoryReservationRepository.CreateReservationAsync(new InventoryReservation
                {
                    Id = Guid.NewGuid(),
                    ReservationId = reservationId,
                    TicketTypeId = item.TicketTypeId,
                    Quantity = item.Quantity,
                    ReservedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(15)
                });
            }

            await _inventoryReservationRepository.SaveChangesAsync();
            return reservationId;
        }
    }
}
