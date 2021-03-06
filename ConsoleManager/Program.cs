using System.Text.Json;
using AliceIdentityService.Models;
using AliceIdentityService.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using OpenIddict.Core;
using OpenIddict.EntityFrameworkCore.Models;

await (new ConsoleManager()).MainControllerAsync();

partial class ConsoleManager
{
    readonly IConfiguration configuration;
    readonly ServiceProvider serviceProvider;

    UserManager<User> userManager => serviceProvider.GetRequiredService<UserManager<User>>();

    OpenIddictApplicationManager<OpenIddictEntityFrameworkCoreApplication> applicationManager =>
        serviceProvider.GetRequiredService<OpenIddictApplicationManager<OpenIddictEntityFrameworkCoreApplication>>();

    OpenIddictScopeManager<OpenIddictEntityFrameworkCoreScope> scopeManager =>
        serviceProvider.GetRequiredService<OpenIddictScopeManager<OpenIddictEntityFrameworkCoreScope>>();

    public ConsoleManager()
    {
        configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../AliceIdentityService"))
            .AddJsonFile("appsettings.json")
            .Build();

        var services = new ServiceCollection();

        services.AddOptions().AddLogging();

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
            options.UseOpenIddict();
        });

        services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore().UseDbContext<AppDbContext>();
            })
            .AddServer(options =>
            {
                options.AddDevelopmentEncryptionCertificate().AddDevelopmentSigningCertificate();
            });

        services.AddIdentityCore<User>(options =>
        {
            options.SignIn.RequireConfirmedEmail = true;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        services.AddAuthentication();

        serviceProvider = services.BuildServiceProvider();
    }

    public async Task MainControllerAsync()
    {
        foreach (var standardScope in AisConstants.StandardScopes)
        {
            if (await scopeManager.FindByNameAsync(standardScope.Key) == null)
            {
                var descriptor = new OpenIddictScopeDescriptor();
                descriptor.Name = standardScope.Key;
                descriptor.DisplayName = standardScope.Key;
                descriptor.Properties["claims"] = JsonSerializer.SerializeToElement(standardScope.Value);
                await scopeManager.CreateAsync(descriptor);
            }
        }

        var done = false;
        do
        {
            var cmd = MainView();
            switch (cmd)
            {
                case "u":
                    await UsersControllerAsync();
                    break;
                case "s":
                    await ScopesControllerAsync();
                    break;
                case "c":
                    await ClientsControllerAsync();
                    break;
                case "t":
                    GenerateCertificates();
                    break;
                case "x":
                    done = true;
                    break;
            }
        } while (!done);

        serviceProvider.Dispose();
    }

    public string MainView()
    {
        var validChoices = new HashSet<string>() { "u", "s", "c", "t", "x" };
        string choice;
        do
        {
            Console.Clear();
            Console.WriteLine("\t Main Menu \n");
            Console.WriteLine("\t u) User Management");
            Console.WriteLine("\t s) Scope Management");
            Console.WriteLine("\t c) Client Management");
            Console.WriteLine("\t t) Generate Certificates");
            Console.WriteLine("\t x) Exit");
            Console.Write("\n  Pleasse enter your choice: ");
            choice = Console.ReadLine().ToLower();
        } while (!validChoices.Contains(choice));

        return choice;
    }
}
