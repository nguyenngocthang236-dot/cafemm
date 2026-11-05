using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanCafe.Helpers;
using QuanCafe.Models;
using QuanCafe.ViewModels;

namespace QuanCafe.Controllers;

public class RevenueController : Controller
{
    private readonly AppDbContext _context;

    public RevenueController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /Revenue/Index?date=2026-11-05
    public async Task<IActionResult> Index(DateTime? date)
    {
        var selectedDate = date?.Date ?? DateTime.Today;

        var donHangs = await _context.Donhangs
            .Include(d => d.Chitietdons)
                .ThenInclude(ct => ct.IdsanphamNavigation)
            .Include(d => d.IdbanNavigation)
            .Where(d =>
                d.Thoigiandat.HasValue &&
                d.Thoigiandat.Value.Date == selectedDate &&
                d.Trangthaithanhtoan == DonHangStatus.DaThanhToan &&   // <-- tính theo ĐÃ THANH TOÁN, không phải trạng thái giao món
                d.Trangthai != "huy" && d.Trangthai != "dahuy")        // bỏ đơn đã hủy (phòng trường hợp dữ liệu cũ bị lệch) — viết trực tiếp, không gọi hàm C#, vì EF Core không dịch được lời gọi hàm tùy ý sang SQL
            .OrderBy(d => d.Thoigiandat)
            .ToListAsync();

        var vm = new RevenueViewModel
        {
            NgayXem = selectedDate,
            TongSoDon = donHangs.Count,
            TongDoanhThu = donHangs.Sum(d => d.Tongtien ?? 0),
            DanhSachDon = donHangs.Select(d => new DonHangDoanhThuDto
            {
                MaDonHang = d.Madonhang,
                MaBan = d.IdbanNavigation.Maban,
                ThoiGianDat = d.Thoigiandat ?? DateTime.MinValue,
                TongTienDon = d.Tongtien ?? 0,
                DanhSachMon = d.Chitietdons.Select(ct => new MonDaDatDto
                {
                    TenSanPham = ct.IdsanphamNavigation.Tensanpham,
                    SoLuong = ct.Soluong,
                    DonGia = ct.Dongia
                }).ToList()
            }).ToList()
        };

        return View(vm);
    }
}
