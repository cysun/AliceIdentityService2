using System.ComponentModel.DataAnnotations;

namespace AliceIdentityService.Models;

public class StatusViewModel
{
    public string Subject { get; set; }
    public string Message { get; set; }
}

public class ErrorViewModel
{
    public string Subject { get; set; }
    public string Message { get; set; }
}

public class AuthorizeViewModel
{
    [Display(Name = "Application")]
    public string ApplicationName { get; set; }

    [Display(Name = "Scope")]
    public string Scope { get; set; }
}
