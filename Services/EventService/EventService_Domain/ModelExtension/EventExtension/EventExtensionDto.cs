using Common;
using EventService_Domain.Enums;

namespace EventService_Domain.ModelExtension.EventExtension
{
    public record EventExtensionDto : BaseQueryParams
    {
        public string? Title { get; set; } = string.Empty;
        public EventRole? Role = null;
        public int? Status = null;
    }
}
