using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using AliceIdentityService.Models;
using AliceIdentityService.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AliceIdentityService.Controllers
{
    [Authorize(Policy = AisConstants.Policy.IsAdmin)]
    public class UserController : Controller
    {
        private readonly UserService _userService;
        private readonly UserManager<User> _userManager;

        private readonly IMapper _mapper;
        private readonly ILogger<UserController> _logger;

        public UserController(UserService userService, UserManager<User> userManager, IMapper mapper,
            ILogger<UserController> logger)
        {
            _userService = userService;
            _userManager = userManager;
            _mapper = mapper;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View(_userService.GetUsers());
        }

        public async Task<IActionResult> ViewAsync(string id)
        {
            var user = _userService.GetUser(id);
            if (user == null) return NotFound();

            ViewBag.Claims = await _userManager.GetClaimsAsync(user);

            return View(user);
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View(new RegistrationInputModel());
        }

        [HttpPost]
        public async Task<IActionResult> AddAsync(RegistrationInputModel input)
        {
            if (!ModelState.IsValid) return View(input);

            var user = _mapper.Map<User>(input);
            user.UserName = input.Email;
            user.ScreenName = $"{input.FirstName} {input.LastName}";
            user.EmailConfirmed = true;
            var result = await _userManager.CreateAsync(user, input.Password);
            if (result.Succeeded)
            {
                _logger.LogInformation("{user} created account for {newUser}", User.Identity.Name, input.Email);
                return RedirectToAction("View", new { id = user.Id });
            }
            else
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(input);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditAsync(string id)
        {
            var user = _userService.GetUser(id);
            if (user == null) return NotFound();

            ViewBag.Claims = await _userManager.GetClaimsAsync(user);

            return View(_mapper.Map<EditUserInputModel>(user));
        }

        [HttpPost]
        public async Task<IActionResult> EditAsync(string id, EditUserInputModel input)
        {
            if (!ModelState.IsValid) return View(input);

            var user = _userService.GetUser(id);

            if (!string.IsNullOrWhiteSpace(input.NewPassword))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, input.NewPassword);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                        ModelState.AddModelError("Password", error.Description);
                    return View(input);
                }
            }

            user.EmailConfirmed = input.EmailConfirmed;
            if (!string.IsNullOrWhiteSpace(input.FirstName))
                user.FirstName = input.FirstName;
            if (!string.IsNullOrWhiteSpace(input.LastName))
                user.LastName = input.LastName;
            if (!string.IsNullOrWhiteSpace(input.ScreenName))
                user.ScreenName = input.ScreenName;

            _userService.SaveChanges();

            _logger.LogInformation("{user} edited account {account}", User.Identity.Name, input.Email);

            return RedirectToAction("View", new { id = user.Id });
        }

        public async Task<IActionResult> AddClaimAsync(string userId, string claimType, string claimValue)
        {
            var user = _userService.GetUser(userId);
            var result = await _userManager.AddClaimAsync(user, new Claim(claimType, claimValue));
            if (result.Succeeded)
            {
                _logger.LogError("{user} added claim {claimType}={claimValue} to {account}",
                    User.Identity.Name, claimType, claimValue, userId);
            }
            else
            {
                _logger.LogError("Failed to add claim {claimType}={claimValue} to {account}: {errors}",
                        claimType, claimValue, userId, result.Errors);
            }
            return RedirectToAction("View", new { id = userId });
        }

        public async Task<IActionResult> RemoveClaimAsync(string userId, string claimType, string claimValue)
        {
            var user = _userService.GetUser(userId);
            var result = await _userManager.RemoveClaimAsync(user, new Claim(claimType, claimValue));
            if (result.Succeeded)
            {
                _logger.LogError("{user} removed claim {claimType}={claimValue} from {account}",
                    User.Identity.Name, claimType, claimValue, userId);
            }
            else
            {
                _logger.LogError("Failed to remove claim {claimType}={claimValue} from {account}: {errors}",
                        claimType, claimValue, userId, result.Errors);
            }
            return RedirectToAction("View", new { id = userId });
        }
    }
}

namespace AliceIdentityService.Models
{
    public class EditUserInputModel
    {
        public string Id { get; set; }

        [MaxLength(255), Display(Name = "First Name")]
        public string FirstName { get; set; }

        [MaxLength(255), Display(Name = "Last Name")]
        public string LastName { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [MaxLength(255), Display(Name = "Screen Name")]
        public string ScreenName { get; set; }

        [DataType(DataType.Password), Display(Name = "New Password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password), Display(Name = "Confirm New Password")]
        [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmNewPassword { get; set; }

        [Display(Name = "Email Confirmed")]
        public bool EmailConfirmed { get; set; }

        public string FullName => $"{FirstName} {LastName}";
    }
}
