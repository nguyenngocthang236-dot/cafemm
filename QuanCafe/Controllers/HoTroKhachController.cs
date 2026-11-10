using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanCafe.Models;

namespace QuanCafe.Controllers;

/// <summary>
/// Chat hỗ trợ phía KHÁCH. Kế thừa KhachController nên chỉ khách đã quét QR (phiên bàn còn mở) mới gọi được;
/// bàn và phiên luôn lấy từ Session, không tin dữ liệu trình duyệt gửi lên.
/// </summary>
public class HoTroKhachController : KhachController
{
    private const int NoiDungToiDa = 500;      // khớp cột noidung nvarchar(500) và maxlength của ô nhập
    private const int SoTinToiDaMoiPhut = 12;  // chống bấm gửi dồn dập
    private const int SoTinMoiLanLay = 200;

    public HoTroKhachController(AppDbContext db) : base(db) { }

    // GET /HoTroKhach/Tin?sauId=0&dangMo=true
    // sauId = 0: lấy lịch sử (tối đa 200 tin mới nhất). sauId > 0: chỉ lấy tin có id lớn hơn.
    // dangMo = true: khách đang mở khung chat -> đánh dấu các tin của quản lý là đã đọc.
    [HttpGet]
    public async Task<IActionResult> Tin(int sauId = 0, bool dangMo = false)
    {
        var idPhien = IdPhien;

        if (dangMo)
        {
            await _db.Hotrotinnhans
                .Where(t => t.Idphien == idPhien && t.Nguoigui == "quanly" && !t.Dadoc)
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

        // Số tin quản lý trả lời mà khách chưa mở xem -> hiện trên nút "Hỗ trợ" khi khung chat đang đóng
        var chuaDoc = dangMo
            ? 0
            : await _db.Hotrotinnhans.CountAsync(t => t.Idphien == idPhien && t.Nguoigui == "quanly" && !t.Dadoc);

        return Json(new { tin = tin.Select(Dang), chuaDoc });
    }

    // POST /HoTroKhach/Gui   (noiDung)
    [HttpPost]
    public async Task<IActionResult> Gui(string? noiDung)
    {
        noiDung = noiDung?.Trim();
        if (string.IsNullOrEmpty(noiDung))
            return Json(new { ok = false, thongBao = "Bạn chưa nhập nội dung tin nhắn." });
        if (noiDung.Length > NoiDungToiDa)
            return Json(new { ok = false, thongBao = $"Tin nhắn tối đa {NoiDungToiDa} ký tự." });

        var idPhien = IdPhien;
        var moc = DateTime.Now.AddMinutes(-1);
        var daGui = await _db.Hotrotinnhans.CountAsync(t =>
            t.Idphien == idPhien && t.Nguoigui == "khach" && t.Thoigian > moc);
        if (daGui >= SoTinToiDaMoiPhut)
            return Json(new { ok = false, thongBao = "Bạn nhắn hơi nhanh, chờ một chút rồi gửi tiếp nhé." });

        var tin = new Hotrotinnhan
        {
            Idphien = idPhien,
            Idban = IdBan,
            Nguoigui = "khach",
            Noidung = noiDung,
            Thoigian = DateTime.Now,
            Dadoc = false
        };
        _db.Hotrotinnhans.Add(tin);
        await _db.SaveChangesAsync();

        return Json(new { ok = true, tin = Dang(tin) });
    }

    // Dạng gọn gửi về trình duyệt: id, tu ("khach"/"quanly"), nd (nội dung), luc (giờ:phút)
    private static object Dang(Hotrotinnhan t) =>
        new { id = t.Idtinnhan, tu = t.Nguoigui, nd = t.Noidung, luc = t.Thoigian.ToString("HH:mm") };
}
