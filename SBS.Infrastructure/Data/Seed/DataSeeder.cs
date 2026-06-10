using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SBS.Domain.Entities;
using SBS.Infrastructure.Identity;
using System;
using System.Collections.Generic;
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
        // Prevent duplicate seeding
        if (!context.Pools.Any())
        {
            return;
        }

        // 1. Seed Roles
        var adminRole = new AppRole("Admin") { Id = Guid.NewGuid() };
        var managerRole = new AppRole("Manager") { Id = Guid.NewGuid() };
        var staffRole = new AppRole("Staff") { Id = Guid.NewGuid() };
        var customerRole = new AppRole("Customer") { Id = Guid.NewGuid() };

        await roleManager.CreateAsync(adminRole);
        await roleManager.CreateAsync(managerRole);
        await roleManager.CreateAsync(staffRole);
        await roleManager.CreateAsync(customerRole);

        // 2. Seed Users
        const string defaultPassword = "Password@123";

        var admin = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = "admin1",
            Email = "admin1@example.com",
            FullName = "System Administrator",
            PhoneNumber = "0900000001",
            Address = "Hà Nội",
            Status = "Active",
            Dob = new DateOnly(1980, 1, 1),
            Gender = "Male",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        await userManager.CreateAsync(admin, defaultPassword);
        await userManager.AddToRoleAsync(admin, "Admin");

        var manager = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = "manager1",
            Email = "manager1@example.com",
            FullName = "Nguyễn Văn Quản Lý",
            PhoneNumber = "0900000002",
            Address = "Hà Nội",
            Status = "Active",
            Dob = new DateOnly(1985, 6, 15),
            Gender = "Male",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        await userManager.CreateAsync(manager, defaultPassword);
        await userManager.AddToRoleAsync(manager, "Manager");

        var staff = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = "staff1",
            Email = "staff1@example.com",
            FullName = "Nguyễn Văn An",
            PhoneNumber = "0910000001",
            Address = "Hà Nội",
            Status = "Active",
            Dob = new DateOnly(1995, 1, 1),
            Gender = "Male",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        await userManager.CreateAsync(staff, defaultPassword);
        await userManager.AddToRoleAsync(staff, "Staff");

        var customer = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = "customer1",
            Email = "cust1@example.com",
            FullName = "Nguyễn Văn A",
            PhoneNumber = "0900001001",
            Address = "Hà Nội",
            Status = "Active",
            Dob = new DateOnly(2000, 1, 1),
            Gender = "Male",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        await userManager.CreateAsync(customer, defaultPassword);
        await userManager.AddToRoleAsync(customer, "Customer");

        // 3. Seed TicketTypes (Fixed tickets)
        var adultTicket = new TicketType
        {
            TicketCode = "SINGLE-ADULT",
            TicketName = "Vé Người lớn",
            Category = "Single",
            BasePrice = 100000m,
            DiscountPercent = 0,
            Description = "Vé bơi dành cho người lớn"
        };
        var childTicket = new TicketType
        {
            TicketCode = "SINGLE-CHILD",
            TicketName = "Vé Trẻ em",
            Category = "Single",
            BasePrice = 70000m,
            DiscountPercent = 30,
            Description = "Vé bơi dành cho trẻ em dưới 12 tuổi"
        };
        var seniorTicket = new TicketType
        {
            TicketCode = "SINGLE-SENIOR",
            TicketName = "Vé Người già",
            Category = "Single",
            BasePrice = 80000m,
            DiscountPercent = 20,
            Description = "Vé bơi dành cho người trên 60 tuổi"
        };
        var familyCombo = new TicketType
        {
            TicketCode = "COMBO-FAMILY",
            TicketName = "Vé Combo Family",
            Category = "Combo",
            BasePrice = 289000m,
            DiscountPercent = 15,
            Description = "Combo 2 người lớn + 2 trẻ em, giảm 15%"
        };
        var trippleCombo = new TicketType
        {
            TicketCode = "COMBO-TRIPBLE",
            TicketName = "Vé Combo Tripble",
            Category = "Combo",
            BasePrice = 270000m,
            DiscountPercent = 10,
            Description = "Combo 3 người lớn, giảm 10%"
        };

        context.TicketTypes.AddRange(adultTicket, childTicket, seniorTicket, familyCombo, trippleCombo);
        await context.SaveChangesAsync();

        // 4. Seed ComboDetails
        context.ComboDetails.AddRange(
            new ComboDetail
            {
                ComboTicketTypeId = familyCombo.TicketTypeId,
                SingleTicketTypeId = adultTicket.TicketTypeId,
                Quantity = 2
            },
            new ComboDetail
            {
                ComboTicketTypeId = familyCombo.TicketTypeId,
                SingleTicketTypeId = childTicket.TicketTypeId,
                Quantity = 2
            },
            new ComboDetail
            {
                ComboTicketTypeId = trippleCombo.TicketTypeId,
                SingleTicketTypeId = adultTicket.TicketTypeId,
                Quantity = 3
            }
        );
        await context.SaveChangesAsync();

        // 5. Seed Pool
        var pool = new Pool
        {
            PoolName = "Bể bơi Quốc gia SBS",
            Address = "Đường Xuân Thủy, Cầu Giấy, Hà Nội",
            Description = "Bể bơi tiêu chuẩn Olympic quốc gia với hệ thống lọc nước hiện đại.",
            OpeningTime = new TimeSpan(6, 0, 0),
            ClosingTime = new TimeSpan(21, 0, 0),
            Status = "Active"
        };
        context.Pools.Add(pool);
        await context.SaveChangesAsync();

        // 6. Seed PoolSlots (for today)
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var poolSlots = new List<PoolSlot>
        {
            new PoolSlot { PoolId = pool.PoolId, SlotName = "Slot sáng 1", StartTime = new TimeSpan(6, 0, 0), EndTime = new TimeSpan(8, 0, 0), SlotDate = today, Capacity = 50, Status = "Open" },
            new PoolSlot { PoolId = pool.PoolId, SlotName = "Slot sáng 2", StartTime = new TimeSpan(8, 30, 0), EndTime = new TimeSpan(10, 30, 0), SlotDate = today, Capacity = 50, Status = "Open" },
            new PoolSlot { PoolId = pool.PoolId, SlotName = "Slot trưa", StartTime = new TimeSpan(11, 0, 0), EndTime = new TimeSpan(13, 0, 0), SlotDate = today, Capacity = 50, Status = "Open" },
            new PoolSlot { PoolId = pool.PoolId, SlotName = "Slot chiều 1", StartTime = new TimeSpan(14, 0, 0), EndTime = new TimeSpan(16, 0, 0), SlotDate = today, Capacity = 50, Status = "Open" },
            new PoolSlot { PoolId = pool.PoolId, SlotName = "Slot chiều 2", StartTime = new TimeSpan(16, 30, 0), EndTime = new TimeSpan(18, 30, 0), SlotDate = today, Capacity = 50, Status = "Open" },
            new PoolSlot { PoolId = pool.PoolId, SlotName = "Slot tối", StartTime = new TimeSpan(19, 0, 0), EndTime = new TimeSpan(21, 0, 0), SlotDate = today, Capacity = 50, Status = "Open" }
        };
        context.PoolSlots.AddRange(poolSlots);
        await context.SaveChangesAsync();

        // 7. Seed PoolTicketTypes (prices for each ticket at this pool)
        var poolTicketTypes = new List<PoolTicketType>
        {
            new PoolTicketType { PoolId = pool.PoolId, TicketTypeId = adultTicket.TicketTypeId, Price = 100000m, Status = "Active" },
            new PoolTicketType { PoolId = pool.PoolId, TicketTypeId = childTicket.TicketTypeId, Price = 70000m, Status = "Active" },
            new PoolTicketType { PoolId = pool.PoolId, TicketTypeId = seniorTicket.TicketTypeId, Price = 80000m, Status = "Active" },
            new PoolTicketType { PoolId = pool.PoolId, TicketTypeId = familyCombo.TicketTypeId, Price = 289000m, Status = "Active" },
            new PoolTicketType { PoolId = pool.PoolId, TicketTypeId = trippleCombo.TicketTypeId, Price = 270000m, Status = "Active" }
        };
        context.PoolTicketTypes.AddRange(poolTicketTypes);
        await context.SaveChangesAsync();
    }
}
