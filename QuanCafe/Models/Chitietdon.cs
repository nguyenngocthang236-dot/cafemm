using System;
using System.Collections.Generic;

namespace QuanCafe.Models;

public partial class Chitietdon
{
    public int Idchitiet { get; set; }

    public int Idonhang { get; set; }

    public int Idsanpham { get; set; }

    public int Soluong { get; set; }

    public decimal Dongia { get; set; }

    public string? Trangthai { get; set; }

    public string? Ghichu { get; set; }

    public virtual Donhang IdonhangNavigation { get; set; } = null!;

    public virtual Sanpham IdsanphamNavigation { get; set; } = null!;
}
