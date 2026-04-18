# Requirement & Function List — Streaming Service

**Project Name**: Hostlistic Streaming Service
**Project Code**: HI-STRM-02
**Normal number of Test cases/KLOC**: 100
**Test Environment Setup Description**: List environment requirement in this system

---

| No | Requirement | Class Name | Function Name | Function Code | Sheet Name | Description | Pre-Condition |
|:---:|:---|:---|:---|:---:|:---|:---|:---|
| 1 | REQ-STRM-LK-001 | LiveKitService | CreateRoomAsync | F010 | STRM_LiveKitMatrix | Verify creation of a LiveKit room via the server API with correct parameters, handling success and failure responses. | LiveKit server is reachable. API key is configured. |
| 2 | REQ-STRM-LK-002 | LiveKitService | EndRoomAsync | F011 | STRM_LiveKitMatrix | Verify termination of an active LiveKit room, ensuring the API call is sent and error responses are handled gracefully. | Room exists in LiveKit server. |
| 3 | REQ-STRM-TK-003 | TokenGenerator | GenerateLiveKitToken | F012 | STRM_TokenMatrix | Verify JWT token generation for LiveKit participants, ensuring correct role-based permissions (canPublish, roomAdmin) are embedded in the token claims. | HMAC secret is at least 256-bit. |
| 4 | REQ-STRM-GUEST-004 | GuestStreamAccessService | GetAttemptStatus | F013 | STRM_GuestMatrix | Verify retrieval of the current failed-attempt count and block status for a guest by event and IP key. | In-memory state is initialized. |
| 5 | REQ-STRM-GUEST-005 | GuestStreamAccessService | RegisterFailedAttempt | F014 | STRM_GuestMatrix | Verify incrementing of failed attempt count and automatic IP block after exceeding the threshold (5 attempts → 10-min block). | In-memory state is initialized. |
| 6 | REQ-STRM-GUEST-006 | GuestStreamAccessService | ResetAttempts | F015 | STRM_GuestMatrix | Verify that a successful guest login resets the failed-attempt counter for a given event + IP key. | Failed attempts exist for the key. |
| 7 | REQ-STRM-GUEST-007 | GuestStreamAccessService | CreateOrReplaceSession | F016 | STRM_GuestMatrix | Verify creation of a new guest live session and replacement of any previous session tied to the same ticket. | Valid ticket validation DTO provided. |
| 8 | REQ-STRM-GUEST-008 | GuestStreamAccessService | TryGetActiveSession | F017 | STRM_GuestMatrix | Verify retrieval of an active, non-expired session by ticket ID, with automatic cleanup of expired entries. | Session was previously created. |
| 9 | REQ-STRM-GUEST-009 | GuestStreamAccessService | TouchSession | F018 | STRM_GuestMatrix | Verify session heartbeat renewal, updating the TTL and last-seen timestamp. Expired sessions are released automatically. | Active session exists for sessionId. |
| 10 | REQ-STRM-GUEST-010 | GuestStreamAccessService | ReleaseSession | F019 | STRM_GuestMatrix | Verify manual release of a guest session, removing it from both the session and ticket-mapping caches. | Session was previously created. |
| 11 | REQ-STRM-CLIENT-011 | EventServiceClient | VerifyStreamAccessAsync | F020 | STRM_ClientMatrix | Verify cross-service HTTP call to Event Service to check if a user has permission to join a specific room. | Event Service is available at configured URL. |
| 12 | REQ-STRM-CLIENT-012 | BookingServiceClient | ValidateGuestLiveTicketAsync | F021 | STRM_ClientMatrix | Verify HTTP call to Booking Service to validate a guest's physical ticket code for live stream access. | Booking Service is available. Valid ticket code provided. |
| 13 | REQ-STRM-REC-013 | LocalRecordingStorageService | SaveAsync | F022 | STRM_RecordingMatrix | Verify saving of an uploaded recording stream to local disk with correct naming, directory structure, and playback URL generation. | Configured local storage path is writable. |
| 14 | REQ-STRM-HUB-014 | StreamingHub | JoinEventGroup | F023 | STRM_HubMatrix | Verify that a SignalR client is added to the correct event group on connection, enabling real-time broadcast. | Client is connected via SignalR. |
| 15 | REQ-STRM-HUB-015 | StreamingHub | SendEventChatMessage | F024 | STRM_HubMatrix | Verify chat message broadcasting to an event group with authentication check, chat-block enforcement, and moderation handling. | User is authenticated. Event session is active. |
