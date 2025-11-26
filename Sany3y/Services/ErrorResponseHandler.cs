using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json;

namespace Sany3y.Services
{
    public static class ErrorResponseHandler
    {
        public static async Task<List<string>> SafeReadErrors(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(content))
                return new List<string> { "Unknown server error." };

            try
            {
                return JsonSerializer.Deserialize<List<string>>(content)
                       ?? new List<string> { "Unknown server error." };
            }
            catch
            {
                return new List<string> { content };
            }
        }

        public static async Task<bool> HandleResponseErrors(HttpResponseMessage response, ModelStateDictionary modelState)
        {
            if (response.IsSuccessStatusCode)
                return true;

            var errors = await SafeReadErrors(response);

            foreach (var e in errors)
                modelState.AddModelError(string.Empty, e);

            return false;
        }
    }
}