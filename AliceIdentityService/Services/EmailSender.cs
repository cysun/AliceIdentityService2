using AliceIdentityService.Models;
using Microsoft.Extensions.Options;
using MimeKit;
using Scriban;

namespace AliceIdentityService.Services;

public class EmailSettings
{
    public string AppUrl { get; set; }
    public string SenderName { get; set; }
    public string SenderEmail { get; set; }
}

public class EmailSender
{
    private readonly string _templateFolder;
    private readonly EmailSettings _settings;

    private readonly RabbitService _rabbitService;

    private ILogger<EmailSender> _logger;

    public EmailSender(IWebHostEnvironment env, IOptions<EmailSettings> settings, RabbitService rabbitService,
        ILogger<EmailSender> logger)
    {
        _templateFolder = $"{env.ContentRootPath}/EmailTemplates";
        _settings = settings.Value;
        _rabbitService = rabbitService;
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

    public async Task SendAsync(MimeMessage message) => await _rabbitService.SendAsync(message);
}
