-- Migration: thêm tồn kho theo từng món (cột sanpham.soluongton)
-- Chạy trên SQL Server (database QuanCafe) TRƯỚC khi chạy ứng dụng bản mới.
-- Chạy lại nhiều lần vẫn an toàn: không ghi đè số lượng đã có.

IF COL_LENGTH('sanpham', 'soluongton') IS NULL
BEGIN
    ALTER TABLE sanpham
        ADD soluongton INT NOT NULL
            CONSTRAINT DF__sanpham__soluongton DEFAULT 0
            CONSTRAINT CK__sanpham__soluongton CHECK (soluongton >= 0);

    -- Chỉ chạy đúng một lần, lúc vừa tạo cột:
    -- món đang bán (conhang = 1) cho tồn kho khởi điểm 50, món đang tạm hết giữ 0.
    -- Sau đó chỉnh số thật ở Dashboard > "Kiểm tra kho hàng".
    EXEC('UPDATE sanpham SET soluongton = CASE WHEN ISNULL(conhang, 1) = 1 THEN 50 ELSE 0 END');

    PRINT N'Đã thêm cột sanpham.soluongton (tồn kho khởi điểm 50 cho món đang bán).';
END
ELSE
BEGIN
    PRINT N'Cột sanpham.soluongton đã tồn tại, bỏ qua.';
END
