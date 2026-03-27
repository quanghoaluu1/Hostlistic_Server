using EventService_Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventService_Application.DTOs
{
    public class SponsorPublicDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? WebsiteUrl { get; set; }
        public string TierName { get; set; } = string.Empty;
    }

    public class TrackInteractionRequest
    {
        public Guid SponsorId { get; set; }
        public InteractionType InteractionType { get; set; }
    }

    public class SponsorInteractionStatsDto
    {
        public Guid SponsorId { get; set; }
        public string SponsorName { get; set; } = string.Empty;
        public Dictionary<string, int> InteractionCounts { get; set; } = new();
        public int TotalInteractions { get; set; }
        public int TotalClickInteractions { get; set; }
    }
}
