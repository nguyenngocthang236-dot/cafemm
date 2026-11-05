using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace QuanCafe.Models;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Ban> Bans { get; set; }

    public virtual DbSet<Chitietdon> Chitietdons { get; set; }

    public virtual DbSet<Congthucsanpham> Congthucsanphams { get; set; }

    public virtual DbSet<Donhang> Donhangs { get; set; }

    public virtual DbSet<Lichsukho> Lichsukhos { get; set; }

    public virtual DbSet<Nguyenlieu> Nguyenlieus { get; set; }

    public virtual DbSet<Nhanvien> Nhanviens { get; set; }

    public virtual DbSet<Phiensudungban> Phiensudungbans { get; set; }

    public virtual DbSet<Sanpham> Sanphams { get; set; }

    public virtual DbSet<Thanhtoan> Thanhtoans { get; set; }

    public virtual DbSet<SaleTheoNgay> SaleTheoNgays { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=CHOCOOPIE\\SQLEXPRESS;Database=QuanCafe;User Id=nhom7;Password=123;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ban>(entity =>
        {
            entity.HasKey(e => e.Idban).HasName("PK__ban__0088724A5206CD6C");

            entity.ToTable("ban");

            entity.HasIndex(e => e.Maban, "UQ__ban__0F4157A979EBDCD3").IsUnique();

            entity.Property(e => e.Idban).HasColumnName("idban");
            entity.Property(e => e.Maban)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("maban");
            entity.Property(e => e.Qrcode)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("qrcode");
            entity.Property(e => e.Succhua)
                .HasDefaultValue(4)
                .HasColumnName("succhua");
            entity.Property(e => e.Thoigiantao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("thoigiantao");
            entity.Property(e => e.Trangthai)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("trong")
                .HasColumnName("trangthai");
        });

        modelBuilder.Entity<Chitietdon>(entity =>
        {
            entity.HasKey(e => e.Idchitiet).HasName("PK__chitietd__CD2E766FA9A87960");

            entity.ToTable("chitietdon");

            entity.Property(e => e.Idchitiet).HasColumnName("idchitiet");
            entity.Property(e => e.Dongia)
                .HasColumnType("decimal(12, 0)")
                .HasColumnName("dongia");
            entity.Property(e => e.Ghichu)
                .HasMaxLength(255)
                .HasColumnName("ghichu");
            entity.Property(e => e.Idonhang).HasColumnName("idonhang");
            entity.Property(e => e.Idsanpham).HasColumnName("idsanpham");
            entity.Property(e => e.Soluong)
                .HasDefaultValue(1)
                .HasColumnName("soluong");
            entity.Property(e => e.Trangthai)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("cholam")
                .HasColumnName("trangthai");

            entity.HasOne(d => d.IdonhangNavigation).WithMany(p => p.Chitietdons)
                .HasForeignKey(d => d.Idonhang)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__chitietdo__idonh__74AE54BC");

            entity.HasOne(d => d.IdsanphamNavigation).WithMany(p => p.Chitietdons)
                .HasForeignKey(d => d.Idsanpham)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__chitietdo__idsan__75A278F5");
        });

        modelBuilder.Entity<Congthucsanpham>(entity =>
        {
            entity.HasKey(e => new { e.Idsanpham, e.Idnguyenlieu }).HasName("PK__congthuc__E3A5068F3DCCD5AC");

            entity.ToTable("congthucsanpham");

            entity.Property(e => e.Idsanpham).HasColumnName("idsanpham");
            entity.Property(e => e.Idnguyenlieu).HasColumnName("idnguyenlieu");
            entity.Property(e => e.Soluongcanthiet)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("soluongcanthiet");

            entity.HasOne(d => d.IdnguyenlieuNavigation).WithMany(p => p.Congthucsanphams)
                .HasForeignKey(d => d.Idnguyenlieu)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__congthucs__idngu__6477ECF3");

            entity.HasOne(d => d.IdsanphamNavigation).WithMany(p => p.Congthucsanphams)
                .HasForeignKey(d => d.Idsanpham)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__congthucs__idsan__6383C8BA");
        });

        modelBuilder.Entity<Donhang>(entity =>
        {
            entity.HasKey(e => e.Idonhang).HasName("PK__donhang__78D9EE9F2B843579");

            entity.ToTable("donhang");

            entity.HasIndex(e => e.Madonhang, "UQ__donhang__74722A5E80C74A37").IsUnique();

            entity.Property(e => e.Idonhang).HasColumnName("idonhang");
            entity.Property(e => e.Ghichu).HasColumnName("ghichu");
            entity.Property(e => e.Idban).HasColumnName("idban");
            entity.Property(e => e.Idphien).HasColumnName("idphien");
            entity.Property(e => e.Madonhang)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("madonhang");
            entity.Property(e => e.Thoigiandat)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("thoigiandat");
            entity.Property(e => e.Tongtien)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(12, 0)")
                .HasColumnName("tongtien");
            entity.Property(e => e.Trangthai)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("choxacnhan")
                .HasColumnName("trangthai");
            entity.Property(e => e.Trangthaithanhtoan)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("chuathanhtoan")
                .HasColumnName("trangthaithanhtoan");

            entity.HasOne(d => d.IdbanNavigation).WithMany(p => p.Donhangs)
                .HasForeignKey(d => d.Idban)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__donhang__idban__6EF57B66");

            entity.HasOne(d => d.IdphienNavigation).WithMany(p => p.Donhangs)
                .HasForeignKey(d => d.Idphien)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__donhang__idphien__6E01572D");
        });

        modelBuilder.Entity<Lichsukho>(entity =>
        {
            entity.HasKey(e => e.Idlichsu).HasName("PK__lichsukh__E98ACAF06B5112CF");

            entity.ToTable("lichsukho");

            entity.Property(e => e.Idlichsu).HasColumnName("idlichsu");
            entity.Property(e => e.Idnguyenlieu).HasColumnName("idnguyenlieu");
            entity.Property(e => e.Idnhanvien).HasColumnName("idnhanvien");
            entity.Property(e => e.IdonhangLienQuan).HasColumnName("idonhangLienQuan");
            entity.Property(e => e.Loai)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("loai");
            entity.Property(e => e.Soluong)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("soluong");
            entity.Property(e => e.Thoigiantao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("thoigiantao");

            entity.HasOne(d => d.IdnguyenlieuNavigation).WithMany(p => p.Lichsukhos)
                .HasForeignKey(d => d.Idnguyenlieu)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__lichsukho__idngu__03F0984C");

            entity.HasOne(d => d.IdnhanvienNavigation).WithMany(p => p.Lichsukhos)
                .HasForeignKey(d => d.Idnhanvien)
                .HasConstraintName("FK__lichsukho__idnha__05D8E0BE");

            entity.HasOne(d => d.IdonhangLienQuanNavigation).WithMany(p => p.Lichsukhos)
                .HasForeignKey(d => d.IdonhangLienQuan)
                .HasConstraintName("FK__lichsukho__idonh__04E4BC85");
        });

        modelBuilder.Entity<Nguyenlieu>(entity =>
        {
            entity.HasKey(e => e.Idnguyenlieu).HasName("PK__nguyenli__658B66A3817D547E");

            entity.ToTable("nguyenlieu");

            entity.Property(e => e.Idnguyenlieu).HasColumnName("idnguyenlieu");
            entity.Property(e => e.Dongia)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(12, 0)")
                .HasColumnName("dongia");
            entity.Property(e => e.Donvitinh)
                .HasMaxLength(20)
                .HasColumnName("donvitinh");
            entity.Property(e => e.Mucconhbao)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("mucconhbao");
            entity.Property(e => e.Soluongtonkho)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("soluongtonkho");
            entity.Property(e => e.Tennguyenlieu)
                .HasMaxLength(100)
                .HasColumnName("tennguyenlieu");
        });

        modelBuilder.Entity<Nhanvien>(entity =>
        {
            entity.HasKey(e => e.Idnhanvien).HasName("PK__nhanvien__A703FF0170806EC9");

            entity.ToTable("nhanvien");

            entity.Property(e => e.Idnhanvien).HasColumnName("idnhanvien");
            entity.Property(e => e.Hoten)
                .HasMaxLength(100)
                .HasColumnName("hoten");
            entity.Property(e => e.Sodienthoai)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("sodienthoai");
            entity.Property(e => e.Trangthai)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("dangdilam")
                .HasColumnName("trangthai");
            entity.Property(e => e.Vaitro)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("vaitro");
        });

        modelBuilder.Entity<Phiensudungban>(entity =>
        {
            entity.HasKey(e => e.Idphien).HasName("PK__phienban__3CE13535454AA4A5");

            entity.ToTable("phiensudungban");

            entity.Property(e => e.Idphien).HasColumnName("idphien");
            entity.Property(e => e.Idban).HasColumnName("idban");
            entity.Property(e => e.Thoigianbatdau)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("thoigianbatdau");
            entity.Property(e => e.Thoigianketthuc)
                .HasColumnType("datetime")
                .HasColumnName("thoigianketthuc");
            entity.Property(e => e.Trangthai)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("dangmo")
                .HasColumnName("trangthai");

            entity.HasOne(d => d.IdbanNavigation).WithMany(p => p.Phiensudungbans)
                .HasForeignKey(d => d.Idban)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__phienban__idban__5441852A");
        });

        modelBuilder.Entity<Sanpham>(entity =>
        {
            entity.HasKey(e => e.Idsanpham).HasName("PK__sanpham__C5FDB0E5F66BDDA4");

            entity.ToTable("sanpham");

            entity.Property(e => e.Idsanpham).HasColumnName("idsanpham");
            entity.Property(e => e.Conhang)
                .HasDefaultValue(true)
                .HasColumnName("conhang");
            entity.Property(e => e.Gia)
                .HasColumnType("decimal(12, 0)")
                .HasColumnName("gia");
            entity.Property(e => e.Hinhanh)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("hinhanh");
            entity.Property(e => e.Loai)
                .HasMaxLength(50)
                .HasColumnName("loai");
            entity.Property(e => e.Mota).HasColumnName("mota");
            entity.Property(e => e.Tensanpham)
                .HasMaxLength(100)
                .HasColumnName("tensanpham");
        });

        modelBuilder.Entity<Thanhtoan>(entity =>
        {
            entity.HasKey(e => e.Idthanhtoan).HasName("PK__thanhtoa__676EF9EAD4A389A7");

            entity.ToTable("thanhtoan");

            entity.Property(e => e.Idthanhtoan).HasColumnName("idthanhtoan");
            entity.Property(e => e.Hinhthuc)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("hinhthuc");
            entity.Property(e => e.Idphien).HasColumnName("idphien");
            entity.Property(e => e.Mamgiaodich)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("mamgiaodich");
            entity.Property(e => e.Sotien)
                .HasColumnType("decimal(12, 0)")
                .HasColumnName("sotien");
            entity.Property(e => e.Thoigianthanhtoan)
                .HasColumnType("datetime")
                .HasColumnName("thoigianthanhtoan");
            entity.Property(e => e.Trangthai)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("chothanhtoan")
                .HasColumnName("trangthai");

            entity.HasOne(d => d.IdphienNavigation).WithMany(p => p.Thanhtoans)
                .HasForeignKey(d => d.Idphien)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__thanhtoan__idphi__7B5B524B");

            entity.HasMany(d => d.Idonhangs).WithMany(p => p.Idthanhtoans)
                .UsingEntity<Dictionary<string, object>>(
                    "Thanhtoandon",
                    r => r.HasOne<Donhang>().WithMany()
                        .HasForeignKey("Idonhang")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__thanhtoan__idonh__7F2BE32F"),
                    l => l.HasOne<Thanhtoan>().WithMany()
                        .HasForeignKey("Idthanhtoan")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__thanhtoan__idtha__7E37BEF6"),
                    j =>
                    {
                        j.HasKey("Idthanhtoan", "Idonhang").HasName("PK__thanhtoa__80E367032288DF35");
                        j.ToTable("thanhtoandon");
                        j.IndexerProperty<int>("Idthanhtoan").HasColumnName("idthanhtoan");
                        j.IndexerProperty<int>("Idonhang").HasColumnName("idonhang");
                    });
        });

        modelBuilder.Entity<SaleTheoNgay>(entity =>
        {
            entity.HasKey(e => e.IdsaleTheoNgay).HasName("PK__saletheo__IdsaleTheoNgay");
            entity.ToTable("sale_theo_ngay");

            entity.Property(e => e.IdsaleTheoNgay).HasColumnName("idsale_theo_ngay");
            entity.Property(e => e.Idsanpham).HasColumnName("idsanpham");
            entity.Property(e => e.PhanTramGiam)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("phantram_giam");
            entity.Property(e => e.ThoiGianBatDau)
                .HasColumnType("datetime")
                .HasColumnName("thoigian_batdau");
            entity.Property(e => e.ThoiGianKetThuc)
                .HasColumnType("datetime")
                .HasColumnName("thoigian_ketthuc");
            entity.Property(e => e.Trangthai)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("dang_chay")
                .HasColumnName("trangthai");
            entity.Property(e => e.Thoigiantao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("thoigiantao");

            entity.HasOne(d => d.IdsanphamNavigation)
                .WithMany()
                .HasForeignKey(d => d.Idsanpham)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__sale_theo_ngay__idsanpham");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
