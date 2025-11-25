using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace Sany3y.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicesController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ServicesController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost("predict")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Predict(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            // قراءة الصورة في بايت
            byte[] imageBytes;
            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                imageBytes = ms.ToArray();
            }

            // إنشاء HttpClient
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri("http://localhost:8000"); // رابط Python API

            using (var formData = new MultipartFormDataContent())
            {
                // إرسال model_type مع الصورة
                formData.Add(new StringContent("arabic_numbers"), "model_type");

                var imageContent = new ByteArrayContent(imageBytes);
                imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse(file.ContentType);
                formData.Add(imageContent, "file", file.FileName);

                var response = await client.PostAsync("/predict/", formData);
                response.EnsureSuccessStatusCode(); // لرمي الاستثناء لو حدث خطأ
                string result = await response.Content.ReadAsStringAsync();
                return Content(result, "application/json");
            }
        }
    }
}