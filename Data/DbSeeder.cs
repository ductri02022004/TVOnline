using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TVOnline.Models;
using static TVOnline.Models.Location;

namespace TVOnline.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            // Seed Roles
            if (!await roleManager.RoleExistsAsync("Employer"))
                await roleManager.CreateAsync(new IdentityRole("Employer"));
            
            if (!await roleManager.RoleExistsAsync("JobSeeker"))
                await roleManager.CreateAsync(new IdentityRole("JobSeeker"));
            
            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        public static void SeedData(AppDbContext context)
        {
            if (!context.Zones.Any())
            {
                var zones = new List<Zone>
                {
                    new Zone { ZoneName = "Miền Bắc" },
                    new Zone { ZoneName = "Miền Trung" },
                    new Zone { ZoneName = "Miền Nam" }
                };

                context.Zones.AddRange(zones);
                context.SaveChanges();

                // Miền Bắc cities
                var northCities = new[]
                {
                    "Hòa Bình", "Sơn La", "Điện Biên", "Lai Châu", "Lào Cai",
                    "Yên Bái", "Phú Thọ", "Hà Giang", "Tuyên Quang", "Cao Bằng",
                    "Bắc Kạn", "Thái Nguyên", "Lạng Sơn", "Quảng Ninh", "Hà Nội",
                    "Bắc Ninh", "Hải Dương", "Hải Phòng", "Hưng Yên", "Thái Bình",
                    "Vĩnh Phúc", "Ninh Bình"
                };

                var northZone = context.Zones.First(z => z.ZoneName == "Miền Bắc");
                foreach (var city in northCities)
                {
                    context.Cities.Add(new Cities { CityName = city, ZoneId = northZone.ZoneId });
                }

                // Miền Trung cities
                var centralCities = new[]
                {
                    "Thanh Hóa", "Nghệ An", "Hà Tĩnh", "Quảng Bình", "Quảng Trị",
                    "Thừa Thiên Huế", "Đà Nẵng", "Quảng Nam", "Quảng Ngãi", "Bình Định",
                    "Phú Yên", "Khánh Hòa", "Ninh Thuận", "Bình Thuận"
                };

                var centralZone = context.Zones.First(z => z.ZoneName == "Miền Trung");
                foreach (var city in centralCities)
                {
                    context.Cities.Add(new Cities { CityName = city, ZoneId = centralZone.ZoneId });
                }

                // Miền Nam cities
                var southCities = new[]
                {
                    "TP Hồ Chí Minh", "Bà Rịa Vũng Tàu", "Bình Dương", "Bình Phước",
                    "Đồng Nai", "Tay Ninh", "An Giang", "Bạc Liêu", "Bến Tre", "Cà Mau",
                    "Kiên Thương", "Đồng Tháp", "Hậu Giang", "Kiên Giang", "Long An",
                    "Sóc Trăng", "Tiền Giang", "Trà Vinh", "Vĩnh Long"
                };

                var southZone = context.Zones.First(z => z.ZoneName == "Miền Nam");
                foreach (var city in southCities)
                {
                    context.Cities.Add(new Cities { CityName = city, ZoneId = southZone.ZoneId });
                }

                context.SaveChanges();
            }
        }
    }
}
