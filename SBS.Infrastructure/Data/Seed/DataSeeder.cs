using Microsoft.AspNetCore.Identity;
using SBS.Domain.Entities;
using SBS.Domain.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SBS.Infrastructure.Data.Seed;

public static class DataSeeder
{
    public static async Task SeedDataAsync(
        ApplicationDbContext context,
        UserManager<AppUser> userManager,
        RoleManager<AppRole> roleManager)
    {
        // 1. Seed Roles
        string[] roleNames = { "Admin", "Manager", "Staff", "Receptionist", "Customer" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new AppRole(roleName));
            }
        }

        // 2. Seed Users
        if (!userManager.Users.Any())
        {
            var admin = new AppUser("admin", "admin@pool.com", "System Admin");
            await userManager.CreateAsync(admin, "Admin@123");
            await userManager.AddToRoleAsync(admin, "Admin");

            var manager = new AppUser("manager", "manager@pool.com", "Pool Manager");
            await userManager.CreateAsync(manager, "Manager@123");
            await userManager.AddToRoleAsync(manager, "Manager");

            var receptionist = new AppUser("recept", "recept@pool.com", "Front Desk");
            await userManager.CreateAsync(receptionist, "Recept@123");
            await userManager.AddToRoleAsync(receptionist, "Receptionist");

            var staff = new AppUser("staff1", "staff1@pool.com", "Lifeguard");
            await userManager.CreateAsync(staff, "Staff@123");
            await userManager.AddToRoleAsync(staff, "Staff");

            var customer = new AppUser("customer1", "customer@pool.com", "John Doe");
            await userManager.CreateAsync(customer, "Customer@123");
            await userManager.AddToRoleAsync(customer, "Customer");
        }

        // 3. Seed Swimming Pools
        if (!context.SwimmingPools.Any())
        {
            var pool1 = new SwimmingPool("Olympic Size Pool", "Zone A", "50x25m", 2.0, 100);
            var pool2 = new SwimmingPool("Kids Pool", "Zone B", "20x10m", 0.8, 30);
            
            context.SwimmingPools.AddRange(pool1, pool2);
            await context.SaveChangesAsync();

            // 4. Seed Pool Schedules
            var schedule1 = new PoolSchedule(pool1.Id, DayOfWeek.Monday, new TimeSpan(8, 0, 0), new TimeSpan(12, 0, 0), 50000, 50);
            var schedule2 = new PoolSchedule(pool1.Id, DayOfWeek.Monday, new TimeSpan(14, 0, 0), new TimeSpan(18, 0, 0), 60000, 50);
            var schedule3 = new PoolSchedule(pool2.Id, DayOfWeek.Saturday, new TimeSpan(8, 0, 0), new TimeSpan(10, 0, 0), 30000, 20);

            context.PoolSchedules.AddRange(schedule1, schedule2, schedule3);
            await context.SaveChangesAsync();

            // 5. Seed Pool Images
            var image1 = new PoolImage(pool1.Id, "https://res.cloudinary.com/demo/image/upload/pool1_main.jpg", "Main View", true);
            var image2 = new PoolImage(pool1.Id, "https://res.cloudinary.com/demo/image/upload/pool1_side.jpg", "Side View", false);
            var image3 = new PoolImage(pool2.Id, "https://res.cloudinary.com/demo/image/upload/pool2_main.jpg", "Kids Pool", true);

            context.PoolImages.AddRange(image1, image2, image3);
            await context.SaveChangesAsync();
        }
    }
}
