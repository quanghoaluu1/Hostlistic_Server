using EventService_Domain.Enums;

namespace EventService_Domain.Constants;

public static class EventPermissions
{
    public const string CanEditEvent = "can_edit_event";
    public const string CanManageTickets = "can_manage_tickets";
    public const string CanManageSessions = "can_manage_sessions";
    public const string CanManageTalent = "can_manage_talent";
    public const string CanManageTeam = "can_manage_team";
    public const string CanCheckin = "can_checkin";
    public const string CanSendNotifications = "can_send_notifications";
    public const string CanViewAnalytics = "can_view_analytics";
    public const string CanManageSponsors = "can_manage_sponsors";
    public const string CanManageVenue = "can_manage_venue";
    public const string CanExportData = "can_export_data";

    /// <summary>Roles that only the organizer (event owner) can assign.</summary>
    public static readonly HashSet<EventRole> OrganizerOnlyInviteRoles = [EventRole.CoOrganizer];

    /// <summary>Permission keys that only the organizer can grant.</summary>
    public static readonly HashSet<string> OrganizerOnlyPermissions = [CanManageTeam];

    /// <summary>Returns the default permission preset for the given role.</summary>
    public static Dictionary<string, bool> GetPreset(EventRole role) => role switch
    {
        EventRole.CoOrganizer => new Dictionary<string, bool>
        {
            [CanEditEvent] = true,
            [CanManageTickets] = true,
            [CanManageSessions] = true,
            [CanManageTalent] = true,
            [CanManageTeam] = false,
            [CanCheckin] = true,
            [CanSendNotifications] = true,
            [CanViewAnalytics] = true,
            [CanManageSponsors] = true,
            [CanManageVenue] = true,
            [CanExportData] = true,
        },
        EventRole.Staff => new Dictionary<string, bool>
        {
            [CanEditEvent] = false,
            [CanManageTickets] = false,
            [CanManageSessions] = false,
            [CanManageTalent] = false,
            [CanManageTeam] = false,
            [CanCheckin] = true,
            [CanSendNotifications] = false,
            [CanViewAnalytics] = false,
            [CanManageSponsors] = false,
            [CanManageVenue] = false,
            [CanExportData] = false,
        },
        _ => [],
    };
}
