using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanCafe.Helpers;
using QuanCafe.Models;
using QuanCafe.ViewModels;

namespace QuanCafe.Controllers;

public class GioHangController : KhachController
{
    // Mỗi món tối đa 20 ly (khớp với TOI_DA trong wwwroot/js/khach.js)
    private const int SoLuongToiDa = 20;

    public GioHangController(AppDbContext db) : base(db) { }

    // GET /GioHang
    public async Task<IActionResult> Index()
    {
        var (gio, coMonBiBo) = await TaiGioAsync();
        if (coMonBiBo)
            TempData["Canh"] = "Một số món vừa tạm hết nên đã được bỏ khỏi giỏ hàng.";

        ViewData["SoLuongGio"] = gio.TongSoLuong;
        return View(gio);
    }

    // POST /GioHang/Them   (trang chi tiết món gọi bằng fetch, trả về JSON)
    [HttpPost]
    public async Task<IActionResult> Them(int idSanPham, int soLuong)
    {
        if (soLuong < 1 || soLuong > SoLuongToiDa)
            return Json(new { ok = false, thongBao = "Số lượng không hợp lệ." });

        var conBan = await _db.Sanphams.AnyAsync(s => s.Idsanpham == idSanPham && s.Conhang == true);
        if (!conBan)
            return Json(new { ok = false, thongBao = "Món này vừa tạm hết. Bạn chọn món khác nhé." });

        var gio = LayGio();
        var dong = gio.FirstOrDefault(x => x.Idsanpham == idSanPham);
        if (dong == null)
            gio.Add(new GioHangItem { Idsanpham = idSanPham, Soluong = soLuong });
        else
            dong.Soluong = Math.Min(dong.Soluong + soLuong, SoLuongToiDa);

        LuuGio(gio);

        // Số hiển thị trên biểu tượng giỏ hàng = tổng số ly. Muốn đếm số loại món thì đổi Sum thành Count.
        return Json(new { ok = true, tongSoLuong = gio.Sum(x => x.Soluong) });
    }

    // POST /GioHang/Xoa
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Xoa(int idSanPham)
    {
        var gio = LayGio();
        gio.RemoveAll(x => x.Idsanpham == idSanPham);
        LuuGio(gio);
        return RedirectToAction(nameof(Index));
    }

    // POST /GioHang/DatHang   -> tạo đơn hàng + chi tiết đơn
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DatHang(string? ghiChu)
    {
        var (gio, coMonBiBo) = await TaiGioAsync();

        if (coMonBiBo)
        {
            TempData["Canh"] = "Một số món vừa tạm hết nên đã được bỏ khỏi giỏ hàng. Bạn kiểm tra lại rồi đặt hàng nhé.";
            return RedirectToAction(nameof(Index));
        }
        if (gio.Dongs.Count == 0)
            return RedirectToAction(nameof(Index));

        ghiChu = string.IsNullOrWhiteSpace(ghiChu) ? null : ghiChu.Trim();
        if (ghiChu != null && ghiChu.Length > 200) ghiChu = ghiChu[..200];

        var donHang = new Donhang
        {
            Idphien = IdPhien,
            Idban = IdBan,
            Trangthai = "choxacnhan",
            Trangthaithanhtoan = "chuathanhtoan",
            Thoigiandat = DateTime.Now,
            Ghichu = ghiChu,
            Tongtien = gio.TongTien      // giá lấy từ database, không tin dữ liệu từ trình duyệt
        };

        foreach (var d in gio.Dongs)
        {
            donHang.Chitietdons.Add(new Chitietdon
            {
                Idsanpham = d.Idsanpham,
                Soluong = d.Soluong,
                Dongia = d.Gia,
                Trangthai = "cholam"
            });
        }

        _db.Donhangs.Add(donHang);

        for (var lan = 1; ; lan++)
        {
            donHang.Madonhang = await TaoMaDonAsync();
            try
            {
                await _db.SaveChangesAsync();
                break;
            }
            catch (DbUpdateException) when (lan < 3)
            {
                // Hai khách đặt cùng một lúc có thể trùng mã đơn -> tạo mã mới rồi lưu lại
            }
        }

        HttpContext.Session.Remove(SessionKeys.GioHang);
        return RedirectToAction("ThanhCong", "DonHang", new { maDon = donHang.Madonhang });
    }

    // ---------- Hàm phụ ----------

    private List<GioHangItem> LayGio() =>
        HttpContext.Session.GetObject<List<GioHangItem>>(SessionKeys.GioHang) ?? new List<GioHangItem>();

    private void LuuGio(List<GioHangItem> gio) =>
        HttpContext.Session.SetObject(SessionKeys.GioHang, gio);

    /// <summary>
    /// Ghép giỏ trong Session với dữ liệu món trong database (tên, ảnh, giá).
    /// Món nào đã bị ẩn / hết hàng thì bỏ khỏi giỏ và báo lại qua coMonBiBo.
    /// </summary>
    private async Task<(GioHangViewModel Gio, bool CoMonBiBo)> TaiGioAsync()
    {
        var gio = LayGio();
        var ids = gio.Select(x => x.Idsanpham).Distinct().ToList();

        var sanPhams = await _db.Sanphams
            .AsNoTracking()
            .Where(s => ids.Contains(s.Idsanpham) && s.Conhang == true)
            .ToDictionaryAsync(s => s.Idsanpham);

        var conBan = gio.Where(x => sanPhams.ContainsKey(x.Idsanpham)).ToList();
        var coMonBiBo = conBan.Count != gio.Count;
        if (coMonBiBo) LuuGio(conBan);

        var vm = new GioHangViewModel
        {
            Dongs = conBan.Select(x =>
            {
                var sp = sanPhams[x.Idsanpham];
                return new GioHangDong
                {
                    Idsanpham = sp.Idsanpham,
                    Tensanpham = sp.Tensanpham,
                    Hinhanh = sp.Hinhanh,
                    Gia = sp.Gia,
                    Soluong = x.Soluong
                };
            }).ToList()
        };

        return (vm, coMonBiBo);
    }

    /// <summary>Mã đơn dạng 261028-007 = ngày + số thứ tự trong ngày (ngắn gọn để nhân viên gọi mã).</summary>
    private async Task<string> TaoMaDonAsync()
    {
        var homNay = DateTime.Today;
        var ngayMai = homNay.AddDays(1);

        var soDonHomNay = await _db.Donhangs
            .CountAsync(d => d.Thoigiandat >= homNay && d.Thoigiandat < ngayMai);

        return $"{homNay:yyMMdd}-{soDonHomNay + 1:000}";
    }
}
