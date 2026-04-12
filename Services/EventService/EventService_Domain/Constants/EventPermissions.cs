using EventService_Domain.Enums;

namespace EventService_Domain.Constants;

public static class EventPermissions
{
    public const string EditEvent = "can_edit_event";
    public const string ManageTickets = "can_manage_tickets";
    public const string ManageSessions = "can_manage_sessions";
    public const string ManageTalent = "can_manage_talent";
    public const string ManageTeam = "can_manage_team";
    public const string CheckIn = "can_checkin";
    public const string SendNotifications = "can_send_notifications";
    public const string ViewAnalytics = "can_view_analytics";
    public const string ManageSponsors = "can_manage_sponsors";
    public const string ManageVenue = "can_manage_venue";
    public const string ExportData = "can_export_data";

    
    /// <summary>Roles that only the organizer (event owner) can assign.</summary>
    public static readonly HashSet<EventRole> OrganizerOnlyInviteRoles = [EventRole.CoOrganizer];

    /// <summary>Permission keys that only the organizer can grant.</summary>
    public static readonly HashSet<string> OrganizerOnlyPermissions = [ManageTeam];
    /// <summary>
    /// All valid permission keys. Used for validation when setting permissions.
    /// </summary>
    public static readonly IReadOnlySet<string> AllKeys = new HashSet<string>
    {
        EditEvent, ManageTickets, ManageSessions, ManageTalent,
        ManageTeam, CheckIn, SendNotifications, ViewAnalytics,
        ManageSponsors, ManageVenue, ExportData
    };

    /// <summary>
    /// Default permission presets by role. Applied when inviting a new team member.
    /// Matches frontend ROLE_PRESETS in team-member.ts.
    /// </summary>
    public static Dictionary<string, bool> GetPreset(EventRole role) => role switch
    {
        EventRole.CoOrganizer => new()
        {
            [EditEvent] = true,
            [ManageTickets] = true,
            [ManageSessions] = true,
            [ManageTalent] = true,
            [ManageTeam] = false,  // Only Organizer can manage team
            [CheckIn] = true,
            [SendNotifications] = true,
            [ViewAnalytics] = true,
            [ManageSponsors] = true,
            [ManageVenue] = true,
            [ExportData] = true,
        },
        EventRole.Staff => new()
        {
            [EditEvent] = false,
            [ManageTickets] = false,
            [ManageSessions] = false,
            [ManageTalent] = false,
            [ManageTeam] = false,
            [CheckIn] = true,
            [SendNotifications] = false,
            [ViewAnalytics] = false,
            [ManageSponsors] = false,
            [ManageVenue] = false,
            [ExportData] = false,
        },
        _ => new() // Unknown role: no permissions
    };
}
