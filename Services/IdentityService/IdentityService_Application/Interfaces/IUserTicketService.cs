using Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService_Application.Interfaces
{
    public interface IUserTicketService
    {
        Task<ApiResponse<object>> GetUserOrdersAsync(Guid userId);
        Task<ApiResponse<object>> GetUserTicketsAsync(Guid userId);
        Task<ApiResponse<object>> GetUserTicketsWithEventDetailsAsync(Guid userId);
    }
}
