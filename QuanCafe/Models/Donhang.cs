using System;
using System.Collections.Generic;

namespace QuanCafe.Models;

public partial class Donhang
{
    public int Idonhang { get; set; }

    public string Madonhang { get; set; } = null!;

    public int Idphien { get; set; }

    public int Idban { get; set; }

    public string? Trangthai { get; set; }

    public string? Trangthaithanhtoan { get; set; }

    public DateTime? Thoigiandat { get; set; }

    public decimal? Tongtien { get; set; }

    public string? Ghichu { get; set; }

    public virtual ICollection<Chitietdon> Chitietdons { get; set; } = new List<Chitietdon>();

    public virtual Ban IdbanNavigation { get; set; } = null!;

    public virtual Phiensudungban IdphienNavigation { get; set; } = null!;

    public virtual ICollection<Lichsukho> Lichsukhos { get; set; } = new List<Lichsukho>();

    public virtual ICollection<Thanhtoan> Idthanhtoans { get; set; } = new List<Thanhtoan>();
}
