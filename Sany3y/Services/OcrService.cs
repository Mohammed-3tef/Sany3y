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
            using var stream = file.OpenReadStream();

            var sc = new StreamContent(stream);
            sc.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

            content.Add(sc, "image", file.FileName);

            var response = await _http.PostAsync("/api/Services/predict", content);
            if (!response.IsSuccessStatusCode) 
                return null;

            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);

            return doc.RootElement.GetProperty("national_id").GetString();
        }
    }
}
