using System;
using System.Collections.Generic;

namespace QuanCafe.ViewModels;

/// <summary>ViewModel cho trang xem doanh thu theo ngày.</summary>
public class RevenueViewModel
{
    public DateTime NgayXem { get; set; }

    public List<DonHangDoanhThuDto> DanhSachDon { get; set; } = new();

    public int TongSoDon { get; set; }

    public decimal TongDoanhThu { get; set; }
}

/// <summary>Thông tin 1 đơn hàng, hiển thị trong bảng doanh thu.</summary>
public class DonHangDoanhThuDto
{
    public string MaDonHang { get; set; } = null!;

    public string MaBan { get; set; } = null!;

    public DateTime ThoiGianDat { get; set; }

    public List<MonDaDatDto> DanhSachMon { get; set; } = new();

    public decimal TongTienDon { get; set; }
}

/// <summary>Một món trong đơn — tên món + số lượng đặt.</summary>
public class MonDaDatDto
{
    public string TenSanPham { get; set; } = null!;

    public int SoLuong { get; set; }

    public decimal DonGia { get; set; }

    public decimal ThanhTien => SoLuong * DonGia;
}