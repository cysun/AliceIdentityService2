using AliceIdentityService.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Scriban;

namespace AliceIdentityService.Services;

public class EmailSettings
{
    public string AppUrl { get; set; }
    public string SenderName { get; set; }
    public string SenderEmail { get; set; }
    public string Host { get; set; }
    public int Port { get; set; }
    public bool RequireAuthentication { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
}

public class EmailSender
{
    private readonly string _templateFolder;
    private readonly EmailSettings _settings;

    private ILogger<EmailSender> _logger;

    public EmailSender(IWebHostEnvironment env, IOptions<EmailSettings> settings, ILogger<EmailSender> logger)
    {
        _templateFolder = $"{env.ContentRootPath}/EmailTemplates";
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendEmailVerificationMessageAsync(User user, string link)
    {
        var msg = new MimeMessage();
        msg.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        msg.To.Add(new MailboxAddress(user.FullName, user.Email));
        msg.Subject = "AIS - Email Verification";

        var template = Template.Parse(File.ReadAllText($"{_templateFolder}/EmailVerification.Body.txt"));
        msg.Body = new TextPart("html")
        {
            Text = template.Render(new { link = $"{_settings.AppUrl}{link}" })
        };

        await SendAsync(msg);
    }

    public async Task SendResetPasswordMessageAsync(string email, string link)
    {
        var msg = new MimeMessage();
        msg.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        msg.To.Add(new MailboxAddress(email, email));
        msg.Subject = "AIS - Reset Password";

        var template = Template.Parse(File.ReadAllText($"{_templateFolder}/ResetPassword.Body.txt"));
        msg.Body = new TextPart("html")
        {
            Text = template.Render(new { link = $"{_settings.AppUrl}{link}" })
        };

        await SendAsync(msg);
    }

    public async Task SendAsync(MimeMessage message)
    {
        using var client = new SmtpClient();

        try
        {
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;
            await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.None);
            if (_settings.RequireAuthentication)
                await client.AuthenticateAsync(_settings.Username, _settings.Password);

            await client.SendAsync(message);

            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email error");
        }
    }
}
