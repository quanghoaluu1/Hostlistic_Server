namespace NotificationService_Domain.Enums;

public enum RecipientGroup
{
    AllTicketHolders = 0,    // Gửi hết cho ai đã mua vé
    SpecificTicketType = 1,  // Chỉ gửi cho vé VIP
    NotCheckedIn = 2,        // Nhắc nhở ai chưa check-in
    ManualList = 3           // Danh sách cụ thể (import excel)
}