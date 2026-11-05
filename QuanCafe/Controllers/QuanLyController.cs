using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanCafe.Helpers;
using QuanCafe.Models;

namespace QuanCafe.Controllers;

/// <summary>Khu vực quản lý đơn hàng và thực đơn của quán.</summary>
public class QuanLyController : Controller
{
    private readonly AppDbContext _db;
    private readonly ILogger<QuanLyController> _logger;
    private readonly IWebHostEnvironment _env;

    public QuanLyController(AppDbContext db, ILogger<QuanLyController> logger, IWebHostEnvironment env)
    {
        _db = db;
        _logger = logger;
        _env = env;
    }

    // GET /Menu/Dashboard
    [HttpGet("Menu/Dashboard")]
    public IActionResult Dashboard() => View();

    // GET /api/donhang?trangThai=...
    [HttpGet("api/donhang")]
    public async Task<IActionResult> GetDonHang(string? trangThai)
    {
        var query = _db.Donhangs
            .AsNoTracking()
            .Include(d => d.Chitietdons).ThenInclude(c => c.IdsanphamNavigation)
            .Include(d => d.IdbanNavigation)
            .Include(d => d.Idthanhtoans)
            .AsQueryable();

        query = trangThai switch
        {
            "cho" => query.Where(d => d.Trangthai == DonHangStatus.ChoXacNhan),
            "chuan-bi" => query.Where(d => d.Trangthai == DonHangStatus.DangChuanBi || d.Trangthai == "dangchuan"),
            "san-sang" => query.Where(d => d.Trangthai == DonHangStatus.SanSangGiao),
            "hoan-tat" => query.Where(d => d.Trangthai == DonHangStatus.HoanTat || d.Trangthai == "hoantat"),
            "da-huy" => query.Where(d => d.Trangthai == DonHangStatus.DaHuy || d.Trangthai == "dahuy"),
            _ => query
        };

        var dons = await query
            .OrderByDescending(d => d.Thoigiandat)
            .Take(100)
            .ToListAsync();

        var result = dons.Select(d =>
        {
            var thanhToanCuoi = d.Idthanhtoans
                .Where(t => t.Trangthai == DonHangStatus.ThanhToanHoanTat)
                .OrderByDescending(t => t.Thoigianthanhtoan)
                .FirstOrDefault();
            var daThanhToan = DonHangStatus.IsPaid(d.Trangthaithanhtoan) || thanhToanCuoi != null;

            return new
            {
                id = d.Idonhang,
                maDon = d.Madonhang,
                maBan = d.IdbanNavigation.Maban,
                trangThai = DonHangStatus.ToApiStatus(d.Trangthai),
                daNhan = d.Trangthai != DonHangStatus.ChoXacNhan,
                daHuy = DonHangStatus.IsCancelled(d.Trangthai),
                tong = d.Tongtien ?? 0,
                thoiGian = d.Thoigiandat,
                ghiChu = d.Ghichu,
                trangThaiThanhToan = daThanhToan ? "daThanhToan" : "chuaThanhToan",
                hinhThucThanhToan = thanhToanCuoi?.Hinhthuc,
                items = d.Chitietdons.Select(c => new
                {
                    ten = c.IdsanphamNavigation.Tensanpham,
                    soLuong = c.Soluong,
                    donGia = c.Dongia,
                    hinhanh = AnhMon.Url(c.IdsanphamNavigation.Hinhanh)
                })
            };
        });

        return Ok(result);
    }

    // POST /api/donhang/{id}/nhan
    [HttpPost("api/donhang/{id:int}/nhan")]
    public async Task<IActionResult> NhanDon(int id)
    {
        var don = await _db.Donhangs.FirstOrDefaultAsync(d => d.Idonhang == id);
        if (don == null) return NotFound(new { loi = "Không tìm thấy đơn hàng." });
        if (DonHangStatus.IsCancelled(don.Trangthai))
            return Conflict(new { loi = "Đơn đã bị hủy." });
        if (DonHangStatus.IsPreparing(don.Trangthai))
            return Ok(new { ok = true, maDon = don.Madonhang, trangThai = "dangChuanBi" });
        if (don.Trangthai != DonHangStatus.ChoXacNhan)
            return Conflict(new { loi = "Chỉ có thể nhận đơn đang chờ xác nhận." });

        don.Trangthai = DonHangStatus.DangChuanBi;
        return await LuuTrangThaiDonAsync(don, "nhận đơn");
    }

