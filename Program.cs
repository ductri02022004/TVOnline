using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using TVOnline.Data;
using TVOnline.Models;
using TVOnline.Helper;

namespace TVOnline
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

            builder.Services.AddIdentity<Users, IdentityRole>(options =>
            {
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
            .AddGoogle(googleOptions =>
            {
                var googleAuthNSection = builder.Configuration.GetSection("Authentication:Google");
                googleOptions.ClientId = googleAuthNSection.GetValue<string>("ClientId") ?? throw new InvalidOperationException("Google ClientId is not configured");
                googleOptions.ClientSecret = googleAuthNSection.GetValue<string>("ClientSecret") ?? throw new InvalidOperationException("Google ClientSecret is not configured");
                googleOptions.CallbackPath = "/signin-google";
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();



            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
