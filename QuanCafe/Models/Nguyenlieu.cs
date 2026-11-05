using System;
using System.Collections.Generic;

namespace QuanCafe.Models;

public partial class Nguyenlieu
{
    public int Idnguyenlieu { get; set; }

    public string Tennguyenlieu { get; set; } = null!;

    public string Donvitinh { get; set; } = null!;

    public decimal? Soluongtonkho { get; set; }

    public decimal? Mucconhbao { get; set; }

    public decimal? Dongia { get; set; }

    public virtual ICollection<Congthucsanpham> Congthucsanphams { get; set; } = new List<Congthucsanpham>();

    public virtual ICollection<Lichsukho> Lichsukhos { get; set; } = new List<Lichsukho>();
}
