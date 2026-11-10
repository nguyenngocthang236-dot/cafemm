using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanCafe.Helpers;
using QuanCafe.Models;

namespace QuanCafe.Controllers;

public class DonHangController : KhachController
{
    public DonHangController(AppDbContext db) : base(db) { }

    // GET /DonHang/ThanhCong?maDon=261028-007
    public async Task<IActionResult> ThanhCong(string maDon)
    {
        var don = await _db.Donhangs
            .AsNoTracking()
            .Include(d => d.Chitietdons).ThenInclude(c => c.IdsanphamNavigation)
            .FirstOrDefaultAsync(d => d.Madonhang == maDon && d.Idphien == IdPhien);

        if (don == null) return RedirectToAction("Index", "Menu");
        return View(don);
    }

    // Màn hình trạng thái được ThanhToanController chuyển về sau khi trả tiền.
    public async Task<IActionResult> TrangThai(string maDon)
    {
        var don = await _db.Donhangs
            .AsNoTracking()
            .Include(d => d.Chitietdons).ThenInclude(c => c.IdsanphamNavigation)
            .FirstOrDefaultAsync(d => d.Madonhang == maDon && d.Idphien == IdPhien);

        if (don == null) return RedirectToAction("Index", "Menu");
        ViewBag.TrangThaiHienThi = DonHangStatus.ToDisplayStatus(don.Trangthai);
        return View(don);
    }
}
