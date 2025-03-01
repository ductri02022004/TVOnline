using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using TVOnline.Data;
using TVOnline.Models;
using TVOnline.Helper;

namespace TVOnline {
    public class Program {
        public static async Task Main(string[] args) {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Add Memory Cache for pending registrations
            builder.Services.AddMemoryCache();

            builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

            builder.Services.AddIdentity<Users, IdentityRole>(options => {
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedAccount = true;
                options.SignIn.RequireConfirmedEmail = true;
                options.SignIn.RequireConfirmedPhoneNumber = false;
            })
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            // Configure Email Service
            builder.Services.AddScoped<IEmailSender, EmailSender>();

            //Google login
            builder.Services.AddAuthentication()
            .AddGoogle(googleOptions => {
                var googleAuthNSection = builder.Configuration.GetSection("Authentication:Google");
                googleOptions.ClientId = googleAuthNSection.GetValue<string>("ClientId") ?? throw new InvalidOperationException("Google ClientId is not configured");
                googleOptions.ClientSecret = googleAuthNSection.GetValue<string>("ClientSecret") ?? throw new InvalidOperationException("Google ClientSecret is not configured");
                googleOptions.CallbackPath = "/signin-google";
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment()) {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            // Seed data
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<AppDbContext>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = services.GetRequiredService<UserManager<Users>>();
                                
                // Đảm bảo roles được tạo trước
                await DbSeeder.SeedRolesAsync(roleManager);
                // Sau đó mới seed data
                DbSeeder.SeedData(context);
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // Đảm bảo thứ tự đúng: Authentication trước, Authorization sau
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
