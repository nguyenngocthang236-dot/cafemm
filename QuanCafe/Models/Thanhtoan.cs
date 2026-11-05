using System;
using System.Collections.Generic;

namespace QuanCafe.Models;

public partial class Thanhtoan
{
    public int Idthanhtoan { get; set; }

    public int Idphien { get; set; }

    public decimal Sotien { get; set; }

    public string Hinhthuc { get; set; } = null!;

    public string? Trangthai { get; set; }

    public DateTime? Thoigianthanhtoan { get; set; }

    public string? Mamgiaodich { get; set; }

    public virtual Phiensudungban IdphienNavigation { get; set; } = null!;

    public virtual ICollection<Donhang> Idonhangs { get; set; } = new List<Donhang>();
}
