using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using QuanCafe.Helpers;
using QuanCafe.Models;
using QuanCafe.ViewModels;

namespace QuanCafe.Controllers;

/// <summary>
/// Lớp cha cho các controller dành cho khách (Menu, GioHang, DonHang).
/// Trước mỗi action nó kiểm tra: khách đã quét QR chưa, phiên của bàn còn đang mở không.
/// Nếu chưa -> hiện trang "Hãy quét mã QR tại bàn".
/// </summary>
public abstract class KhachController : Controller
{
    protected readonly AppDbContext _db;

    protected KhachController(AppDbContext db) => _db = db;

    // Mỗi món tối đa 20 ly trong giỏ, kể cả khi kho còn nhiều hơn
    protected const int SoLuongToiDa = 20;

    protected int IdPhien => HttpContext.Session.GetInt32(SessionKeys.IdPhien) ?? 0;
    protected int IdBan => HttpContext.Session.GetInt32(SessionKeys.IdBan) ?? 0;

    /// <summary>
    /// Số phần tối đa của món còn làm được theo kho: với mỗi nguyên liệu trong công thức lấy
    /// (tồn kho ÷ định lượng mỗi phần), rồi chọn nguyên liệu cạn nhất.
    /// Trả về null nếu món chưa khai báo công thức (không có cơ sở để giới hạn theo kho).
    /// </summary>
    protected async Task<int?> SoPhanToiDaTheoKhoAsync(int idSanPham)
    {
        var congThuc = await _db.Congthucsanphams
            .AsNoTracking()
            .Where(c => c.Idsanpham == idSanPham && c.Soluongcanthiet > 0)
            .Select(c => new { c.Soluongcanthiet, Ton = c.IdnguyenlieuNavigation.Soluongtonkho ?? 0m })
            .ToListAsync();

        if (congThuc.Count == 0) return null;

        var soPhan = congThuc.Min(c => Math.Floor(c.Ton / c.Soluongcanthiet));
        return (int)Math.Clamp(soPhan, 0m, 1000m);
    }

    /// <summary>Giới hạn thực tế của một món trong giỏ = số nhỏ hơn giữa "còn trong kho" và SoLuongToiDa.</summary>
    protected async Task<int> GioiHanMotMonAsync(int idSanPham)
    {
        var theoKho = await SoPhanToiDaTheoKhoAsync(idSanPham);
        return theoKho.HasValue ? Math.Min(theoKho.Value, SoLuongToiDa) : SoLuongToiDa;
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var session = context.HttpContext.Session;
        var idPhien = session.GetInt32(SessionKeys.IdPhien);
        var idBan = session.GetInt32(SessionKeys.IdBan);

        var phienConMo = false;
        if (idPhien != null && idBan != null)
        {
            phienConMo = await _db.Phiensudungbans.AnyAsync(p =>
                p.Idphien == idPhien && p.Idban == idBan && p.Trangthai == "dangmo");
        }

        if (!phienConMo)
        {
            session.Clear();
            context.Result = View("ChuaQuetQr");
            return;
        }

        // Dữ liệu dùng chung cho thanh trên cùng (số bàn, số món trong giỏ)
        ViewData["MaBan"] = session.GetString(SessionKeys.MaBan);
        ViewData["SoLuongGio"] =
            session.GetObject<List<GioHangItem>>(SessionKeys.GioHang)?.Sum(x => x.Soluong) ?? 0;

        await next();
    }
}
