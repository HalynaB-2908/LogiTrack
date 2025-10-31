using LogiTrack.WebApi.Data;
using LogiTrack.WebApi.Models;
using Microsoft.AspNetCore.Identity;

namespace LogiTrack.WebApi.Seed
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var db = services.GetRequiredService<LogiTrackDbContext>();

            await db.Database.EnsureCreatedAsync();

            string[] roles = { "Admin", "User" };

            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            var adminUserName = "admin";
            var adminEmail = "admin@example.com";
            var adminPassword = "Admin123!";

            var admin = await userManager.FindByNameAsync(adminUserName);
            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminUserName,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
                else
                {
                    Console.WriteLine("Не вдалося створити адміністратора:");
                    foreach (var error in result.Errors)
                        Console.WriteLine($" - {error.Description}");
                }
            }

            var userUserName = "user1";
            var userEmail = "user@example.com";
            var userPassword = "User123!";

            var user = await userManager.FindByNameAsync(userUserName);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = userUserName,
                    Email = userEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, userPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "User");
                }
                else
                {
                    Console.WriteLine("Не вдалося створити користувача:");
                    foreach (var error in result.Errors)
                        Console.WriteLine($" - {error.Description}");
                }
            }
        }
    }
}
