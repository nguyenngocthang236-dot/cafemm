using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanCafe.Helpers;
using QuanCafe.Models;

namespace QuanCafe.Controllers;

public class NhanVienController : Controller
{
    private readonly AppDbContext _db;
    private readonly ILogger<NhanVienController> _logger;

    public NhanVienController(AppDbContext db, ILogger<NhanVienController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // GET: /NhanVien/DanhSachDon
    public async Task<IActionResult> DanhSachDon()
    {
        var donHangs = await _db.Donhangs
            .Include(d => d.Chitietdons).ThenInclude(c => c.IdsanphamNavigation)
            .Include(d => d.IdbanNavigation)
            // Đọc cả các giá trị trạng thái cũ để không làm mất đơn lịch sử.
            .Where(d => d.Trangthai != DonHangStatus.HoanTat
                     && d.Trangthai != "hoantat"
                     && d.Trangthai != DonHangStatus.DaHuy
                     && d.Trangthai != "dahuy")
            .OrderBy(d => d.Thoigiandat)
            .ToListAsync();

        return View(donHangs);
    }

    // POST: /NhanVien/CapNhatMon
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CapNhatMon(int idChiTiet, string trangThaiMoi)
    {
        var chiTiet = await _db.Chitietdons.FindAsync(idChiTiet);
        if (chiTiet == null) return NotFound();

        var donHang = await _db.Donhangs
            .Include(d => d.Chitietdons)
            .FirstOrDefaultAsync(d => d.Idonhang == chiTiet.Idonhang);
        if (donHang == null) return NotFound();

        if (DonHangStatus.IsCancelled(donHang.Trangthai) || DonHangStatus.IsCompleted(donHang.Trangthai))
        {
            TempData["Loi"] = "Đơn đã bị hủy hoặc hoàn tất nên không thể sửa món.";
            return RedirectToAction(nameof(DanhSachDon));
        }

        // Chỉ cho phép chuyển đúng một bước: chờ làm -> đang làm -> xong.
        var trangThaiHopLe = chiTiet.Trangthai switch
        {
            "cholam" => "danglam",
            "danglam" => "xong",
            _ => null
        };

        if (trangThaiHopLe == null || !string.Equals(trangThaiMoi, trangThaiHopLe, StringComparison.Ordinal))
        {
            TempData["Loi"] = "Trạng thái món không hợp lệ hoặc món đã được cập nhật trước đó. Hãy tải lại trang.";
            return RedirectToAction(nameof(DanhSachDon));
        }

        chiTiet.Trangthai = trangThaiHopLe;
        if (trangThaiHopLe == "danglam" && donHang.Trangthai == DonHangStatus.ChoXacNhan)
            donHang.Trangthai = DonHangStatus.DangChuanBi;

        if (donHang.Chitietdons.Count > 0 && donHang.Chitietdons.All(c => c.Trangthai == "xong"))
            donHang.Trangthai = DonHangStatus.SanSangGiao;

        try
        {
            await _db.SaveChangesAsync();
            TempData["ThongBao"] = donHang.Trangthai == DonHangStatus.SanSangGiao
                ? $"Đơn {donHang.Madonhang} đã chuẩn bị xong, có thể giao cho khách."
                : "Đã cập nhật trạng thái món.";
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Xung đột cập nhật món {IdChiTiet} trong đơn {IdDonHang}", idChiTiet, donHang.Idonhang);
            TempData["Loi"] = "Đơn hoặc món vừa được cập nhật ở nơi khác. Trang đã được tải lại; hãy kiểm tra trạng thái mới.";
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Lỗi SQL khi cập nhật món {IdChiTiet} trong đơn {IdDonHang}", idChiTiet, donHang.Idonhang);
            TempData["Loi"] = "Không lưu được trạng thái món vào SQL Server. Hãy kiểm tra log ứng dụng để xem lỗi chi tiết.";
        }

        return RedirectToAction(nameof(DanhSachDon));
    }

    // POST: /NhanVien/XacNhanGiao
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> XacNhanGiao(int idDonHang)
    {
        var don = await _db.Donhangs
            .Include(d => d.Chitietdons)
            .FirstOrDefaultAsync(d => d.Idonhang == idDonHang);
        if (don == null) return NotFound();

        if (DonHangStatus.IsCancelled(don.Trangthai))
        {
            TempData["Loi"] = "Đơn đã bị hủy.";
            return RedirectToAction(nameof(DanhSachDon));
        }
        if (DonHangStatus.IsCompleted(don.Trangthai))
        {
            TempData["ThongBao"] = "Đơn này đã được xác nhận giao trước đó.";
            return RedirectToAction(nameof(DanhSachDon));
        }
        if (don.Trangthai != DonHangStatus.SanSangGiao || don.Chitietdons.Count == 0 || don.Chitietdons.Any(c => c.Trangthai != "xong"))
        {
            TempData["Loi"] = "Chỉ có thể giao đơn khi tất cả món đã chuẩn bị xong.";
            return RedirectToAction(nameof(DanhSachDon));
        }

        don.Trangthai = DonHangStatus.HoanTat;
        try
        {
            await _db.SaveChangesAsync();
            TempData["ThongBao"] = $"Đã xác nhận giao đơn {don.Madonhang} cho khách.";
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Xung đột dữ liệu khi xác nhận giao đơn {IdDonHang}", idDonHang);
            TempData["Loi"] = "Đơn vừa được cập nhật ở nơi khác. Hãy kiểm tra trạng thái mới.";
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Lỗi SQL khi xác nhận giao đơn {IdDonHang}", idDonHang);
            TempData["Loi"] = "Không lưu được trạng thái đơn vào SQL Server. Hãy kiểm tra log ứng dụng.";
        }

        return RedirectToAction(nameof(DanhSachDon));
    }
}
