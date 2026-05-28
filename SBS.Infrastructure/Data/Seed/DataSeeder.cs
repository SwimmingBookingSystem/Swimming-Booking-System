using Microsoft.AspNetCore.Identity;
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
        if (context.Roles.Any())
        {
            return;
        }

        // 1. Seed Roles
        var roles = new List<AppRole>
        {
            new AppRole("admin") { Id = 1, NormalizedName = "ADMIN" },
            new AppRole("manager") { Id = 2, NormalizedName = "MANAGER" },
            new AppRole("staff") { Id = 3, NormalizedName = "STAFF" },
            new AppRole("customer") { Id = 4, NormalizedName = "CUSTOMER" }
        };
        context.Roles.AddRange(roles);
        await context.SaveChangesAsync();

        // 2. Seed Users
        var users = new List<AppUser>();

        // Admin
        users.Add(new AppUser
        {
            Id = 1,
            UserName = "admin1",
            NormalizedUserName = "ADMIN1",
            Email = "admin1@example.com",
            NormalizedEmail = "ADMIN1@EXAMPLE.COM",
            PasswordHash = "7676aaafb027c825bd9abab78b234070e702752f625b752e55e55b48e607e358",
            FullName = "Nguyễn Văn Chính",
            PhoneNumber = "0900000001",
            Address = "Trụ sở chính",
            RoleId = 1,
            Status = true,
            Dob = new DateOnly(1980, 1, 1),
            Gender = "Male",
            CreatedAt = DateTime.UtcNow
        });

        // Managers
        var managersInfo = new[]
        {
            (Name: "Manager One", Email: "manager1@example.com", Phone: "0987654321", Address: "456 Manager Rd", Gender: "Female", Dob: new DateOnly(1985, 5, 15), Hash: "324d52ea400e79ae65163f0b369e295c4993d26204c66317ee8e53f31ae003e3"),
            (Name: "Manager HCM", Email: "manager2@example.com", Phone: "0900000002", Address: "Hồ Chí Minh", Gender: "Female", Dob: new DateOnly(1986, 2, 2), Hash: "7bf6c1f1dc6875e22262f4db1b36336792e3acf2731803bbd659e279134387b7"),
            (Name: "Manager Đà Nẵng", Email: "manager3@example.com", Phone: "0900000003", Address: "Đà Nẵng", Gender: "Male", Dob: new DateOnly(1987, 3, 3), Hash: "c9b9b308dfc897e1d60b63ab260f59e9d210a6982f2f6ad74ccc5d89115b815a"),
            (Name: "Manager Cần Thơ", Email: "manager4@example.com", Phone: "0900000004", Address: "Cần Thơ", Gender: "Female", Dob: new DateOnly(1988, 4, 4), Hash: "1763bac215e28c909dcbd0a094b15eb66e7a5d5f1688157dddb053f584cfdce5"),
            (Name: "Manager Quy Nhơn", Email: "manager5@example.com", Phone: "0900000005", Address: "Quy Nhơn", Gender: "Male", Dob: new DateOnly(1989, 5, 5), Hash: "58bde54eb7266c46bb244b48dc0a8408412bb99d27ad57cd3812d2b2ed1aa747")
        };

        for (int i = 0; i < managersInfo.Length; i++)
        {
            var m = managersInfo[i];
            users.Add(new AppUser
            {
                Id = 2 + i,
                UserName = $"manager{i + 1}",
                NormalizedUserName = $"MANAGER{i + 1}".ToUpperInvariant(),
                Email = m.Email,
                NormalizedEmail = m.Email.ToUpperInvariant(),
                PasswordHash = m.Hash,
                FullName = m.Name,
                PhoneNumber = m.Phone,
                Address = m.Address,
                RoleId = 2,
                Status = true,
                Dob = m.Dob,
                Gender = m.Gender,
                CreatedAt = DateTime.UtcNow
            });
        }

        // Staff 1 to 25
        var staffNames = new[]
        {
            "Nguyễn Văn An", "Trần Thị Bích", "Lê Văn Cường", "Phạm Thị Dung", "Hoàng Văn Đức",
            "Nguyễn Thị Hạnh", "Lê Quốc Huy", "Phan Thị Hương", "Trịnh Văn Khoa", "Vũ Thị Lan",
            "Đặng Văn Long", "Bùi Thị Mai", "Ngô Văn Minh", "Huỳnh Thị Nga", "Tống Văn Nam",
            "Thái Thị Oanh", "Đỗ Văn Phúc", "Nguyễn Thị Quỳnh", "Trương Văn Quốc", "Tạ Thị Sương",
            "Lương Văn Sơn", "Phạm Thị Trang", "Lý Văn Thắng", "Trịnh Thị Tuyết", "Cao Văn Vinh"
        };

        for (int i = 0; i < 25; i++)
        {
            users.Add(new AppUser
            {
                Id = 7 + i,
                UserName = $"staff{i + 1}",
                NormalizedUserName = $"STAFF{i + 1}".ToUpperInvariant(),
                Email = $"staff{i + 1}@example.com",
                NormalizedEmail = $"STAFF{i + 1}@EXAMPLE.COM",
                PasswordHash = "b5465d786a2b98bbd4b8b798da4f86b34c52f64dc9a382b50c0fdb0f73f8baf1",
                FullName = staffNames[i],
                PhoneNumber = $"09100000{i + 1:D2}",
                Address = "Trụ sở phụ",
                RoleId = 3,
                Status = true,
                Dob = new DateOnly(1995, 1, 1).AddDays(i),
                Gender = i % 2 == 0 ? "Male" : "Female",
                CreatedAt = DateTime.UtcNow
            });
        }

        // Customers 1 to 17
        var customerNames = new[]
        {
            "Nguyễn Văn A", "Trần Thị B", "Lê Văn C", "Phạm Thị D", "Hoàng Văn E",
            "Đỗ Thị F", "Ngô Văn G", "Huỳnh Thị H", "Bùi Văn I", "Vũ Thị K",
            "Trịnh Văn L", "Tống Thị M", "Thân Văn N", "Trần Thị O", "Phan Văn P",
            "Trần Thị O", "Trần Thị O"
        };

        for (int i = 0; i < 17; i++)
        {
            users.Add(new AppUser
            {
                Id = 32 + i,
                UserName = $"customer{i + 1}",
                NormalizedUserName = $"CUSTOMER{i + 1}".ToUpperInvariant(),
                Email = $"cust{i + 1}@example.com",
                NormalizedEmail = $"CUST{i + 1}@EXAMPLE.COM",
                PasswordHash = "02f7f6b0cf1fd5bd487acaf5fbfcfbcac78726ef006273bcfeb76e5cfe9a0d68",
                FullName = customerNames[i],
                PhoneNumber = $"09000010{i + 1:D2}",
                Address = i % 2 == 0 ? "Hà Nội" : "Hồ Chí Minh",
                RoleId = 4,
                Status = true,
                Dob = new DateOnly(2000, 1, 1).AddYears(i % 5).AddMonths(i % 12).AddDays(i),
                Gender = i % 2 == 0 ? "Male" : "Female",
                CreatedAt = DateTime.UtcNow
            });
        }

        // Staff 26 to 101
        var staffNames2 = new[]
        {
            "Cao Văn Vinh", "Cao Văn Vu", "Cao Văn Vin", "Cao Văn Veo", "Cao Văn Van", "Cao Văn Vit", "Cao Văn Vo", "Cao Văn Vem", "Cao Văn Vot", "Cao Văn Vut",
            "Cao Văn Me", "Cao Văn Meo", "Cao Văn Teo", "Cao Văn Cu", "Cao Văn Ô", "Cao Văn Soc", "Cao Văn Tran", "Cao Văn Te", "Cao Văn Vuc", "Cao Văn Mit",
            "Cao Văn Va", "Cao Văn Vau", "Cao Văn Môi", "Cao Văn Muỗi", "Cao Văn Tít", "Cao Văn Tao", "Cao Văn Bôi", "Cao Văn Cười", "Cao Văn Nút", "Cao Văn Nít",
            "Cao Văn Ma", "Cao Văn Quỷ", "Cao Văn Hẹo", "Cao Văn Chiến", "Cao Văn C", "Cao Văn VB", "Cao Văn VS", "Cao Văn VD", "Cao Văn VF", "Cao Văn VB",
            "Cao Văn VQ", "Cao Văn VR", "Cao Văn VT", "Cao Văn VY", "Cao Văn VU", "Cao Văn VO", "Cao Văn VP", "Cao Văn VYE", "Cao Văn VE", "Cao Văn VL",
            "Cao Văn VM", "Cao Văn VMB", "Cao Văn VHH", "Cao Văn VĐ", "Cao Văn VDD", "Cao Văn VFF", "Cao Văn VGG", "Cao Văn HGH", "Cao Văn BBB", "Cao Văn CCC",
            "Cao Văn ABC", "Cao Văn BBC", "Cao Văn NNG", "Cao Văn MML", "Cao Văn JJJ", "Cao Văn AAa", "Cao Văn afF", "Cao Văn TTT", "Cao Văn CCV", "Cao Văn CHU",
            "Cao Văn Bro", "Cao Văn Bưởi", "Cao Văn Chuối", "Cao Văn CAM", "Cao Văn CHia", "Cao Văn Tĩnh"
        };

        for (int i = 0; i < staffNames2.Length; i++)
        {
            users.Add(new AppUser
            {
                Id = 49 + i,
                UserName = $"staff{26 + i}",
                NormalizedUserName = $"STAFF{26 + i}".ToUpperInvariant(),
                Email = $"staff{26 + i}@example.com",
                NormalizedEmail = $"STAFF{26 + i}@EXAMPLE.COM",
                PasswordHash = "b5465d786a2b98bbd4b8b798da4f86b34c52f64dc9a382b50c0fdb0f73f8baf1",
                FullName = staffNames2[i],
                PhoneNumber = "0910000025",
                Address = "Trụ sở phụ",
                RoleId = 3,
                Status = true,
                Dob = new DateOnly(1995, 1, 25),
                Gender = "Male",
                CreatedAt = DateTime.UtcNow
            });
        }

        context.Users.AddRange(users);
        await context.SaveChangesAsync();

        // Seed Identity UserRoles
        var userRoles = users.Select(u => new IdentityUserRole<int> { UserId = u.Id, RoleId = u.RoleId }).ToList();
        context.UserRoles.AddRange(userRoles);
        await context.SaveChangesAsync();

        // 3. Seed Branches
        var branches = new List<Branch>
        {
            new Branch { BranchId = 1, BranchName = "Chi nhánh Hà Nội", ManagerId = 2 },
            new Branch { BranchId = 2, BranchName = "Chi nhánh Hồ Chí Minh", ManagerId = 3 },
            new Branch { BranchId = 3, BranchName = "Chi nhánh Đà Nẵng", ManagerId = 4 },
            new Branch { BranchId = 4, BranchName = "Chi nhánh Cần Thơ", ManagerId = 5 },
            new Branch { BranchId = 5, BranchName = "Chi nhánh Quy Nhơn", ManagerId = 6 }
        };
        context.Branchs.AddRange(branches);
        await context.SaveChangesAsync();

        // 4. Seed Pools
        var poolsInfo = new[]
        {
            // Hanoi (Branch 1)
            (Name: "Hồ bơi Thanh Xuân", Road: "Đường Nguyễn Trãi", Address: "Hà Nội", Slots: 50, Open: "06:00:00", Close: "20:00:00", Img: "https://villas-guide.com/wp/wp-content/uploads/2024/03/villas_1.jpeg", Desc: "Bể bơi ngoài trời với không gian xanh mát, lý tưởng cho gia đình."),
            (Name: "Hồ bơi Cầu Giấy", Road: "Đường Cầu Giấy", Address: "Hà Nội", Slots: 45, Open: "06:30:00", Close: "19:30:00", Img: "https://xaydunghoboigiare.com/upload/images/5-tieu-chuan-quan-trong-trong-thiet-ke-be-boi-gia-dinh-1(1).jpg", Desc: "Hồ bơi trong nhà với hệ thống nước nóng hiện đại."),
            (Name: "Hồ bơi Đống Đa", Road: "Đường Xã Đàn", Address: "Hà Nội", Slots: 40, Open: "07:00:00", Close: "20:00:00", Img: "https://png.pngtree.com/background/20230408/original/pngtree-beautiful-indoor-swimming-pool-picture-image_2336818.jpg", Desc: "Bể bơi có khu vui chơi trẻ em an toàn và sôi động."),
            (Name: "Hồ bơi Long Biên", Road: "Đường Nguyễn Văn Cừ", Address: "Hà Nội", Slots: 60, Open: "05:30:00", Close: "21:00:00", Img: "https://booking.pystravel.vn/uploads/posts/avatar/1601051675.jpg", Desc: "Bể bơi đạt chuẩn thi đấu quốc gia, phù hợp luyện tập chuyên nghiệp."),
            (Name: "Hồ bơi Ba Đình", Road: "Đường Kim Mã", Address: "Hà Nội", Slots: 55, Open: "06:00:00", Close: "20:00:00", Img: "https://i.pinimg.com/originals/36/64/43/366443b18bf1e6e94dbd3213d0fe005a.jpg", Desc: "Khu hồ bơi thư giãn với dịch vụ spa liền kề."),
            
            // HCM (Branch 2)
            (Name: "Hồ bơi Quận 1", Road: "Đường Lê Lợi", Address: "Hồ Chí Minh", Slots: 60, Open: "05:00:00", Close: "20:00:00", Img: "https://maxtanzt.de/wp-content/uploads/17_vrh-t_pool_victors-hotel-teistungen-scaled.jpg", Desc: "Hồ bơi vô cực hướng nhìn ra công viên thành phố."),
            (Name: "Hồ bơi Quận 3", Road: "Đường Cách Mạng Tháng 8", Address: "Hồ Chí Minh", Slots: 50, Open: "06:00:00", Close: "21:00:00", Img: "https://slovenskycestovatel.sk/images/items/1171/aqua-relax-titris7355451.jpg", Desc: "Bể bơi gia đình với thiết kế hiện đại, sạch sẽ."),
            (Name: "Hồ bơi Quận 5", Road: "Đường Trần Hưng Đạo", Address: "Hồ Chí Minh", Slots: 45, Open: "06:30:00", Close: "20:00:00", Img: "https://th.bing.com/th/id/R.7c13bf7145846ee447f8cfd562336c7a?rik=YWYyLwj5s9kKbA&riu=http%3a%2f%2fhanteco.vn%2fhinhanh%2ftintuc%2fbe-boi-vo-cuc-3.jpeg&ehk=sT%2b135Jd5ASbRygifVdn7oaga3jYLIjBgkgforNif%2bM%3d&risl=&pid=ImgRaw&r=0", Desc: "Hồ bơi kết hợp cafe và khu ngồi nghỉ thư giãn."),
            (Name: "Hồ bơi Thủ Đức", Road: "Đường Võ Văn Ngân", Address: "Hồ Chí Minh", Slots: 70, Open: "05:30:00", Close: "19:30:00", Img: "https://www.prachachat.net/wp-content/uploads/2020/04/S__19570748.jpg", Desc: "Bể bơi nước mặn sử dụng công nghệ lọc tự nhiên."),
            (Name: "Hồ bơi Quận 7", Road: "Đường Nguyễn Thị Thập", Address: "Hồ Chí Minh", Slots: 55, Open: "06:00:00", Close: "20:30:00", Img: "https://wanderonwards.co/wp-content/uploads/2019/04/padma-1024x683.jpg", Desc: "Bể bơi có hệ thống âm thanh dưới nước độc đáo."),

            // Da Nang (Branch 3)
            (Name: "Hồ bơi Hải Châu", Road: "Đường Lê Duẩn", Address: "Đà Nẵng", Slots: 50, Open: "06:00:00", Close: "19:00:00", Img: "https://th.bing.com/th/id/R.905c0c37b9ed2c8ba141fb95826bc5df?rik=TN2AwKdoi0LE9w&riu=http%3a%2f%2fwww.worldwayhk.com%2fUpLoadFiles%2f20151110%2f2015111010202846.jpg&ehk=F9q02sqTyInEk4Se%2fDfeWNSV6sovHG2qe2vi3nrb9%2bM%3d&risl=&pid=ImgRaw&r=0", Desc: "Bể bơi trong khu nghỉ dưỡng cao cấp ven sông."),
            (Name: "Hồ bơi Sơn Trà", Road: "Đường Trần Hưng Đạo", Address: "Đà Nẵng", Slots: 45, Open: "06:30:00", Close: "18:30:00", Img: "https://dynamic-media-cdn.tripadvisor.com/media/photo-o/13/2a/a1/f6/centara-ras-fushi-resort.jpg?w=1200&h=-1&s=1", Desc: "Hồ bơi lộ thiên với thiết kế thân thiện với môi trường."),
            (Name: "Hồ bơi Thanh Khê", Road: "Đường Hà Huy Tập", Address: "Đà Nẵng", Slots: 40, Open: "07:00:00", Close: "19:30:00", Img: "https://thethaodonga.com/wp-content/uploads/2022/06/ho-boi-o-tphcm-2.jpg", Desc: "Bể bơi có đường bơi phân làn cho người tập thể thao."),
            (Name: "Hồ bơi Liên Chiểu", Road: "Đường Nguyễn Lương Bằng", Address: "Đà Nẵng", Slots: 60, Open: "05:30:00", Close: "20:00:00", Img: "https://ktmt.vnmediacdn.com/images/2024/04/10/83-1712742140-352375012-699494991981699-6157285300830981686-n.jpg", Desc: "Bể bơi dành riêng cho người lớn, yên tĩnh và riêng tư."),
            (Name: "Hồ bơi Ngũ Hành Sơn", Road: "Đường Ngũ Hành Sơn", Address: "Đà Nẵng", Slots: 55, Open: "06:00:00", Close: "19:30:00", Img: "https://vnanet.vn/Data/Articles/2024/06/17/7435552/vna_potal_hai_duong_tang_cuong_day_boi_cho_tre_em_dip_he_de_phong_tranh_duoi_nuoc__stand.jpg", Desc: "Bể bơi giải trí có cầu trượt nước và vòi phun vui nhộn."),

            // Can Tho (Branch 4)
            (Name: "Hồ bơi Ninh Kiều", Road: "Đường 30 Tháng 4", Address: "Cần Thơ", Slots: 50, Open: "06:00:00", Close: "20:00:00", Img: "https://turftown.in/_next/image?url=https%3A%2F%2Fturftown.s3.ap-south-1.amazonaws.com%2Fsuper_admin%2Ftt-1722768448280.webp&w=3840&q=75", Desc: "Hồ bơi nhỏ dành cho trẻ sơ sinh với độ sâu an toàn."),
            (Name: "Hồ bơi Bình Thủy", Road: "Đường Nguyễn Trãi", Address: "Cần Thơ", Slots: 45, Open: "06:30:00", Close: "19:30:00", Img: "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSx26g6WKHSQW34d2dRT6KVuG3KqJ0BPNuLLw&s", Desc: "Hồ bơi trên tầng thượng khách sạn với tầm nhìn toàn cảnh."),
            (Name: "Hồ bơi Cái Răng", Road: "Đường Quốc lộ 91B", Address: "Cần Thơ", Slots: 40, Open: "07:00:00", Close: "20:00:00", Img: "https://hoboinhatrang.vn/filestorage/article/large/lc2.jpg", Desc: "Bể bơi ban đêm có hệ thống đèn LED đổi màu hiện đại."),
            (Name: "Hồ bơi Ô Môn", Road: "Đường Phan Văn Trị", Address: "Cần Thơ", Slots: 60, Open: "05:30:00", Close: "21:00:00", Img: "https://www.srsmith.com/media/180835/build-your-own-pool-slide-beauty-923x730.jpg?mode=pad&width=725&height=573&bgcolor=fff&rnd=133054065692270000", Desc: "Bể bơi có lớp học bơi hàng ngày cho mọi lứa tuổi."),
            (Name: "Hồ bơi Thốt Nốt", Road: "Đường Trần Hưng Đạo", Address: "Cần Thơ", Slots: 55, Open: "06:00:00", Close: "20:00:00", Img: "https://www.eugene-or.gov/ImageRepository/Document?documentID=67182", Desc: "Hồ bơi có mái che linh hoạt, sử dụng quanh năm."),

            // Quy Nhon (Branch 5)
            (Name: "Hồ bơi Nguyễn Huệ", Road: "Đường Nguyễn Huệ", Address: "Quy Nhơn", Slots: 50, Open: "06:00:00", Close: "19:00:00", Img: "https://cdn.baogialai.com.vn/images/b6e9e273388cf373c7197a59d2310437352c2851cd5b8e027e0695c8f296f53af1dc3f5b91b26aae470176cec4d4439d2e69976aae899185fffdddb094a72d3e/1vn-5274.jpg", Desc: "Bể bơi phong cách resort giữa lòng thành phố."),
            (Name: "Hồ bơi An Dương Vương", Road: "Đường An Dương Vương", Address: "Quy Nhơn", Slots: 45, Open: "06:30:00", Close: "19:30:00", Img: "https://static.vinwonders.com/production/ho-boi-quan-9.jpg", Desc: "Hồ bơi nước ấm, phù hợp cho người cao tuổi."),
            (Name: "Hồ bơi Lê Lợi", Road: "Đường Lê Lợi", Address: "Quy Nhơn", Slots: 40, Open: "07:00:00", Close: "20:00:00", Img: "https://streamline.imgix.net/eb0267a4-922d-4297-a476-194d6c689471/82abffc9-632a-44b3-abed-d0ea03d20d84/IMG_5342_edited.jpg?ixlib=rb-1.1.0&w=2000&h=2000&fit=max&or=0&s=58e787b840cd62bfacb839b701e5e9e0", Desc: "Bể bơi thiết kế hình dạng đặc biệt, tạo cảm hứng sáng tạo."),
            (Name: "Hồ bơi Trần Phú", Road: "Đường Trần Phú", Address: "Quy Nhơn", Slots: 60, Open: "05:30:00", Close: "21:00:00", Img: "https://bcp.cdnchinhphu.vn/334894974524682240/2024/4/11/an-toan-thiet-bi-be-boi-17128195925281735826251.jpg", Desc: "Bể bơi nằm cạnh vườn cây nhiệt đới, không khí trong lành."),
            (Name: "Hồ bơi Phú Tài", Road: "Đường Tây Sơn", Address: "Quy Nhơn", Slots: 55, Open: "06:00:00", Close: "20:00:00", Img: "https://prihoda.co.uk/wp-content/uploads/2015/09/Swiming-pool-fabric-ducting-prihoda-8.jpg", Desc: "Bể bơi dịch vụ cao cấp với phòng thay đồ riêng biệt.")
        };

        var pools = new List<Pool>();
        for (int i = 0; i < poolsInfo.Length; i++)
        {
            var p = poolsInfo[i];
            pools.Add(new Pool
            {
                PoolId = i + 1,
                PoolName = p.Name,
                PoolRoad = p.Road,
                PoolAddress = p.Address,
                MaxSlot = p.Slots,
                OpenTime = TimeSpan.Parse(p.Open),
                CloseTime = TimeSpan.Parse(p.Close),
                PoolStatus = true,
                PoolImage = p.Img,
                PoolDescription = p.Desc,
                BranchId = (i / 5) + 1,
                CreatedAt = DateTime.UtcNow
            });
        }
        context.Pools.AddRange(pools);
        await context.SaveChangesAsync();

        // 5. Seed Staff Types
        var staffTypes = new List<StaffType>
        {
            new StaffType { StaffTypeId = 1, TypeName = "Nhân viên kỹ thuật", Description = "Bảo trì thiết bị và cơ sở hạ tầng" },
            new StaffType { StaffTypeId = 2, TypeName = "Nhân viên soát vé", Description = "Kiểm tra vé khách hàng tại bể bơi" },
            new StaffType { StaffTypeId = 3, TypeName = "Nhân viên kiểm tra thiết bị", Description = "Theo dõi và báo cáo tình trạng thiết bị" },
            new StaffType { StaffTypeId = 4, TypeName = "Nhân viên hỗ trợ dịch vụ", Description = "Hỗ trợ khách hàng sử dụng dịch vụ tại bể bơi" }
        };
        context.StaffTypes.AddRange(staffTypes);
        await context.SaveChangesAsync();

        // 6. Seed Staffs (Repeating 4 staff types per pool sequentially)
        var staffUserIds = new List<int>();
        for (int id = 7; id <= 31; id++) staffUserIds.Add(id);
        for (int id = 49; id <= 124; id++) staffUserIds.Add(id);

        int staffIndex = 0;
        for (int poolId = 1; poolId <= 25; poolId++)
        {
            for (int typeId = 1; typeId <= 4; typeId++)
            {
                if (staffIndex < staffUserIds.Count)
                {
                    int userId = staffUserIds[staffIndex++];
                    int branchId = ((poolId - 1) / 5) + 1;
                    context.Staffs.Add(new Staff
                    {
                        UserId = userId,
                        BranchId = branchId,
                        PoolId = poolId,
                        StaffTypeId = typeId
                    });
                }
            }
        }
        await context.SaveChangesAsync();

        // 7. Seed Pool Devices for all 25 Pools
        var deviceTemplates = new[]
        {
            (Name: "Camera giám sát", Img: "https://cameraluuphuc.com/wp-content/uploads/2021/10/DH-HAC-HDW1200R-VF.png"),
            (Name: "Tủ thuốc sơ cứu", Img: "https://noithattheones.com/wp-content/uploads/2023/05/tu-thuoc-y-te-dep-TYT02I430.jpg"),
            (Name: "Loa thông báo", Img: "https://amthanhthudo.com/wp-content/uploads/hinh-anh-loa-thong-bao-Aplus-A-H150T.jpg"),
            (Name: "Ghế tắm nắng", Img: "https://banghexanh.vn/wp-content/uploads/2024/02/Giuong-Tam-Nang-Be-Boi-TL813.jpg"),
            (Name: "Ô che nắng", Img: "https://th.bing.com/th/id/OIP.ne1Rv1HeeEPi4haW4MHUAgHaE-?rs=1&pid=ImgDetMain"),
            (Name: "Phao cứu hộ", Img: "https://th.bing.com/th/id/OIP.5BAEvEdVd_VsZ-j5hAp6hQHaHa?w=197&h=197&c=7&r=0&o=5&pid=1.7"),
            (Name: "Cầu trượt nước", Img: "https://www.travelingan.net/wp-content/uploads/2019/07/Wahana-Jogja-Bay-Waterpark.jpg"),
            (Name: "Đồng hồ đo nhiệt độ nước", Img: "https://th.bing.com/th/id/OIP.e2Mny9THyddXLj0Qlhs9vgHaHa?rs=1&pid=ImgDetMain"),
            (Name: "Thiết bị đo chất lượng nước (pH, Clo)", Img: "https://th.bing.com/th/id/OIP.UJIBD9OAoPmy2TG-wgb1ZAAAAA?rs=1&pid=ImgDetMain"),
            (Name: "Máy bán hàng tự động", Img: "https://nwzimg.wezhan.cn/contents/sitefiles2066/10332247/images/48333183.jpg"),
            (Name: "Vòi sen ngoài trời", Img: "https://i.pinimg.com/474x/61/fd/66/61fd665a4cee278ca7350b0d93954d63.jpg"),
            (Name: "Thảm chống trơn trượt", Img: "https://th.bing.com/th/id/OIP.JHwxdZYNKl2YCojmAr6QQAHaEz?rs=1&pid=ImgDetMain"),
            (Name: "Giá treo khăn", Img: "https://th.bing.com/th/id/OIP.7GZAVYKI3C5KXdduktHM4wHaHa?rs=1&pid=ImgDetMain"),
            (Name: "Máy sấy tóc", Img: "https://th.bing.com/th/id/OIP.9tXCgr7wlVgTvOdYw6DH1wHaHa?rs=1&pid=ImgDetMain"),
            (Name: "Bảng nội quy/hướng dẫn an toàn", Img: "https://hoabico.com/wp-content/uploads/2022/05/noi-quy-be-boi.jpg"),
            (Name: "Đèn chiếu sáng khu vực hồ", Img: "https://nclighting.vn/wp-content/uploads/2021/07/Den-Led-be-boi-chieu-sang.jpg"),
            (Name: "Hệ thống báo động khẩn cấp", Img: "https://th.bing.com/th/id/OIP.nOLn0XWcvysYz38_1RT9XQHaHa?rs=1&pid=ImgDetMain"),
            (Name: "Tủ giữ điện thoại/chìa khóa", Img: "https://down-vn.img.susercontent.com/file/vn-11134201-7qukw-lglyflm1se9e9d"),
            (Name: "Cây nước lọc uống trực tiếp", Img: "https://sanakyvietnam.net/wp-content/uploads/binh-nong-lanh-alaska-co-tot-khong.jpg"),
            (Name: "Tủ đồ cá nhân", Img: "https://th.bing.com/th/id/OIP.VuLqwyno-aqvXZ2Yco6S_QHaHa?w=217&h=217&c=7&r=0&o=5&pid=1.7")
        };

        var poolDevices = new List<PoolDevice>();
        for (int poolId = 1; poolId <= 25; poolId++)
        {
            // Seed 20 devices for this pool
            foreach (var dev in deviceTemplates)
            {
                poolDevices.Add(new PoolDevice
                {
                    PoolId = poolId,
                    DeviceImage = dev.Img,
                    DeviceName = dev.Name,
                    Quantity = 20,
                    DeviceStatus = "available",
                    Notes = null
                });
            }
        }
        context.PoolDevices.AddRange(poolDevices);
        await context.SaveChangesAsync();
    }
}
