using System.ComponentModel.DataAnnotations;
using System.Text.Json;
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
    public class ScopeController : Controller
    {
        private readonly OpenIddictScopeManager<OpenIddictEntityFrameworkCoreScope> _scopeManager;

        private readonly IMapper _mapper;
        private readonly ILogger<ScopeController> _logger;

        public ScopeController(OpenIddictScopeManager<OpenIddictEntityFrameworkCoreScope> scopeManager,
            IMapper mapper, ILogger<ScopeController> logger)
        {
            _scopeManager = scopeManager;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IActionResult> IndexAsync()
        {
            return View(await _scopeManager.ListAsync().ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> ViewAsync(string id)
        {
            var scope = await _scopeManager.FindByIdAsync(id);
            if (scope == null) return NotFound();

            var descriptor = new OpenIddictScopeDescriptor();
            await _scopeManager.PopulateAsync(descriptor, scope);
            ViewBag.Claims = descriptor.Properties["claims"].EnumerateArray().Select(e => e.GetString()).ToList();

            return View(scope);
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View(new ScopeInputModel());
        }

        [HttpPost]
        public async Task<IActionResult> AddAsync(ScopeInputModel input)
        {
            if (!ModelState.IsValid) return View(input);

            var descriptor = _mapper.Map<OpenIddictScopeDescriptor>(input);
            descriptor.Properties["claims"] = JsonSerializer.SerializeToElement(new string[] { });
            var scope = await _scopeManager.CreateAsync(descriptor);
            _logger.LogInformation("{user} created new scope {scope}", User.Identity.Name, scope.Name);
            return RedirectToAction("View", new { id = scope.Id });
        }

        [HttpGet]
        public async Task<IActionResult> EditAsync(string id)
        {
            var scope = await _scopeManager.FindByIdAsync(id);
            if (scope == null) return NotFound();

            var descriptor = new OpenIddictScopeDescriptor();
            await _scopeManager.PopulateAsync(descriptor, scope);
            ViewBag.Claims = descriptor.Properties["claims"].EnumerateArray().Select(e => e.GetString()).ToList();

            return View(_mapper.Map<ScopeInputModel>(scope));
        }

        [HttpPost]
        public async Task<IActionResult> EditAsync(string id, ScopeInputModel input)
        {
            if (!ModelState.IsValid) return View(input);

            var scope = await _scopeManager.FindByIdAsync(id);
            if (scope == null) return NotFound();

            var descriptor = new OpenIddictScopeDescriptor();
            await _scopeManager.PopulateAsync(descriptor, scope);

            _mapper.Map(input, descriptor);
            await _scopeManager.UpdateAsync(scope, descriptor);
            _logger.LogInformation("{user} updated scope {scope}", User.Identity.Name, descriptor.Name);

            return RedirectToAction("View", new { id });
        }

        public async Task<IActionResult> AddClaimAsync(string scopeId, string claim)
        {
            var scope = await _scopeManager.FindByIdAsync(scopeId);
            if (scope == null) return NotFound();

            var descriptor = new OpenIddictScopeDescriptor();
            await _scopeManager.PopulateAsync(descriptor, scope);

            var claims = descriptor.Properties["claims"].EnumerateArray().Select(e => e.GetString()).ToList();
            claims.Add(claim);
            descriptor.Properties["claims"] = JsonSerializer.SerializeToElement(claims);

            await _scopeManager.UpdateAsync(scope, descriptor);
            _logger.LogInformation("{user} added claim {claim} to {scope}", User.Identity.Name, claim, scope.Name);

            return RedirectToAction("View", new { id = scopeId });
        }

        public async Task<IActionResult> RemoveClaimAsync(string scopeId, string claim)
        {
            var scope = await _scopeManager.FindByIdAsync(scopeId);
            if (scope == null) return NotFound();

            var descriptor = new OpenIddictScopeDescriptor();
            await _scopeManager.PopulateAsync(descriptor, scope);

            var claims = descriptor.Properties["claims"].EnumerateArray().Select(e => e.GetString()).ToList();
            claims.Remove(claim);
            descriptor.Properties["claims"] = JsonSerializer.SerializeToElement(claims);

            await _scopeManager.UpdateAsync(scope, descriptor);
            _logger.LogInformation("{user} removed claim {claim} from {scope}", User.Identity.Name, claim, scope.Name);

            return RedirectToAction("View", new { id = scopeId });
        }
    }
}

namespace AliceIdentityService.Models
{
    public class ScopeInputModel
    {
        public string Id { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; }

        [Display(Name = "Display Name")]
        public string DisplayName { get; set; }

        public string Description { get; set; }
    }
}
