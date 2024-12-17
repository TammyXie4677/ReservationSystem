using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using RestaurantReservationSystem.Models;
using RestaurantReservationSystem.Services;

public partial class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews();

        // Register DbContext with SQL Server
        builder.Services.AddDbContext<RestaurantReservationContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        // Add cookie authentication
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/User/Login"; // Redirect here if not authenticated
                options.LogoutPath = "/User/Logout";
                options.AccessDeniedPath = "/User/AccessDenied";
            });

        // Add authorization
        builder.Services.AddAuthorization();

        // Get Azure storage connection string and container name from appsettings
        var azureStorageConfig = builder.Configuration.GetSection("AzureBlobStorage");
        var connectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentNullException(nameof(connectionString), "Azure Storage connection string is not configured.");
        }

        var containerName = builder.Configuration.GetValue<string>("AzureBlobStorage:ContainerName");
        if (string.IsNullOrEmpty(containerName))
        {
            throw new ArgumentNullException(nameof(containerName), "Azure Storage container name is not configured.");
        }

        // Register FileUploadService with the container
        builder.Services.AddSingleton(new FileUploadService(connectionString, containerName));

        // Build the app
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseRouting();

        // Use authentication and authorization
        app.UseAuthentication();
        app.UseAuthorization();

        // Map static assets (images, styles, etc.)
        app.MapStaticAssets();

        app.MapControllerRoute(
            name: "user",
            pattern: "{controller=User}/{action=Register}/{id?}");

        app.MapControllerRoute(
            name: "admin",
            pattern: "Admin/{action=AdminDashboard}/{id?}",
            defaults: new { controller = "Admin", action = "AdminDashboard" });

        app.MapControllerRoute(
            name: "adminBooking",
            pattern: "AdminBooking/{action=Index}/{id?}");

        app.Run();
    }
}