using System;
using System.Collections.Generic;

namespace QuanCafe.Models;

public partial class Congthucsanpham
{
    public int Idsanpham { get; set; }

    public int Idnguyenlieu { get; set; }

    public decimal Soluongcanthiet { get; set; }

    public virtual Nguyenlieu IdnguyenlieuNavigation { get; set; } = null!;

    public virtual Sanpham IdsanphamNavigation { get; set; } = null!;
}
