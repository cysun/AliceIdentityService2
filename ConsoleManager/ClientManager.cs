using System.Security.Cryptography;
using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;

partial class ConsoleManager
{
    private async Task ClientsControllerAsync()
    {
        var done = false;
        do
        {
            var clients = await applicationManager.ListAsync().ToListAsync();
            var cmd = ClientsView(clients);
            switch (cmd)
            {
                case "a":
                    await AddClientAsync();
                    break;
                case "b":
                    done = true;
                    break;
                default:
                    int index;
                    bool isNumber = int.TryParse(cmd, out index);
                    if (isNumber && index < clients.Count)
                        await ClientControllerAsync(clients[index].Id);
                    break;
            }
        } while (!done);
    }

    private string ClientsView(List<OpenIddictEntityFrameworkCoreApplication> clients)
    {
        string choice;
        var validChoices = new HashSet<string>() { "a", "b" };
        do
        {
            Console.Clear();
            Console.WriteLine("\t Client Management \n");
            Console.WriteLine("\t a) Add a Client");
            Console.WriteLine("\t b) Back to Main Menu\n");

            for (int i = 0; i < clients.Count; ++i)
            {
                Console.WriteLine($"\t {i}) {clients[i].ClientId}");
                validChoices.Add(i.ToString());
            }

            Console.Write("\n Pleasse enter your choice: ");
            choice = Console.ReadLine().ToLower();
        } while (!validChoices.Contains(choice));
        return choice;
    }

    // XXX Only support adding Web App and SPA clients for now.
    private async Task AddClientAsync()
    {
        Console.Clear();
        Console.WriteLine("\t Add Client\n");

        var client = new OpenIddictApplicationDescriptor
        {
            Permissions =
            {
                OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                OpenIddictConstants.Permissions.Scopes.Profile,
                OpenIddictConstants.Permissions.Scopes.Email
            }
        };

        Console.Write("\t Client Id: ");
        client.ClientId = Console.ReadLine();
        Console.Write("\t Display Name: ");
        client.DisplayName = Console.ReadLine();
        Console.Write("\t Redirect URIs (comma-separated): ");
        client.RedirectUris.UnionWith(
            Console.ReadLine().Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(uri => new Uri(uri))
        );

        Console.Write("\t Client Secret? [y|n] ");
        if (Console.ReadLine().ToLower() == "y")
        {
            var bytes = new byte[32];
            using var rand = RandomNumberGenerator.Create();
            rand.GetBytes(bytes);
            client.ClientSecret = Convert.ToBase64String(bytes);
            Console.WriteLine($"\t\t {client.ClientSecret}");
        }

        Console.Write("\t Save or Cancel? [s|c] ");
        var cmd = Console.ReadLine();
        if (cmd.ToLower() == "s")
        {
            try
            {
                await applicationManager.CreateAsync(client);
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n\t Failed to create the client");
                Console.WriteLine($"\t {ex.Message}");
                Console.Write("\n\n\t Press [Enter] key to continue");
                Console.ReadLine();
            }
        }
    }

    private async Task ClientControllerAsync(String clientId)
    {
        var done = false;
        do
        {
            var client = await applicationManager.FindByIdAsync(clientId);
            var cmd = ClientView(client);
            switch (cmd)
            {
                case "a":
                    // await AddClaimAsync(scope);
                    break;
                case "b":
                    done = true;
                    break; ;
            }
        } while (!done);
    }

    private string ClientView(OpenIddictEntityFrameworkCoreApplication client)
    {
        string choice;
        var validChoices = new HashSet<string>() { "a", "b" };
        do
        {
            Console.Clear();
            Console.WriteLine($"\t Client Management - {client.ClientId} \n");
            Console.WriteLine($"\t Client Id: \t {client.ClientId}");
            Console.WriteLine($"\t Client Secret:  {(client.ClientSecret == null ? "(No)" : "(Yes)")}");
            Console.WriteLine($"\t Display Name: \t {client.DisplayName}");
            Console.WriteLine($"\t Permissions: \t {client.Permissions}");
            Console.WriteLine($"\t Return URIs: \t {client.RedirectUris} \n");
            Console.WriteLine("\t a) Add a scope");
            Console.WriteLine("\t b) Back to Clients Menu");
            Console.Write("\n Pleasse enter your choice: ");
            choice = Console.ReadLine().ToLower();
        } while (!validChoices.Contains(choice));
        return choice;
    }
}
