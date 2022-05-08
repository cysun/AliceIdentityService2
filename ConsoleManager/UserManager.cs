using AliceIdentityService.Models;

partial class ConsoleManager
{
    private async Task UsersControllerAsync()
    {
        var done = false;
        do
        {
            var users = userManager.Users.OrderBy(u => u.UserName).ToList();
            var cmd = UsersView(users);
            switch (cmd)
            {
                case "a":
                    await AddUserAsync();
                    break;
                case "b":
                    done = true;
                    break;
                default:
                    int index;
                    bool isNumber = int.TryParse(cmd, out index);
                    if (isNumber && index < users.Count)
                        await UserViewAsync(users[index]);
                    break;
            }
        } while (!done);
    }

    private string UsersView(List<User> users)
    {
        var validChoices = new HashSet<string>() { "a", "b" };
        for (int i = 0; i < users.Count; ++i)
            validChoices.Add(i.ToString());

        string choice;
        do
        {
            Console.Clear();
            Console.WriteLine("\t User Management \n");
            Console.WriteLine("\t a) Add a user");
            Console.WriteLine("\t b) Back to Main Menu\n");
            for (int i = 0; i < users.Count; ++i)
                Console.WriteLine($"\t {i}) {users[i].UserName}");
            Console.Write("\n Pleasse enter your choice: ");
            choice = Console.ReadLine().ToLower();
        } while (!validChoices.Contains(choice));
        return choice;
    }

    private async Task UserViewAsync(User user)
    {
        Console.Clear();
        Console.WriteLine($"\t User Management - {user.UserName} \n");
        var claims = await userManager.GetClaimsAsync(user);
        foreach (var claim in claims)
            Console.WriteLine($"\t\t {claim.Type}: {claim.Value}");

        Console.Write("\n\t Add a claim? [y|n] ");
        var ans = Console.ReadLine().ToLower();
        while (ans == "y")
        {
            Console.Write("\t Claim Type: ");
            var claimType = Console.ReadLine();
            Console.Write("\t Claim Value: ");
            var claimValue = Console.ReadLine();
            await userManager.AddClaimAsync(user, new System.Security.Claims.Claim(claimType, claimValue));
            Console.Write("\t Add another claim? [y|n] ");
            ans = Console.ReadLine().ToLower();
        }
    }

    private async Task AddUserAsync()
    {
        Console.Clear();
        Console.WriteLine("\t Add User \n");
        Console.Write("\t First Name: ");
        var firstName = Console.ReadLine();
        Console.Write("\t Last Name: ");
        var lastName = Console.ReadLine();
        Console.Write("\t Email: ");
        var email = Console.ReadLine();
        Console.Write("\t Password: ");
        var password = Console.ReadLine();
        Console.Write("\t Save or Cancel? [s|c] ");
        var cmd = Console.ReadLine();
        if (cmd.ToLower() == "s")
        {
            var user = new User
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                ScreenName = $"{firstName} {lastName}"
            };
            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                await userManager.ConfirmEmailAsync(user, token);
            }
            else
            {
                Console.WriteLine("\n\t Failed to create the user");
                foreach (var error in result.Errors)
                    Console.WriteLine($"\t {error.Description}");
                Console.Write("\n\n\t Press [Enter] key to continue");
                Console.ReadLine();
            }
        }
    }
}
