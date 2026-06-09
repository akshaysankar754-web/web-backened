using System.Net;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using InternshipPortalApi.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace InternshipPortalApi.Services
{
    public class EmailSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty;
        public string FromDisplayName { get; set; } = string.Empty;
    }

    public class BrevoSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty;
        public string FromDisplayName { get; set; } = string.Empty;
    }

    public class MailtrapSettings
    {
        public string ApiToken { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty;
        public string FromDisplayName { get; set; } = string.Empty;
    }

    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string to, string subject, string body);
        Task<bool> SendOtpAsync(string to, string otp);
    }

    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly BrevoSettings _brevoSettings;
        private readonly MailtrapSettings _mailtrapSettings;
        private readonly ILogger<EmailService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public EmailService(
            IConfiguration configuration,
            ILogger<EmailService> logger,
            IHttpClientFactory httpClientFactory)
        {
            _settings = configuration.GetSection("Smtp").Get<EmailSettings>() ?? new EmailSettings();
            _brevoSettings = configuration.GetSection("Brevo").Get<BrevoSettings>() ?? new BrevoSettings();
            _mailtrapSettings = configuration.GetSection("Mailtrap").Get<MailtrapSettings>() ?? new MailtrapSettings();
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            if (!string.IsNullOrWhiteSpace(_brevoSettings.Host)
                && !string.IsNullOrWhiteSpace(_brevoSettings.Username)
                && !string.IsNullOrWhiteSpace(_brevoSettings.Password)
                && !string.IsNullOrWhiteSpace(_brevoSettings.From))
            {
                return await SendViaBrevoAsync(to, subject, body);
            }

            if (!string.IsNullOrWhiteSpace(_settings.Host) && !string.IsNullOrWhiteSpace(_settings.From))
            {
                return await SendViaSmtpAsync(to, subject, body);
            }

            if (!string.IsNullOrWhiteSpace(_mailtrapSettings.ApiToken))
            {
                return await SendViaMailtrapApiAsync(to, subject, body);
            }

            // Development fallback: log the email instead of throwing to avoid 500s.
            _logger.LogWarning("SMTP settings are not configured. Skipping actual email send. To enable, add either Brevo or Smtp settings to appsettings.json.");
            _logger.LogInformation("Email to: {To}\nSubject: {Subject}\nBody: {Body}", to, subject, body);
            return false;
        }

        public Task<bool> SendOtpAsync(string to, string otp)
        {
            var subject = "Your Internship Portal OTP";
            var body = $"Your OTP is {otp}. It expires in 5 minutes.";

            return SendEmailAsync(to, subject, body);
        }

        private async Task<bool> SendViaSmtpAsync(string to, string subject, string body)
        {
            using var message = new MailMessage();
            message.From = new MailAddress(_settings.From, _settings.FromDisplayName);
            message.To.Add(new MailAddress(to));
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = false;

            using var client = new System.Net.Mail.SmtpClient(_settings.Host, _settings.Port)
            {
                EnableSsl = _settings.EnableSsl,
                Credentials = new NetworkCredential(_settings.Username, _settings.Password)
            };

            try
            {
                await client.SendMailAsync(message);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}", to);
                return false;
            }
        }

        private async Task<bool> SendViaBrevoAsync(string to, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_brevoSettings.FromDisplayName, _brevoSettings.From));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new TextPart("plain")
            {
                Text = body
            };

            using var client = new MailKit.Net.Smtp.SmtpClient();

            try
            {
                await client.ConnectAsync(
                    _brevoSettings.Host,
                    _brevoSettings.Port,
                    _brevoSettings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);

                await client.AuthenticateAsync(
                    _brevoSettings.Username,
                    _brevoSettings.Password);

                await client.SendAsync(message);
                await client.DisconnectAsync(true);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email through Brevo to {To}", to);
                return false;
            }
        }

        private async Task<bool> SendViaMailtrapApiAsync(string to, string subject, string body)
        {
            var payload = new
            {
                from = new
                {
                    email = string.IsNullOrWhiteSpace(_mailtrapSettings.From) ? "no-reply@internship-portal.test" : _mailtrapSettings.From,
                    name = string.IsNullOrWhiteSpace(_mailtrapSettings.FromDisplayName) ? "Internship Portal" : _mailtrapSettings.FromDisplayName
                },
                to = new[] { new { email = to } },
                subject,
                text = body
            };

            var client = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://send.api.mailtrap.io/api/send");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _mailtrapSettings.ApiToken);
            request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            try
            {
                var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Mailtrap API send failed ({StatusCode}): {Body}", response.StatusCode, responseBody);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email through Mailtrap API to {To}", to);
                return false;
            }
        }
    }
}
