var builder = DistributedApplication.CreateBuilder(args);

// ── Secrets ───────────────────────────────────────────────
var jwtKey                  = builder.AddParameter("JwtKey", secret: true);
var geminiApiKey            = builder.AddParameter("GeminiApiKey", secret: true);
var resendApiKey            = builder.AddParameter("ResendApiKey", secret: true);
var cloudinaryCloud         = builder.AddParameter("CloudinaryCloud");
var cloudinaryKey           = builder.AddParameter("CloudinaryApiKey", secret: true);
var cloudinarySecret        = builder.AddParameter("CloudinarySecret", secret: true);
var googleClientId          = builder.AddParameter("GoogleClientId");
var payosClientId           = builder.AddParameter("PayosClientId");
var payosApiKey             = builder.AddParameter("PayosApiKey", secret: true);
var payosChecksumKey        = builder.AddParameter("PayosChecksumKey", secret: true);
var frontendUrl             = builder.AddParameter("FrontendUrl");
var qrSecret                = builder.AddParameter("QrSecret", secret: true);

// ── Cloud Connection Strings (pass-through, không spin up local container) ──
var identityDb      = builder.AddConnectionString("IdentityDbConnection");
var eventDb         = builder.AddConnectionString("EventDbConnection");
var bookingDb       = builder.AddConnectionString("BookingDbConnection");
var notificationDb  = builder.AddConnectionString("NotificationDbConnection");
var aiDb            = builder.AddConnectionString("AiDbConnection");
var streamingDb     = builder.AddConnectionString("StreamingDbConnection");
var redis           = builder.AddConnectionString("RedisConnection");

var rabbitMq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin();
// ── Services ──────────────────────────────────────────────
var identityService = builder.AddProject<Projects.IdentityService_Api>("identity-service")
    .WithReference(identityDb)
    .WithReference(redis)
    .WithEnvironment("Jwt__Key", jwtKey)
    .WithEnvironment("Jwt__Issuer", "hostlistic")
    .WithEnvironment("Jwt__Audience", "hostlistic")
    .WithEnvironment("CloudinarySettings__CloudName", cloudinaryCloud)
    .WithEnvironment("CloudinarySettings__ApiKey", cloudinaryKey)
    .WithEnvironment("CloudinarySettings__ApiSecret", cloudinarySecret)
    .WithEnvironment("Google__ClientId", googleClientId)
    .WithEnvironment("Services__NotificationService", "http://localhost:5097")
    .WithEnvironment("Services__BookingService", "http://localhost:5077");

var eventService = builder.AddProject<Projects.EventService_Api>("event-service")
    .WithReference(eventDb)
    .WithReference(redis)
    .WithReference(rabbitMq)
    .WithEnvironment("Jwt__Key", jwtKey)
    .WithEnvironment("Jwt__Issuer", "hostlistic")
    .WithEnvironment("Jwt__Audience", "hostlistic");

var bookingService = builder.AddProject<Projects.BookingService_Api>("booking-service")
    .WithReference(bookingDb)
    .WithReference(rabbitMq)
    .WithEnvironment("Jwt__Key", jwtKey)
    .WithEnvironment("Jwt__Issuer", "hostlistic")
    .WithEnvironment("Jwt__Audience", "hostlistic")
    .WithEnvironment("CloudinarySettings__CloudName", cloudinaryCloud)
    .WithEnvironment("CloudinarySettings__ApiKey", cloudinaryKey)
    .WithEnvironment("CloudinarySettings__ApiSecret", cloudinarySecret)
    .WithEnvironment("PayOs__ClientId", payosClientId)
    .WithEnvironment("PayOs__ApiKey", payosApiKey)
    .WithEnvironment("PayOs__ChecksumKey", payosChecksumKey)
    .WithEnvironment("ServiceUrls__EventService", "http://localhost:5139")
    .WithEnvironment("ServiceUrls__IdentityService", "http://localhost:5049")
    .WithEnvironment("ServiceUrls__NotificationService", "http://localhost:5097")
    .WithEnvironment("FrontendUrl", frontendUrl)
    .WithEnvironment("QrSecret", qrSecret);

var notificationService = builder.AddProject<Projects.NotificationService_Api>("notification-service")
        .WithReference(notificationDb)
        .WithReference(redis)
        .WithReference(rabbitMq)
        .WithEnvironment("Jwt__Key", jwtKey)
        .WithEnvironment("Jwt__Issuer", "hostlistic")
        .WithEnvironment("Jwt__Audience", "hostlistic")
        .WithEnvironment("Resend__ApiToken", resendApiKey);

var aiService = builder.AddProject<Projects.AIService_Api>("ai-service")
    .WithReference(aiDb)
    .WithEnvironment("Jwt__Key", jwtKey)
    .WithEnvironment("Jwt__Issuer", "hostlistic")
    .WithEnvironment("Jwt__Audience", "hostlistic")
    .WithEnvironment("Gemini__ApiKey", geminiApiKey);

var streamingService = builder.AddProject<Projects.StreamingService_Api>("streaming-service")
    .WithReference(streamingDb)
    .WithEnvironment("Jwt__Key", jwtKey)
    .WithEnvironment("Jwt__Issuer", "hostlistic")
    .WithEnvironment("Jwt__Audience", "hostlistic");
builder.AddProject<Projects.ApiGateway>("api-gateway")
    .WithReference(identityService)
    .WithReference(eventService)
    .WithReference(bookingService)
    .WithReference(notificationService)
    .WithReference(aiService);

builder.Build().Run();
