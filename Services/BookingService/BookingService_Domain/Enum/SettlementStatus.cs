namespace BookingService_Domain.Enum;

public enum SettlementStatus
{
    
    Pending,      // Đang tính toán
    Settled,      // Đã credit vào wallet
    Rejected,     
    NoRevenue     // Event không có doanh thu (free event)
}