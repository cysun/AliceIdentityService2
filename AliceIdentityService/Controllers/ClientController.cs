using System.ComponentModel.DataAnnotations;
using AliceIdentityService.Helpers;
using AliceIdentityService.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Core;
using OpenIddict.EntityFrameworkCore.Models;

namespace AliceIdentityService.Controllers
{
    [Authorize(Policy = AisConstants.Policy.IsAdmin)]
    public class ClientController : Controller
    {
        private readonly OpenIddictApplicationManager<OpenIddictEntityFrameworkCoreApplication> _applicationManager;

        private readonly IMapper _mapper;
        private readonly ILogger<ClientController> _logger;

        public ClientController(OpenIddictApplicationManager<OpenIddictEntityFrameworkCoreApplication> applicationManager,
            IMapper mapper, ILogger<ClientController> logger)
        {
            _applicationManager = applicationManager;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IActionResult> IndexAsync()
        {
            return View(await _applicationManager.ListAsync().ToListAsync());
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View(new ApplicationInputModel());
        }

        public IActionResult GenerateSecret()
        {
            var secret = Utility.GenerateClientSecret();
            return new JsonResult(new { secret });
        }

        [HttpPost]
        public async Task<IActionResult> AddAsync(ApplicationInputModel input)
        {
            if (!ModelState.IsValid) return View(input);

            var descriptor = new OpenIddictApplicationDescriptor
            {
                Permissions =
                {
                    OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                    OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                    OpenIddictConstants.Permissions.Scopes.Profile,
                    OpenIddictConstants.Permissions.Scopes.Email
                },
                ConsentType = OpenIddictConstants.ConsentTypes.Implicit
            };

            _mapper.Map(input, descriptor);

            var client = await _applicationManager.CreateAsync(descriptor);
            _logger.LogInformation("{user} created new client {client}", User.Identity.Name, client.ClientId);
            // return RedirectToAction("View", new { id = client.Id });
            return RedirectToAction("Index");
        }
    }
}

namespace AliceIdentityService.Models
{
    public class ApplicationInputModel
    {
        public string Id { get; set; }

        [Display(Name = "Display Name")]
        public string DisplayName { get; set; }

        [Required, MaxLength(100), Display(Name = "Client Id")]
        public string ClientId { get; set; }

        [Display(Name = "Client Secret")]
        public string ClientSecret { get; set; }

        [Display(Name = "Redirect URIs")]
        public string RedirectUris { get; set; }

        [Display(Name = "Post-Logout Redirect URIs")]
        public string PostLogoutRedirectUris { get; set; }
    }
}
