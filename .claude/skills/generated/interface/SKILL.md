---
name: interface
description: "Skill for the Interface area of Hostlistic_Server. 16 symbols across 11 files."
---

# Interface

16 symbols | 11 files | Cohesion: 60%

## When to Use

- Working with code in `Services/`
- Understanding how AiRequest, EmailContentResponse, EventServiceClient work
- Modifying interface-related functionality

## Key Files

| File | Symbols |
|------|---------|
| `Services/AIService/AIService_Application/Interface/IPromptTemplateEngine.cs` | Render, BuildEmailParameters, ParseEmailResponse |
| `Services/AIService/AIService_Application/Services/AiContentService.cs` | GenerateEmailAsync, StripHtmlTags |
| `Services/AIService/AIService_Application/Interface/IEventServiceClient.cs` | GetEventByIdAsync, IEventServiceClient |
| `Services/AIService/AIService_Application/Clients/EventServiceClient.cs` | GetEventByIdAsync, EventServiceClient |
| `Services/AIService/AIService_Infrastructure/Repositories/AiRequestRepository.cs` | Update |
| `Services/AIService/AIService_Domain/Interfaces/IAiRequestRepository.cs` | Update |
| `Services/AIService/AIService_Domain/Entities/AiRequest.cs` | AiRequest |
| `Services/AIService/AIService_Application/Services/PromptTemplateEngine.cs` | BuildEmailParameters |
| `Services/AIService/AIService_Application/Interface/IAiContentService.cs` | GenerateEmailAsync |
| `Services/AIService/AIService_Api/Controllers/AiContentController.cs` | GenerateEmail |

## Entry Points

Start here when exploring this area:

- **`AiRequest`** (Class) — `Services/AIService/AIService_Domain/Entities/AiRequest.cs:5`
- **`EmailContentResponse`** (Class) — `Services/AIService/AIService_Application/DTOs/Responses/EmailContentResponse.cs:2`
- **`EventServiceClient`** (Class) — `Services/AIService/AIService_Application/Clients/EventServiceClient.cs:9`
- **`Update`** (Method) — `Services/AIService/AIService_Infrastructure/Repositories/AiRequestRepository.cs:30`
- **`BuildEmailParameters`** (Method) — `Services/AIService/AIService_Application/Services/PromptTemplateEngine.cs:116`

## Key Symbols

| Symbol | Type | File | Line |
|--------|------|------|------|
| `AiRequest` | Class | `Services/AIService/AIService_Domain/Entities/AiRequest.cs` | 5 |
| `EmailContentResponse` | Class | `Services/AIService/AIService_Application/DTOs/Responses/EmailContentResponse.cs` | 2 |
| `EventServiceClient` | Class | `Services/AIService/AIService_Application/Clients/EventServiceClient.cs` | 9 |
| `IEventServiceClient` | Interface | `Services/AIService/AIService_Application/Interface/IEventServiceClient.cs` | 4 |
| `Update` | Method | `Services/AIService/AIService_Infrastructure/Repositories/AiRequestRepository.cs` | 30 |
| `BuildEmailParameters` | Method | `Services/AIService/AIService_Application/Services/PromptTemplateEngine.cs` | 116 |
| `GenerateEmailAsync` | Method | `Services/AIService/AIService_Application/Services/AiContentService.cs` | 132 |
| `GetEventByIdAsync` | Method | `Services/AIService/AIService_Application/Clients/EventServiceClient.cs` | 16 |
| `GenerateEmail` | Method | `Services/AIService/AIService_Api/Controllers/AiContentController.cs` | 70 |
| `Update` | Method | `Services/AIService/AIService_Domain/Interfaces/IAiRequestRepository.cs` | 10 |
| `StripHtmlTags` | Method | `Services/AIService/AIService_Application/Services/AiContentService.cs` | 342 |
| `Render` | Method | `Services/AIService/AIService_Application/Interface/IPromptTemplateEngine.cs` | 7 |
| `BuildEmailParameters` | Method | `Services/AIService/AIService_Application/Interface/IPromptTemplateEngine.cs` | 9 |
| `ParseEmailResponse` | Method | `Services/AIService/AIService_Application/Interface/IPromptTemplateEngine.cs` | 18 |
| `GetEventByIdAsync` | Method | `Services/AIService/AIService_Application/Interface/IEventServiceClient.cs` | 6 |
| `GenerateEmailAsync` | Method | `Services/AIService/AIService_Application/Interface/IAiContentService.cs` | 13 |

## Connected Areas

| Area | Connections |
|------|-------------|
| Services | 8 calls |
| Repositories | 2 calls |
| Interfaces | 1 calls |

## How to Explore

1. `gitnexus_context({name: "AiRequest"})` — see callers and callees
2. `gitnexus_query({query: "interface"})` — find related execution flows
3. Read key files listed above for implementation details
