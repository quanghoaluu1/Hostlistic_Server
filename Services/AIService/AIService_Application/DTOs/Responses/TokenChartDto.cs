namespace AIService_Application.DTOs.Responses
{
    public class TokenChartDto
    {
        public string Date { get; set; } = string.Empty;
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public int TotalTokens { get; set; }
    }
}
