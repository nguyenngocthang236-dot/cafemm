-- Migration: Tạo bảng sale_theo_ngay
-- Chạy trên SQL Server (QuanCafe database)

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'sale_theo_ngay')
BEGIN
    CREATE TABLE sale_theo_ngay (
        idsale_theo_ngay    INT             IDENTITY(1,1)   NOT NULL,
        idsanpham           INT             NOT NULL,
        phantram_giam       DECIMAL(5,2)    NOT NULL,
        thoigian_batdau     DATETIME        NOT NULL,
        thoigian_ketthuc    DATETIME        NOT NULL,
        trangthai           VARCHAR(20)     NOT NULL DEFAULT 'dang_chay',
        thoigiantao         DATETIME        NOT NULL DEFAULT GETDATE(),

        CONSTRAINT PK__sale_theo_ngay__idsaleTheoNgay
            PRIMARY KEY CLUSTERED (idsale_theo_ngay),

        CONSTRAINT FK__sale_theo_ngay__idsanpham
            FOREIGN KEY (idsanpham) REFERENCES sanpham(idsanpham) ON DELETE CASCADE,

        CONSTRAINT CK__sale_phantram
            CHECK (phantram_giam > 0 AND phantram_giam <= 100),

        CONSTRAINT CK__sale_thoigian
            CHECK (thoigian_batdau < thoigian_ketthuc)
    );

    PRINT 'Đã tạo bảng sale_theo_ngay';
END
ELSE
BEGIN
    PRINT 'Bảng sale_theo_ngay đã tồn tại, bỏ qua.';
END
