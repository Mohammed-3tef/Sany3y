using System.Net.Http.Headers;
using System.Text.Json;

namespace Sany3y.API.Services
{
    public class ApiOcrService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ApiOcrService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string?> DetectNationalIdAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

            byte[] imageBytes;
            await using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                imageBytes = ms.ToArray();
            }

            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri("http://localhost:8000");

            using var formData = new MultipartFormDataContent();
            formData.Add(new StringContent("arabic_numbers"), "model_type");

            var imageContent = new ByteArrayContent(imageBytes);
            imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse(
                string.IsNullOrWhiteSpace(file.ContentType) ? "image/jpeg" : file.ContentType
            );
            formData.Add(imageContent, "file", file.FileName);

            var response = await client.PostAsync("/predict/", formData);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("id_number", out var prop))
            {
                return prop.GetString();
            }

            return null;
        }
    }
}
