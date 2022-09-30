using AliceIdentityService.Models;
using FluentEmail.Core;

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
    private readonly IFluentEmail _email;

    private ILogger<EmailSender> _logger;

    public EmailSender(IWebHostEnvironment env, EmailSettings settings, IFluentEmail email, ILogger<EmailSender> logger)
    {
        _templateFolder = $"{env.ContentRootPath}/EmailTemplates";
        _settings = settings;
        _email = email;
        _logger = logger;
    }

    public async void SendEmailVerificationMessageAsync(User user, string link)
    {
        var email = _email
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
