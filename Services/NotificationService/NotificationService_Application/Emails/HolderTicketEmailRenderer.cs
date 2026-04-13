using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using NotificationService_Application.Dtos;
using QRCoder;

namespace NotificationService_Application.Emails;

public sealed class HolderTicketEmailRenderer(IWebHostEnvironment env, ILogger<HolderTicketEmailRenderer> logger)
{
    private const string TemplatePath = "Templates/Email/HolderTicketDelivery.html";
    private const string ItemPartialPath = "Templates/Email/HolderTicketItem.html";
 
    public async Task<string> RenderAsync(HolderTicketEmailModel model)
    {
        var templateHtml = await LoadTemplateAsync(TemplatePath);
        var itemPartialHtml = await LoadTemplateAsync(ItemPartialPath);
 
        // Build each ticket item
        var ticketsHtml = string.Concat(model.Tickets.Select(t =>
            
            itemPartialHtml
                .Replace("{{TicketTypeName}}", Encode(t.TicketTypeName))
                .Replace("{{HolderName}}", Encode(model.HolderName))
                .Replace("{{EventName}}", Encode(model.EventName))
                .Replace("{{TicketCode}}", Encode(t.TicketCode))
                .Replace("{{QrCodeUrl}}", BuildQrImageUrl(t.QrCodeUrl))
                .Replace("{{Price}}", FormatPrice(t.Price))
        ));
 
        return templateHtml
            .Replace("{{HolderName}}", Encode(model.HolderName))
            .Replace("{{BuyerName}}", Encode(model.BuyerName))
            .Replace("{{EventName}}", Encode(model.EventName))
            .Replace("{{EventDate}}", Encode(model.EventDate.ToString("dddd, MMMM d, yyyy · h:mm tt")))
            .Replace("{{EventLocation}}", Encode(model.EventLocation))
            .Replace("{{TicketsHtml}}", ticketsHtml)  // Raw HTML — no encoding
            .Replace("{{PortalUrl}}", model.PortalUrl)
            .Replace("{{LogoUrl}}", model.LogoUrl ?? string.Empty)
            .Replace("{{Year}}", DateTime.UtcNow.Year.ToString());
    }
 
    private async Task<string> LoadTemplateAsync(string relativePath)
    {
        var fullPath = Path.Combine(env.ContentRootPath, relativePath);
        if (!File.Exists(fullPath))
        {
            logger.LogError("Email template not found at path: {Path}", fullPath);
            throw new FileNotFoundException($"Email template not found: {relativePath}");
        }
        return await File.ReadAllTextAsync(fullPath);
    }
 
    // Encode user-supplied data to prevent XSS inside HTML attributes / text nodes
    private static string Encode(string? value) =>
        System.Net.WebUtility.HtmlEncode(value ?? string.Empty);
    
    private static string FormatPrice(decimal price) =>
        price == 0 ? "Free" : $"VND {price:N0}";
    
    private static string BuildQrImageUrl(string data)
    {
        var encoded = Uri.EscapeDataString(data);
        return $"https://api.qrserver.com/v1/create-qr-code/?size=150x150&data={encoded}&margin=6&bgcolor=ffffff";
    }
}