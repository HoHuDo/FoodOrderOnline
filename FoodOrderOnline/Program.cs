
using FoodOrderOnline.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/TaiKhoan/Login"; // Trang đăng nhập
        options.AccessDeniedPath = "/TaiKhoan/AccessDenied"; // Trang cấm truy cập
        options.ExpireTimeSpan = TimeSpan.FromDays(30); // Thời gian "nhớ"
    });

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<FoodOrderContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("FoodOrderOnline"));
});

// Đăng ký session (chỉ 1 lần cache)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    // options.IdleTimeout = TimeSpan.FromMinutes(20);
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IPasswordHasher<TaiKhoan>, PasswordHasher<TaiKhoan>>();
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 👉 BẬT SESSION Ở ĐÂY (trước MapControllerRoute)
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=TrangChu}/{action=Index}/{id?}");

app.Run();
