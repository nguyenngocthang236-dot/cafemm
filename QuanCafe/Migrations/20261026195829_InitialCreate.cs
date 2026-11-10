using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanCafe.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ban",
                columns: table => new
                {
                    idban = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    maban = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    qrcode = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    succhua = table.Column<int>(type: "int", nullable: true, defaultValue: 4),
                    trangthai = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true, defaultValue: "trong"),
                    thoigiantao = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ban__0088724A5206CD6C", x => x.idban);
                });

            migrationBuilder.CreateTable(
                name: "nguyenlieu",
                columns: table => new
                {
                    idnguyenlieu = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    tennguyenlieu = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    donvitinh = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    soluongtonkho = table.Column<decimal>(type: "decimal(12,2)", nullable: true, defaultValue: 0m),
                    mucconhbao = table.Column<decimal>(type: "decimal(12,2)", nullable: true, defaultValue: 0m),
                    dongia = table.Column<decimal>(type: "decimal(12,0)", nullable: true, defaultValue: 0m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__nguyenli__658B66A3817D547E", x => x.idnguyenlieu);
                });

            migrationBuilder.CreateTable(
                name: "nhanvien",
                columns: table => new
                {
                    idnhanvien = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    hoten = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    vaitro = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    sodienthoai = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: true),
                    trangthai = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true, defaultValue: "dangdilam")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__nhanvien__A703FF0170806EC9", x => x.idnhanvien);
                });

            migrationBuilder.CreateTable(
                name: "sanpham",
                columns: table => new
                {
                    idsanpham = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    tensanpham = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    loai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    gia = table.Column<decimal>(type: "decimal(12,0)", nullable: false),
                    hinhanh = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    conhang = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    mota = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__sanpham__C5FDB0E5F66BDDA4", x => x.idsanpham);
                });

            migrationBuilder.CreateTable(
                name: "phiensudungban",
                columns: table => new
                {
                    idphien = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    idban = table.Column<int>(type: "int", nullable: false),
                    thoigianbatdau = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    thoigianketthuc = table.Column<DateTime>(type: "datetime", nullable: true),
                    trangthai = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true, defaultValue: "dangmo")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__phienban__3CE13535454AA4A5", x => x.idphien);
                    table.ForeignKey(
                        name: "FK__phienban__idban__5441852A",
                        column: x => x.idban,
                        principalTable: "ban",
                        principalColumn: "idban");
                });

            migrationBuilder.CreateTable(
                name: "congthucsanpham",
                columns: table => new
                {
                    idsanpham = table.Column<int>(type: "int", nullable: false),
                    idnguyenlieu = table.Column<int>(type: "int", nullable: false),
                    soluongcanthiet = table.Column<decimal>(type: "decimal(12,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__congthuc__E3A5068F3DCCD5AC", x => new { x.idsanpham, x.idnguyenlieu });
                    table.ForeignKey(
                        name: "FK__congthucs__idngu__6477ECF3",
                        column: x => x.idnguyenlieu,
                        principalTable: "nguyenlieu",
                        principalColumn: "idnguyenlieu");
                    table.ForeignKey(
                        name: "FK__congthucs__idsan__6383C8BA",
                        column: x => x.idsanpham,
                        principalTable: "sanpham",
                        principalColumn: "idsanpham");
                });

            migrationBuilder.CreateTable(
                name: "donhang",
                columns: table => new
                {
                    idonhang = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    madonhang = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    idphien = table.Column<int>(type: "int", nullable: false),
                    idban = table.Column<int>(type: "int", nullable: false),
                    trangthai = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true, defaultValue: "choxacnhan"),
                    trangthaithanhtoan = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true, defaultValue: "chuathanhtoan"),
                    thoigiandat = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    tongtien = table.Column<decimal>(type: "decimal(12,0)", nullable: true, defaultValue: 0m),
                    ghichu = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__donhang__78D9EE9F2B843579", x => x.idonhang);
                    table.ForeignKey(
                        name: "FK__donhang__idban__6EF57B66",
                        column: x => x.idban,
                        principalTable: "ban",
                        principalColumn: "idban");
                    table.ForeignKey(
                        name: "FK__donhang__idphien__6E01572D",
                        column: x => x.idphien,
                        principalTable: "phiensudungban",
                        principalColumn: "idphien");
                });

            migrationBuilder.CreateTable(
                name: "thanhtoan",
                columns: table => new
                {
                    idthanhtoan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    idphien = table.Column<int>(type: "int", nullable: false),
                    sotien = table.Column<decimal>(type: "decimal(12,0)", nullable: false),
                    hinhthuc = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    trangthai = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true, defaultValue: "chothanhtoan"),
                    thoigianthanhtoan = table.Column<DateTime>(type: "datetime", nullable: true),
                    mamgiaodich = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__thanhtoa__676EF9EAD4A389A7", x => x.idthanhtoan);
                    table.ForeignKey(
                        name: "FK__thanhtoan__idphi__7B5B524B",
                        column: x => x.idphien,
                        principalTable: "phiensudungban",
                        principalColumn: "idphien");
                });

            migrationBuilder.CreateTable(
                name: "chitietdon",
                columns: table => new
                {
                    idchitiet = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    idonhang = table.Column<int>(type: "int", nullable: false),
                    idsanpham = table.Column<int>(type: "int", nullable: false),
                    soluong = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    dongia = table.Column<decimal>(type: "decimal(12,0)", nullable: false),
                    trangthai = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true, defaultValue: "cholam"),
                    ghichu = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__chitietd__CD2E766FA9A87960", x => x.idchitiet);
                    table.ForeignKey(
                        name: "FK__chitietdo__idonh__74AE54BC",
                        column: x => x.idonhang,
                        principalTable: "donhang",
                        principalColumn: "idonhang");
                    table.ForeignKey(
                        name: "FK__chitietdo__idsan__75A278F5",
                        column: x => x.idsanpham,
                        principalTable: "sanpham",
                        principalColumn: "idsanpham");
                });

            migrationBuilder.CreateTable(
                name: "lichsukho",
                columns: table => new
                {
                    idlichsu = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    idnguyenlieu = table.Column<int>(type: "int", nullable: false),
                    loai = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    soluong = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    idonhangLienQuan = table.Column<int>(type: "int", nullable: true),
                    idnhanvien = table.Column<int>(type: "int", nullable: true),
                    thoigiantao = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__lichsukh__E98ACAF06B5112CF", x => x.idlichsu);
                    table.ForeignKey(
                        name: "FK__lichsukho__idngu__03F0984C",
                        column: x => x.idnguyenlieu,
                        principalTable: "nguyenlieu",
                        principalColumn: "idnguyenlieu");
                    table.ForeignKey(
                        name: "FK__lichsukho__idnha__05D8E0BE",
                        column: x => x.idnhanvien,
                        principalTable: "nhanvien",
                        principalColumn: "idnhanvien");
                    table.ForeignKey(
                        name: "FK__lichsukho__idonh__04E4BC85",
                        column: x => x.idonhangLienQuan,
                        principalTable: "donhang",
                        principalColumn: "idonhang");
                });

            migrationBuilder.CreateTable(
                name: "thanhtoandon",
                columns: table => new
                {
                    idthanhtoan = table.Column<int>(type: "int", nullable: false),
                    idonhang = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__thanhtoa__80E367032288DF35", x => new { x.idthanhtoan, x.idonhang });
                    table.ForeignKey(
                        name: "FK__thanhtoan__idonh__7F2BE32F",
                        column: x => x.idonhang,
                        principalTable: "donhang",
                        principalColumn: "idonhang");
                    table.ForeignKey(
                        name: "FK__thanhtoan__idtha__7E37BEF6",
                        column: x => x.idthanhtoan,
                        principalTable: "thanhtoan",
                        principalColumn: "idthanhtoan");
                });

            migrationBuilder.CreateIndex(
                name: "UQ__ban__0F4157A979EBDCD3",
                table: "ban",
                column: "maban",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_chitietdon_idonhang",
                table: "chitietdon",
                column: "idonhang");

            migrationBuilder.CreateIndex(
                name: "IX_chitietdon_idsanpham",
                table: "chitietdon",
                column: "idsanpham");

            migrationBuilder.CreateIndex(
                name: "IX_congthucsanpham_idnguyenlieu",
                table: "congthucsanpham",
                column: "idnguyenlieu");

            migrationBuilder.CreateIndex(
                name: "IX_donhang_idban",
                table: "donhang",
                column: "idban");

            migrationBuilder.CreateIndex(
                name: "IX_donhang_idphien",
                table: "donhang",
                column: "idphien");

            migrationBuilder.CreateIndex(
                name: "UQ__donhang__74722A5E80C74A37",
                table: "donhang",
                column: "madonhang",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_lichsukho_idnguyenlieu",
                table: "lichsukho",
                column: "idnguyenlieu");

            migrationBuilder.CreateIndex(
                name: "IX_lichsukho_idnhanvien",
                table: "lichsukho",
                column: "idnhanvien");

            migrationBuilder.CreateIndex(
                name: "IX_lichsukho_idonhangLienQuan",
                table: "lichsukho",
                column: "idonhangLienQuan");

            migrationBuilder.CreateIndex(
                name: "IX_phiensudungban_idban",
                table: "phiensudungban",
                column: "idban");

            migrationBuilder.CreateIndex(
                name: "IX_thanhtoan_idphien",
                table: "thanhtoan",
                column: "idphien");

            migrationBuilder.CreateIndex(
                name: "IX_thanhtoandon_idonhang",
                table: "thanhtoandon",
                column: "idonhang");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "chitietdon");

            migrationBuilder.DropTable(
                name: "congthucsanpham");

            migrationBuilder.DropTable(
                name: "lichsukho");

            migrationBuilder.DropTable(
                name: "thanhtoandon");

            migrationBuilder.DropTable(
                name: "sanpham");

            migrationBuilder.DropTable(
                name: "nguyenlieu");

            migrationBuilder.DropTable(
                name: "nhanvien");

            migrationBuilder.DropTable(
                name: "donhang");

            migrationBuilder.DropTable(
                name: "thanhtoan");

            migrationBuilder.DropTable(
                name: "phiensudungban");

            migrationBuilder.DropTable(
                name: "ban");
        }
    }
}