    // POST /api/donhang/{id}/chuan-bi — giữ endpoint cũ để dashboard cũ không bị gãy.
    [HttpPost("api/donhang/{id:int}/chuan-bi")]
    public async Task<IActionResult> ChuanBiDon(int id)
    {
        var don = await _db.Donhangs.FirstOrDefaultAsync(d => d.Idonhang == id);
        if (don == null) return NotFound(new { loi = "Không tìm thấy đơn hàng." });
        if (DonHangStatus.IsCancelled(don.Trangthai))
            return Conflict(new { loi = "Đơn đã bị hủy." });
        if (DonHangStatus.IsPreparing(don.Trangthai))
            return Ok(new { ok = true, maDon = don.Madonhang, trangThai = "dangChuanBi" });
        if (don.Trangthai != DonHangStatus.ChoXacNhan)
            return Conflict(new { loi = "Đơn không ở trạng thái có thể chuyển sang chuẩn bị." });

        don.Trangthai = DonHangStatus.DangChuanBi;
        return await LuuTrangThaiDonAsync(don, "chuyển đơn sang chuẩn bị");
    }

    // POST /api/donhang/{id}/chuan-bi-xong — chỉ đánh dấu sẵn sàng khi mọi món đã xong.
    [HttpPost("api/donhang/{id:int}/chuan-bi-xong")]
    public async Task<IActionResult> ChuanBiXong(int id)
    {
        var don = await _db.Donhangs
            .Include(d => d.Chitietdons)
            .FirstOrDefaultAsync(d => d.Idonhang == id);
        if (don == null) return NotFound(new { loi = "Không tìm thấy đơn hàng." });
        if (DonHangStatus.IsCancelled(don.Trangthai))
            return Conflict(new { loi = "Đơn đã bị hủy." });
        if (don.Trangthai == DonHangStatus.SanSangGiao)
            return Ok(new { ok = true, maDon = don.Madonhang, trangThai = "sanSangGiao" });
        if (!DonHangStatus.IsPreparing(don.Trangthai))
            return Conflict(new { loi = "Đơn phải ở trạng thái đang chuẩn bị trước." });
        if (don.Chitietdons.Count == 0 || don.Chitietdons.Any(c => c.Trangthai != "xong"))
            return BadRequest(new { loi = "Chưa thể hoàn tất: cần đánh dấu xong tất cả món trong đơn." });

        don.Trangthai = DonHangStatus.SanSangGiao;
        return await LuuTrangThaiDonAsync(don, "đánh dấu chuẩn bị xong");
    }

    // POST /api/donhang/{id}/giao — chuyển từ sẵn sàng giao sang đã hoàn tất.
    [HttpPost("api/donhang/{id:int}/giao")]
    public async Task<IActionResult> XacNhanGiao(int id)
    {
        var don = await _db.Donhangs.FirstOrDefaultAsync(d => d.Idonhang == id);
        if (don == null) return NotFound(new { loi = "Không tìm thấy đơn hàng." });
        if (DonHangStatus.IsCancelled(don.Trangthai))
            return Conflict(new { loi = "Đơn đã bị hủy." });
        if (DonHangStatus.IsCompleted(don.Trangthai))
            return Ok(new { ok = true, maDon = don.Madonhang, trangThai = "daHoanTat" });
        if (don.Trangthai != DonHangStatus.SanSangGiao)
            return Conflict(new { loi = "Chỉ được xác nhận giao khi đơn đã sẵn sàng." });

        don.Trangthai = DonHangStatus.HoanTat;
        return await LuuTrangThaiDonAsync(don, "xác nhận giao đơn");
    }

