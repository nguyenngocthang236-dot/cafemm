using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanCafe.Models;

namespace QuanCafe.Controllers;

/// <summary>
/// Chat hỗ trợ phía QUẢN LÝ (nút "Yêu cầu hỗ trợ" trên Dashboard).
/// Giống các API khác của khu quản lý (api/donhang, api/sale...): hiện chưa có đăng nhập.
/// </summary>
public class HoTroQuanLyController : Controller
{
    private const int NoiDungToiDa = 500;
    private const int SoTinMoiLanLay = 200;

    private readonly AppDbContext _db;

    public HoTroQuanLyController(AppDbContext db) => _db = db;

    // GET /api/hotro/ban
    // Tất cả các bàn + số tin khách chưa được đọc ở mỗi bàn (chỉ tính phiên đang mở).
    [HttpGet("api/hotro/ban")]
    public async Task<IActionResult> DanhSachBan()
    {
        var bans = await _db.Bans.AsNoTracking()
            .OrderBy(b => b.Maban)
            .Select(b => new { b.Idban, b.Maban })
            .ToListAsync();

        var phienMo = await _db.Phiensudungbans.AsNoTracking()
            .Where(p => p.Trangthai == "dangmo")
            .Select(p => new { p.Idphien, p.Idban })
            .ToListAsync();
        var idPhienMo = phienMo.Select(p => p.Idphien).ToList();
        var idBanCoKhach = phienMo.Select(p => p.Idban).ToHashSet();

        var chuaDocTheoBan = await _db.Hotrotinnhans.AsNoTracking()
            .Where(t => idPhienMo.Contains(t.Idphien) && t.Nguoigui == "khach" && !t.Dadoc)
            .GroupBy(t => t.Idban)
            .Select(g => new { Idban = g.Key, So = g.Count() })
            .ToDictionaryAsync(x => x.Idban, x => x.So);

        var ban = bans.Select(b => new
        {
            id = b.Idban,
            maBan = b.Maban,
            coKhach = idBanCoKhach.Contains(b.Idban),
            chuaDoc = chuaDocTheoBan.TryGetValue(b.Idban, out var so) ? so : 0
        }).ToList();

        return Ok(new { tong = ban.Sum(b => b.chuaDoc), ban });
    }

    // GET /api/hotro/ban/5/tin?sauId=0&dangMo=true
    // dangMo = true: quản lý đang mở khung chat của bàn này -> đánh dấu tin của khách là đã đọc.
    [HttpGet("api/hotro/ban/{idBan:int}/tin")]
    public async Task<IActionResult> Tin(int idBan, int sauId = 0, bool dangMo = true)
    {
        var idPhien = await PhienDangMoAsync(idBan);
        if (idPhien == null)
            return Ok(new { coKhach = false, tin = Array.Empty<object>() });

        if (dangMo)
        {
            await _db.Hotrotinnhans
                .Where(t => t.Idphien == idPhien && t.Nguoigui == "khach" && !t.Dadoc)
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.Dadoc, true));
        }

        var goc = _db.Hotrotinnhans.AsNoTracking().Where(t => t.Idphien == idPhien);

        List<Hotrotinnhan> tin;
        if (sauId <= 0)
        {
            tin = await goc.OrderByDescending(t => t.Idtinnhan).Take(SoTinMoiLanLay).ToListAsync();
            tin.Reverse();
        }
        else
        {
            tin = await goc.Where(t => t.Idtinnhan > sauId).OrderBy(t => t.Idtinnhan).Take(SoTinMoiLanLay).ToListAsync();
        }

        return Ok(new { coKhach = true, tin = tin.Select(Dang) });
    }

    // POST /api/hotro/ban/5/gui   (form: noiDung)
    [HttpPost("api/hotro/ban/{idBan:int}/gui")]
    public async Task<IActionResult> Gui(int idBan, [FromForm] string? noiDung)
    {
        noiDung = noiDung?.Trim();
        if (string.IsNullOrEmpty(noiDung))
            return BadRequest(new { loi = "Bạn chưa nhập nội dung tin nhắn." });
        if (noiDung.Length > NoiDungToiDa)
            return BadRequest(new { loi = $"Tin nhắn tối đa {NoiDungToiDa} ký tự." });

        var idPhien = await PhienDangMoAsync(idBan);
        if (idPhien == null)
            return Conflict(new { loi = "Bàn này hiện chưa có khách (phiên đã đóng) nên chưa thể nhắn." });

        var tin = new Hotrotinnhan
        {
            Idphien = idPhien.Value,
            Idban = idBan,
            Nguoigui = "quanly",
            Noidung = noiDung,
            Thoigian = DateTime.Now,
            Dadoc = false
        };
        _db.Hotrotinnhans.Add(tin);
        await _db.SaveChangesAsync();

        return Ok(new { ok = true, tin = Dang(tin) });
    }

    // Phiên đang mở của bàn (null = bàn đang trống)
    private Task<int?> PhienDangMoAsync(int idBan) =>
        _db.Phiensudungbans.AsNoTracking()
            .Where(p => p.Idban == idBan && p.Trangthai == "dangmo")
            .OrderByDescending(p => p.Idphien)
            .Select(p => (int?)p.Idphien)
            .FirstOrDefaultAsync();

    private static object Dang(Hotrotinnhan t) =>
        new { id = t.Idtinnhan, tu = t.Nguoigui, nd = t.Noidung, luc = t.Thoigian.ToString("HH:mm") };
}
