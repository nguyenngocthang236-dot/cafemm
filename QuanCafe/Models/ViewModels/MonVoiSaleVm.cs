namespace QuanCafe.Models.ViewModels;

/// <summary>
/// Thông tin một món kèm thông tin sale đang chạy (nếu có).
/// </summary>
public class MonVoiSaleVm
{
    public Sanpham Mon { get; set; } = null!;

    /// <summary>Phần trăm giảm (0 nếu không có sale).</summary>
    public decimal PhanTramGiam { get; set; }

    /// <summary>Giá sau khi áp sale. Bằng Gia gốc nếu không có sale.</summary>
    public decimal GiaSale => PhanTramGiam > 0
        ? Math.Round(Mon.Gia * (1 - PhanTramGiam / 100))
        : Mon.Gia;

    public bool DangSale => PhanTramGiam > 0;
}
