using AliceIdentityService.Models;
using FluentEmail.Core;
using Microsoft.Extensions.Options;

namespace AliceIdentityService.Services;

public class EmailSettings
{
    public string AppUrl { get; set; }
    public string SenderName { get; set; }
    public string SenderEmail { get; set; }
    public string SendGridKey { get; set; }
}

public class EmailSender
{
    private readonly string _templateFolder;
    private readonly EmailSettings _settings;
    private readonly IFluentEmail _email;

    private ILogger<EmailSender> _logger;

    public EmailSender(IWebHostEnvironment env, IOptions<EmailSettings> settings,
        IFluentEmail email, ILogger<EmailSender> logger)
    {
        _templateFolder = $"{env.ContentRootPath}/EmailTemplates";
        _settings = settings.Value;
        _email = email;
        _logger = logger;
    }

    public async void SendEmailVerificationMessageAsync(User user, string link)
    {
        var email = _email
             .Tag("AIS") // See https://github.com/lukencode/FluentEmail/issues/317
             .Subject("AIS - Email Verification")
             .To(user.Email, user.FullName)
             .UsingTemplateFromFile($"{_templateFolder}/EmailVerification.Body.txt",
                new { link = $"{_settings.AppUrl}{link}" });

        var response = await email.SendAsync();

        if (response.Successful)
            _logger.LogInformation("Email verification message sent to {email}", user.Email);
        else
            _logger.LogError("Failed to send email notification: {error}", response.ErrorMessages);
    }
}
