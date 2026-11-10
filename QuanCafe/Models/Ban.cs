using System;
using System.Collections.Generic;

namespace QuanCafe.Models;

public partial class Ban
{
    public int Idban { get; set; }

    public string Maban { get; set; } = null!;

    public string Qrcode { get; set; } = null!;

    public int? Succhua { get; set; }

    public string? Trangthai { get; set; }

    public DateTime? Thoigiantao { get; set; }

    public virtual ICollection<Donhang> Donhangs { get; set; } = new List<Donhang>();

    public virtual ICollection<Phiensudungban> Phiensudungbans { get; set; } = new List<Phiensudungban>();
}
