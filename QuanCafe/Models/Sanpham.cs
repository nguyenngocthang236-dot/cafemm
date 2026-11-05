using System;
using System.Collections.Generic;

namespace QuanCafe.Models;

public partial class Sanpham
{
    public int Idsanpham { get; set; }

    public string Tensanpham { get; set; } = null!;

    public string? Loai { get; set; }

    public decimal Gia { get; set; }

    public string? Hinhanh { get; set; }

    public bool? Conhang { get; set; }

    public string? Mota { get; set; }

    public virtual ICollection<Chitietdon> Chitietdons { get; set; } = new List<Chitietdon>();

    public virtual ICollection<Congthucsanpham> Congthucsanphams { get; set; } = new List<Congthucsanpham>();
}