    // POST /api/donhang/{id}/huy
    [HttpPost("api/donhang/{id:int}/huy")]
    public async Task<IActionResult> HuyDon(int id)
    {
        var don = await _db.Donhangs
            .Include(d => d.Idthanhtoans)
            .FirstOrDefaultAsync(d => d.Idonhang == id);
        if (don == null) return NotFound(new { loi = "Không tìm thấy đơn hàng." });
        if (DonHangStatus.IsCancelled(don.Trangthai))
            return Ok(new { ok = true, maDon = don.Madonhang, trangThai = "daHuy" });
        if (DonHangStatus.IsCompleted(don.Trangthai) || don.Trangthai == DonHangStatus.SanSangGiao)
            return Conflict(new { loi = "Đơn đã hoàn tất hoặc sẵn sàng giao, không thể hủy bằng thao tác này." });
        if (DonHangStatus.IsPaid(don.Trangthaithanhtoan) || don.Idthanhtoans.Any(t => t.Trangthai == DonHangStatus.ThanhToanHoanTat))
            return Conflict(new { loi = "Đơn đã thanh toán. Cần xử lý hoàn tiền trước khi hủy." });

        don.Trangthai = DonHangStatus.DaHuy;
        return await LuuTrangThaiDonAsync(don, "hủy đơn");
    }

    // GET /api/donhang/{id}/thanh-toan — xem trạng thái và lịch sử thanh toán của đơn.
    [HttpGet("api/donhang/{id:int}/thanh-toan")]
    public async Task<IActionResult> GetThanhToanDon(int id)
    {
        var tonTai = await _db.Donhangs.AsNoTracking().AnyAsync(d => d.Idonhang == id);
        if (!tonTai) return NotFound(new { loi = "Không tìm thấy đơn hàng." });

        var thanhToans = await _db.Thanhtoans.AsNoTracking()
            .Where(t => t.Idonhangs.Any(d => d.Idonhang == id))
            .OrderByDescending(t => t.Thoigianthanhtoan)
            .Select(t => new
            {
                id = t.Idthanhtoan,
                soTien = t.Sotien,
                hinhThuc = t.Hinhthuc,
                trangThai = t.Trangthai,
                thoiGian = t.Thoigianthanhtoan,
                maGiaoDich = t.Mamgiaodich
            })
            .ToListAsync();

        var flagDaThanhToan = await _db.Donhangs.AsNoTracking()
            .Where(d => d.Idonhang == id)
            .Select(d => d.Trangthaithanhtoan == DonHangStatus.DaThanhToan)
            .FirstAsync();
        var coGiaoDichThanhCong = await _db.Thanhtoans.AsNoTracking()
            .Where(t => t.Idonhangs.Any(d => d.Idonhang == id))
            .AnyAsync(t => t.Trangthai == DonHangStatus.ThanhToanHoanTat);
        var daThanhToan = flagDaThanhToan || coGiaoDichThanhCong;

        return Ok(new { idDonHang = id, trangThaiThanhToan = daThanhToan ? "dathanhtoan" : "chuathanhtoan", lichSu = thanhToans });
    }

