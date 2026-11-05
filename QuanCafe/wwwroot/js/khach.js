/* khach.js - giao diện khách hàng */
(function () {
  'use strict';

  // Số lượng tối đa mỗi món (khớp với SoLuongToiDa trong GioHangController.cs)
  const TOI_DA = 20;

  // 25000 -> "25.000đ"
  function dinhDangTien(so) {
    return Math.round(so).toString().replace(/\B(?=(\d{3})+(?!\d))/g, '.') + 'đ';
  }

  // ---------- Thông báo nhỏ ----------
  let toastEl = null;
  let toastTimer = null;

  function hienToast(noiDung, laLoi) {
    toastEl = toastEl || document.querySelector('[data-toast]');
    if (!toastEl) return;
    toastEl.textContent = noiDung;
    toastEl.classList.toggle('toast--loi', Boolean(laLoi));
    toastEl.classList.add('toast--hien');
    clearTimeout(toastTimer);
    toastTimer = setTimeout(function () { toastEl.classList.remove('toast--hien'); }, 2600);
  }

  // ---------- Số trên biểu tượng giỏ hàng ----------
  function capNhatGio(tongSoLuong) {
    document.querySelectorAll('[data-gio-so]').forEach(function (el) {
      el.textContent = tongSoLuong;
      el.classList.toggle('gio__so--rong', tongSoLuong === 0);
      el.classList.remove('gio__so--nay');
      void el.offsetWidth; // để animation chạy lại ở lần thêm tiếp theo
      el.classList.add('gio__so--nay');
    });
  }

  // ---------- Trang chi tiết món ----------
  function khoiTaoChiTiet() {
    const goc = document.querySelector('[data-chi-tiet]');
    if (!goc) return;

    const gia = Number(goc.dataset.gia);
    const ten = goc.dataset.ten;
    const idSanPham = goc.dataset.id;
    const urlThem = goc.dataset.urlThem;

    const elSoLuong = goc.querySelector('[data-so-luong]');
    const elGia = goc.querySelector('[data-tong-gia]');
    const nutTru = goc.querySelector('[data-tru]');
    const nutCong = goc.querySelector('[data-cong]');
    const nutThem = goc.querySelector('[data-them-gio]');

    let soLuong = 1; // mặc định 1

    function hienThi() {
      elSoLuong.textContent = soLuong;
      elGia.textContent = dinhDangTien(gia * soLuong); // thêm 1 ly = cộng thêm 1 lần giá gốc
      nutTru.disabled = soLuong <= 1;                  // đang là 1 thì nút trừ không bấm được
      nutCong.disabled = soLuong >= TOI_DA;
    }

    nutTru.addEventListener('click', function () {
      if (soLuong > 1) { soLuong -= 1; hienThi(); }
    });
    nutCong.addEventListener('click', function () {
      if (soLuong < TOI_DA) { soLuong += 1; hienThi(); }
    });

    nutThem.addEventListener('click', function () {
      nutThem.disabled = true;

      fetch(urlThem, {
        method: 'POST',
        body: new URLSearchParams({ idSanPham: idSanPham, soLuong: soLuong })
      })
        .then(function (res) {
          if (!res.ok) throw new Error('HTTP ' + res.status);
          return res.json();
        })
        .then(function (kq) {
          if (!kq.ok) {
            hienToast(kq.thongBao || 'Chưa thêm được món này.', true);
            return;
          }
          capNhatGio(kq.tongSoLuong);
          hienToast('Đã thêm ' + soLuong + ' x ' + ten + ' vào giỏ hàng');
        })
        .catch(function () {
          hienToast('Chưa thêm được món. Hãy tải lại trang hoặc quét lại mã QR ở bàn.', true);
        })
        .finally(function () { nutThem.disabled = false; });
    });

    hienThi();
  }

  // ---------- Ảnh bị lỗi / chưa có file -> dùng ảnh mặc định ----------
  function khoiTaoAnhDuPhong() {
    const macDinh = document.body.dataset.anhMacDinh;
    if (!macDinh) return;

    function thay(img) {
      if (img.dataset.daThay) return;
      img.dataset.daThay = '1';
      img.src = macDinh;
    }

    document.addEventListener('error', function (e) {
      if (e.target && e.target.tagName === 'IMG') thay(e.target);
    }, true);

    document.querySelectorAll('img').forEach(function (img) {
      if (img.complete && img.naturalWidth === 0) thay(img);
    });
  }

  // ---------- Form đặt hàng: chặn bấm 2 lần ----------
  function khoiTaoFormMotLan() {
    document.querySelectorAll('form[data-mot-lan]').forEach(function (form) {
      form.addEventListener('submit', function () {
        const nut = form.querySelector('[type="submit"]');
        if (nut) {
          nut.disabled = true;
          nut.textContent = 'Đang gửi đơn...';
        }
      });
    });

    // Quay lại bằng nút Back: tải lại để giỏ hàng và nút không bị kẹt trạng thái cũ
    window.addEventListener('pageshow', function (e) {
      if (e.persisted) location.reload();
    });
  }

  document.addEventListener('DOMContentLoaded', function () {
    khoiTaoChiTiet();
    khoiTaoAnhDuPhong();
    khoiTaoFormMotLan();
  });
})();
