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

    protected int IdPhien => HttpContext.Session.GetInt32(SessionKeys.IdPhien) ?? 0;
    protected int IdBan => HttpContext.Session.GetInt32(SessionKeys.IdBan) ?? 0;

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
