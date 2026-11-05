using System;
using System.Collections.Generic;

namespace QuanCafe.Models;

public partial class Nhanvien
{
    public int Idnhanvien { get; set; }

    public string Hoten { get; set; } = null!;

    public string Vaitro { get; set; } = null!;

    public string? Sodienthoai { get; set; }

    public string? Trangthai { get; set; }

    public virtual ICollection<Lichsukho> Lichsukhos { get; set; } = new List<Lichsukho>();
}
