using System;

namespace QuanCafe.Models;

/// <summary>Một tin nhắn trong khung chat hỗ trợ giữa khách (tại bàn) và quản lý.</summary>
public partial class Hotrotinnhan
{
    public int Idtinnhan { get; set; }

    /// <summary>Phiên sử dụng bàn đang mở. Hết phiên (khách thanh toán xong) thì khung chat của bàn đó trống lại.</summary>
    public int Idphien { get; set; }

    public int Idban { get; set; }

    /// <summary>"khach" hoặc "quanly".</summary>
    public string Nguoigui { get; set; } = null!;

    public string Noidung { get; set; } = null!;

    public DateTime Thoigian { get; set; }

    /// <summary>
    /// Phía bên kia đã đọc chưa. Tin của khách: quản lý đã mở khung chat của bàn đó chưa.
    /// Tin của quản lý: khách đã mở khung chat chưa.
    /// </summary>
    public bool Dadoc { get; set; }
}
