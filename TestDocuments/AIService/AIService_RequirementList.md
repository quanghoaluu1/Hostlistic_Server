# Requirement & Function List — AI Service

**Project Name**: Hostlistic AI Service
**Project Code**: HI-AI-01
**Normal number of Test cases/KLOC**: 100
**Test Environment Setup Description**: List environment requirement in this system

---

| No | Requirement | Class Name | Function Name | Function Code | Sheet Name | Description | Pre-Condition |
|:---:|:---|:---|:---|:---:|:---|:---|:---|
| 1 | REQ-AI-DESC-001 | AiContentService | GenerateDescriptionAsync | F001 | AI_DescriptionMatrix | Verify generating event description with valid input, ensuring correct prompt building, AI provider call, token tracking, and HTML response. | Event exists. Prompt template exists. AI provider available. |
| 2 | REQ-AI-EMAIL-002 | AiContentService | GenerateEmailAsync | F002 | AI_EmailMatrix | Verify email content generation for confirmation, reminder, and follow-up email types, ensuring correct template selection and content assembly. | Event exists. Organizer is authenticated. |
| 3 | REQ-AI-SOCIAL-003 | AiContentService | GenerateSocialPostAsync | F003 | AI_SocialMatrix | Verify social media post generation per platform (X, LinkedIn, Facebook), ensuring character-limit enforcement and hashtag separation. | Event exists. Platform is specified. |
| 4 | REQ-AI-SPEAKER-004 | AiContentService | GenerateSpeakerIntroAsync | F004 | AI_SpeakerMatrix | Verify speaker introduction generation from event lineup data, supporting both summarize and from-name modes with data quality scoring. | Event exists. Talent exists in lineup. |
| 5 | REQ-AI-SESSION-005 | AiContentService | GenerateSessionAbstractAsync | F005 | AI_SessionMatrix | Verify session abstract generation from track/session data, supporting expand and rewrite modes with proper validation. | Event exists. Session exists in tracks. |
| 6 | REQ-AI-TOKEN-006 | AiContentService | GetAIToken | F006 | AI_TokenMatrix | Verify retrieval and aggregation of AI token usage chart data grouped by day for dashboard display. | AI usage data exists in repository. |
| 7 | REQ-AI-ENT-007 | AiPlanEntitlementService | EnsureCanUseAiAsync | F007 | AI_EntitlementMatrix | Verify subscription plan validation enforces AI access restriction based on plan features, returning structured 403 responses for ineligible users. | User exists. Subscription plan data is seeded. |
| 8 | REQ-AI-TPL-008 | PromptTemplateService | GetAllAsync | F008 | AI_TemplateMatrix | Verify retrieval of all available prompt templates for admin management panel. | At least one template exists in database. |
| 9 | REQ-AI-TPL-009 | PromptTemplateService | GetByCategoryAsync | F009 | AI_TemplateMatrix | Verify filtering of prompt templates by category (e.g., Email, Description, Social). | Templates with specified category exist. |
| 10 | REQ-AI-TPL-010 | PromptTemplateService | GetByKeyAsync | F010 | AI_TemplateMatrix | Verify retrieval of a single prompt template by its unique enum key. | Template with specified key exists. |
| 11 | REQ-AI-TPL-011 | PromptTemplateService | GetByIdAsync | F011 | AI_TemplateMatrix | Verify retrieval of a single prompt template by its GUID. | Template with specified ID exists. |
| 12 | REQ-AI-TPL-012 | PromptTemplateService | CreateAsync | F012 | AI_TemplateMatrix | Verify creation of a new prompt template with unique key validation and duplicate prevention. | No template with the same key exists. |
| 13 | REQ-AI-TPL-013 | PromptTemplateService | UpdateAsync | F013 | AI_TemplateMatrix | Verify updating existing prompt template content and metadata with not-found handling. | Template with specified ID exists. |
| 14 | REQ-AI-TPL-014 | PromptTemplateService | DeleteAsync | F014 | AI_TemplateMatrix | Verify soft or hard deletion of prompt template with proper not-found error handling. | Template with specified ID exists. |
