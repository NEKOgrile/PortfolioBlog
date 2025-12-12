using Microsoft.AspNetCore.Identity;

namespace PortfolioBlog.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        // Roles
        string[] roles = ["Admin", "Author"];
        foreach (var r in roles)
            if (!await roleManager.RoleExistsAsync(r))
                await roleManager.CreateAsync(new IdentityRole(r));

        // Admin user
        var adminEmail = "admin@demo.com";
        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            admin = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
            await userManager.CreateAsync(admin, "Admin123!");
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        // Author user
        var authorEmail = "author@demo.com";
        var author = await userManager.FindByEmailAsync(authorEmail);
        if (author is null)
        {
            author = new IdentityUser { UserName = authorEmail, Email = authorEmail, EmailConfirmed = true };
            await userManager.CreateAsync(author, "Author123!");
            await userManager.AddToRoleAsync(author, "Author");
        }
    }
}
