using System;
using System.Collections.Generic;
using System.Text;

namespace BookingService_Application.Interfaces
{
    public interface IQrCodeService
    {
        Task<string> GenerateQrCodeAsync(string ticketCode);
    }
}
