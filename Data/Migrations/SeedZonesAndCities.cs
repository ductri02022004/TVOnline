using Microsoft.EntityFrameworkCore.Migrations;

namespace TVOnline.Data.Migrations
{
    public partial class SeedZonesAndCities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Thêm các Zone
            migrationBuilder.InsertData(
                table: "Zones",
                columns: ["ZoneId", "ZoneName"],
                values: [1, "Miền Bắc"]
            );

            migrationBuilder.InsertData(
                table: "Zones",
                columns: ["ZoneId", "ZoneName"],
                values: [2, "Miền Trung"]
            );

            migrationBuilder.InsertData(
                table: "Zones",
                columns: ["ZoneId", "ZoneName"],
                values: [3, "Miền Nam"]
            );

            // Thêm các thành phố miền Bắc
            var northCities = new[] {
                "Hòa Bình", "Sơn La", "Điện Biên", "Lai Châu", "Lào Cai",
                "Yên Bái", "Phú Thọ", "Hà Giang", "Tuyên Quang", "Cao Bằng",
                "Bắc Kạn", "Thái Nguyên", "Lạng Sơn", "Quảng Ninh", "Hà Nội",
                "Bắc Ninh", "Hải Dương", "Hải Phòng", "Hưng Yên", "Thái Bình",
                "Vĩnh Phúc", "Ninh Bình"
            };

            for (int i = 0; i < northCities.Length; i++)
            {
                migrationBuilder.InsertData(
                    table: "Cities",
                    columns: ["CityId", "CityName", "ZoneId"],
                    values: [i + 1, northCities[i], 1]
                );
            }

            // Thêm các thành phố miền Trung
            var centralCities = new[] {
                "Thanh Hóa", "Nghệ An", "Hà Tĩnh", "Quảng Bình", "Quảng Trị",
                "Thừa Thiên Huế", "Đà Nẵng", "Quảng Nam", "Quảng Ngãi", "Bình Định",
                "Phú Yên", "Khánh Hòa", "Ninh Thuận", "Bình Thuận"
            };

            for (int i = 0; i < centralCities.Length; i++)
            {
                migrationBuilder.InsertData(
                    table: "Cities",
                    columns: ["CityId", "CityName", "ZoneId"],
                    values: [northCities.Length + i + 1, centralCities[i], 2]
                );
            }

            // Thêm các thành phố miền Nam
            var southCities = new[] {
                "TP Hồ Chí Minh", "Bà Rịa Vũng Tàu", "Bình Dương", "Bình Phước",
                "Đồng Nai", "Tay Ninh", "An Giang", "Bạc Liêu", "Bến Tre", "Cà Mau",
                "Kiên Thương", "Đồng Tháp", "Hậu Giang", "Kiên Giang", "Long An",
                "Sóc Trăng", "Tiền Giang", "Trà Vinh", "Vĩnh Long"
            };

            for (int i = 0; i < southCities.Length; i++)
            {
                migrationBuilder.InsertData(
                    table: "Cities",
                    columns: ["CityId", "CityName", "ZoneId"],
                    values: [northCities.Length + centralCities.Length + i + 1, southCities[i], 3]
                );
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "CityId",
                keyValues: Enumerable.Range(1, 55).Select(i => (object)i).ToArray()
            );

            migrationBuilder.DeleteData(
                table: "Zones",
                keyColumn: "ZoneId",
                keyValues: [1, 2, 3]
            );
        }
    }
}
