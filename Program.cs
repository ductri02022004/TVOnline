using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using TVOnline.Data;
using TVOnline.Helper;
using TVOnline.Hubs;
using TVOnline.Models;
using TVOnline.Repository.Employers;
using TVOnline.Repository.Job;
using TVOnline.Repository.Location;
using TVOnline.Repository.Posts;
using TVOnline.Repository.UserCVs;
using TVOnline.Service;
using TVOnline.Service.Employers;
using TVOnline.Service.Jobs;
using TVOnline.Service.Location;
using TVOnline.Service.Post;
using TVOnline.Service.UserCVs;
using TVOnline.Service.Vnpay;
using TVOnline.Services;

namespace TVOnline
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure services
            ConfigureServices(builder.Services, builder.Configuration);

            var app = builder.Build();

            // Configure the HTTP request pipeline
            ConfigureMiddleware(app, builder.Environment);

            // Seed data
            try
            {
                await SeedDataAsync(app);
                Console.WriteLine("Database seeded successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding database: {ex.Message}");
            }

            await app.RunAsync();
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // Configure HttpClient
            services.AddHttpClient<VnPayService>();

            // Connect VNPay API
            services.AddScoped<IVnPayService, VnPayService>();

            // Add services to the container.
            services.AddControllersWithViews();

            // Add services into IoC container
            services.AddScoped<IJobsRepository, JobsRepository>();
            services.AddScoped<IUserCvRepository, UserCvRepository>();
            services.AddScoped<IPostRepository, PostRepository>();
            services.AddScoped<ILocationRepository, LocationRepository>();
            services.AddScoped<IEmployerRepository, EmployerRepository>();
            services.AddScoped<IJobsService, JobsService>();
            services.AddScoped<IUserCvService, UserCvService>();
            services.AddScoped<IPremiumUserService, PremiumUserService>();
            services.AddScoped<IPostService, PostService>();
            services.AddScoped<ILocationService, LocationService>();
            services.AddScoped<IEmployersService, EmployersService>();

            // Đăng ký ChatService
            services.AddScoped<IChatService, ChatService>();

            // Thêm SignalR
            services.AddSignalR();

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("Default")));

            services.AddIdentity<Users, IdentityRole>(options =>
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
            services.AddScoped<IEmailSender, EmailSender>();

            // Google login
            services.AddAuthentication()
                .AddGoogle(googleOptions =>
                {
                    var googleAuthNSection = configuration.GetSection("Authentication:Google");
                    googleOptions.ClientId = googleAuthNSection.GetValue<string>("ClientId") ?? throw new InvalidOperationException("Google ClientId is not configured");
                    googleOptions.ClientSecret = googleAuthNSection.GetValue<string>("ClientSecret") ?? throw new InvalidOperationException("Google ClientSecret is not configured");
                    googleOptions.CallbackPath = "/signin-google";
                });

            // Add logging
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddConsole();
                loggingBuilder.AddDebug();
            });

            // Add CORS policy
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                    builder =>
                    {
                        builder.WithOrigins("https://localhost:7216", "http://localhost:5216")
                               .AllowAnyMethod()
                               .AllowAnyHeader()
                               .AllowCredentials();
                    });
            });
        }

        private static void ConfigureMiddleware(WebApplication app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // Use CORS policy
            app.UseCors("AllowAllOrigins");

            // Ensure correct order: Authentication before Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            // Modern .NET 6+ routing configuration
            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "vnpay",
                pattern: "payment/{action=Index}/{id?}",
                defaults: new { controller = "Payment" });

            // Thêm hub endpoints
            app.MapHub<ChatHub>("/chatHub");
        }

        private static async Task SeedDataAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var context = services.GetRequiredService<AppDbContext>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<Users>>();

            try
            {
                // Apply any pending migrations
                await context.Database.MigrateAsync();

                // Ensure roles are created first
                await DbSeeder.SeedRolesAsync(roleManager);

                // Then seed users and assign roles
                await DbSeeder.SeedUsersAsync(userManager);

                // Seed admin user
                await DbSeeder.SeedAdminUserAsync(userManager);

                // Seed location data
                DbSeeder.SeedData(context);

                // Seed employers and posts
                await DbSeeder.SeedEmployersAsync(context);
                await DbSeeder.SeedPostsAsync(context);
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while seeding the database.");
                throw; // Re-throw to handle in the Main method
            }
        }
    }
}