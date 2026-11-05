using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanCafe.Helpers;
using QuanCafe.Models;

namespace QuanCafe.Controllers;

public class BanController : Controller
{
    private readonly AppDbContext _db;
    private readonly ILogger<BanController> _logger;

    public BanController(AppDbContext db, ILogger<BanController> logger)
    {
        _db = db;
        _logger = logger;
    }

    private const int MaBanToiDa = 20;      // ban.maban nvarchar(20)

    // Danh sách bàn cho trang /Ban/DanhSachQr (gọi lại sau khi thêm/xóa để tự cập nhật, không cần tải lại trang).
    [HttpGet("api/ban")]
    public async Task<IActionResult> DanhSachBan()
    {
        var bans = await _db.Bans.AsNoTracking()
            .OrderBy(b => b.Maban)
            .Select(b => new { id = b.Idban, maBan = b.Maban })
            .ToListAsync();
        return Ok(bans);
    }

    // Thêm một bàn mới -> xuất hiện ngay trong danh sách mã QR, không cần sửa database bằng tay.
    [HttpPost("api/ban")]
    public async Task<IActionResult> ThemBan([FromForm] string? maBan)
    {
        maBan = maBan?.Trim();
        if (string.IsNullOrWhiteSpace(maBan))
            return BadRequest(new { loi = "Vui lòng nhập mã bàn." });
        if (maBan.Length > MaBanToiDa)
            return BadRequest(new { loi = $"Mã bàn tối đa {MaBanToiDa} ký tự." });

        if (await _db.Bans.AnyAsync(b => b.Maban == maBan))
            return Conflict(new { loi = $"Mã bàn \"{maBan}\" đã tồn tại." });

        var ban = new Ban
        {
            Maban = maBan,
            // Cột succhua (sức chứa) có trong database nhưng ứng dụng chưa dùng tới ở đâu khác,
            // nên không hỏi khi thêm bàn; để 4 cho có giá trị hợp lệ.
            Succhua = 4,
            Trangthai = "trong",
            Qrcode = string.Empty   // cột bắt buộc có giá trị; điền đường dẫn thật ngay sau khi biết idban
        };
        _db.Bans.Add(ban);
        try
        {
            await _db.SaveChangesAsync();
            ban.Qrcode = $"Ban/Quet/{ban.Idban}";
            await _db.SaveChangesAsync();
            return Ok(new { id = ban.Idban, maBan = ban.Maban });
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Không thêm được bàn {MaBan}", maBan);
            return StatusCode(500, new { loi = "Không lưu được bàn mới vào cơ sở dữ liệu. Xem log ứng dụng để biết lỗi SQL chi tiết." });
        }
    }

    // Xóa một bàn (chỉ khi bàn chưa từng có đơn hàng/phiên sử dụng nào, để không làm mất dữ liệu cũ).
    [HttpDelete("api/ban/{id:int}")]
    public async Task<IActionResult> XoaBan(int id)
    {
        var ban = await _db.Bans.FindAsync(id);
        if (ban == null) return NotFound(new { loi = "Không tìm thấy bàn." });

        _db.Bans.Remove(ban);
        try
        {
            await _db.SaveChangesAsync();
            return Ok(new { ok = true });
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Không xóa được bàn {IdBan}", id);
            return Conflict(new { loi = "Không thể xóa bàn này vì đã có đơn hàng hoặc phiên sử dụng gắn với bàn." });
        }
    }

    // Trang quản lý: hiển thị / in mã QR cho tất cả các bàn đang có trong database.
    // Mã QR được vẽ ngay trên trình duyệt (xem wwwroot/js/qr-ban.js), mỗi mã trỏ tới /Ban/Quet/{idban}.
    [HttpGet("Ban/DanhSachQr")]
    public async Task<IActionResult> DanhSachQr()
    {
        var bans = await _db.Bans.AsNoTracking().OrderBy(b => b.Maban).ToListAsync();
        return View(bans);
    }

    // Mã QR dán ở mỗi bàn trỏ tới:  https://<tên-miền>/Ban/Quet/{idban}
    [HttpGet("Ban/Quet/{idban:int}")]
    public async Task<IActionResult> Quet(int idban)
    {
        var ban = await _db.Bans.FindAsync(idban);
        if (ban == null) return NotFound("Bàn không tồn tại");

        // Mỗi bàn chỉ có 1 phiên đang mở; khách cùng bàn dùng chung phiên
        var phien = await _db.Phiensudungbans
            .FirstOrDefaultAsync(p => p.Idban == idban && p.Trangthai == "dangmo");

        if (phien == null)
        {
            phien = new Phiensudungban
            {
                Idban = idban,
                Thoigianbatdau = DateTime.Now,
                Trangthai = "dangmo"
            };
            _db.Phiensudungbans.Add(phien);
            ban.Trangthai = "dangphucvu";
            await _db.SaveChangesAsync();
        }

        var session = HttpContext.Session;

        // Quét sang bàn/phiên khác thì bỏ giỏ hàng cũ
        if (session.GetInt32(SessionKeys.IdPhien) != phien.Idphien)
            session.Remove(SessionKeys.GioHang);

        session.SetInt32(SessionKeys.IdPhien, phien.Idphien);
        session.SetInt32(SessionKeys.IdBan, ban.Idban);
        session.SetString(SessionKeys.MaBan, ban.Maban);

        return RedirectToAction("Index", "Menu");
    }
}
