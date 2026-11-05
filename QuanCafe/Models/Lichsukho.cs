using System;
using System.Collections.Generic;

namespace QuanCafe.Models;

public partial class Lichsukho
{
    public int Idlichsu { get; set; }

    public int Idnguyenlieu { get; set; }

    public string Loai { get; set; } = null!;

    public decimal Soluong { get; set; }

    public int? IdonhangLienQuan { get; set; }

    public int? Idnhanvien { get; set; }

    public DateTime? Thoigiantao { get; set; }

    public virtual Nguyenlieu IdnguyenlieuNavigation { get; set; } = null!;

    public virtual Nhanvien? IdnhanvienNavigation { get; set; }

    public virtual Donhang? IdonhangLienQuanNavigation { get; set; }
}
