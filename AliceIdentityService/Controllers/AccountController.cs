using System.ComponentModel.DataAnnotations;
using System.Text;
using AliceIdentityService.Models;
using AliceIdentityService.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace AliceIdentityService.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly EmailSender _emailSender;

        private readonly IMapper _mapper;
        private readonly ILogger<AccountController> _logger;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager,
            EmailSender emailSender, IMapper mapper, ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginInputModel());
        }

        [HttpPost]
        public async Task<IActionResult> LoginAsync(LoginInputModel input, string returnUrl)
        {
            if (!ModelState.IsValid) return View(input);

            returnUrl ??= Url.Content("~/");

            var result = await _signInManager.PasswordSignInAsync(input.Email, input.Password, input.RememberMe, lockoutOnFailure: true);
            if (result.Succeeded)
            {
                _logger.LogInformation("{user} signed in", input.Email);
                return Redirect(returnUrl);
            }
            else
            {
                _logger.LogInformation("{user} failed to log in. LockedOut: {lockedOut}; NotAllowed: {notAllowed}",
                    input.Email, result.IsLockedOut, result.IsNotAllowed);
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(input);
            }
        }

        public async Task<IActionResult> LogoutAsync(string returnUrl)
        {
            var name = User.Identity.Name;
            await _signInManager.SignOutAsync();
            _logger.LogInformation("{user} signed out", name);
            return LocalRedirect(returnUrl ?? Url.Content("~/"));
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegistrationInputModel());
        }

        [HttpPost]
        public async Task<IActionResult> RegisterAsync(RegistrationInputModel input)
        {
            if (!ModelState.IsValid) return View(input);

            var user = _mapper.Map<User>(input);
            user.UserName = input.Email;
            user.ScreenName = $"{input.FirstName} {input.LastName}";
            var result = await _userManager.CreateAsync(user, input.Password);
            if (result.Succeeded)
            {
                _logger.LogInformation("New account for {user} created", input.Email);

                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var link = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code });
                await _emailSender.SendEmailVerificationMessageAsync(user, link);
                _logger.LogInformation("Verification email to {email}", user.Email);

                return View("Status", new StatusViewModel
                {
                    Subject = "Registration",
                    Message = $@"Thank you for registering on Alice Identity Service. An email has been
                        sent to the address {user.Email}. Please click on the link in the email to confirm
                        your email address and activate your account."
                });
            }
            else
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(input);
            }
        }

        public async Task<IActionResult> ConfirmEmailAsync(string userId, string code)
        {
            if (userId == null || code == null)
                return LocalRedirect("~/");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound($"Unable to load user with ID '{userId}'.");

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);

            if (result.Succeeded)
            {
                _logger.LogInformation("Email confirmatin successful for {userId}", userId);
                return View("Status", new StatusViewModel
                {
                    Subject = "Email Confirmed",
                    Message = "Thank you for confirming your email. Your account is now activated."
                });
            }
            else
            {
                _logger.LogError("Email confirmation failed for {userId}", userId);
                return View("Error", new ErrorViewModel
                {
                    Message = "Sorry we cannot verify your email."
                });
            }
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null && await _userManager.IsEmailConfirmedAsync(user))
            {
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var link = Url.Action("ResetPassword", "Account", new { code });
                await _emailSender.SendResetPasswordMessageAsync(email, link);
                _logger.LogInformation("ResetPassword email to {email}", user.Email);
            }

            // Don't reveal that the user does not exist or is not confirmed
            return View("Status", new StatusViewModel
            {
                Subject = "Reset Password",
                Message = @"An email is sent to you with instructions on how to reset your password. If you don't
                        get the email after a couple of minutes, please check your spam folder."
            });
        }

        [HttpGet]
        public IActionResult ResetPassword(string code)
        {
            if (code == null)
                return View("Error", new ErrorViewModel
                {
                    Message = "A code must be supplied for password reset."
                });

            return View(new ResetPasswordInputModel
            {
                Code = code
            });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPasswordAsync(ResetPasswordInputModel input)
        {
            if (!ModelState.IsValid) return View(input);

            var statusViewModel = new StatusViewModel
            {
                Subject = "Reset Password",
                Message = "Your password has been reset."
            };

            var user = await _userManager.FindByEmailAsync(input.Email);

            // Don't reveal that the user does not exist
            if (user == null) return View("Status", statusViewModel);

            var result = await _userManager.ResetPasswordAsync(user,
                Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(input.Code)), input.Password);
            if (result.Succeeded)
            {
                _logger.LogInformation("Password reset successful for {email}", input.Email);
                return View("Status", statusViewModel);
            }
            else
            {
                _logger.LogError("Password reset failed for {email}. {result}", input.Email, result);
                return View("Error", new ErrorViewModel
                {
                    Message = "Sorry we cannot reset your password."
                });
            }
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [Authorize]
        public IActionResult Profile()
        {
            return View();
        }
    }
}

namespace AliceIdentityService.Models
{
    public class LoginInputModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }

    public class RegistrationInputModel
    {
        [Required, MaxLength(255), Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required, MaxLength(255), Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        [Required, DataType(DataType.Password), Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class ResetPasswordInputModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, DataType(DataType.Password), Display(Name = "New Password")]
        public string Password { get; set; }

        [Required, DataType(DataType.Password), Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        public string Code { get; set; }
    }
}
