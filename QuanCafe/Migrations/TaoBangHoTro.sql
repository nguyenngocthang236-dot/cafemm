-- Migration: Tạo bảng hotro_tinnhan (tin nhắn chat hỗ trợ giữa khách và quản lý)
-- Chạy trên SQL Server (QuanCafe database), cùng cách với AddSaleTheoNgay.sql
-- ĐẶT VÀO: Migrations/TaoBangHoTro.sql  (chỉ để lưu lại, chạy bằng SSMS / Azure Data Studio)

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'hotro_tinnhan')
BEGIN
    CREATE TABLE hotro_tinnhan (
        idtinnhan   INT             IDENTITY(1,1)   NOT NULL,
        idphien     INT             NOT NULL,       -- phiên sử dụng bàn đang mở (mỗi lượt khách ngồi bàn = 1 phiên)
        idban       INT             NOT NULL,
        nguoigui    VARCHAR(10)     NOT NULL,       -- 'khach' hoặc 'quanly'
        noidung     NVARCHAR(500)   NOT NULL,
        thoigian    DATETIME        NOT NULL CONSTRAINT DF__hotro_tinnhan__thoigian DEFAULT GETDATE(),
        dadoc       BIT             NOT NULL CONSTRAINT DF__hotro_tinnhan__dadoc DEFAULT 0,
                                                    -- tin của khách: quản lý đã mở xem chưa
                                                    -- tin của quản lý: khách đã mở khung chat xem chưa

        CONSTRAINT PK__hotro_tinnhan
            PRIMARY KEY CLUSTERED (idtinnhan),

        CONSTRAINT FK__hotro_tinnhan__idphien
            FOREIGN KEY (idphien) REFERENCES phiensudungban(idphien),

        CONSTRAINT FK__hotro_tinnhan__idban
            FOREIGN KEY (idban) REFERENCES ban(idban),

        CONSTRAINT CK__hotro_tinnhan__nguoigui
            CHECK (nguoigui IN ('khach', 'quanly'))
    );

    -- Lấy tin của một phiên theo thứ tự (khung chat)
    CREATE INDEX IX_hotro_tinnhan_phien   ON hotro_tinnhan (idphien, idtinnhan);
    -- Đếm tin chưa đọc theo từng bàn (số đỏ trên Dashboard)
    CREATE INDEX IX_hotro_tinnhan_chuadoc ON hotro_tinnhan (idban, nguoigui, dadoc);

    PRINT 'Đã tạo bảng hotro_tinnhan';
END
ELSE
BEGIN
    PRINT 'Bảng hotro_tinnhan đã tồn tại, bỏ qua.';
END
