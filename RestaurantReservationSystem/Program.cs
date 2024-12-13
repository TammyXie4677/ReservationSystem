using Microsoft.EntityFrameworkCore;
using RestaurantReservationSystem.Models;  // Make sure to adjust the namespace to match your project

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register DbContext with SQL Server
builder.Services.AddDbContext<RestaurantReservationContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services for authentication and authorization if needed (example below)
builder.Services.AddAuthorization();
builder.Services.AddAuthentication();

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

// Define the default route pattern for controllers
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllerRoute(
    name: "user",
    pattern: "{controller=User}/{action=Register}/{id?}");


app.Run();
