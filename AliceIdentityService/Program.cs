using System.Security.Cryptography.X509Certificates;
using AliceIdentityService.Models;
using AliceIdentityService.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Serilog;
using static OpenIddict.Abstractions.OpenIddictConstants;

var builder = WebApplication.CreateBuilder(args);
// builder.WebHost.UseUrls("http://localhost:5000");
builder.Host.UseSerilog((hostingContext, loggerConfiguration) =>
    loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration));

var configuration = builder.Configuration;
var services = builder.Services;

// Configure Services

services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
    options.UseOpenIddict();
});

services.AddQuartz(options =>
{
    options.UseMicrosoftDependencyInjectionJobFactory();
    options.UseSimpleTypeLoader();
    options.UseInMemoryStore();
});

services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore().UseDbContext<AppDbContext>();
        options.UseQuartz();
    })
    .AddServer(options =>
    {
        options.AllowAuthorizationCodeFlow().RequireProofKeyForCodeExchange();
        options.IgnoreEndpointPermissions();
        options.IgnoreResponseTypePermissions();
        options.SetAuthorizationEndpointUris("/connect/authorize");
        options.SetTokenEndpointUris("/connect/token");
        options.AddEncryptionCertificate(new X509Certificate2(Path.Combine(configuration["Application:CertificateFolder"], "encryption-certificate.pfx")));
        options.AddSigningCertificate(new X509Certificate2(Path.Combine(configuration["Application:CertificateFolder"], "signing-certificate.pfx")));
        options.UseAspNetCore().EnableTokenEndpointPassthrough().EnableAuthorizationEndpointPassthrough();
        options.DisableAccessTokenEncryption(); // for testing
    });

// services.AddDefaultIdentity() is part of Identity UI which is not used in this project.
// see https://github.com/aspnet/Identity/blob/master/src/UI/IdentityServiceCollectionUIExtensions.cs for
// what AddDefaultIdentity() does, and see https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity
// for additional configuration options

services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
.AddIdentityCookies(options => { });

services.AddIdentityCore<User>(options =>
{
    options.SignIn.RequireConfirmedEmail = true;
    // Configure Identity to use the same JWT claims as OpenIddict
    options.ClaimsIdentity.UserNameClaimType = Claims.Name;
    options.ClaimsIdentity.UserIdClaimType = Claims.Subject;
    options.ClaimsIdentity.RoleClaimType = Claims.Role;
    options.ClaimsIdentity.EmailClaimType = Claims.Email;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders()
.AddSignInManager();

services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/Account/AccessDenied";
});

services.AddScoped<IUserClaimsPrincipalFactory<User>, AppUserClaimsPrincipalFactory>();

services.AddFluentEmail(configuration["Email:SenderEmail"], configuration["Email:SenderName"])
    .AddSendGridSender(configuration["Email:SendGridKey"])
    .AddLiquidRenderer();
services.Configure<EmailSettings>(configuration.GetSection("Email"));
services.AddScoped<EmailSender>();

services.AddAutoMapper(config => config.AddProfile<MapperProfile>());

services.AddControllersWithViews();

// Build App

var app = builder.Build();

// Configure Middleware Pipeline

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Home/Error");

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Run App

app.Run();
