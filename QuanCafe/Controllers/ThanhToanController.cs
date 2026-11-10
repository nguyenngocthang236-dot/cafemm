using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanCafe.Helpers;
using QuanCafe.Models;

namespace QuanCafe.Controllers;

public class ThanhToanController : Controller
{
    private readonly AppDbContext _db;
    private readonly ILogger<ThanhToanController> _logger;

    public ThanhToanController(AppDbContext db, ILogger<ThanhToanController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // POST /ThanhToan/ThanhToanNgay — khách trả ngay sau khi đặt đơn.
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ThanhToanNgay(string maDon, string hinhThuc)
    {
        var idPhien = HttpContext.Session.GetInt32("IdPhien");
        if (idPhien == null) return RedirectToAction("Index", "Menu");
        hinhThuc = ChuanHoaHinhThuc(hinhThuc);
        if (!HinhThucHopLe(hinhThuc))
        {
            TempData["Loi"] = "Hình thức thanh toán không hợp lệ.";
            return RedirectToAction("ThanhCong", "DonHang", new { maDon });
        }

        await using var transaction = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        try
        {
            var don = await _db.Donhangs
                .Include(d => d.Idthanhtoans)
                .FirstOrDefaultAsync(d => d.Madonhang == maDon && d.Idphien == idPhien.Value);
            if (don == null) return NotFound();
            if (DonHangStatus.IsCancelled(don.Trangthai))
            {
                TempData["Loi"] = "Đơn đã bị hủy nên không thể thanh toán.";
                return RedirectToAction("ThanhCong", "DonHang", new { maDon });
            }
            if (DonHangStatus.IsPaid(don.Trangthaithanhtoan) || don.Idthanhtoans.Any(t => t.Trangthai == DonHangStatus.ThanhToanHoanTat))
            {
                TempData["ThongBao"] = "Đơn này đã được thanh toán trước đó.";
                return RedirectToAction("TrangThai", "DonHang", new { maDon });
            }

            var soTien = don.Tongtien ?? 0m;
            if (soTien <= 0m)
            {
                TempData["Loi"] = "Tổng tiền của đơn không hợp lệ.";
                return RedirectToAction("ThanhCong", "DonHang", new { maDon });
            }

            var thanhToan = new Thanhtoan
            {
                Idphien = don.Idphien,
                Sotien = soTien,
                Hinhthuc = hinhThuc,
                Trangthai = DonHangStatus.ThanhToanHoanTat,
                Thoigianthanhtoan = DateTime.Now
            };
            don.Idthanhtoans.Add(thanhToan);
            don.Trangthaithanhtoan = DonHangStatus.DaThanhToan;

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
            return RedirectToAction("TrangThai", "DonHang", new { maDon });
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await transaction.RollbackAsync();
            _logger.LogWarning(ex, "Xung đột dữ liệu khi khách thanh toán đơn {MaDon}", maDon);
            TempData["Loi"] = "Đơn vừa được cập nhật ở nơi khác. Vui lòng tải lại trạng thái đơn.";
            return RedirectToAction("ThanhCong", "DonHang", new { maDon });
        }
        catch (DbUpdateException ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Lỗi SQL khi khách thanh toán đơn {MaDon}", maDon);
            TempData["Loi"] = "Không lưu được thanh toán. Hãy báo nhân viên kiểm tra log ứng dụng.";
            return RedirectToAction("ThanhCong", "DonHang", new { maDon });
        }
    }

    // GET /ThanhToan/ThanhToanCuoi — khách thanh toán tất cả đơn còn nợ trong phiên bàn.
    public async Task<IActionResult> ThanhToanCuoi()
    {
        int? idPhien = HttpContext.Session.GetInt32("IdPhien");
        if (idPhien == null) return RedirectToAction("Index", "Menu");

        var donChuaTra = await _db.Donhangs
            .Include(d => d.Chitietdons).ThenInclude(c => c.IdsanphamNavigation)
            .Where(d => d.Idphien == idPhien.Value
                     && (d.Trangthaithanhtoan == DonHangStatus.ChoThanhToan || d.Trangthaithanhtoan == null)
                     && !d.Idthanhtoans.Any(t => t.Trangthai == DonHangStatus.ThanhToanHoanTat))
            .ToListAsync();

        ViewBag.TongTien = donChuaTra.Sum(d => d.Tongtien ?? 0m);
        return View(donChuaTra);
    }

    // POST /ThanhToan/XacNhanThanhToanCuoi
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> XacNhanThanhToanCuoi(string hinhThuc)
    {
        int? idPhien = HttpContext.Session.GetInt32("IdPhien");
        int? idBan = HttpContext.Session.GetInt32("IdBan");
        if (idPhien == null || idBan == null) return RedirectToAction("Index", "Menu");

        hinhThuc = ChuanHoaHinhThuc(hinhThuc);
        if (!HinhThucHopLe(hinhThuc))
        {
            TempData["Loi"] = "Hình thức thanh toán không hợp lệ.";
            return RedirectToAction(nameof(ThanhToanCuoi));
        }

        await using var transaction = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        try
        {
            var donChuaTra = await _db.Donhangs
                .Include(d => d.Idthanhtoans)
                .Where(d => d.Idphien == idPhien.Value
                         && (d.Trangthaithanhtoan == DonHangStatus.ChoThanhToan || d.Trangthaithanhtoan == null))
                .ToListAsync();

            if (donChuaTra.Count == 0)
            {
                await transaction.RollbackAsync();
                TempData["ThongBao"] = "Không còn đơn nào chưa thanh toán.";
                return View("CamOn");
            }

            if (donChuaTra.Any(d => DonHangStatus.IsCancelled(d.Trangthai)))
            {
                await transaction.RollbackAsync();
                TempData["Loi"] = "Có đơn đã bị hủy trong phiên bàn. Vui lòng nhờ nhân viên kiểm tra hóa đơn.";
                return RedirectToAction(nameof(ThanhToanCuoi));
            }

            decimal tongTien = donChuaTra.Sum(d => d.Tongtien ?? 0m);
            if (tongTien <= 0m)
            {
                await transaction.RollbackAsync();
                TempData["Loi"] = "Tổng tiền phải lớn hơn 0 mới có thể thanh toán.";
                return RedirectToAction(nameof(ThanhToanCuoi));
            }

            var thanhToan = new Thanhtoan
            {
                Idphien = idPhien.Value,
                Sotien = tongTien,
                Hinhthuc = hinhThuc,
                Trangthai = DonHangStatus.ThanhToanHoanTat,
                Thoigianthanhtoan = DateTime.Now
            };

            foreach (var don in donChuaTra)
            {
                don.Idthanhtoans.Add(thanhToan);
                don.Trangthaithanhtoan = DonHangStatus.DaThanhToan;
            }

            var phien = await _db.Phiensudungbans.FindAsync(idPhien.Value);
            if (phien != null)
            {
                phien.Trangthai = "dadong";
                phien.Thoigianketthuc = DateTime.Now;
            }

            var ban = await _db.Bans.FindAsync(idBan.Value);
            if (ban != null) ban.Trangthai = "chodon";

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
            HttpContext.Session.Clear();
            return View("CamOn");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await transaction.RollbackAsync();
            _logger.LogWarning(ex, "Xung đột dữ liệu khi thanh toán phiên bàn {IdPhien}", idPhien);
            TempData["Loi"] = "Đơn vừa được cập nhật ở nơi khác. Vui lòng tải lại hóa đơn.";
            return RedirectToAction(nameof(ThanhToanCuoi));
        }
        catch (DbUpdateException ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Lỗi SQL khi thanh toán phiên bàn {IdPhien}", idPhien);
            TempData["Loi"] = "Không lưu được thanh toán. Hãy báo nhân viên kiểm tra log ứng dụng.";
            return RedirectToAction(nameof(ThanhToanCuoi));
        }
    }

    private static string ChuanHoaHinhThuc(string? hinhThuc) => (hinhThuc ?? "").Trim().ToLowerInvariant();

    private static bool HinhThucHopLe(string hinhThuc) =>
        hinhThuc is "tienmat" or "chuyenkhoan" or "vidientu";
}
