using Microsoft.AspNetCore.Mvc;
using Sany3y.Infrastructure.Models;

namespace Sany3y.Controllers
{
    public class PaymentController : Controller
    {
        private readonly HttpClient _http;

        public PaymentController(IHttpClientFactory httpClientFactory)
        {
            _http = httpClientFactory.CreateClient();
            _http.BaseAddress = new Uri("https://localhost:7178/");
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(int taskId, decimal price, string serviceName, long customerId)
        {
            var request = new
            {
                TaskId = taskId,
                Price = price,
                ServiceName = serviceName,
                CustomerId = customerId
            };

            var response = await _http.PostAsJsonAsync("api/Payment/create-checkout-session", request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<CheckoutResponse>();
                return Ok(new { url = result.Url });
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return BadRequest(new { error = "Failed to initiate payment", details = errorContent });
        }

        public async Task<IActionResult> Success(string session_id)
        {
            if (string.IsNullOrEmpty(session_id))
            {
                return RedirectToAction("Index", "Home");
            }

            // Call API to verify payment
            var response = await _http.GetAsync($"api/Payment/verify-payment/{session_id}");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<PaymentVerificationResult>();
                ViewBag.PaymentStatus = result.Status;
                ViewBag.TaskId = result.TaskId;
            }
            else
            {
                ViewBag.PaymentStatus = "Failed";
            }

            return View();
        }

        public class PaymentVerificationResult
        {
            public string Status { get; set; }
            public string TaskId { get; set; }
        }

        public async Task<IActionResult> Cancel(int taskId)
        {
            if (taskId > 0)
            {
                // Call API to cancel the booking and release the slot
                await _http.PostAsync($"api/TechnicianSchedule/CancelBooking/{taskId}", null);
            }
            return View();
        }

        public class CheckoutResponse
        {
            [System.Text.Json.Serialization.JsonPropertyName("url")]
            public string Url { get; set; }
        }
    }
}