    // POST /api/donhang/{id}/thanh-toan — quản lý ghi nhận thanh toán một đơn.
    [HttpPost("api/donhang/{id:int}/thanh-toan")]
    public async Task<IActionResult> ThanhToanDon(int id, [FromBody] ThanhToanDonRequest? request)
    {
        var hinhThuc = request?.HinhThuc?.Trim().ToLowerInvariant();
        if (hinhThuc is not ("tienmat" or "chuyenkhoan" or "vidientu"))
            return BadRequest(new { loi = "Hình thức thanh toán không hợp lệ." });

        await using var transaction = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        try
        {
            var don = await _db.Donhangs
                .Include(d => d.Idthanhtoans)
                .FirstOrDefaultAsync(d => d.Idonhang == id);
            if (don == null) return NotFound(new { loi = "Không tìm thấy đơn hàng." });
            if (DonHangStatus.IsCancelled(don.Trangthai))
                return Conflict(new { loi = "Đơn đã bị hủy, không thể thanh toán." });

            var daCoGiaoDichThanhCong = don.Idthanhtoans.Any(t => t.Trangthai == DonHangStatus.ThanhToanHoanTat);
            if (DonHangStatus.IsPaid(don.Trangthaithanhtoan) || daCoGiaoDichThanhCong)
                return Conflict(new { loi = "Đơn này đã được thanh toán. Không tạo giao dịch trùng." });

            var soTien = don.Tongtien ?? 0m;
            if (soTien <= 0m)
                return BadRequest(new { loi = "Tổng tiền của đơn phải lớn hơn 0 mới có thể thanh toán." });

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

            return Ok(new
            {
                ok = true,
                idDonHang = don.Idonhang,
                maDon = don.Madonhang,
                soTien,
                hinhThuc,
                trangThaiThanhToan = "daThanhToan",
                thoiGian = thanhToan.Thoigianthanhtoan
            });
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await transaction.RollbackAsync();
            _logger.LogWarning(ex, "Xung đột cập nhật khi thanh toán đơn {IdDonHang}", id);
            return Conflict(new { loi = "Đơn vừa được cập nhật ở nơi khác. Hãy tải lại danh sách trước khi thử lại." });
        }
        catch (DbUpdateException ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Không lưu được thanh toán cho đơn {IdDonHang}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { loi = "Không lưu được thanh toán. Kiểm tra log ứng dụng để xem lỗi SQL chi tiết." });
        }
    }

    // ============================================================
    // API SẢN PHẨM
    // Quy ước ảnh: cột hinhanh luôn lưu "images/menu/<ten-file>" (xem AnhMon),
    // API trả về "/images/menu/<ten-file>" để cả quản lý lẫn khách dùng chung một đường dẫn.
    // ============================================================

    private const int TenToiDa = 100;      // nvarchar(100)
    private const int LoaiToiDa = 50;      // nvarchar(50)
    private const int MotaToiDa = 1000;
    private const decimal GiaToiDa = 999_999_999_999m;   // decimal(12,0)

    [HttpGet("api/sanpham")]
    public async Task<IActionResult> GetSanPham()
    {
        var rows = await _db.Sanphams
            .AsNoTracking()
            .OrderBy(s => s.Loai).ThenBy(s => s.Tensanpham)
            .ToListAsync();

        var mons = rows.Select(s => new
        {
            id = s.Idsanpham,
            ten = s.Tensanpham,
            gia = s.Gia,
            loai = s.Loai,
            moTa = s.Mota,
            anh = AnhMon.Url(s.Hinhanh),
            conHang = s.Conhang ?? true
        });

        return Ok(mons);
    }

    [HttpPost("api/sanpham")]
    public async Task<IActionResult> ThemSanPham([FromForm] string? ten, [FromForm] decimal? gia,
        [FromForm] string? loai, [FromForm] string? mota, [FromForm] IFormFile? anh)
    {
        var loi = KiemTraThongTinMon(ten, gia, loai, mota);
        if (loi != null) return BadRequest(new { loi });

        string? duongDanAnh = null;
        if (anh != null && anh.Length > 0)
        {
            try
            {
                duongDanAnh = await LuuAnh(anh);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                _logger.LogError(ex, "Không ghi được file ảnh món {TenSanPham}", ten);
                return StatusCode(500, new { loi = "Không lưu được file ảnh trên máy chủ. Kiểm tra quyền ghi thư mục wwwroot/images/menu." });
            }
            if (duongDanAnh == null)
                return BadRequest(new { loi = "File ảnh không hợp lệ (chỉ JPG/PNG/WEBP dưới 2MB)." });
        }

        var sp = new Sanpham
        {
            Tensanpham = ten!.Trim(),
            Gia = Math.Round(gia!.Value, 0),
            Loai = string.IsNullOrWhiteSpace(loai) ? null : loai.Trim(),
            Mota = string.IsNullOrWhiteSpace(mota) ? null : mota.Trim(),
            Hinhanh = duongDanAnh,
            Conhang = true
        };
        _db.Sanphams.Add(sp);
        try
        {
            await _db.SaveChangesAsync();
            return Ok(new { id = sp.Idsanpham, anh = AnhMon.Url(sp.Hinhanh) });
        }
        catch (DbUpdateException ex)
        {
            XoaFileAnhVuaTao(duongDanAnh);
            _logger.LogError(ex, "Không thêm được sản phẩm {TenSanPham}", ten);
            return StatusCode(500, new { loi = "Không lưu được món mới vào cơ sở dữ liệu. Xem log ứng dụng để biết lỗi SQL chi tiết." });
        }
    }

    [HttpPut("api/sanpham/{id:int}")]
    public async Task<IActionResult> SuaSanPham(int id, [FromForm] string? ten, [FromForm] decimal? gia,
        [FromForm] string? loai, [FromForm] string? mota, [FromForm] IFormFile? anh, [FromForm] bool xoaAnh = false)
    {
        var sp = await _db.Sanphams.FindAsync(id);
        if (sp == null) return NotFound(new { loi = "Không tìm thấy món." });

        var loi = KiemTraThongTinMon(ten, gia, loai, mota);
        if (loi != null) return BadRequest(new { loi });

        string? anhMoi = null;
        if (anh != null && anh.Length > 0)
        {
            try
            {
                anhMoi = await LuuAnh(anh);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                _logger.LogError(ex, "Không ghi được file ảnh món {IdSanPham}", id);
                return StatusCode(500, new { loi = "Không lưu được file ảnh trên máy chủ. Kiểm tra quyền ghi thư mục wwwroot/images/menu." });
            }
            if (anhMoi == null)
                return BadRequest(new { loi = "File ảnh không hợp lệ (chỉ JPG/PNG/WEBP dưới 2MB)." });
        }

        sp.Tensanpham = ten!.Trim();
        sp.Gia = Math.Round(gia!.Value, 0);
        sp.Loai = string.IsNullOrWhiteSpace(loai) ? null : loai.Trim();
        // Chỉ ghi đè mô tả khi form có gửi trường này (tránh xóa mô tả khi client cũ không gửi).
        if (Request.HasFormContentType && Request.Form.ContainsKey("mota"))
            sp.Mota = string.IsNullOrWhiteSpace(mota) ? null : mota.Trim();

        if (anhMoi != null)
            sp.Hinhanh = anhMoi;
        else if (xoaAnh)
            sp.Hinhanh = null;

        try
        {
            await _db.SaveChangesAsync();
            return Ok(new { ok = true, anh = AnhMon.Url(sp.Hinhanh) });
        }
        catch (DbUpdateException ex)
        {
            XoaFileAnhVuaTao(anhMoi);
            _logger.LogError(ex, "Không sửa được sản phẩm {IdSanPham}", id);
            return StatusCode(500, new { loi = "Không lưu được thay đổi sản phẩm. Xem log ứng dụng để biết lỗi SQL chi tiết." });
        }
    }

    [HttpDelete("api/sanpham/{id:int}")]
    public async Task<IActionResult> XoaSanPham(int id)
    {
        var sp = await _db.Sanphams.FindAsync(id);
        if (sp == null) return NotFound(new { loi = "Không tìm thấy món." });
        _db.Sanphams.Remove(sp);
        try
        {
            await _db.SaveChangesAsync();
            return Ok(new { ok = true });
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Không xóa được sản phẩm {IdSanPham}", id);
            return Conflict(new { loi = "Không thể xóa món đang được sử dụng trong đơn hàng hoặc công thức." });
        }
    }

    /// <summary>Kiểm tra dữ liệu nhập; trả về câu báo lỗi, hoặc null nếu hợp lệ.</summary>
    private static string? KiemTraThongTinMon(string? ten, decimal? gia, string? loai, string? mota)
    {
        if (string.IsNullOrWhiteSpace(ten)) return "Tên món không được để trống.";
        if (ten.Trim().Length > TenToiDa) return $"Tên món tối đa {TenToiDa} ký tự.";
        if (gia == null) return "Vui lòng nhập giá món.";
        if (gia < 0) return "Giá không được âm.";
        if (gia > GiaToiDa) return "Giá món quá lớn.";
        if (loai != null && loai.Trim().Length > LoaiToiDa) return $"Loại món tối đa {LoaiToiDa} ký tự.";
        if (mota != null && mota.Trim().Length > MotaToiDa) return $"Mô tả tối đa {MotaToiDa} ký tự.";
        return null;
    }

    // ============================================================
    // API SALE THEO NGÀY
    // ============================================================

    [HttpGet("api/sale")]
    public async Task<IActionResult> GetSale()
    {
        var now = DateTime.Now;
        var rows = await _db.SaleTheoNgays
            .AsNoTracking()
            .Include(s => s.IdsanphamNavigation)
            .OrderByDescending(s => s.Thoigiantao)
            .ToListAsync();

        var result = rows.Select(s =>
        {
            // Tự động tính trạng thái theo thời gian thực
            string trangThai;
            if (s.Trangthai == "vo_hieu") trangThai = "vo_hieu";
            else if (now < s.ThoiGianBatDau) trangThai = "chua_bat_dau";
            else if (now > s.ThoiGianKetThuc) trangThai = "da_ket_thuc";
            else trangThai = "dang_chay";

            return new
            {
                id = s.IdsaleTheoNgay,
                idSanPham = s.Idsanpham,
                tenSanPham = s.IdsanphamNavigation.Tensanpham,
                anhSanPham = AnhMon.Url(s.IdsanphamNavigation.Hinhanh),
                giaSanPham = s.IdsanphamNavigation.Gia,
                phanTramGiam = s.PhanTramGiam,
                giaSauGiam = Math.Round(s.IdsanphamNavigation.Gia * (1 - s.PhanTramGiam / 100), 0),
                thoiGianBatDau = s.ThoiGianBatDau,
                thoiGianKetThuc = s.ThoiGianKetThuc,
                trangThai,
                thoiGianTao = s.Thoigiantao
            };
        });

        return Ok(result);
    }

    [HttpPost("api/sale")]
    public async Task<IActionResult> ThemSale([FromBody] SaleRequest? req)
    {
        var loi = KiemTraSaleRequest(req);
        if (loi != null) return BadRequest(new { loi });

        var sp = await _db.Sanphams.FindAsync(req!.IdSanPham);
        if (sp == null) return NotFound(new { loi = "Không tìm thấy sản phẩm." });

        var sale = new SaleTheoNgay
        {
            Idsanpham = req.IdSanPham,
            PhanTramGiam = req.PhanTramGiam,
            ThoiGianBatDau = req.ThoiGianBatDau,
            ThoiGianKetThuc = req.ThoiGianKetThuc,
            Thoigiantao = DateTime.Now
        };
        _db.SaleTheoNgays.Add(sale);
        try
        {
            await _db.SaveChangesAsync();
            return Ok(new { id = sale.IdsaleTheoNgay, ok = true });
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Không thêm được sale mới");
            return StatusCode(500, new { loi = "Không lưu được sale. Xem log ứng dụng." });
        }
    }

    [HttpPut("api/sale/{id:int}")]
    public async Task<IActionResult> SuaSale(int id, [FromBody] SaleRequest? req)
    {
        var loi = KiemTraSaleRequest(req);
        if (loi != null) return BadRequest(new { loi });

        var sale = await _db.SaleTheoNgays.FindAsync(id);
        if (sale == null) return NotFound(new { loi = "Không tìm thấy sale." });

        var sp = await _db.Sanphams.FindAsync(req!.IdSanPham);
        if (sp == null) return NotFound(new { loi = "Không tìm thấy sản phẩm." });

        sale.Idsanpham = req.IdSanPham;
        sale.PhanTramGiam = req.PhanTramGiam;
        sale.ThoiGianBatDau = req.ThoiGianBatDau;
        sale.ThoiGianKetThuc = req.ThoiGianKetThuc;

        try
        {
            await _db.SaveChangesAsync();
            return Ok(new { ok = true });
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Không sửa được sale {IdSale}", id);
            return StatusCode(500, new { loi = "Không lưu được thay đổi. Xem log ứng dụng." });
        }
    }

    [HttpDelete("api/sale/{id:int}")]
    public async Task<IActionResult> XoaSale(int id)
    {
        var sale = await _db.SaleTheoNgays.FindAsync(id);
        if (sale == null) return NotFound(new { loi = "Không tìm thấy sale." });
        _db.SaleTheoNgays.Remove(sale);
        try
        {
            await _db.SaveChangesAsync();
            return Ok(new { ok = true });
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Không xóa được sale {IdSale}", id);
            return StatusCode(500, new { loi = "Không xóa được sale." });
        }
    }

    private static string? KiemTraSaleRequest(SaleRequest? req)
    {
        if (req == null) return "Dữ liệu không hợp lệ.";
        if (req.IdSanPham <= 0) return "Chưa chọn sản phẩm.";
        if (req.PhanTramGiam <= 0 || req.PhanTramGiam > 100) return "Phần trăm giảm phải từ 1 đến 100.";
        if (req.ThoiGianBatDau >= req.ThoiGianKetThuc)
            return "Thời gian bắt đầu phải trước thời gian kết thúc.";
        return null;
    }

    private async Task<IActionResult> LuuTrangThaiDonAsync(Donhang don, string thaoTac)
    {
        try
        {
            await _db.SaveChangesAsync();
            return Ok(new { ok = true, id = don.Idonhang, maDon = don.Madonhang, trangThai = DonHangStatus.ToApiStatus(don.Trangthai) });
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Xung đột dữ liệu khi {ThaoTac} đơn {IdDonHang}", thaoTac, don.Idonhang);
            return Conflict(new { loi = "Đơn vừa được cập nhật ở nơi khác. Danh sách đã thay đổi, vui lòng tải lại." });
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Lỗi SQL khi {ThaoTac} đơn {IdDonHang}", thaoTac, don.Idonhang);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { loi = "Không thể lưu trạng thái đơn vào SQL Server. Kiểm tra log ứng dụng để xem lỗi chi tiết." });
        }
    }

    /// <summary>Thư mục vật lý chứa ảnh món: &lt;wwwroot&gt;/images/menu (không phụ thuộc thư mục chạy lệnh).</summary>
    private string ThuMucAnhMon()
    {
        var webRoot = _env.WebRootPath;
        if (string.IsNullOrEmpty(webRoot)) webRoot = Path.Combine(_env.ContentRootPath, "wwwroot");
        return Path.Combine(webRoot, "images", "menu");
    }

    /// <summary>
    /// Lưu file ảnh vào wwwroot/images/menu và trả về giá trị để ghi vào cột hinhanh
    /// ("images/menu/&lt;guid&gt;.jpg"). Trả về null nếu file không hợp lệ.
    /// </summary>
    private async Task<string?> LuuAnh(IFormFile file)
    {
        var dinhDangHopLe = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!dinhDangHopLe.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase)
            || file.Length == 0 || file.Length > 2 * 1024 * 1024) return null;
        var duoiFile = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (duoiFile is not (".jpg" or ".jpeg" or ".png" or ".webp")) return null;

        var thuMuc = ThuMucAnhMon();
        Directory.CreateDirectory(thuMuc);

        var tenFile = Guid.NewGuid().ToString("N") + duoiFile;
        await using (var stream = new FileStream(Path.Combine(thuMuc, tenFile), FileMode.CreateNew))
        {
            await file.CopyToAsync(stream);
        }
        return AnhMon.ThuMuc + "/" + tenFile;
    }

    /// <summary>Xóa file ảnh vừa lưu khi bước ghi database thất bại, để không để lại file mồ côi.</summary>
    private void XoaFileAnhVuaTao(string? duongDanDb)
    {
        if (string.IsNullOrEmpty(duongDanDb)) return;
        try
        {
            var duongDan = Path.Combine(ThuMucAnhMon(), Path.GetFileName(duongDanDb));
            if (System.IO.File.Exists(duongDan)) System.IO.File.Delete(duongDan);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logger.LogWarning(ex, "Không xóa được file ảnh tạm {DuongDan}", duongDanDb);
        }
    }
}

public sealed record ThanhToanDonRequest(string? HinhThuc);

public sealed record SaleRequest(
    int IdSanPham,
    decimal PhanTramGiam,
    DateTime ThoiGianBatDau,
    DateTime ThoiGianKetThuc
);

// ============================================================
// API SALE THEO NGÀY
// ============================================================
// GET  /api/sale          -> danh sách tất cả sale
// POST /api/sale          -> tạo sale mới
// PUT  /api/sale/{id}     -> cập nhật sale
// DELETE /api/sale/{id}   -> xóa sale
