using Microsoft.EntityFrameworkCore;
using QuanCafe.Models;

namespace QuanCafe.Helpers;

/// <summary>
/// Tồn kho theo từng món (cột sanpham.soluongton).
/// Cộng/trừ kho luôn làm bằng MỘT câu UPDATE có điều kiện ngay trong SQL Server,
/// nên nhiều khách đặt cùng lúc cũng không thể làm tồn kho bị âm (bán lố).
/// </summary>
public static class KhoHang
{
    /// <summary>Tồn kho tối đa của một món (chặn nhập nhầm số quá lớn).</summary>
    public const int ToiDa = 99_999;

    /// <summary>Còn từng này trở xuống thì báo "sắp hết" (khớp SAP_HET trong wwwroot/js/dashboard.js).</summary>
    public const int SapHet = 5;

    /// <summary>
    /// Cộng (thayDoi &gt; 0) hoặc trừ (thayDoi &lt; 0) tồn kho của một món, đồng thời cập nhật cột conhang.
    /// Trả về false nếu không làm được: món không tồn tại, hoặc kết quả sẽ &lt; 0 hay &gt; <see cref="ToiDa"/>.
    /// Nếu đang có giao dịch (BeginTransaction) thì câu lệnh chạy trong giao dịch đó.
    /// </summary>
    public static async Task<bool> ThayDoiAsync(AppDbContext db, int idSanPham, int thayDoi, CancellationToken ct = default)
    {
        var soDong = await db.Database.ExecuteSqlInterpolatedAsync($@"
UPDATE sanpham
SET soluongton = soluongton + {thayDoi},
    conhang = CASE WHEN soluongton + {thayDoi} > 0 THEN 1 ELSE 0 END
WHERE idsanpham = {idSanPham}
  AND soluongton + {thayDoi} >= 0
  AND soluongton + {thayDoi} <= {ToiDa}", ct);

        return soDong > 0;
    }
}
