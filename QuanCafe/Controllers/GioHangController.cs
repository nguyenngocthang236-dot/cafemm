using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanCafe.Helpers;
using QuanCafe.Models;
using QuanCafe.ViewModels;

namespace QuanCafe.Controllers;

public class GioHangController : KhachController
{
    // Không còn giới hạn cứng 20 nữa: số lượng khách gọi được CHỈ phụ thuộc vào tồn kho của món
    // (cột sanpham.soluongton). Kho còn bao nhiêu thì gọi được bấy nhiêu.
    // Hằng số này được giữ lại chỉ để MenuController.ChiTiet (đang tham chiếu nó) vẫn chạy đúng;
    // nó bằng mức tồn kho tối đa của một món nên không bao giờ chặn trước kho.
    public const int SoLuongToiDa = KhoHang.ToiDa;

    public GioHangController(AppDbContext db) : base(db) { }

    // GET /GioHang
    public async Task<IActionResult> Index()
    {
        var (gio, coThayDoi) = await TaiGioAsync();
        if (coThayDoi && !TempData.ContainsKey("Canh"))
            TempData["Canh"] = "Một số món vừa hết hoặc không còn đủ số lượng nên giỏ hàng đã được điều chỉnh theo kho.";

        ViewData["SoLuongGio"] = gio.TongSoLuong;
        return View(gio);
    }

    // POST /GioHang/Them   (trang chi tiết món gọi bằng fetch, trả về JSON)
    // conLai = số lượng khách còn có thể thêm cho món này sau lần gọi này (JS dùng để khóa nút +).
    [HttpPost]
    public async Task<IActionResult> Them(int idSanPham, int soLuong)
    {
        if (soLuong < 1)
            return Json(new { ok = false, thongBao = "Số lượng không hợp lệ." });

        var sp = await _db.Sanphams.AsNoTracking().FirstOrDefaultAsync(s => s.Idsanpham == idSanPham);
        if (sp == null)
            return Json(new { ok = false, thongBao = "Không tìm thấy món này.", conLai = 0 });
        if (sp.Soluongton <= 0)
            return Json(new { ok = false, thongBao = "Món này đã hết hàng. Bạn chọn món khác nhé.", conLai = 0 });

        // Được gọi thêm = tồn kho của món trừ đi phần đã có trong giỏ.
        var gio = LayGio();
        var dong = gio.FirstOrDefault(x => x.Idsanpham == idSanPham);
        var daCo = dong?.Soluong ?? 0;
        var conLai = Math.Max(0, sp.Soluongton - daCo);

        if (soLuong > conLai)
        {
            var thongBao = conLai > 0
                ? $"Món này chỉ còn {sp.Soluongton} trong kho, bạn chỉ có thể thêm tối đa {conLai} nữa."
                : $"Món này chỉ còn {sp.Soluongton} trong kho và bạn đã có {daCo} trong giỏ hàng, không thể thêm nữa.";
            return Json(new { ok = false, thongBao, conLai });
        }

        if (dong == null)
            gio.Add(new GioHangItem { Idsanpham = idSanPham, Soluong = soLuong });
        else
            dong.Soluong += soLuong;

        LuuGio(gio);

        // Số hiển thị trên biểu tượng giỏ hàng = tổng số ly. Muốn đếm số loại món thì đổi Sum thành Count.
        return Json(new { ok = true, tongSoLuong = gio.Sum(x => x.Soluong), conLai = conLai - soLuong });
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

    // POST /GioHang/DatHang   -> trừ kho + tạo đơn hàng + chi tiết đơn (cùng một giao dịch)
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DatHang(string? ghiChu)
    {
        var (gio, coThayDoi) = await TaiGioAsync();

        if (coThayDoi)
        {
            TempData["Canh"] = "Một số món vừa hết hoặc không còn đủ số lượng nên giỏ hàng đã được điều chỉnh. Bạn kiểm tra lại rồi đặt hàng nhé.";
            return RedirectToAction(nameof(Index));
        }
        if (gio.Dongs.Count == 0)
            return RedirectToAction(nameof(Index));

        ghiChu = string.IsNullOrWhiteSpace(ghiChu) ? null : ghiChu.Trim();
        if (ghiChu != null && ghiChu.Length > 200) ghiChu = ghiChu[..200];

        // Trừ kho và tạo đơn trong CÙNG một giao dịch: lỗi ở bước nào thì cả hai cùng được hoàn lại.
        await using var transaction = await _db.Database.BeginTransactionAsync();

        foreach (var d in gio.Dongs)
        {
            // UPDATE có điều kiện: chỉ trừ được khi kho còn đủ -> không bao giờ bán lố,
            // kể cả khi nhiều khách đặt cùng một món cùng một lúc.
            if (!await KhoHang.ThayDoiAsync(_db, d.Idsanpham, -d.Soluong))
            {
                await transaction.RollbackAsync();
                TempData["Canh"] = $"Món \"{d.Tensanpham}\" vừa có khách khác đặt nên không còn đủ số lượng. Bạn kiểm tra lại giỏ hàng nhé.";
                return RedirectToAction(nameof(Index));
            }
        }

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

        await transaction.CommitAsync();

        HttpContext.Session.Remove(SessionKeys.GioHang);
        return RedirectToAction("ThanhCong", "DonHang", new { maDon = donHang.Madonhang });
    }

    // ---------- Hàm phụ ----------

    private List<GioHangItem> LayGio() =>
        HttpContext.Session.GetObject<List<GioHangItem>>(SessionKeys.GioHang) ?? new List<GioHangItem>();

    private void LuuGio(List<GioHangItem> gio) =>
        HttpContext.Session.SetObject(SessionKeys.GioHang, gio);

    /// <summary>
    /// Ghép giỏ trong Session với dữ liệu món trong database (tên, ảnh, giá, tồn kho).
    /// - Món đã hết hàng (hoặc bị xóa) -> bỏ khỏi giỏ.
    /// - Số lượng trong giỏ lớn hơn tồn kho -> hạ xuống bằng số còn lại trong kho.
    /// Có thay đổi nào thì trả coThayDoi = true để báo cho khách biết.
    /// </summary>
    private async Task<(GioHangViewModel Gio, bool CoThayDoi)> TaiGioAsync()
    {
        var gio = LayGio();
        var ids = gio.Select(x => x.Idsanpham).Distinct().ToList();

        var sanPhams = await _db.Sanphams
            .AsNoTracking()
            .Where(s => ids.Contains(s.Idsanpham) && s.Soluongton > 0)
            .ToDictionaryAsync(s => s.Idsanpham);

        var hopLe = new List<GioHangItem>();
        var coThayDoi = false;
        foreach (var x in gio)
        {
            if (!sanPhams.TryGetValue(x.Idsanpham, out var sp))
            {
                coThayDoi = true;      // món đã hết hàng hoặc không còn
                continue;
            }

            if (x.Soluong > sp.Soluongton)
            {
                x.Soluong = sp.Soluongton;   // giỏ đang giữ nhiều hơn số còn trong kho
                coThayDoi = true;
            }
            hopLe.Add(x);
        }
        if (coThayDoi) LuuGio(hopLe);

        var vm = new GioHangViewModel
        {
            Dongs = hopLe.Select(x =>
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

        return (vm, coThayDoi);
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
