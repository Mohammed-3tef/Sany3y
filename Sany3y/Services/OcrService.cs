using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Sany3y.Infrastructure.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace Sany3y.Services
{
    public class OcrService
    {
        private readonly HttpClient _http;

        public OcrService(HttpClient http)
        {
            _http = http;
        }

        public async Task<string> DetectNationalIdAsync(IFormFile file)
        {
            using var content = new MultipartFormDataContent();

            // Copy file to MemoryStream
            await using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            ms.Position = 0;

            var sc = new StreamContent(ms);
            sc.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

            // ⚠ Parameter name MUST match API ("file")
            content.Add(sc, "file", file.FileName);

            // Optional: send a model_type string if needed by API
            content.Add(new StringContent("arabic_numbers"), "model_type");

            var response = await _http.PostAsync("/api/Services/predict", content);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"API error: {response.StatusCode} - {error}");
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            return doc.RootElement.GetProperty("id_number").GetString();
        }
    }
}
