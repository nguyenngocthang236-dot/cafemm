namespace QuanCafe.Helpers;

/// <summary>
/// Bộ trạng thái dùng chung cho đơn hàng.
///
/// QUAN TRỌNG: các giá trị GHI vào cột donhang.trangthai phải nằm trong CHECK constraint của
/// database QuanCafe:  choxacnhan | danglam | sansanggiao | dagiao | huy
/// (ghi giá trị khác thì SQL Server từ chối với lỗi CHECK constraint).
/// Các giá trị "dangchuan", "hoantat", "dahuy" từng được code cũ dùng nên vẫn được ĐỌC (alias),
/// nhưng không bao giờ được ghi nữa.
/// </summary>
public static class DonHangStatus
{
    // --- Giá trị ghi vào donhang.trangthai (khớp CHECK constraint của database) ---
    public const string ChoXacNhan = "choxacnhan";
    public const string DangChuanBi = "danglam";
    public const string SanSangGiao = "sansanggiao";
    public const string HoanTat = "dagiao";
    public const string DaHuy = "huy";

    // --- Giá trị cũ, chỉ để đọc ---
    private const string DangChuanBiCu = "dangchuan";
    private const string HoanTatCu = "hoantat";
    private const string DaHuyCu = "dahuy";

    // --- donhang.trangthaithanhtoan (CHECK: chuathanhtoan | dathanhtoan) ---
    public const string ChoThanhToan = "chuathanhtoan";
    public const string DaThanhToan = "dathanhtoan";

    // --- thanhtoan.trangthai (CHECK: chothanhtoan | hoantat | thatbai) ---
    public const string ThanhToanHoanTat = "hoantat";

    public static bool IsCancelled(string? status) =>
        status is DaHuy or DaHuyCu;

    public static bool IsCompleted(string? status) =>
        status is HoanTat or HoanTatCu;

    public static bool IsPreparing(string? status) =>
        status is DangChuanBi or DangChuanBiCu;

    public static bool IsPaid(string? status) =>
        status == DaThanhToan;

    public static string ToApiStatus(string? status) => status switch
    {
        ChoXacNhan => "cho",
        DangChuanBi or DangChuanBiCu => "dangChuanBi",
        SanSangGiao => "sanSangGiao",
        HoanTat or HoanTatCu => "daHoanTat",
        DaHuy or DaHuyCu => "daHuy",
        _ => "khongXacDinh"
    };

    public static string ToDisplayStatus(string? status) => status switch
    {
        ChoXacNhan => "Đang chờ xác nhận",
        DangChuanBi or DangChuanBiCu => "Đang chuẩn bị",
        SanSangGiao => "Đã chuẩn bị xong, chờ giao",
        HoanTat or HoanTatCu => "Đã hoàn tất",
        DaHuy or DaHuyCu => "Đã hủy",
        _ => "Không xác định"
    };
}
