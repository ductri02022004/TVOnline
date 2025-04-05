using Microsoft.AspNetCore.Identity;
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
                
            if (!await roleManager.RoleExistsAsync("Premium"))
                await roleManager.CreateAsync(new IdentityRole("Premium"));
        }

        public static async Task SeedUsersAsync(UserManager<Users> userManager)
        {
            if (!userManager.Users.Any())
            {
                var users = new List<Users>
                {
                    new Users { Id = "useremp001", UserName = "employer1@example.com", Email = "employer1@example.com", EmailConfirmed = true },
                    new Users { Id = "useremp002", UserName = "employer2@example.com", Email = "employer2@example.com", EmailConfirmed = true },
                    new Users { Id = "useremp003", UserName = "employer3@example.com", Email = "employer3@example.com", EmailConfirmed = true },
                    new Users { Id = "useremp004", UserName = "employer4@example.com", Email = "employer4@example.com", EmailConfirmed = true },
                    new Users { Id = "useremp005", UserName = "employer5@example.com", Email = "employer5@example.com", EmailConfirmed = true },
                    // ... Thêm các User khác nếu cần
                };

                foreach (var user in users)
                {
                    var result = await userManager.CreateAsync(user, "P@$$wOrd123"); // Mật khẩu mặc định, nên thay đổi trong thực tế
                    if (!result.Succeeded)
                    {
                        // Xử lý lỗi nếu tạo user không thành công
                        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    }
                    else
                    {
                        await userManager.AddToRoleAsync(user, "Employer");
                    }
                }
            }
        }

        public static async Task SeedAdminUserAsync(UserManager<Users> userManager)
        {
            var adminUser = await userManager.FindByEmailAsync("admin@tvonline.com");
            if (adminUser == null)
            {
                var admin = new Users
                {
                    UserName = "admin@tvonline.com",
                    Email = "admin@tvonline.com",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true
                };
                var result = await userManager.CreateAsync(admin, "123123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }
        }

        public static async Task SeedEmployersAsync(AppDbContext context)
        {
            if (!context.Employers.Any())
            {
                var employers = new List<Employers>
            {
                new Employers
                {
                    EmployerId = "EMP001",
                    UserId = "useremp001", // Linked to the first employer user
                    Email = "employer1@example.com", // Match with user email
                    CompanyName = "FPT Corporation",
                    Description = "FPT is a leading technology corporation in Vietnam.",
                    Field = "Information Technology",
                    LogoURL = "https://upload.wikimedia.org/wikipedia/commons/thumb/1/11/FPT_logo_2010.svg/1200px-FPT_logo_2010.svg.png",
                    CityId = 16, // Hà Nội
                    CreatedAt = DateTime.Now.AddDays(-30)
                },
                new Employers
                {
                    EmployerId = "EMP002",
                    UserId = "useremp002", // Linked to the second employer user
                    Email = "employer2@example.com", // Match with user email
                    CompanyName = "Vingroup",
                    Description = "Vingroup is the largest conglomerate in Vietnam.",
                    Field = "Real Estate, Retail, Technology",
                    LogoURL = "https://upload.wikimedia.org/wikipedia/vi/thumb/9/98/Vingroup_logo.svg/1200px-Vingroup_logo.svg.png",
                    CityId = 18, // Hải Phòng
                    CreatedAt = DateTime.Now.AddDays(-25)
                },
                new Employers
                {
                    EmployerId = "EMP003",
                    UserId = "useremp003", // Linked to the third employer user
                    Email = "employer3@example.com", // Match with user email
                    CompanyName = "Viettel Group",
                    Description = "Viettel is a Vietnamese multinational telecommunications company.",
                    Field = "Telecommunications, Technology",
                    LogoURL = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTFf_gfrXDExCO4VEww3h5M-oa8q0YMeySM9Q&s",
                    CityId = 16, // Hà Nội
                    CreatedAt = DateTime.Now.AddDays(-20)
                },
                new Employers
                {
                    EmployerId = "EMP004",
                    UserId = "useremp004", // Linked to the fourth employer user
                    Email = "employer4@example.com", // Match with user email
                    CompanyName = "Techcombank",
                    Description = "Techcombank is one of the largest commercial banks in Vietnam.",
                    Field = "Banking, Finance",
                    LogoURL = "https://upload.wikimedia.org/wikipedia/commons/7/7c/Techcombank_logo.png",
                    CityId = 19, // Hưng Yên
                    CreatedAt = DateTime.Now.AddDays(-15)
                },
                new Employers
                {
                    EmployerId = "EMP005",
                    UserId = "useremp005", // Linked to the fifth employer user
                    Email = "employer5@example.com", // Match with user email
                    CompanyName = "VNPT",
                    Description = "VNPT is a leading telecommunications group in Vietnam.",
                    Field = "Telecommunications",
                    LogoURL = "https://upload.wikimedia.org/wikipedia/vi/thumb/6/65/VNPT_Logo.svg/1200px-VNPT_Logo.svg.png",
                    CityId = 16, // Hà Nội
                    CreatedAt = DateTime.Now.AddDays(-10)
                }
            };
                context.Employers.AddRange(employers);
                await context.SaveChangesAsync();
            }
        }

        public static async Task SeedPostsAsync(AppDbContext context)
        {
            if (!context.Posts.Any())
            {
                List<Post> posts =
                [
                    new Post
                    {
                        PostId = "POST001", // Added PostId as string
                        Title = "Lập trình viên Backend",
                        Description =
                            "Chúng tôi đang tìm kiếm một lập trình viên Backend có kinh nghiệm làm việc với C# và .NET Core để phát triển hệ thống quản lý doanh nghiệp.",
                        Benefits = """
                                   - Lương thưởng hấp dẫn.
                                   - Môi trường làm việc chuyên nghiệp, năng động.
                                   - Cơ hội thăng tiến và phát triển bản thân.
                                   """,
                        Salary = 15000000m,
                        Position = "Backend Developer",
                        Experience = "2 năm kinh nghiệm",
                        CityId = 16, // Hà Nội
                        EmployerId = "EMP001",
                        CreatedAt = DateTime.Now.AddDays(-1),
                        IsActive = true,
                        Requirements = """
                                       - Thành thạo C# và .NET Core.
                                       - Có kinh nghiệm làm việc với SQL Server hoặc MySQL.
                                       - Hiểu về kiến trúc microservices và RESTful API.
                                       """,
                        JobType = "Full-time"
                    },

                    new Post
                    {
                        PostId = "POST002", // Added PostId as string
                        Title = "Nhân viên hỗ trợ IT",
                        Description =
                            "Công ty cần tuyển nhân viên hỗ trợ IT có trách nhiệm cài đặt, bảo trì hệ thống máy tính, phần mềm và hỗ trợ nhân viên trong công ty.",
                        Benefits = """
                                   - Bảo hiểm đầy đủ theo quy định nhà nước.
                                   - Phụ cấp ăn trưa và xăng xe.
                                   - Du lịch hàng năm cùng công ty.
                                   """,
                        Salary = 12000000m,
                        Position = "IT Support",
                        Experience = "1 năm kinh nghiệm",
                        CityId = 18, // Hải Phòng
                        EmployerId = "EMP002",
                        CreatedAt = DateTime.Now.AddDays(-2),
                        IsActive = true,
                        Requirements = """
                                       - Biết về phần cứng và hệ điều hành Windows.
                                       - Có khả năng xử lý các sự cố phần mềm và mạng máy tính.
                                       - Giao tiếp tốt và có tinh thần hỗ trợ khách hàng.
                                       """,
                        JobType = "Full-time"
                    },

                    new Post
                    {
                        PostId = "POST003", // Added PostId as string
                        Title = "Lập trình viên Frontend",
                        Description =
                            "Chúng tôi cần tuyển lập trình viên Frontend làm việc từ xa, yêu cầu có kinh nghiệm với ReactJS để xây dựng các giao diện web tương tác.",
                        Benefits = """
                                   - Làm việc từ xa, thời gian linh hoạt.
                                   - Lương thưởng cạnh tranh.
                                   - Cơ hội làm việc với đội ngũ kỹ sư quốc tế.
                                   """,
                        Salary = 18000000m,
                        Position = "Frontend Developer",
                        Experience = "3 năm kinh nghiệm",
                        CityId = 2, // Sơn La
                        EmployerId = "EMP003",
                        CreatedAt = DateTime.Now.AddDays(-3),
                        IsActive = true,
                        Requirements = """
                                       - Thành thạo ReactJS, HTML, CSS.
                                       - Có kinh nghiệm với Redux hoặc các thư viện state management khác.
                                       - Hiểu về responsive design và tối ưu hiệu suất frontend.
                                       """,
                        JobType = "Remote"
                    },

                    new Post
                    {
                        PostId = "POST004", // Added PostId as string
                        Title = "Chuyên viên QA/QC",
                        Description =
                            "Tuyển chuyên viên QA/QC để kiểm thử phần mềm, đảm bảo chất lượng sản phẩm trước khi đưa vào sử dụng.",
                        Benefits = """
                                   - Môi trường làm việc chuyên nghiệp, năng động.
                                   - Được đào tạo chuyên sâu về testing và automation.
                                   - Lương thưởng hấp dẫn, chế độ bảo hiểm đầy đủ.
                                   """,
                        Salary = 14000000m,
                        Position = "QA Engineer",
                        Experience = "2 năm kinh nghiệm",
                        CityId = 16, // Hà Nội
                        EmployerId = "EMP004",
                        CreatedAt = DateTime.Now.AddDays(-4),
                        IsActive = true,
                        Requirements = """
                                       - Có kinh nghiệm kiểm thử manual và automation.
                                       - Thành thạo các công cụ như Selenium, JMeter.
                                       - Hiểu về quy trình phát triển phần mềm và Agile/Scrum.
                                       """,
                        JobType = "Full-time"
                    },

                    new Post
                    {
                        PostId = "POST005", // Added PostId as string
                        Title = "Lập trình viên Mobile",
                        Description =
                            "Chúng tôi cần một lập trình viên mobile chuyên về React Native hoặc Flutter để phát triển ứng dụng di động trên iOS và Android.",
                        Benefits = """
                                   - Làm việc tại văn phòng hoặc từ xa tùy chọn.
                                   - Cơ hội làm việc với dự án startup sáng tạo.
                                   - Thưởng theo hiệu suất và đóng góp cho dự án.
                                   """,
                        Salary = 20000000m,
                        Position = "Mobile Developer",
                        Experience = "2 năm kinh nghiệm",
                        CityId = 18, // Hải Phòng
                        EmployerId = "EMP005",
                        CreatedAt = DateTime.Now.AddDays(-5),
                        IsActive = true,
                        Requirements = """
                                       - Thành thạo React Native hoặc Flutter.
                                       - Có kinh nghiệm làm việc với Firebase hoặc GraphQL.
                                       - Khả năng tối ưu hiệu suất ứng dụng trên thiết bị di động.
                                       """,
                        JobType = "Remote"
                    },

                    new Post
                    {
                        PostId = "POST006", // Added PostId as string
                        Title = "Data Analyst",
                        Description =
                            "Công ty đang tìm kiếm một chuyên gia phân tích dữ liệu để xử lý và trực quan hóa dữ liệu, hỗ trợ ra quyết định kinh doanh.",
                        Benefits = """
                                   - Lương thưởng hấp dẫn.
                                   - Làm việc với hệ thống dữ liệu lớn, cơ hội học hỏi cao.
                                   - Hỗ trợ chi phí đào tạo và chứng chỉ chuyên môn.
                                   """,
                        Salary = 22000000m,
                        Position = "Data Analyst",
                        Experience = "3 năm kinh nghiệm",
                        CityId = 3, // Điện Biên
                        EmployerId = "EMP001",
                        CreatedAt = DateTime.Now.AddDays(-6),
                        IsActive = true,
                        Requirements = """
                                       - Có kinh nghiệm với SQL, Python, hoặc R.
                                       - Hiểu biết về Power BI hoặc Tableau.
                                       - Khả năng phân tích và trực quan hóa dữ liệu tốt.
                                       """,
                        JobType = "Full-time"
                    },

                    new Post
                    {
                        PostId = "POST007", // Added PostId as string
                        Title = "DevOps Engineer",
                        Description =
                            "Công ty đang tìm kiếm DevOps Engineer để tối ưu hóa quy trình CI/CD và quản lý hạ tầng cloud.",
                        Benefits = """
                                   - Làm việc với công nghệ cloud tiên tiến.
                                   - Thời gian làm việc linh hoạt.
                                   - Hỗ trợ thiết bị làm việc và các chi phí liên quan.
                                   """,
                        Salary = 25000000m,
                        Position = "DevOps Engineer",
                        Experience = "3-5 năm kinh nghiệm",
                        CityId = 16, // Hà Nội
                        EmployerId = "EMP002",
                        CreatedAt = DateTime.Now.AddDays(-7),
                        IsActive = true,
                        Requirements = """
                                       - Thành thạo Docker, Kubernetes, và CI/CD pipeline.
                                       - Có kinh nghiệm với AWS, Azure, hoặc GCP.
                                       - Hiểu biết về bảo mật hệ thống và quản lý hạ tầng.
                                       """,
                        JobType = "Full-time"
                    },

                    new Post
                    {
                        PostId = "POST008", // Added PostId as string
                        Title = "Business Analyst",
                        Description =
                            "Công ty cần tuyển Business Analyst để thu thập và phân tích yêu cầu từ khách hàng, hỗ trợ phát triển phần mềm.",
                        Benefits = """
                                   - Được làm việc với đội ngũ chuyên gia công nghệ.
                                   - Cơ hội thăng tiến lên vị trí quản lý.
                                   - Lương thưởng theo dự án và KPI.
                                   """,
                        Salary = 18000000m,
                        Position = "Business Analyst",
                        Experience = "2-4 năm kinh nghiệm",
                        CityId = 18, // Hải Phòng
                        EmployerId = "EMP003",
                        CreatedAt = DateTime.Now.AddDays(-8),
                        IsActive = true,
                        Requirements = """
                                       - Có kinh nghiệm làm việc với khách hàng để thu thập yêu cầu.
                                       - Thành thạo kỹ năng phân tích hệ thống.
                                       - Có kiến thức về Agile/Scrum.
                                       """,
                        JobType = "Full-time"
                    },

                    new Post
                    {
                        PostId = "POST009", // Added PostId as string
                        Title = "System Administrator",
                        Description =
                            "Công ty cần tuyển System Administrator để quản lý và duy trì hệ thống máy chủ, đảm bảo hoạt động ổn định của hệ thống IT.",
                        Benefits = """
                                   - Lương thưởng hấp dẫn, thưởng theo dự án.
                                   - Được đào tạo và cập nhật công nghệ mới thường xuyên.
                                   - Bảo hiểm sức khỏe toàn diện và các chế độ phúc lợi khác.
                                   """,
                        Salary = 20000000m,
                        Position = "System Administrator",
                        Experience = "3 năm kinh nghiệm",
                        CityId = 3, // Điện Biên
                        EmployerId = "EMP004",
                        CreatedAt = DateTime.Now.AddDays(-9),
                        IsActive = true,
                        Requirements = """
                                       - Có kinh nghiệm quản trị hệ thống Linux và Windows Server.
                                       - Thành thạo các công cụ giám sát hệ thống như Zabbix, Prometheus.
                                       - Hiểu biết về bảo mật hệ thống và mạng.
                                       """,
                        JobType = "Full-time"
                    },

                    new Post
                    {
                        PostId = "POST010", // Added PostId as string
                        Title = "DevOps Engineer",
                        Description =
                            "Chúng tôi đang tìm kiếm DevOps Engineer có kinh nghiệm để tối ưu hóa quy trình CI/CD và đảm bảo hệ thống vận hành ổn định.",
                        Benefits = """
                                   - Môi trường làm việc linh hoạt, có thể làm việc remote.
                                   - Chế độ lương thưởng cạnh tranh, thưởng hiệu suất hàng quý.
                                   - Cơ hội tiếp cận các công nghệ mới như Kubernetes, Docker, Terraform.
                                   """,
                        Salary = 25000000m,
                        Position = "DevOps Engineer",
                        Experience = "3-5 năm kinh nghiệm",
                        CityId = 16, // Hà Nội
                        EmployerId = "EMP005",
                        CreatedAt = DateTime.Now.AddDays(-10),
                        IsActive = true,
                        Requirements = """
                                       - Có kinh nghiệm với Docker, Kubernetes, CI/CD Pipelines.
                                       - Thành thạo các công cụ quản lý cấu hình như Ansible, Terraform.
                                       - Kiến thức vững về hệ thống Linux và bảo mật cloud.
                                       """,
                        JobType = "Remote"
                    },

                    new Post
                    {
                        PostId = "POST011", // Added PostId as string
                        Title = "Nhân viên Marketing Online",
                        Description =
                            "Tuyển nhân viên Marketing Online có kinh nghiệm chạy quảng cáo Facebook, Google Ads.",
                        Benefits = """
                                   - Lương cơ bản và hoa hồng theo doanh số.
                                   - Thưởng các dịp lễ, Tết.
                                   - Môi trường làm việc trẻ trung, năng động.
                                   """,
                        Salary = 10000000m,
                        Position = "Marketing Executive",
                        Experience = "1 năm kinh nghiệm",
                        CityId = 30, // TP Hồ Chí Minh
                        EmployerId = "EMP001",
                        CreatedAt = DateTime.Now.AddDays(-11),
                        IsActive = true,
                        Requirements = """
                                       - Kinh nghiệm chạy quảng cáo Facebook, Google Ads.
                                       - Có kiến thức về SEO, content marketing là một lợi thế.
                                       - Sáng tạo, nhiệt tình, chịu được áp lực công việc.
                                       """,
                        JobType = "Full-time"
                    },

                    new Post
                    {
                        PostId = "POST012", // Added PostId as string
                        Title = "Nhân viên Kinh doanh Bất động sản",
                        Description =
                            "Công ty Bất động sản cần tuyển nhân viên kinh doanh có kinh nghiệm, đam mê với lĩnh vực bất động sản.",
                        Benefits = """
                                   - Hoa hồng cao, thưởng nóng hấp dẫn.
                                   - Được đào tạo chuyên nghiệp về bất động sản.
                                   - Cơ hội thăng tiến nhanh.
                                   """,
                        Salary = 8000000m,
                        Position = "Sales Agent",
                        Experience = "Ưu tiên có kinh nghiệm",
                        CityId = 35, // Bình Dương
                        EmployerId = "EMP002",
                        CreatedAt = DateTime.Now.AddDays(-12),
                        IsActive = true,
                        Requirements = """
                                       - Đam mê kinh doanh, đặc biệt là bất động sản.
                                       - Kỹ năng giao tiếp, thuyết phục tốt.
                                       - Năng động, chịu khó, có trách nhiệm.
                                       """,
                        JobType = "Full-time"
                    },

                    new Post
                    {
                        PostId = "POST013", // Added PostId as string
                        Title = "Giáo viên Tiếng Anh",
                        Description = "Trung tâm Anh ngữ cần tuyển giáo viên Tiếng Anh full-time và part-time.",
                        Benefits = """
                                   - Lương cạnh tranh, thưởng theo năng lực.
                                   - Môi trường làm việc quốc tế, chuyên nghiệp.
                                   - Cơ hội phát triển sự nghiệp trong ngành giáo dục.
                                   """,
                        Salary = 12000000m,
                        Position = "English Teacher",
                        Experience = "Không yêu cầu kinh nghiệm",
                        CityId = 32, // Bà Rịa Vũng Tàu
                        EmployerId = "EMP003",
                        CreatedAt = DateTime.Now.AddDays(-13),
                        IsActive = true,
                        Requirements = """
                                       - Có chứng chỉ TESOL hoặc tương đương là một lợi thế.
                                       - Phát âm chuẩn, ngữ pháp vững.
                                       - Yêu thích công việc giảng dạy, nhiệt tình.
                                       """,
                        JobType = "Full-time/Part-time"
                    },

                    new Post
                    {
                        PostId = "POST014", // Added PostId as string
                        Title = "Kế toán Tổng hợp",
                        Description = "Công ty cần tuyển Kế toán Tổng hợp có kinh nghiệm làm việc ít nhất 2 năm.",
                        Benefits = """
                                   - Lương thỏa thuận theo năng lực.
                                   - Đóng BHXH, BHYT, BHTN theo quy định.
                                   - Môi trường làm việc ổn định, lâu dài.
                                   """,
                        Salary = 14000000m,
                        Position = "Chief Accountant",
                        Experience = "2 năm kinh nghiệm",
                        CityId = 40, // Đồng Nai
                        EmployerId = "EMP004",
                        CreatedAt = DateTime.Now.AddDays(-14),
                        IsActive = true,
                        Requirements = """
                                       - Tốt nghiệp Đại học chuyên ngành Kế toán, Kiểm toán.
                                       - Kinh nghiệm ít nhất 2 năm ở vị trí tương đương.
                                       - Sử dụng thành thạo phần mềm kế toán.
                                       """,
                        JobType = "Full-time"
                    },

                    new Post
                    {
                        PostId = "POST015", // Added PostId as string
                        Title = "Nhân viên Thiết kế Đồ họa",
                        Description = "Tuyển nhân viên Thiết kế Đồ họa có kinh nghiệm sử dụng Photoshop, Illustrator.",
                        Benefits = """
                                   - Lương hấp dẫn theo năng lực thiết kế.
                                   - Thưởng dự án, thưởng sáng tạo.
                                   - Môi trường làm việc thoải mái, sáng tạo.
                                   """,
                        Salary = 11000000m,
                        Position = "Graphic Designer",
                        Experience = "1 năm kinh nghiệm",
                        CityId = 45, // Cần Thơ
                        EmployerId = "EMP005",
                        CreatedAt = DateTime.Now.AddDays(-15),
                        IsActive = true,
                        Requirements = """
                                       - Sử dụng thành thạo Photoshop, Illustrator, InDesign.
                                       - Có tư duy sáng tạo, gu thẩm mỹ tốt.
                                       - Chịu được áp lực công việc, đúng deadline.
                                       """,
                        JobType = "Full-time"
                    },

                    new Post
                    {
                        PostId = "POST016", // Added PostId as string
                        Title = "Quản lý Nhà hàng",
                        Description = "Nhà hàng cao cấp tuyển Quản lý Nhà hàng có kinh nghiệm quản lý ít nhất 3 năm.",
                        Benefits = """
                                   - Lương thỏa thuận, thưởng theo doanh số nhà hàng.
                                   - Tip, phụ cấp, các chế độ đãi ngộ khác.
                                   - Cơ hội làm việc trong môi trường chuyên nghiệp.
                                   """,
                        Salary = 16000000m,
                        Position = "Restaurant Manager",
                        Experience = "3 năm kinh nghiệm",
                        CityId = 50, // Kiên Giang
                        EmployerId = "EMP001",
                        CreatedAt = DateTime.Now.AddDays(-16),
                        IsActive = true,
                        Requirements = """
                                       - Kinh nghiệm quản lý nhà hàng ít nhất 3 năm.
                                       - Kỹ năng quản lý nhân sự, điều hành nhà hàng tốt.
                                       - Chịu được áp lực công việc cao.
                                       """,
                        JobType = "Full-time"
                    },

                    new Post
                    {
                        PostId = "POST017", // Added PostId as string
                        Title = "Đầu bếp",
                        Description = "Nhà hàng cần tuyển Đầu bếp có kinh nghiệm nấu món Âu hoặc món Á.",
                        Benefits = """
                                   - Lương cạnh tranh, thưởng theo tay nghề.
                                   - Bao ăn ở, phụ cấp khác.
                                   - Môi trường làm việc chuyên nghiệp, bếp sạch sẽ.
                                   """,
                        Salary = 13000000m,
                        Position = "Chef",
                        Experience = "2 năm kinh nghiệm",
                        CityId = 55, // Vĩnh Long
                        EmployerId = "EMP002",
                        CreatedAt = DateTime.Now.AddDays(-17),
                        IsActive = true,
                        Requirements = """
                                       - Kinh nghiệm nấu món Âu hoặc món Á ít nhất 2 năm.
                                       - Sức khỏe tốt, nhanh nhẹn, sạch sẽ.
                                       - Yêu thích công việc nấu ăn.
                                       """,
                        JobType = "Full-time"
                    },

                    new Post
                    {
                        PostId = "POST018", // Added PostId as string
                        Title = "Lễ tân Khách sạn",
                        Description = "Khách sạn 5 sao tuyển Lễ tân Khách sạn, yêu cầu Tiếng Anh giao tiếp tốt.",
                        Benefits = """
                                   - Lương và các khoản phụ cấp hấp dẫn.
                                   - Được đào tạo nghiệp vụ lễ tân chuyên nghiệp.
                                   - Cơ hội làm việc trong môi trường quốc tế.
                                   """,
                        Salary = 9000000m,
                        Position = "Receptionist",
                        Experience = "Không yêu cầu kinh nghiệm",
                        CityId = 25, // Đà Nẵng
                        EmployerId = "EMP003",
                        CreatedAt = DateTime.Now.AddDays(-18),
                        IsActive = true,
                        Requirements = """
                                       - Tiếng Anh giao tiếp tốt.
                                       - Ngoại hình ưa nhìn, giao tiếp tốt.
                                       - Nhanh nhẹn, nhiệt tình, có trách nhiệm.
                                       """,
                        JobType = "Full-time"
                    },

                    new Post
                    {
                        PostId = "POST019", // Added PostId as string
                        Title = "Hướng dẫn viên Du lịch",
                        Description = "Công ty Du lịch cần tuyển Hướng dẫn viên Du lịch nội địa và quốc tế.",
                        Benefits = """
                                   - Lương cơ bản và hoa hồng theo tour.
                                   - Thưởng theo hiệu suất công việc.
                                   - Cơ hội đi nhiều nơi, mở rộng kiến thức.
                                   """,
                        Salary = 10000000m,
                        Position = "Tour Guide",
                        Experience = "Ưu tiên có kinh nghiệm",
                        CityId = 28, // Quảng Nam
                        EmployerId = "EMP004",
                        CreatedAt = DateTime.Now.AddDays(-19),
                        IsActive = true,
                        Requirements = """
                                       - Yêu thích du lịch, am hiểu về văn hóa, lịch sử.
                                       - Kỹ năng giao tiếp, thuyết trình tốt.
                                       - Ngoại ngữ là một lợi thế.
                                       """,
                        JobType = "Full-time"
                    },

                    new Post
                    {
                        PostId = "POST020", // Added PostId as string
                        Title = "Nhân viên Bán hàng",
                        Description = "Cửa hàng thời trang tuyển Nhân viên Bán hàng, làm việc theo ca.",
                        Benefits = """
                                   - Lương cơ bản và hoa hồng theo doanh số bán hàng.
                                   - Thưởng theo tháng, quý, năm.
                                   - Môi trường làm việc thân thiện, năng động.
                                   """,
                        Salary = 7000000m,
                        Position = "Sales Staff",
                        Experience = "Không yêu cầu kinh nghiệm",
                        CityId = 20, // Thái Bình
                        EmployerId = "EMP005",
                        CreatedAt = DateTime.Now.AddDays(-20),
                        IsActive = true,
                        Requirements = """
                                       - Yêu thích thời trang, có gu thẩm mỹ.
                                       - Kỹ năng giao tiếp, bán hàng tốt.
                                       - Nhanh nhẹn, trung thực, chịu khó.
                                       """,
                        JobType = "Part-time/Full-time"
                    },

                    new Post
                    {
                        PostId = "POST021", // Added PostId as string
                        Title = "Chuyên viên Tuyển dụng",
                        Description = "Công ty cần tuyển Chuyên viên Tuyển dụng có kinh nghiệm tuyển dụng IT.",
                        Benefits = """
                                   - Lương thỏa thuận, thưởng theo KPI tuyển dụng.
                                   - Được đào tạo chuyên sâu về tuyển dụng.
                                   - Môi trường làm việc chuyên nghiệp, HR team support tốt.
                                   """,
                        Salary = 15000000m,
                        Position = "Recruiter",
                        Experience = "2 năm kinh nghiệm",
                        CityId = 16, // Hà Nội
                        EmployerId = "EMP001",
                        CreatedAt = DateTime.Now.AddDays(-21),
                        IsActive = true,
                        Requirements = """
                                       - Kinh nghiệm tuyển dụng, ưu tiên tuyển dụng IT.
                                       - Kỹ năng phỏng vấn, đánh giá ứng viên tốt.
                                       - Am hiểu thị trường lao động IT.
                                       """,
                        JobType = "Full-time"
                    },

                    new Post
                    {
                        PostId = "POST022", // Added PostId as string
                        Title = "Chuyên viên Pháp lý",
                        Description = "Tập đoàn lớn tuyển Chuyên viên Pháp lý, có kinh nghiệm về luật doanh nghiệp.",
                        Benefits = """
                                   - Lương hấp dẫn, chế độ đãi ngộ tốt.
                                   - Được làm việc trong môi trường pháp lý chuyên nghiệp.
                                   - Cơ hội thăng tiến lên vị trí quản lý.
                                   """,
                        Salary = 18000000m,
                        Position = "Legal Counsel",
                        Experience = "3 năm kinh nghiệm",
                        CityId = 30, // TP Hồ Chí Minh
                        EmployerId = "EMP002",
                        CreatedAt = DateTime.Now.AddDays(-22),
                        IsActive = true,
                        Requirements = """
                                       - Tốt nghiệp Đại học Luật, chuyên ngành Luật doanh nghiệp.
                                       - Kinh nghiệm làm việc pháp lý doanh nghiệp ít nhất 3 năm.
                                       - Am hiểu luật pháp Việt Nam.
                                       """,
                        JobType = "Full-time"
                    },

                    new Post
                    {
                        PostId = "POST023", // Added PostId as string
                        Title = "Trợ lý Giám đốc",
                        Description =
                            "Công ty cần tuyển Trợ lý Giám đốc, có kinh nghiệm làm việc với lãnh đạo cấp cao.",
                        Benefits = """
                                   - Lương thỏa thuận, xứng đáng với năng lực.
                                   - Môi trường làm việc năng động, chuyên nghiệp.
                                   - Cơ hội học hỏi và phát triển bản thân.
                                   """,
                        Salary = 14000000m,
                        Position = "Executive Assistant",
                        Experience = "2 năm kinh nghiệm",
                        CityId = 18, // Hải Phòng
                        EmployerId = "EMP003",
                        CreatedAt = DateTime.Now.AddDays(-23),
                        IsActive = true,
                        Requirements = """
                                       - Kinh nghiệm làm trợ lý, thư ký cho lãnh đạo cấp cao.
                                       - Kỹ năng giao tiếp, sắp xếp công việc tốt.
                                       - Ngoại ngữ là một lợi thế.
                                       """,
                        JobType = "Full-time"
                    },

                    new Post
                    {
                        PostId = "POST024", // Added PostId as string
                        Title = "Nhân viên Xuất nhập khẩu",
                        Description =
                            "Công ty Thương mại tuyển Nhân viên Xuất nhập khẩu, có kinh nghiệm làm thủ tục hải quan.",
                        Benefits = """
                                   - Lương cơ bản và thưởng theo doanh số xuất nhập khẩu.
                                   - Đóng BHXH, BHYT, BHTN đầy đủ.
                                   - Môi trường làm việc ổn định, lâu dài.
                                   """,
                        Salary = 12000000m,
                        Position = "Import-Export Staff",
                        Experience = "1 năm kinh nghiệm",
                        CityId = 35, // Bình Dương
                        EmployerId = "EMP004",
                        CreatedAt = DateTime.Now.AddDays(-24),
                        IsActive = true,
                        Requirements = """
                                       - Kinh nghiệm làm thủ tục hải quan, xuất nhập khẩu.
                                       - Am hiểu về Incoterms, thanh toán quốc tế.
                                       - Tiếng Anh giao tiếp tốt.
                                       """,
                        JobType = "Full-time"
                    },

                    new Post
                    {
                        PostId = "POST025", // Added PostId as string
                        Title = "Nhân viên Hành chính Nhân sự",
                        Description = "Văn phòng đại diện cần tuyển Nhân viên Hành chính Nhân sự.",
                        Benefits = """
                                   - Lương thỏa thuận theo kinh nghiệm.
                                   - Thưởng các dịp lễ, Tết, sinh nhật.
                                   - Môi trường làm việc văn phòng, giờ hành chính.
                                   """,
                        Salary = 10000000m,
                        Position = "HR Admin",
                        Experience = "Không yêu cầu kinh nghiệm",
                        CityId = 40, // Đồng Nai
                        EmployerId = "EMP005",
                        CreatedAt = DateTime.Now.AddDays(-25),
                        IsActive = true,
                        Requirements = """
                                       - Nhanh nhẹn, cẩn thận, chịu khó học hỏi.
                                       - Kỹ năng giao tiếp, xử lý tình huống tốt.
                                       - Ưu tiên ứng viên có kinh nghiệm hành chính nhân sự.
                                       """,
                        JobType = "Full-time"
                    },

                    new Post
                    {
                        PostId = "POST026", // Added PostId as string
                        Title = "Kỹ sư Xây dựng",
                        Description = "Công ty Xây dựng cần tuyển Kỹ sư Xây dựng Dân dụng và Công nghiệp.",
                        Benefits = """
                                   - Lương thỏa thuận, thưởng theo dự án.
                                   - Phụ cấp công trường, nhà ở.
                                   - Cơ hội tham gia các dự án lớn.
                                   """,
                        Salary = 16000000m,
                        Position = "Civil Engineer",
                        Experience = "2 năm kinh nghiệm",
                        CityId = 16, // Hà Nội
                        EmployerId = "EMP001",
                        CreatedAt = DateTime.Now.AddDays(-26),
                        IsActive = true,
                        Requirements = """
                                       - Tốt nghiệp Đại học chuyên ngành Xây dựng Dân dụng và Công nghiệp.
                                       - Kinh nghiệm giám sát công trình xây dựng.
                                       - Sử dụng thành thạo AutoCAD.
                                       """,
                        JobType = "Full-time"
                    },

                    new Post
                    {
                        PostId = "POST027", // Added PostId as string
                        Title = "Kiến trúc sư",
                        Description =
                            "Văn phòng Kiến trúc tuyển Kiến trúc sư, có kinh nghiệm thiết kế nhà phố, biệt thự.",
                        Benefits = """
                                   - Lương hấp dẫn, thưởng theo thiết kế.
                                   - Môi trường làm việc sáng tạo, chuyên nghiệp.
                                   - Cơ hội phát triển sự nghiệp trong ngành kiến trúc.
                                   """,
                        Salary = 15000000m,
                        Position = "Architect",
                        Experience = "2 năm kinh nghiệm",
                        CityId = 30, // TP Hồ Chí Minh
                        EmployerId = "EMP002",
                        CreatedAt = DateTime.Now.AddDays(-27),
                        IsActive = true,
                        Requirements = """
                                       - Tốt nghiệp Đại học chuyên ngành Kiến trúc.
                                       - Kinh nghiệm thiết kế nhà phố, biệt thự.
                                       - Sử dụng thành thạo AutoCAD, Revit, 3ds Max.
                                       """,
                        JobType = "Full-time"
                    },

                    new Post
                    {
                        PostId = "POST028", // Added PostId as string
                        Title = "Nhân viên Chăm sóc Khách hàng",
                        Description = "Công ty Dịch vụ tuyển Nhân viên Chăm sóc Khách hàng, làm việc online.",
                        Benefits = """
                                   - Lương cơ bản và thưởng theo hiệu suất CSKH.
                                   - Làm việc tại nhà, thời gian linh hoạt.
                                   - Được đào tạo kỹ năng CSKH chuyên nghiệp.
                                   """,
                        Salary = 9000000m,
                        Position = "Customer Care Staff",
                        Experience = "Không yêu cầu kinh nghiệm",
                        CityId = 18, // Hải Phòng
                        EmployerId = "EMP003",
                        CreatedAt = DateTime.Now.AddDays(-28),
                        IsActive = true,
                        Requirements = """
                                       - Kỹ năng giao tiếp, lắng nghe tốt.
                                       - Kiên nhẫn, nhiệt tình, có trách nhiệm.
                                       - Có laptop, internet ổn định để làm việc online.
                                       """,
                        JobType = "Remote"
                    },

                    new Post
                    {
                        PostId = "POST029", // Added PostId as string
                        Title = "Biên tập viên Nội dung",
                        Description = "Tạp chí Online tuyển Biên tập viên Nội dung, có kinh nghiệm viết bài SEO.",
                        Benefits = """
                                   - Lương thỏa thuận, nhuận bút theo bài viết.
                                   - Làm việc remote, thời gian linh hoạt.
                                   - Cơ hội phát triển kỹ năng viết lách, SEO content.
                                   """,
                        Salary = 11000000m,
                        Position = "Content Writer",
                        Experience = "1 năm kinh nghiệm",
                        CityId = 35, // Bình Dương
                        EmployerId = "EMP004",
                        CreatedAt = DateTime.Now.AddDays(-29),
                        IsActive = true,
                        Requirements = """
                                       - Kinh nghiệm viết bài chuẩn SEO, content marketing.
                                       - Có kiến thức về SEO onpage, offpage.
                                       - Sáng tạo, có khả năng viết đa dạng chủ đề.
                                       """,
                        JobType = "Remote"
                    },

                    new Post
                    {
                        PostId = "POST030", // Added PostId as string
                        Title = "Nhân viên Telesales",
                        Description = "Trung tâm Đào tạo tuyển Nhân viên Telesales, bán khóa học online.",
                        Benefits = """
                                   - Lương cơ bản và hoa hồng theo doanh số telesales.
                                   - Thưởng nóng, thưởng tuần, tháng.
                                   - Môi trường làm việc năng động, đội nhóm hỗ trợ tốt.
                                   """,
                        Salary = 8000000m,
                        Position = "Telesales Staff",
                        Experience = "Không yêu cầu kinh nghiệm",
                        CityId = 40, // Đồng Nai
                        EmployerId = "EMP005",
                        CreatedAt = DateTime.Now.AddDays(-30),
                        IsActive = true,
                        Requirements = """
                                       - Kỹ năng giao tiếp, thuyết phục qua điện thoại tốt.
                                       - Năng động, nhiệt tình, chịu được áp lực doanh số.
                                       - Có kinh nghiệm telesales là một lợi thế.
                                       """,
                        JobType = "Full-time"
                    }
                ];
                context.Posts.AddRange(posts);
                await context.SaveChangesAsync();
            }
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
