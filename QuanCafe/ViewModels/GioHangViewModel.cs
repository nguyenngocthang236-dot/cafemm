namespace QuanCafe.ViewModels;

/// <summary>Một dòng trong giỏ hàng khi lưu trong Session: chỉ nhớ món nào, mấy ly.</summary>
public class GioHangItem
{
    public int Idsanpham { get; set; }
    public int Soluong { get; set; }
}

/// <summary>Một dòng giỏ hàng để hiển thị (đã ghép tên, ảnh, giá lấy từ database).</summary>
public class GioHangDong
{
    public int Idsanpham { get; set; }
    public string Tensanpham { get; set; } = "";
    public string? Hinhanh { get; set; }
    public decimal Gia { get; set; }
    public int Soluong { get; set; }

    public decimal ThanhTien => Gia * Soluong;
}

public class GioHangViewModel
{
    public List<GioHangDong> Dongs { get; set; } = new();

    public int TongSoLuong => Dongs.Sum(d => d.Soluong);
    public decimal TongTien => Dongs.Sum(d => d.ThanhTien);
}
