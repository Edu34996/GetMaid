using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using Utils.Models;

namespace Business.Services
{
    public class BrevoEmailSender : IEmailSender
    {
        private readonly HttpClient _httpClient;
        private readonly BrevoSettings _settings;

        public BrevoEmailSender(HttpClient httpClient, IOptions<BrevoSettings> options)
        {
            _httpClient = httpClient;
            _settings = options.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var payload = new
            {
                sender = new { name = _settings.FromName, email = _settings.FromEmail },
                to = new[] { new { email } },
                subject,
                htmlContent = htmlMessage
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Add("api-key", _settings.ApiKey);
            request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Brevo send failed: {(int)response.StatusCode} {body}");
            }
        }
    }
}