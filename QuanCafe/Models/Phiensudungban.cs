using System;
using System.Collections.Generic;

namespace QuanCafe.Models;

public partial class Phiensudungban
{
    public int Idphien { get; set; }

    public int Idban { get; set; }

    public DateTime? Thoigianbatdau { get; set; }

    public DateTime? Thoigianketthuc { get; set; }

    public string? Trangthai { get; set; }

    public virtual ICollection<Donhang> Donhangs { get; set; } = new List<Donhang>();

    public virtual Ban IdbanNavigation { get; set; } = null!;

    public virtual ICollection<Thanhtoan> Thanhtoans { get; set; } = new List<Thanhtoan>();
}
