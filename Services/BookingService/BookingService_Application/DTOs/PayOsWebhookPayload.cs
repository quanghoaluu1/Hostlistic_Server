namespace BookingService_Application.DTOs;

public class PayOsWebhookPayload
{
    public string Code { get; set; } = string.Empty;
    public string Desc { get; set; } = string.Empty;
    public bool Success { get; set; }
    public PayOsWebhookDataDto? Data { get; set; }
    public string Signature { get; set; } = string.Empty;
}

public class PayOsWebhookDataDto
{
    public long OrderCode { get; set; }
    public int Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public string TransactionDateTime { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Desc { get; set; } = string.Empty;
    public string CounterAccountBankId { get; set; } = string.Empty;
    public string CounterAccountBankName { get; set; } = string.Empty;
    public string CounterAccountName { get; set; } = string.Empty;
    public string CounterAccountNumber { get; set; } = string.Empty;
}