using System;
using System.Collections.Generic;
using System.Text;

namespace EventService_Application.DTOs
{
    public class StreamAuthResponseDto
    {
        public bool IsAllowed { get; set; }
        public string Role { get; set; } = string.Empty; // "Organizer", "CoOrganizer", "Staff", "Attendee"
        public string? ErrorMessage { get; set; }
    }
}
