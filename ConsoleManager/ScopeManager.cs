using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;

partial class ConsoleManager
{
    private async Task ScopesControllerAsync()
    {
        var done = false;
        do
        {
            var scopes = await scopeManager.ListAsync().ToListAsync();
            var cmd = ScopesView(scopes);
            switch (cmd)
            {
                case "a":
                    await AddScopeAsync();
                    break;
                case "b":
                    done = true;
                    break;
                default:
                    int index;
                    bool isNumber = int.TryParse(cmd, out index);
                    if (isNumber && index < scopes.Count)
                        await ScopeControllerAsync(scopes[index].Id);
                    break;
            }
        } while (!done);
    }

    private string ScopesView(List<OpenIddictEntityFrameworkCoreScope> scopes)
    {
        string choice;
        var validChoices = new HashSet<string>() { "a", "b" };
        do
        {
            Console.Clear();
            Console.WriteLine("\t Scope Management \n");
            Console.WriteLine("\t a) Add a Scope");
            Console.WriteLine("\t b) Back to Main Menu\n");

            for (int i = 0; i < scopes.Count; ++i)
            {
                Console.WriteLine($"\t {i}) {scopes[i].Name}");
                validChoices.Add(i.ToString());
            }

            Console.Write("\n Pleasse enter your choice: ");
            choice = Console.ReadLine().ToLower();
        } while (!validChoices.Contains(choice));
        return choice;
    }

    private async Task AddScopeAsync()
    {
        Console.Clear();
        Console.WriteLine("\t Add Scope\n");

        var scope = new OpenIddictScopeDescriptor();

        Console.Write("\t Name: ");
        scope.Name = Console.ReadLine();
        Console.Write("\t Display Name: ");
        scope.DisplayName = Console.ReadLine();

        Console.Write("\t Save or Cancel? [s|c] ");
        var cmd = Console.ReadLine();
        if (cmd.ToLower() == "s")
        {
            try
            {
                await scopeManager.CreateAsync(scope);
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n\t Failed to create the scope");
                Console.WriteLine($"\t {ex.Message}");
                Console.Write("\n\n\t Press [Enter] key to continue");
                Console.ReadLine();
            }
        }
    }

    private async Task ScopeControllerAsync(String scopeId)
    {
        var done = false;
        do
        {
            var scope = await scopeManager.FindByIdAsync(scopeId);
            var cmd = ScopeView(scope);
            switch (cmd)
            {
                case "a":
                    await AddClaimAsync(scope);
                    break;
                case "b":
                    done = true;
                    break; ;
            }
        } while (!done);
    }

    private string ScopeView(OpenIddictEntityFrameworkCoreScope scope)
    {
        string choice;
        var validChoices = new HashSet<string>() { "a", "b" };
        do
        {
            Console.Clear();
            Console.WriteLine($"\t Scope Management - {scope.Name} \n");
            Console.WriteLine($"\t Name: \t\t {scope.Name}");
            Console.WriteLine($"\t Display Name: \t {scope.DisplayName}");
            Console.WriteLine($"\t Claims: \t {scope.Resources} \n");
            Console.WriteLine("\t a) Add a claim");
            Console.WriteLine("\t b) Back to Scopes Menu");
            Console.Write("\n Pleasse enter your choice: ");
            choice = Console.ReadLine().ToLower();
        } while (!validChoices.Contains(choice));
        return choice;
    }

    private async Task AddClaimAsync(OpenIddictEntityFrameworkCoreScope scope)
    {
        Console.Clear();
        Console.WriteLine($"\t Scope Management - {scope.Name} - Add Claim\n");
        string ans;
        do
        {
            Console.Write("\t Claim: ");
            var newClaim = Console.ReadLine();
            var descriptor = new OpenIddictScopeDescriptor();
            await scopeManager.PopulateAsync(descriptor, scope);
            descriptor.Resources.Add(newClaim);
            await scopeManager.UpdateAsync(scope, descriptor);
            Console.Write("\t Add another claim? [y|n] ");
            ans = Console.ReadLine().ToLower();
        } while (ans == "y");
    }
}
