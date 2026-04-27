using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QuanLyDoanKham.API.Services
{
    public interface IGeminiService
    {
        Task<string> GetStaffSuggestionAsync(string prompt);
        Task<string> GetClinicalSummaryAsync(string prompt);
    }

    /// <summary>
    /// Lưu ý: Tên lớp vẫn giữ là GeminiService để tránh phải sửa đổi Dependency Injection ở nhiều nơi,
    /// nhưng logic bên trong đã được chuyển sang sử dụng Groq Cloud API (OpenAI Format).
    /// </summary>
    public class GeminiService : IGeminiService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public GeminiService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task<string> GetStaffSuggestionAsync(string prompt)
        {
            return await CallGroqAsync(prompt);
        }

        public async Task<string> GetClinicalSummaryAsync(string prompt)
        {
            return await CallGroqAsync(prompt);
        }

        private async Task<string> CallGroqAsync(string prompt)
        {
            var apiKey = _configuration["AppSettings:GroqApiKey"];
            if (string.IsNullOrEmpty(apiKey))
                throw new Exception("Groq API Key is missing in configuration. Please add 'GroqApiKey' to AppSettings.");

            var model = _configuration["AppSettings:GroqModel"] ?? "llama-3.3-70b-versatile";
            var url = "https://api.groq.com/openai/v1/chat/completions";

            var requestBody = new
            {
                model = model,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                temperature = 0.7,
                max_tokens = 2048
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Thêm Header Authorization cho Groq
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var response = await _httpClient.PostAsync(url, content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[DEBUG] Groq API Error: {responseString}");
                throw new Exception($"Groq API error: {responseString}");
            }

            using var doc = JsonDocument.Parse(responseString);
            var text = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "";

            return text;
        }
    }
}
