using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanCafe.Helpers;
using QuanCafe.Models;
using QuanCafe.Models.ViewModels;
using QuanCafe.ViewModels;

namespace QuanCafe.Controllers;

public class MenuController : KhachController
{
    public MenuController(AppDbContext db) : base(db) { }

    // GET /  hoặc  /Menu   -> trang chào mừng + danh sách món
    public async Task<IActionResult> Index()
    {
        // Hiện TẤT CẢ món; món hết hàng (tồn kho = 0) vẫn hiện nhưng gắn nhãn "Hết hàng" và không bấm vào được.
        var sanPhams = await _db.Sanphams
            .AsNoTracking()
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
            .FirstOrDefaultAsync(s => s.Idsanpham == id);

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

        // Số lượng khách còn có thể thêm = min(20, tồn kho) trừ phần đã có trong giỏ
        var trongGio = HttpContext.Session.GetObject<List<GioHangItem>>(SessionKeys.GioHang)?
            .Where(x => x.Idsanpham == id)
            .Sum(x => x.Soluong) ?? 0;

        ViewBag.PhanTramGiam = phanTramGiam;
        ViewBag.ToiDaGoi = Math.Max(0, Math.Min(GioHangController.SoLuongToiDa, sanPham.Soluongton) - trongGio);
        return View(sanPham);
    }
}
