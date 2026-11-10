/* khach.js - giao diện khách hàng
   Bản gộp: dùng logic ô nhập số lượng (<input>) làm chính, đồng thời vẫn chạy được với
   giao diện cũ dùng <span> + nút +/- (data-toi-da, data-ghi-chu-ton). */
(function () {
  'use strict';

  // Không còn giới hạn cứng 20 ly/món: số tối đa chính là số còn trong kho (máy chủ tính sẵn).
  // Giá trị này chỉ dùng khi trang không gửi data-con-lai / data-toi-da (máy chủ vẫn kiểm tra lại khi thêm).
  const DU_PHONG_KHONG_RO_KHO = 99999;

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

  // Đọc một số từ data-*; không có hoặc không phải số thì trả về null
  function docSo(giaTri) {
    if (giaTri === undefined || giaTri === '') return null;
    const so = Number(giaTri);
    return isFinite(so) ? so : null;
  }

  // ---------- Trang chi tiết món ----------
  function khoiTaoChiTiet() {
    const goc = document.querySelector('[data-chi-tiet]');
    if (!goc) return;

    const gia = Number(goc.dataset.gia);
    const ten = goc.dataset.ten;
    const idSanPham = goc.dataset.id;
    const urlThem = goc.dataset.urlThem;

    const elSoLuong = goc.querySelector('[data-so-luong]'); // <input> (mới) hoặc <span> (cũ)
    const elGia = goc.querySelector('[data-tong-gia]');
    const elGhiChu = goc.querySelector('[data-ghi-chu]') || goc.querySelector('[data-ghi-chu-ton]');
    const nutTru = goc.querySelector('[data-tru]');
    const nutCong = goc.querySelector('[data-cong]');
    const nutThem = goc.querySelector('[data-them-gio]');

    // Món đã hết hàng: trang có thể không render nút chọn số lượng / thêm giỏ hàng
    if (!elSoLuong || !elGia || !nutTru || !nutCong || !nutThem) return;

    const laOInput = elSoLuong.tagName === 'INPUT';

    // Do máy chủ tính:
    //   conLai  = số ly còn thêm được = số còn trong kho trừ số ly món này đã có trong giỏ
    //             (bản mới: data-con-lai, bản cũ: data-toi-da)
    //   gioiHan = số còn trong kho (bản mới: data-gioi-han; bản cũ không có -> lấy conLai ban đầu)
    let conLai = docSo(goc.dataset.conLai);
    if (conLai === null) conLai = docSo(goc.dataset.toiDa);
    if (conLai === null) conLai = DU_PHONG_KHONG_RO_KHO;
    conLai = Math.max(0, conLai);

    let gioiHan = docSo(goc.dataset.gioiHan);
    if (gioiHan === null) gioiHan = conLai;
    gioiHan = Math.max(0, gioiHan);

    let soLuong = conLai > 0 ? 1 : 0; // mặc định 1 (hết hàng thì 0)
    let dangGui = false;

    function thongBaoVuotGioiHan() {
      const daCo = gioiHan - conLai;
      return 'Chỉ có thể thêm tối đa ' + conLai + ' phần' +
        (daCo > 0 ? ' nữa (trong giỏ đã có ' + daCo + ')' : '') + '.';
    }

    // dangGoDo = true: người dùng đang gõ dở (vd. xoá hết để gõ số mới) -> không ghi đè nội dung ô
    function hienThi(dangGoDo) {
      const hetHang = conLai <= 0;

      if (soLuong > conLai) soLuong = conLai;
      if (conLai > 0 && soLuong < 1 && !dangGoDo) soLuong = 1;

      if (!dangGoDo) {
        if (laOInput) elSoLuong.value = String(soLuong);
        else elSoLuong.textContent = soLuong;
      }
      elGia.textContent = dinhDangTien(gia * Math.max(soLuong, 1)); // thêm 1 ly = cộng thêm 1 lần giá gốc

      if (laOInput) elSoLuong.disabled = hetHang;
      nutTru.disabled = hetHang || soLuong <= 1;     // đang là 1 thì nút trừ không bấm được
      nutCong.disabled = hetHang || soLuong >= conLai; // chạm mức còn lại thì khóa nút cộng
      nutThem.disabled = dangGui || hetHang || soLuong < 1;

      if (elGhiChu) {
        elGhiChu.hidden = !hetHang;
        if (hetHang) {
          const daCo = gioiHan - conLai;
          elGhiChu.textContent = daCo > 0
            ? 'Bạn đã thêm ' + daCo + ' phần vào giỏ – đã đạt số lượng tối đa của món này.'
            : 'Món này tạm hết nguyên liệu.';
        }
      }
    }

    // ----- Chỉ khi số lượng là ô <input>: chặn & chuẩn hoá nội dung gõ vào -----
    if (laOInput) {
      // Chặn từ lúc gõ: phím chữ, dấu cách, + - . e ... không vào được ô
      elSoLuong.addEventListener('beforeinput', function (e) {
        if (e.inputType === 'insertText' && e.data && /\D/.test(e.data)) e.preventDefault();
      });

      // Lưới an toàn cho mọi đường khác (dán, kéo thả, bộ gõ, tự điền): giữ lại đúng các chữ số,
      // bỏ số 0 đứng đầu, và không cho vượt quá số còn lại
      elSoLuong.addEventListener('input', function () {
        const chuSo = elSoLuong.value.replace(/\D/g, '').replace(/^0+(?=\d)/, '');
        let so = chuSo === '' ? 0 : Number(chuSo);
        let moi = chuSo;

        if (so > conLai) {
          so = conLai;
          moi = String(conLai);
          hienToast(thongBaoVuotGioiHan(), true);
        }

        if (elSoLuong.value !== moi) elSoLuong.value = moi; // chỉ ghi khi khác, để con trỏ không bị nhảy
        soLuong = so;
        hienThi(true);
      });

      // Rời khỏi ô mà để trống hoặc 0 -> về 1
      elSoLuong.addEventListener('blur', function () {
        if (conLai > 0 && soLuong < 1) soLuong = 1;
        hienThi();
      });

      // Bấm vào ô là bôi đen sẵn số hiện tại, gõ số mới đè lên luôn
      elSoLuong.addEventListener('focus', function () { elSoLuong.select(); });

      elSoLuong.addEventListener('keydown', function (e) {
        if (e.key === 'Enter') { e.preventDefault(); elSoLuong.blur(); }
      });
    }

    nutTru.addEventListener('click', function () {
      if (soLuong > 1) { soLuong -= 1; hienThi(); }
    });
    nutCong.addEventListener('click', function () {
      if (soLuong < conLai) { soLuong += 1; hienThi(); }
    });

    nutThem.addEventListener('click', function () {
      if (dangGui || soLuong < 1 || soLuong > conLai) return;

      const soLuongGui = soLuong;
      dangGui = true;
      hienThi();

      fetch(urlThem, {
        method: 'POST',
        body: new URLSearchParams({ idSanPham: idSanPham, soLuong: soLuongGui })
      })
        .then(function (res) {
          if (!res.ok) throw new Error('HTTP ' + res.status);
          return res.json();
        })
        .then(function (kq) {
          // Máy chủ báo còn thêm được bao nhiêu (đã trừ số trong giỏ, kể cả khi bị từ chối
          // vì kho đã đổi) -> đồng bộ lại số lượng và ô nhập
          if (typeof kq.conLai === 'number') {
            conLai = Math.max(0, kq.conLai);
            soLuong = conLai > 0 ? Math.min(Math.max(soLuong, 1), conLai) : 0;
          }
          if (!kq.ok) {
            hienToast(kq.thongBao || 'Chưa thêm được món này.', true);
            return;
          }
          capNhatGio(kq.tongSoLuong);
          hienToast('Đã thêm ' + soLuongGui + ' x ' + ten + ' vào giỏ hàng');
        })
        .catch(function () {
          hienToast('Chưa thêm được món. Hãy tải lại trang hoặc quét lại mã QR ở bàn.', true);
        })
        .finally(function () {
          dangGui = false;
          hienThi();
        });
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
