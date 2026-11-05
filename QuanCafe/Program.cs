using Microsoft.EntityFrameworkCore;
using QuanCafe.Models;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDistributedMemoryCache(); // bắt buộc phải có, Session cần cache để lưu dữ liệu
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(3);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
var app = builder.Build() ;

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
// Bắt buộc có: MapStaticAssets() chỉ phục vụ file đã có từ lúc build, nên ảnh món vừa upload
// từ trang quản lý (ghi vào wwwroot/images/menu khi ứng dụng đang chạy) sẽ bị 404 nếu thiếu dòng này.
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Menu}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
