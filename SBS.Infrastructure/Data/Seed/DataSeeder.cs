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
        if (await roleManager.Roles.AnyAsync())
        {
            // Seed Waitlist Data if missing
            if (!await context.PoolSlots.AnyAsync(s => s.SlotName == "Slot Test Waitlist (FULL)"))
            {
                var wPool = await context.Pools.FirstOrDefaultAsync();
                var wStandardTicket = await context.TicketTypes.FirstOrDefaultAsync(t => t.TicketCode == "STANDARD");
                var wPoolTicketTypes = await context.PoolTicketTypes.Where(pt => pt.PoolId == wPool!.PoolId).ToListAsync();
                var wCustomer = await userManager.FindByNameAsync("customer1");

                var wWaitlistSlot = new PoolSlot 
                { 
                    PoolId = wPool!.PoolId, 
                    SlotName = "Slot Test Waitlist (FULL)", 
                    StartTime = new TimeSpan(15, 0, 0), 
                    EndTime = new TimeSpan(17, 0, 0), 
                    SlotDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1), 
                    Capacity = 2, 
                    Status = "Open" 
                };
                context.PoolSlots.Add(wWaitlistSlot);
                await context.SaveChangesAsync();

                var wBooking = new Booking
                {
                    UserId = wCustomer!.Id,
                    PoolSlotId = wWaitlistSlot.PoolSlotId,
                    BookingDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1),
                    TotalAmount = 100000m,
                    Status = "Paid",
                    BookingCode = "BKG-TEST-001"
                };
                context.Bookings.Add(wBooking);
                await context.SaveChangesAsync();

                var wBookingDetail = new BookingDetail
                {
                    BookingId = wBooking.BookingId,
                    PoolTicketTypeId = wPoolTicketTypes[0].PoolTicketTypeId,
                    Quantity = 2,
                    UnitPrice = 50000m,
                    SubTotal = 100000m
                };
                context.BookingDetails.Add(wBookingDetail);
                await context.SaveChangesAsync();
            }

            if (await userManager.FindByNameAsync("customer2") == null)
            {
                var wCustomer2 = new AppUser
                {
                    Id = Guid.NewGuid(),
                    UserName = "customer2",
                    Email = "cust2@example.com",
                    FullName = "Nguyễn Văn B (Test Waitlist)",
                    PhoneNumber = "0900001002",
                    Address = "Hà Nội",
                    Status = "Active",
                    Dob = new DateOnly(2001, 1, 1),
                    Gender = "Male",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };
                await userManager.CreateAsync(wCustomer2, "Password@123");
                await userManager.AddToRoleAsync(wCustomer2, "Customer");
            }

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

        var customer2 = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = "customer2",
            Email = "cust2@example.com",
            FullName = "Nguyễn Văn B (Test Waitlist)",
            PhoneNumber = "0900001002",
            Address = "Hà Nội",
            Status = "Active",
            Dob = new DateOnly(2001, 1, 1),
            Gender = "Male",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        await userManager.CreateAsync(customer2, defaultPassword);
        await userManager.AddToRoleAsync(customer2, "Customer");

        // 3. Seed TicketTypes (Fixed tickets)
        var standardTicket = new TicketType
        {
            TicketCode = "STANDARD",
            TicketName = "Vé Cá nhân",
            Category = "Single",
            BasePrice = 100000m,
            DiscountPercent = 0,
            Description = "Vé bơi tiêu chuẩn dành cho 1 người"
        };
        var combo3Ticket = new TicketType
        {
            TicketCode = "COMBO_3",
            TicketName = "Combo 3 Người",
            Category = "Combo",
            BasePrice = 300000m,
            DiscountPercent = 10,
            Description = "Combo tiết kiệm dành cho 3 người, giảm 10%"
        };
        var combo5Ticket = new TicketType
        {
            TicketCode = "COMBO_5",
            TicketName = "Combo 5 Người",
            Category = "Combo",
            BasePrice = 500000m,
            DiscountPercent = 15,
            Description = "Combo tiết kiệm dành cho nhóm 5 người, giảm 15%"
        };

        context.TicketTypes.AddRange(standardTicket, combo3Ticket, combo5Ticket);
        await context.SaveChangesAsync();

        // 4. Seed ComboDetails
        context.ComboDetails.AddRange(
            new ComboDetail
            {
                ComboTicketTypeId = combo3Ticket.TicketTypeId,
                SingleTicketTypeId = standardTicket.TicketTypeId,
                Quantity = 3
            },
            new ComboDetail
            {
                ComboTicketTypeId = combo5Ticket.TicketTypeId,
                SingleTicketTypeId = standardTicket.TicketTypeId,
                Quantity = 5
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
            new PoolTicketType { PoolId = pool.PoolId, TicketTypeId = standardTicket.TicketTypeId, Price = 50000m, Status = "Active" },
            new PoolTicketType { PoolId = pool.PoolId, TicketTypeId = combo3Ticket.TicketTypeId, Price = 140000m, Status = "Active" },
            new PoolTicketType { PoolId = pool.PoolId, TicketTypeId = combo5Ticket.TicketTypeId, Price = 220000m, Status = "Active" }
        };
        context.PoolTicketTypes.AddRange(poolTicketTypes);
        await context.SaveChangesAsync();

        // 8. Seed Waitlist Test Data
        var waitlistSlot = new PoolSlot 
        { 
            PoolId = pool.PoolId, 
            SlotName = "Slot Test Waitlist (FULL)", 
            StartTime = new TimeSpan(15, 0, 0), 
            EndTime = new TimeSpan(17, 0, 0), 
            SlotDate = today.AddDays(1), 
            Capacity = 2, 
            Status = "Open" 
        };
        context.PoolSlots.Add(waitlistSlot);
        await context.SaveChangesAsync();

        var booking = new Booking
        {
            UserId = customer.Id,
            PoolSlotId = waitlistSlot.PoolSlotId,
            BookingDate = today,
            TotalAmount = 100000m,
            Status = "Paid",
            BookingCode = "BKG-TEST-001"
        };
        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        var bookingDetail = new BookingDetail
        {
            BookingId = booking.BookingId,
            PoolTicketTypeId = poolTicketTypes[0].PoolTicketTypeId,
            Quantity = 2,
            UnitPrice = 50000m,
            SubTotal = 100000m
        };
        context.BookingDetails.Add(bookingDetail);
        await context.SaveChangesAsync();
    }
}
