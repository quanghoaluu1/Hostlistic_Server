using Microsoft.AspNetCore.Mvc;

namespace BookingService_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public TestController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost("test-email")]
        public async Task<IActionResult> TestEmail([FromBody] TestEmailRequest request)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var emailRequest = new
                {
                    Email = request.Email,
                    CustomerName = "Test User",
                    EventName = "Test Event",
                    EventDate = DateTime.Now.AddDays(7),
                    EventLocation = "Test Location",
                    OrderId = Guid.NewGuid(),
                    TotalAmount = 100.00m,
                    PurchaseDate = DateTime.Now,
                    Tickets = new[]
                    {
                    new
                    {
                        TicketCode = "TEST-001",
                        QrCodeUrl = "",
                        TicketTypeName = "Test Ticket",
                        Price = 100.00m
                    }
                }
                };

                var response = await httpClient.PostAsJsonAsync("http://localhost:5097/api/Email/send-ticket-confirmation", emailRequest);

                if (response.IsSuccessStatusCode)
                {
                    return Ok("Test email sent successfully!");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return BadRequest($"Email failed: {response.StatusCode} - {error}");
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Exception: {ex.Message}");
            }
        }
    }

    public class TestEmailRequest
    {
        public string Email { get; set; } = string.Empty;
    }
}
