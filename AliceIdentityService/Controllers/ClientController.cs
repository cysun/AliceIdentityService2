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
        private readonly OpenIddictScopeManager<OpenIddictEntityFrameworkCoreScope> _scopeManager;
        private readonly OpenIddictApplicationManager<OpenIddictEntityFrameworkCoreApplication> _applicationManager;

        private readonly IMapper _mapper;
        private readonly ILogger<ClientController> _logger;

        public ClientController(OpenIddictScopeManager<OpenIddictEntityFrameworkCoreScope> scopeManager,
            OpenIddictApplicationManager<OpenIddictEntityFrameworkCoreApplication> applicationManager,
            IMapper mapper, ILogger<ClientController> logger)
        {
            _scopeManager = scopeManager;
            _applicationManager = applicationManager;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IActionResult> IndexAsync()
        {
            return View(await _applicationManager.ListAsync().ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> ViewAsync(string id)
        {
            var client = await _applicationManager.FindByIdAsync(id);
            if (client == null) return NotFound();

            var descriptor = new OpenIddictApplicationDescriptor();
            await _applicationManager.PopulateAsync(descriptor, client);
            var allowedScopes = Utility.GetAllowedScopes(descriptor);
            var availableScopes = (await _scopeManager.ListAsync().ToListAsync())
                .Where(s => !allowedScopes.Contains(s.Name)).Select(s => s.Name);

            ViewBag.Client = client;
            ViewBag.Scopes = allowedScopes;
            ViewBag.AvailableScopes = availableScopes;

            return View(descriptor);
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View(new ApplicationInputModel() { IsNewClientSecret = true });
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
                    OpenIddictConstants.Permissions.ResponseTypes.Code,
                    OpenIddictConstants.Permissions.Scopes.Profile,
                    OpenIddictConstants.Permissions.Scopes.Email
                },
                Requirements =
                {
                    OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange
                },
                ConsentType = OpenIddictConstants.ConsentTypes.Implicit
            };

            _mapper.Map(input, descriptor);

            var client = await _applicationManager.CreateAsync(descriptor);
            _logger.LogInformation("{user} created new client {client}", User.Identity.Name, client.ClientId);
            return RedirectToAction("View", new { id = client.Id });
        }

        [HttpGet]
        public async Task<IActionResult> EditAsync(string id)
        {
            var client = await _applicationManager.FindByIdAsync(id);
            if (client == null) return NotFound();

            var descriptor = new OpenIddictApplicationDescriptor();
            await _applicationManager.PopulateAsync(descriptor, client);
            var allowedScopes = Utility.GetAllowedScopes(descriptor);
            var availableScopes = (await _scopeManager.ListAsync().ToListAsync())
                .Where(s => !allowedScopes.Contains(s.Name)).Select(s => s.Name);

            ViewBag.Client = client;
            ViewBag.Scopes = allowedScopes;
            ViewBag.AvailableScopes = availableScopes;

            return View(_mapper.Map<ApplicationInputModel>(descriptor));
        }

        [HttpPost]
        public async Task<IActionResult> EditAsync(string id, ApplicationInputModel input)
        {
            if (!ModelState.IsValid) return View(input);

            var client = await _applicationManager.FindByIdAsync(id);
            if (client == null) return NotFound();

            var descriptor = new OpenIddictApplicationDescriptor();
            await _applicationManager.PopulateAsync(descriptor, client);

            _mapper.Map(input, descriptor);
            // It's not easy to map a bool to a readonly collection with Automapper so we just do it here.
            if (input.IsPkce) descriptor.Requirements.Add(OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange);
            else descriptor.Requirements.Remove(OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange);

            await _applicationManager.UpdateAsync(client, descriptor);
            _logger.LogInformation("{user} updated client {client}", User.Identity.Name, descriptor.ClientId);

            return RedirectToAction("View", new { id });
        }

        [HttpGet]
        public async Task<IActionResult> DeleteAsync(string id)
        {
            var client = await _applicationManager.FindByIdAsync(id);
            if (client == null) return NotFound();

            var clientId = client.ClientId;
            await _applicationManager.DeleteAsync(client);
            _logger.LogInformation("{user} deleted client {client}", User.Identity.Name, clientId);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> AddScopeAsync(string clientId, string scope)
        {
            var client = await _applicationManager.FindByIdAsync(clientId);
            if (scope == null) return NotFound();

            var descriptor = new OpenIddictApplicationDescriptor();
            await _applicationManager.PopulateAsync(descriptor, client);

            descriptor.Permissions.Add(OpenIddictConstants.Permissions.Prefixes.Scope + scope);

            await _applicationManager.UpdateAsync(client, descriptor);
            _logger.LogInformation("{user} added scope {scope} to {client}", User.Identity.Name, scope, client.ClientId);

            return RedirectToAction("View", new { id = clientId });
        }

        public async Task<IActionResult> RemoveScopeAsync(string clientId, string scope)
        {
            var client = await _applicationManager.FindByIdAsync(clientId);
            if (scope == null) return NotFound();

            var descriptor = new OpenIddictApplicationDescriptor();
            await _applicationManager.PopulateAsync(descriptor, client);

            descriptor.Permissions.Remove(OpenIddictConstants.Permissions.Prefixes.Scope + scope);

            await _applicationManager.UpdateAsync(client, descriptor);
            _logger.LogInformation("{user} removed scope {scope} from {client}", User.Identity.Name, scope, client.ClientId);

            return RedirectToAction("View", new { id = clientId });
        }

        public IActionResult GenerateSecret()
        {
            var secret = Utility.GenerateClientSecret();
            return new JsonResult(new { secret });
        }
    }
}

namespace AliceIdentityService.Models
{
    public class ApplicationInputModel
    {
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

        public bool IsNewClientSecret { get; set; }

        [Display(Name = "PKCE")]
        public bool IsPkce { get; set; } = true;
    }
}
