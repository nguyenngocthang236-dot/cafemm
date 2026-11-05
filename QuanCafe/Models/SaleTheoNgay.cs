using System;

namespace QuanCafe.Models;

public class SaleTheoNgay
{
    public int IdsaleTheoNgay { get; set; }

    public int Idsanpham { get; set; }

    /// <summary>Phần trăm giảm giá (0–100).</summary>
    public decimal PhanTramGiam { get; set; }

    public DateTime ThoiGianBatDau { get; set; }

    public DateTime ThoiGianKetThuc { get; set; }

    /// <summary>Trạng thái: "dang_chay", "chua_bat_dau", "da_ket_thuc", "vo_hieu".</summary>
    public string Trangthai { get; set; } = "dang_chay";

    public DateTime Thoigiantao { get; set; } = DateTime.Now;

    public virtual Sanpham IdsanphamNavigation { get; set; } = null!;
}
