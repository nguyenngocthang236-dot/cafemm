using System.Globalization;
using System.Text.Json;

namespace QuanCafe.Helpers;

/// <summary>Tên các khoá lưu trong Session (dùng chung cho mọi controller).</summary>
public static class SessionKeys
{
    public const string IdPhien = "IdPhien";
    public const string IdBan = "IdBan";
    public const string MaBan = "MaBan";
    public const string GioHang = "GioHang";
}

/// <summary>Lưu / đọc object (dạng JSON) trong Session.</summary>
public static class SessionJsonExtensions
{
    public static void SetObject<T>(this ISession session, string key, T value) =>
        session.SetString(key, JsonSerializer.Serialize(value));

    public static T? GetObject<T>(this ISession session, string key)
    {
        var json = session.GetString(key);
        return json == null ? default : JsonSerializer.Deserialize<T>(json);
    }
}

/// <summary>Hàm hỗ trợ hiển thị trong View.</summary>
public static class HienThiExtensions
{
    /// <summary>25000 -> "25.000đ" (không phụ thuộc cấu hình ngôn ngữ của máy chủ).</summary>
    public static string Tien(this decimal so) =>
        so.ToString("N0", CultureInfo.InvariantCulture).Replace(',', '.') + "đ";

    public static string Tien(this decimal? so) => (so ?? 0m).Tien();

    /// <summary>
    /// Đường dẫn ảnh món để đưa vào Url.Content(...) ở giao diện khách.
    /// Dùng chung quy ước với giao diện quản lý (xem <see cref="AnhMon"/>); để trống thì dùng ảnh mặc định.
    /// </summary>
    public static string DuongDanAnh(this string? hinhanh) => AnhMon.DuongDanView(hinhanh);
}

/// <summary>
/// MỘT quy ước duy nhất cho ảnh món, dùng ở cả giao diện quản lý lẫn giao diện khách.
/// Cột <c>sanpham.hinhanh</c> lưu dạng "images/menu/ten-file.jpg" (tính từ wwwroot).
/// Hàm vẫn đọc được dữ liệu cũ/lệch chuẩn: chỉ có tên file ("abc.jpg"),
/// có dấu "/" hoặc "~/" ở đầu, hoặc dùng dấu "\".
/// </summary>
public static class AnhMon
{
    /// <summary>Thư mục chứa ảnh món, tính từ wwwroot.</summary>
    public const string ThuMuc = "images/menu";

    /// <summary>Đưa giá trị bất kỳ về dạng lưu trong DB: "images/menu/ten-file.jpg". Rỗng -> null.</summary>
    public static string? ChuanHoa(string? hinhanh)
    {
        if (string.IsNullOrWhiteSpace(hinhanh)) return null;

        var s = hinhanh.Trim().Replace('\\', '/');
        if (s.Contains("://", StringComparison.Ordinal)) return s;   // link ngoài: giữ nguyên

        s = s.TrimStart('~').TrimStart('/');
        if (s.Length == 0) return null;

        // Chỉ có tên file (ảnh thêm từ trang quản lý theo kiểu cũ) -> gắn thư mục ảnh món vào.
        return s.Contains('/') ? s : ThuMuc + "/" + s;
    }

    /// <summary>Địa chỉ ảnh để trình duyệt tải, ví dụ "/images/menu/abc.jpg". Rỗng -> null.</summary>
    public static string? Url(string? hinhanh)
    {
        var s = ChuanHoa(hinhanh);
        if (s == null) return null;
        return s.Contains("://", StringComparison.Ordinal) ? s : "/" + s;
    }

    /// <summary>Dạng "~/images/menu/abc.jpg" để đưa vào Url.Content(...) trong Razor; rỗng -> ảnh mặc định.</summary>
    public static string DuongDanView(string? hinhanh)
    {
        var s = ChuanHoa(hinhanh);
        if (s == null) return "~/images/placeholder.svg";
        return s.Contains("://", StringComparison.Ordinal) ? s : "~/" + s;
    }
}
