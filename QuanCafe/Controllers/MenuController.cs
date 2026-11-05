using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanCafe.Models;
using QuanCafe.Models.ViewModels;

namespace QuanCafe.Controllers;

public class MenuController : KhachController
{
    public MenuController(AppDbContext db) : base(db) { }

    // GET /  hoặc  /Menu   -> trang chào mừng + danh sách món
    public async Task<IActionResult> Index()
    {
        var sanPhams = await _db.Sanphams
            .AsNoTracking()
            .Where(s => s.Conhang == true)
            .OrderBy(s => s.Idsanpham)
            .ToListAsync();

        // Lấy các sale đang chạy (thời gian hiện tại nằm trong khoảng sale)
        var now = DateTime.Now;
        Dictionary<int, decimal> saleDangChay = new();

        try
        {
            var sales = await _db.SaleTheoNgays
                .AsNoTracking()
                .Where(s => s.ThoiGianBatDau <= now && s.ThoiGianKetThuc >= now)
                .ToListAsync();

            // Nếu một món có nhiều sale đang chạy thì lấy cái giảm nhiều nhất
            foreach (var s in sales)
                if (!saleDangChay.ContainsKey(s.Idsanpham) || s.PhanTramGiam > saleDangChay[s.Idsanpham])
                    saleDangChay[s.Idsanpham] = s.PhanTramGiam;
        }
        catch
        {
            // Bảng sale chưa tồn tại → bỏ qua, hiện giá gốc bình thường
        }

        var vm = sanPhams.Select(sp => new MonVoiSaleVm
        {
            Mon = sp,
            PhanTramGiam = saleDangChay.TryGetValue(sp.Idsanpham, out var ptg) ? ptg : 0
        }).ToList();

        return View(vm);
    }

    // GET /Menu/ChiTiet/5   -> trang riêng của một món
    public async Task<IActionResult> ChiTiet(int id)
    {
        var sanPham = await _db.Sanphams
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Idsanpham == id && s.Conhang == true);

        if (sanPham == null) return RedirectToAction(nameof(Index));

        // Lấy sale đang chạy cho món này (nếu có)
        var now = DateTime.Now;
        decimal phanTramGiam = 0;
        try
        {
            var sale = await _db.SaleTheoNgays
                .AsNoTracking()
                .Where(s => s.Idsanpham == id && s.ThoiGianBatDau <= now && s.ThoiGianKetThuc >= now)
                .OrderByDescending(s => s.PhanTramGiam)
                .FirstOrDefaultAsync();
            if (sale != null) phanTramGiam = sale.PhanTramGiam;
        }
        catch { }

        ViewBag.PhanTramGiam = phanTramGiam;
        return View(sanPham);
    }
}
