using Microsoft.EntityFrameworkCore;
using SBS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SBS.Infrastructure.Data.Seed.CustomerSeed;

public static class CustomerPoolSeeder
{
    public static async Task SeedCustomerPoolsAsync(ApplicationDbContext context)
    {
        // Kiểm tra nếu trong CSDL chỉ có tối đa 1 bể bơi (bể bơi Quốc gia mặc định) thì mới seed thêm
        if (await context.Pools.CountAsync(p => p.Status == "Active") <= 1)
        {
            // Lấy danh sách các loại vé mặc định đã được seeder chính tạo trước đó
            var ticketTypes = await context.TicketTypes.ToListAsync();
            var adultTicket = ticketTypes.FirstOrDefault(t => t.TicketCode == "SINGLE-ADULT");
            var childTicket = ticketTypes.FirstOrDefault(t => t.TicketCode == "SINGLE-CHILD");
            var seniorTicket = ticketTypes.FirstOrDefault(t => t.TicketCode == "SINGLE-SENIOR");
            var familyCombo = ticketTypes.FirstOrDefault(t => t.TicketCode == "COMBO-FAMILY");
            var trippleCombo = ticketTypes.FirstOrDefault(t => t.TicketCode == "COMBO-TRIPBLE");

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var tomorrow = today.AddDays(1);

            // Danh sách 10 bể bơi mới
            var poolsToSeed = new List<Pool>
            {
                new Pool
                {
                    PoolName = "Hồ bơi Cầu Giấy",
                    Address = "Số 1 Cầu Giấy, Quan Hoa, Cầu Giấy, Hà Nội",
                    Description = "Bể bơi trong nhà với hệ thống nước nóng hiện đại, phù hợp cho gia đình và trẻ em.",
                    OpeningTime = new TimeSpan(6, 30, 0),
                    ClosingTime = new TimeSpan(21, 30, 0),
                    Status = "Active"
                },
                new Pool
                {
                    PoolName = "Hồ bơi Lam Sơn",
                    Address = "242 Trần Bình Trọng, Quận 5, Hồ Chí Minh",
                    Description = "Bể bơi ngoài trời quy mô lớn, chất lượng nước đạt chuẩn thi đấu chuyên nghiệp.",
                    OpeningTime = new TimeSpan(5, 30, 0),
                    ClosingTime = new TimeSpan(20, 0, 0),
                    Status = "Active"
                },
                new Pool
                {
                    PoolName = "Hồ bơi Sông Hàn",
                    Address = "Đường Bạch Đằng, Hải Châu, Đà Nẵng",
                    Description = "Bể bơi vô cực view trực diện sông Hàn thơ mộng, không gian sang trọng cao cấp.",
                    OpeningTime = new TimeSpan(6, 0, 0),
                    ClosingTime = new TimeSpan(22, 0, 0),
                    Status = "Active"
                },
                new Pool
                {
                    PoolName = "Hồ bơi Quy Nhơn",
                    Address = "12 An Dương Vương, Quy Nhơn",
                    Description = "Hồ bơi sát biển Quy Nhơn, gió lộng mát mẻ, hệ thống lọc nước muối sinh học an toàn.",
                    OpeningTime = new TimeSpan(6, 0, 0),
                    ClosingTime = new TimeSpan(20, 30, 0),
                    Status = "Active"
                },
                new Pool
                {
                    PoolName = "Hồ bơi Cần Thơ",
                    Address = "Khu dân cư Hồng Phát, An Bình, Ninh Kiều, Cần Thơ",
                    Description = "Bể bơi sinh thái thiết kế hòa hợp thiên nhiên miền Tây sông nước, thoáng đãng.",
                    OpeningTime = new TimeSpan(6, 0, 0),
                    ClosingTime = new TimeSpan(21, 0, 0),
                    Status = "Active"
                },
                new Pool
                {
                    PoolName = "Hồ bơi Nha Trang",
                    Address = "10 Trần Phú, Lộc Thọ, Nha Trang",
                    Description = "Hồ bơi trung tâm thành phố Nha Trang với đầy đủ dịch vụ tiện ích đi kèm.",
                    OpeningTime = new TimeSpan(5, 30, 0),
                    ClosingTime = new TimeSpan(21, 0, 0),
                    Status = "Active"
                },
                new Pool
                {
                    PoolName = "Hồ bơi Hải Phòng",
                    Address = "Lạch Tray, Ngô Quyền, Hải Phòng",
                    Description = "Bể bơi tiêu chuẩn quốc gia phục vụ huấn luyện và bơi lội tự do.",
                    OpeningTime = new TimeSpan(6, 0, 0),
                    ClosingTime = new TimeSpan(21, 0, 0),
                    Status = "Active"
                },
                new Pool
                {
                    PoolName = "Hồ bơi Vũng Tàu",
                    Address = "15 Hoàng Hoa Thám, Vũng Tàu",
                    Description = "Bể bơi nước mặn rộng lớn thích hợp thư giãn và rèn luyện thể chất.",
                    OpeningTime = new TimeSpan(6, 0, 0),
                    ClosingTime = new TimeSpan(22, 0, 0),
                    Status = "Active"
                },
                new Pool
                {
                    PoolName = "Hồ bơi Cố Đô",
                    Address = "2 Lê Lợi, Vĩnh Ninh, Huế",
                    Description = "Bể bơi thơ mộng bên bờ sông Hương, không gian yên bình và thư thái.",
                    OpeningTime = new TimeSpan(6, 0, 0),
                    ClosingTime = new TimeSpan(20, 0, 0),
                    Status = "Active"
                },
                new Pool
                {
                    PoolName = "Hồ bơi Đà Lạt",
                    Address = "1 Trần Quốc Toản, Phường 1, Đà Lạt",
                    Description = "Bể bơi nước ấm duy nhất giữa lòng Đà Lạt, thiết kế kính bao quanh ngắm cảnh hồ Xuân Hương.",
                    OpeningTime = new TimeSpan(7, 0, 0),
                    ClosingTime = new TimeSpan(20, 0, 0),
                    Status = "Active"
                }
            };

            // Danh sách ảnh mẫu từ Unsplash tương ứng cho từng bể bơi (1 ảnh chính IsCover, 3 ảnh phụ)
            var poolImagesUrls = new List<string[]>
            {
                // 1. Hồ bơi Cầu Giấy (Bể trong nhà, nước ấm, gia đình)
                new[] 
                { 
                    "https://images.unsplash.com/photo-1554104707-a76b270e4bbb?w=800", // Cover
                    "https://images.unsplash.com/photo-1534438327276-14e5300c3a48?w=800", 
                    "https://images.unsplash.com/photo-1560185007-c5ca9d2c014d?w=800", 
                    "https://images.unsplash.com/photo-1584622650111-993a426fbf0a?w=800" 
                },
                // 2. Hồ bơi Lam Sơn (Bể ngoài trời, làn bơi thi đấu)
                new[] 
                { 
                    "https://images.unsplash.com/photo-1519741497674-611481863552?w=800", // Cover
                    "https://images.unsplash.com/photo-1575424909138-46b05e5919ec?w=800", 
                    "https://images.unsplash.com/photo-1606907291031-7e4df45cc71e?w=800", 
                    "https://images.unsplash.com/photo-1470229722913-7c0e2dbbafd3?w=800" 
                },
                // 3. Hồ bơi Sông Hàn (Vô cực, sang trọng, view sông)
                new[] 
                { 
                    "https://images.unsplash.com/photo-1540555700478-4be289fbecef?w=800", // Cover
                    "https://images.unsplash.com/photo-1544984243-ec57ea16fe25?w=800", 
                    "https://images.unsplash.com/photo-1613977257363-707ba9348227?w=800", 
                    "https://images.unsplash.com/photo-1600607687939-ce8a6c25118c?w=800" 
                },
                // 4. Hồ bơi Quy Nhơn (Sát biển, nước muối sinh học)
                new[] 
                { 
                    "https://images.unsplash.com/photo-1520250497591-112f2f40a3f4?w=800", // Cover
                    "https://images.unsplash.com/photo-1595206133361-b1fe343e5e23?w=800", 
                    "https://images.unsplash.com/photo-1571003123894-1f0594d2b5d9?w=800", 
                    "https://images.unsplash.com/photo-1507525428034-b723cf961d3e?w=800" 
                },
                // 5. Hồ bơi Cần Thơ (Sinh thái, thiên nhiên Tây Nam Bộ)
                new[] 
                { 
                    "https://images.unsplash.com/photo-1583037189850-1921ae7c6c22?w=800", // Cover
                    "https://images.unsplash.com/photo-1505156018113-d2d790d81084?w=800", 
                    "https://images.unsplash.com/photo-1518156677180-95a2893f3e9f?w=800", 
                    "https://images.unsplash.com/photo-1562790351-d273a961e0e9?w=800" 
                },
                // 6. Hồ bơi Nha Trang (Trung tâm thành phố, tiện ích cao cấp)
                new[] 
                { 
                    "https://images.unsplash.com/photo-1566073771259-6a8506099945?w=800", // Cover
                    "https://images.unsplash.com/photo-1551882547-ff40c63fe5fa?w=800", 
                    "https://images.unsplash.com/photo-1591088398332-8a7791972843?w=800", 
                    "https://images.unsplash.com/photo-1568605117036-5fe5e7bab0b7?w=800" 
                },
                // 7. Hồ bơi Hải Phòng (Tiêu chuẩn huấn luyện quốc gia)
                new[] 
                { 
                    "https://images.unsplash.com/photo-1576013551627-0cc20b96c2a7?w=800", // Cover
                    "https://images.unsplash.com/photo-1533105079780-92b9be482077?w=800", 
                    "https://images.unsplash.com/photo-1540541338287-41700207dee6?w=800", 
                    "https://images.unsplash.com/photo-1564013799919-ab600027ffc6?w=800" 
                },
                // 8. Hồ bơi Vũng Tàu (Nước mặn, thư giãn và thể thao)
                new[] 
                { 
                    "https://images.unsplash.com/photo-1572331130797-3dbd13063cce?w=800", // Cover
                    "https://images.unsplash.com/photo-1600585154340-be6161a56a0c?w=800", 
                    "https://images.unsplash.com/photo-1600596542815-ffad4c1539a9?w=800", 
                    "https://images.unsplash.com/photo-1578683010236-d716f9a3f461?w=800" 
                },
                // 9. Hồ bơi Cố Đô (Huế) (Thơ mộng bên sông Hương)
                new[] 
                { 
                    "https://images.unsplash.com/photo-1529290130-4ca3753253ae?w=800", // Cover
                    "https://images.unsplash.com/photo-1582268611958-ebfd161ef9cf?w=800", 
                    "https://images.unsplash.com/photo-1512917774080-9991f1c4c750?w=800", 
                    "https://images.unsplash.com/photo-1570129477492-45c003edd2be?w=800" 
                },
                // 10. Hồ bơi Đà Lạt (Nước ấm, phòng kính view hồ)
                new[] 
                { 
                    "https://images.unsplash.com/photo-1590073844006-33379778ae09?w=800", // Cover
                    "https://images.unsplash.com/photo-1571896349842-33c89424de2d?w=800", 
                    "https://images.unsplash.com/photo-1582719508461-905c673771fd?w=800", 
                    "https://images.unsplash.com/photo-1542314831-068cd1dbfeeb?w=800" 
                }
            };

            for (int i = 0; i < poolsToSeed.Count; i++)
            {
                var pool = poolsToSeed[i];

                // 1. Thêm ảnh cho bể bơi
                var images = poolImagesUrls[i];
                for (int imgIdx = 0; imgIdx < images.Length; imgIdx++)
                {
                    pool.PoolImages.Add(new PoolImage
                    {
                        ImageUrl = images[imgIdx],
                        IsCover = imgIdx == 0,
                        SortOrder = imgIdx + 1,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                // 2. Thêm giá vé cho bể bơi
                if (adultTicket != null)
                    pool.PoolTicketTypes.Add(new PoolTicketType { TicketTypeId = adultTicket.TicketTypeId, Price = 100000m, Status = "Active" });
                if (childTicket != null)
                    pool.PoolTicketTypes.Add(new PoolTicketType { TicketTypeId = childTicket.TicketTypeId, Price = 70000m, Status = "Active" });
                if (seniorTicket != null)
                    pool.PoolTicketTypes.Add(new PoolTicketType { TicketTypeId = seniorTicket.TicketTypeId, Price = 80000m, Status = "Active" });
                if (familyCombo != null)
                    pool.PoolTicketTypes.Add(new PoolTicketType { TicketTypeId = familyCombo.TicketTypeId, Price = 289000m, Status = "Active" });
                if (trippleCombo != null)
                    pool.PoolTicketTypes.Add(new PoolTicketType { TicketTypeId = trippleCombo.TicketTypeId, Price = 270000m, Status = "Active" });

                // 3. Thêm các slot hoạt động cho hôm nay và ngày mai với sức chứa (Capacity) đa dạng để test bộ lọc
                var poolCapacities = new[] { 30, 50, 25, 40, 30, 45, 60, 50, 35, 20 };
                var slotNames = new[] { "Slot sáng 1", "Slot sáng 2", "Slot trưa", "Slot chiều 1", "Slot chiều 2", "Slot tối" };
                var startTimes = new[] { new TimeSpan(6, 0, 0), new TimeSpan(8, 30, 0), new TimeSpan(11, 0, 0), new TimeSpan(14, 0, 0), new TimeSpan(16, 30, 0), new TimeSpan(19, 0, 0) };
                var endTimes = new[] { new TimeSpan(8, 0, 0), new TimeSpan(10, 30, 0), new TimeSpan(13, 0, 0), new TimeSpan(16, 0, 0), new TimeSpan(18, 30, 0), new TimeSpan(21, 0, 0) };

                // Seed slot cho cả hôm nay và ngày mai
                var dates = new[] { today, tomorrow };

                foreach (var date in dates)
                {
                    for (int j = 0; j < slotNames.Length; j++)
                    {
                        // Đảm bảo slot nằm trong khoảng giờ mở cửa/đóng cửa của bể bơi
                        if (startTimes[j] >= pool.OpeningTime && endTimes[j] <= pool.ClosingTime)
                        {
                            pool.PoolSlots.Add(new PoolSlot
                            {
                                SlotName = slotNames[j],
                                StartTime = startTimes[j],
                                EndTime = endTimes[j],
                                SlotDate = date,
                                Capacity = poolCapacities[i],
                                Status = "Open",
                                CreatedAt = DateTime.UtcNow
                            });
                        }
                    }
                }

                context.Pools.Add(pool);
            }

            await context.SaveChangesAsync();
        }
    }
}
