/* =========================================================
   dashboard.js — /Menu/Dashboard (trang quản trị)

   Dữ liệu đơn hàng và sản phẩm được lấy từ CSDL thật qua các API:
     GET    /api/donhang?trangThai=...   -> danh sách đơn theo bộ lọc
     POST   /api/donhang/{id}/nhan       -> staff xác nhận đã nhận đơn
     POST   /api/donhang/{id}/chuan-bi   -> chuyển đơn sang "đang chuẩn bị"
     POST   /api/donhang/{id}/huy        -> hủy đơn
     GET    /api/sanpham                 -> danh sách món
     POST   /api/sanpham                 -> thêm món (multipart/form-data)
     PUT    /api/sanpham/{id}            -> sửa món (multipart/form-data)
     DELETE /api/sanpham/{id}            -> xóa món
   ========================================================= */
(function () {
  'use strict';

  // ---------- Hằng số ----------
  var DUNG_LUONG_ANH_TOI_DA = 2 * 1024 * 1024; // 2MB
  var DINH_DANG_ANH_HOP_LE = ['image/jpeg', 'image/png', 'image/webp'];
  var MAU_LY_MUOI = '#1B2150';   // cốc navy cho cà phê muối
  var MAU_LY_THUONG = '#A9754F'; // cốc nâu be cho các món còn lại
  var KHOANG_TU_DONG_LAM_MOI = 15000; // tự làm mới danh sách đơn mỗi 15 giây

  // Dữ liệu trong bộ nhớ
  var danhSachDon = [];
  var danhSachMon = [];

  // ========================================================
  // NGUỒN DỮ LIỆU — gọi API backend, đồng bộ với CSDL thật
  // ========================================================

  function goiApi(phuongThuc, url, body) {
    var tuyChon = { method: phuongThuc, headers: {} };
    if (body instanceof FormData) {
      tuyChon.body = body;
    } else if (body) {
      tuyChon.headers['Content-Type'] = 'application/json';
      tuyChon.body = JSON.stringify(body);
    }
    return fetch(url, tuyChon).then(function (res) {
      // Đọc dạng chữ rồi tự parse: nếu server trả về trang lỗi HTML hoặc nội dung rỗng
      // thì vẫn báo được thông điệp dễ hiểu thay vì lỗi "Unexpected token ...".
      return res.text().then(function (text) {
        var data = null;
        if (text) {
          try { data = JSON.parse(text); } catch (e) { data = null; }
        }
        if (!res.ok) {
          var thongBao = data && (data.loi || data.title);
          if (data && data.errors) {
            var khoa = Object.keys(data.errors)[0];
            if (khoa && data.errors[khoa] && data.errors[khoa][0]) thongBao = data.errors[khoa][0];
          }
          throw new Error(thongBao || ('Lỗi máy chủ (HTTP ' + res.status + '). Xem log ứng dụng để biết chi tiết.'));
        }
        return data;
      });
    });
  }

  function taiDanhSachDon(trangThai) {
    var url = '/api/donhang' + (trangThai && trangThai !== 'tat-ca' ? '?trangThai=' + trangThai : '');
    return goiApi('GET', url).then(function (data) {
      danhSachDon = data;
    });
  }

  function taiDanhSachMon() {
    return goiApi('GET', '/api/sanpham').then(function (data) {
      danhSachMon = data;
    });
  }

  // ========================================================
  // HÀM DÙNG CHUNG
  // ========================================================

  function dinhDangTien(so) {
    return Math.round(so).toString().replace(/\B(?=(\d{3})+(?!\d))/g, '.') + 'đ';
  }

  var toastEl, toastTimer;
  function hienToast(noiDung, laLoi) {
    toastEl = toastEl || document.getElementById('toast');
    toastEl.textContent = noiDung;
    toastEl.style.background = laLoi ? '#B23A52' : '';
    toastEl.classList.add('toast--hien');
    clearTimeout(toastTimer);
    toastTimer = setTimeout(function () { toastEl.classList.remove('toast--hien'); }, 2600);
  }

  // Icon ly cà phê mặc định (SVG)
  function anhLyMacDinh(tenMon) {
    var mau = /mu[oố]i/i.test(tenMon || '') ? MAU_LY_MUOI : MAU_LY_THUONG;
    var svg =
      '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 64 64">' +
      '<rect width="64" height="64" fill="#FDEBCB"/>' +
      '<path d="M16 22h28l-3 23a6 6 0 0 1-6 5H25a6 6 0 0 1-6-5z" fill="' + mau + '"/>' +
      '<path d="M44 25h6a7 7 0 0 1 0 14h-6" fill="none" stroke="' + mau + '" stroke-width="4"/>' +
      '<path d="M22 13c-3 3 3 5 0 9M30 13c-3 3 3 5 0 9" stroke="' + mau + '" stroke-width="3" fill="none" stroke-linecap="round" opacity=".55"/>' +
      '</svg>';
    return 'data:image/svg+xml;utf8,' + encodeURIComponent(svg);
  }

  // Ảnh không tải được (file thiếu/404) -> thay bằng icon ly mặc định, không để ô ảnh vỡ.
  document.addEventListener('error', function (e) {
    var img = e.target;
    if (!img || img.tagName !== 'IMG' || img.id === 'xem-truoc-anh' || img.dataset.daThay) return;
    img.dataset.daThay = '1';
    img.src = anhLyMacDinh(img.alt || img.title || '');
  }, true);

  function anhHienThiCuaItem(item) {
    // item.hinhanh là đường dẫn từ server (e.g. /images/menu/abc.jpg)
    if (item && item.hinhanh) return item.hinhanh;
    return anhLyMacDinh(item ? item.ten : '');
  }

  function anhHienThiCuaMon(mon) {
    if (mon && mon.anh) return mon.anh;
    return anhLyMacDinh(mon ? mon.ten : '');
  }

  function thoatHtml(chuoi) {
    var div = document.createElement('div');
    div.textContent = chuoi == null ? '' : String(chuoi);
    return div.innerHTML;
  }

  // ========================================================
  // RENDER: nhóm ly đồ uống
  // ========================================================

  function renderNhomLy(items, hienHieuUngHoi) {
    var html = '';
    items.forEach(function (muc) {
      var anh = thoatHtml(anhHienThiCuaItem(muc));
      var hoi = hienHieuUngHoi
        ? '<svg class="ly__hoi" viewBox="0 0 20 14" fill="none" stroke="#F28C6B" stroke-width="1.6" stroke-linecap="round" aria-hidden="true">' +
          '<path d="M5 13c2-3-2-5 0-8"/><path d="M13 13c2-3-2-5 0-8"/></svg>'
        : '';
      for (var i = 0; i < muc.soLuong; i++) {
        html += '<div class="ly" title="' + thoatHtml(muc.ten) + '">' + hoi +
          '<div class="ly__vong"><img src="' + anh + '" alt="' + thoatHtml(muc.ten) + '" /></div></div>';
      }
    });
    return html;
  }

  // ========================================================
  // RENDER: danh sách đơn hàng (kanban 4 cột)
  // ========================================================

  var BO_LOC_DANG_CHON_MAC_DINH = 'tat-ca';
  var bieuMauLoc = BO_LOC_DANG_CHON_MAC_DINH;

  function donKhopBoLoc(don) {
    switch (bieuMauLoc) {
      case 'cho': return don.trangThai === 'cho' && !don.daHuy;
      case 'chuan-bi': return don.trangThai === 'dangChuanBi' && !don.daHuy;
      case 'san-sang': return don.trangThai === 'sanSangGiao' && !don.daHuy;
      case 'hoan-tat': return don.trangThai === 'daHoanTat';
      default: return true; // 'tat-ca'
    }
  }

  function renderDanhSachDon() {
    var vungDs = document.getElementById('danh-sach-don');
    var vungRong = document.getElementById('don-rong');
    var dsHienThi = danhSachDon.filter(donKhopBoLoc);

    vungRong.hidden = dsHienThi.length > 0;
    vungDs.innerHTML = dsHienThi.map(renderMotDon).join('');
  }

  function renderMotDon(don) {
    var moTaMon = don.items.map(function (m) { return m.ten + ' x' + m.soLuong; }).join(', ');
    var daThanhToan = don.trangThaiThanhToan === 'daThanhToan';
    var hinhThucText = {
      tienmat: 'Tiền mặt',
      chuyenkhoan: 'Chuyển khoản',
      vidientu: 'Ví điện tử'
    }[don.hinhThucThanhToan] || '';

    var nhan = '';
    if (don.daHuy) nhan = '<span class="don-hang__nhan don-hang__nhan--huy">Đã hủy</span>';
    else if (don.trangThai === 'sanSangGiao') nhan = '<span class="don-hang__nhan don-hang__nhan--da-nhan">Đã chuẩn bị xong</span>';
    else if (don.trangThai === 'daHoanTat') nhan = '<span class="don-hang__nhan don-hang__nhan--da-nhan">Đã giao</span>';
    else if (don.daNhan) nhan = '<span class="don-hang__nhan don-hang__nhan--da-nhan">Đã nhận</span>';

    var lyCho = don.trangThai === 'cho' ? renderNhomLy(don.items, true) : '';
    var lyChuanBi = (don.trangThai === 'dangChuanBi' || don.trangThai === 'sanSangGiao')
      ? renderNhomLy(don.items, false) : '';

    var khoaNhan = don.trangThai !== 'cho' || don.daHuy;
    var khoaChuanBi = don.trangThai !== 'cho' || don.daHuy;
    var khoaHuy = don.daHuy || don.trangThai === 'sanSangGiao' || don.trangThai === 'daHoanTat' || daThanhToan;

    var nutChuanBiXong = don.trangThai === 'dangChuanBi'
      ? '<button type="button" class="nut-don nut-don--chinh" data-hanh-dong="chuan-bi-xong">Đã chuẩn bị xong</button>' : '';
    var nutGiao = don.trangThai === 'sanSangGiao'
      ? '<button type="button" class="nut-don nut-don--chinh" data-hanh-dong="giao">Đã giao cho khách</button>' : '';
    var khuThanhToan = '<div class="don-hang__thanh-toan"><div class="don-hang__trang-thai-thanh-toan">Thanh toán: ' +
      (daThanhToan ? 'Đã thanh toán' + (hinhThucText ? ' (' + hinhThucText + ')' : '') : 'Chưa thanh toán') + '</div>' +
      (!daThanhToan && !don.daHuy
        ? '<div class="don-hang__thanh-toan-tac-vu">' +
          '<select class="don-hang__chon-hinh-thuc" data-hinh-thuc aria-label="Hình thức thanh toán">' +
            '<option value="tienmat">Tiền mặt</option>' +
            '<option value="chuyenkhoan">Chuyển khoản</option>' +
            '<option value="vidientu">Ví điện tử</option>' +
          '</select>' +
          '<button type="button" class="nut-don nut-don--chinh" data-hanh-dong="thanh-toan">Xác nhận thanh toán</button>' +
        '</div>' : '') + '</div>';

    return (
      '<div class="don-hang' + (don.daHuy ? ' don-hang--huy' : '') + '" data-id="' + don.id + '">' +
        '<div class="don-hang__o">' +
          '<div class="don-hang__ma">' +
            (don.maBan ? '<span class="don-hang__ban">Bàn ' + thoatHtml(don.maBan) + '</span>' : '') +
            'Mã đơn: ' + thoatHtml(don.maDon) + '</div>' +
          '<div class="don-hang__mon">' + thoatHtml(moTaMon) + '</div>' +
          '<div class="don-hang__tong">Tổng: ' + dinhDangTien(don.tong) + '</div>' +
          nhan +
        '</div>' +
        '<div class="don-hang__o don-hang__o--cho"><div class="ly-nhom">' +
          (lyCho || '<span class="ly-trong">—</span>') + '</div></div>' +
        '<div class="don-hang__o don-hang__o--chuan-bi"><div class="ly-nhom">' +
          (lyChuanBi || '<span class="ly-trong">—</span>') + '</div></div>' +
        '<div class="don-hang__o don-hang__o--tac-vu">' +
          '<button type="button" class="nut-don nut-don--chinh" data-hanh-dong="nhan"' + (khoaNhan ? ' disabled' : '') + '>' +
            '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" class="cham-tick"><path d="M5 12.5l4.5 4.5L19 7.5"/></svg>' +
            'Nhận đơn</button>' +
          '<button type="button" class="nut-don nut-don--chinh" data-hanh-dong="chuan-bi"' + (khoaChuanBi ? ' disabled' : '') + '>' +
            '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M3 9l9-6 9 6v10a1 1 0 0 1-1 1H4a1 1 0 0 1-1-1z"/></svg>' +
            'Chuyển sang chuẩn bị</button>' +
          nutChuanBiXong + nutGiao +
          '<button type="button" class="nut-don nut-don--huy" data-hanh-dong="huy"' + (khoaHuy ? ' disabled' : '') + '>' +
            '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" class="cham-x"><path d="M6 6l12 12M18 6L6 18"/></svg>' +
            'Hủy đơn</button>' +
          khuThanhToan +
        '</div>' +
      '</div>'
    );
  }

  // Bắt sự kiện nút trong danh sách đơn (event delegation)
  document.getElementById('danh-sach-don').addEventListener('click', function (e) {
    var nut = e.target.closest('[data-hanh-dong]');
    if (!nut || nut.disabled) return;

    var hangDon = e.target.closest('.don-hang');
    var id = Number(hangDon.dataset.id);
    var don = danhSachDon.find(function (d) { return d.id === id; });
    if (!don) return;

    var hanhDong = nut.dataset.hanhDong;

    if (hanhDong === 'nhan') {
      nut.disabled = true;
      goiApi('POST', '/api/donhang/' + id + '/nhan')
        .then(function (res) {
          don.daNhan = true;
          don.trangThai = 'dangChuanBi';
          hienToast('Đã nhận đơn ' + don.maDon + '.');
          renderDanhSachDon();
        })
        .catch(function (err) {
          hienToast(err.message || 'Không thể nhận đơn.', true);
          nut.disabled = false;
        });

    } else if (hanhDong === 'chuan-bi') {
      nut.disabled = true;
      goiApi('POST', '/api/donhang/' + id + '/chuan-bi')
        .then(function (res) {
          don.trangThai = 'dangChuanBi';
          hienToast('Đơn ' + don.maDon + ' đã chuyển sang chuẩn bị.');
          renderDanhSachDon();
        })
        .catch(function (err) {
          hienToast(err.message || 'Không thể cập nhật đơn.', true);
          nut.disabled = false;
        });

    } else if (hanhDong === 'chuan-bi-xong') {
      nut.disabled = true;
      goiApi('POST', '/api/donhang/' + id + '/chuan-bi-xong')
        .then(function () {
          don.trangThai = 'sanSangGiao';
          hienToast('Đơn ' + don.maDon + ' đã chuẩn bị xong.');
          renderDanhSachDon();
        })
        .catch(function (err) {
          hienToast(err.message || 'Chưa thể hoàn tất chuẩn bị đơn.', true);
          nut.disabled = false;
        });

    } else if (hanhDong === 'giao') {
      nut.disabled = true;
      goiApi('POST', '/api/donhang/' + id + '/giao')
        .then(function () {
          don.trangThai = 'daHoanTat';
          don.daNhan = true;
          hienToast('Đã xác nhận giao đơn ' + don.maDon + '.');
          renderDanhSachDon();
        })
        .catch(function (err) {
          hienToast(err.message || 'Không thể xác nhận giao đơn.', true);
          nut.disabled = false;
        });

    } else if (hanhDong === 'thanh-toan') {
      var oChonHinhThuc = hangDon.querySelector('[data-hinh-thuc]');
      var hinhThuc = oChonHinhThuc ? oChonHinhThuc.value : 'tienmat';
      nut.disabled = true;
      goiApi('POST', '/api/donhang/' + id + '/thanh-toan', { hinhThuc: hinhThuc })
        .then(function (res) {
          don.trangThaiThanhToan = 'daThanhToan';
          don.hinhThucThanhToan = res.hinhThuc || hinhThuc;
          hienToast('Đã ghi nhận thanh toán đơn ' + don.maDon + '.');
          renderDanhSachDon();
        })
        .catch(function (err) {
          hienToast(err.message || 'Không thể ghi nhận thanh toán.', true);
          nut.disabled = false;
        });

    } else if (hanhDong === 'huy') {
      var dongY = window.confirm('Bạn chắc chắn muốn hủy đơn ' + don.maDon + '?');
      if (!dongY) return;
      nut.disabled = true;
      goiApi('POST', '/api/donhang/' + id + '/huy')
        .then(function (res) {
          don.daHuy = true;
          don.trangThai = 'daHuy';
          hienToast('Đã hủy đơn ' + don.maDon + '.', true);
          renderDanhSachDon();
        })
        .catch(function (err) {
          hienToast(err.message || 'Không thể hủy đơn.', true);
          nut.disabled = false;
        });
    }
  });

  // Bộ lọc tab
  document.querySelectorAll('.bo-loc__nut').forEach(function (nut) {
    nut.addEventListener('click', function () {
      document.querySelectorAll('.bo-loc__nut').forEach(function (n) {
        n.classList.remove('bo-loc__nut--dang-chon');
        n.setAttribute('aria-selected', 'false');
      });
      nut.classList.add('bo-loc__nut--dang-chon');
      nut.setAttribute('aria-selected', 'true');
      bieuMauLoc = nut.dataset.loc;
      renderDanhSachDon();
    });
  });

  // Tự động làm mới danh sách đơn
  setInterval(function () {
    taiDanhSachDon().then(renderDanhSachDon).catch(function () {});
  }, KHOANG_TU_DONG_LAM_MOI);

  // ========================================================
  // MODAL: Thêm / sửa món (gọi API thật thay vì localStorage)
  // ========================================================

  var modal = document.getElementById('modal-mon');
  var form = document.getElementById('form-mon');
  var oMonId = document.getElementById('mon-id');
  var oTen = document.getElementById('mon-ten');
  var oGia = document.getElementById('mon-gia');
  var oLoai = document.getElementById('mon-loai');
  var oMoTa = document.getElementById('mon-mota');
  var oXoaMon = document.getElementById('nut-xoa-mon');

  var vungTha = document.getElementById('vung-tha-anh');
  var vungThaTrong = document.getElementById('vung-tha-anh-trong');
  var xemTruocAnh = document.getElementById('xem-truoc-anh');
  var chonFileAnh = document.getElementById('chon-file-anh');
  var loiAnh = document.getElementById('loi-anh');
  var truongAnhNut = document.getElementById('truong-anh-nut');

  var fileAnhDangChon = null; // File object thật, gửi lên server
  var xoaAnhHienTai = false;

  function moModal() {
    modal.hidden = false;
    taiDanhSachMon().then(renderDsMonNho).catch(function () {
      hienToast('Không tải được danh sách món.', true);
    });
  }
  function dongModal() { modal.hidden = true; }

  document.getElementById('nut-cap-nhat-thuc-don').addEventListener('click', moModal);
  document.getElementById('nut-dong-modal').addEventListener('click', dongModal);
  modal.addEventListener('click', function (e) { if (e.target === modal) dongModal(); });
  document.addEventListener('keydown', function (e) {
    if (e.key === 'Escape' && !modal.hidden) dongModal();
  });

  function renderDsMonNho() {
    var vung = document.getElementById('mon-ds-nho');
    vung.innerHTML = danhSachMon.map(function (mon) {
      return '<button type="button" class="mon-the-nho" data-id="' + mon.id + '">' +
        '<img class="mon-the-nho__anh" src="' + thoatHtml(anhHienThiCuaMon(mon)) + '" alt="' + thoatHtml(mon.ten) + '" />' +
        thoatHtml(mon.ten) + '</button>';
    }).join('');
  }

  document.getElementById('mon-ds-nho').addEventListener('click', function (e) {
    var nut = e.target.closest('.mon-the-nho');
    if (!nut) return;
    var mon = danhSachMon.find(function (m) { return m.id === Number(nut.dataset.id); });
    if (mon) napMonVaoForm(mon);
  });

  function napMonVaoForm(mon) {
    oMonId.value = mon.id;
    oTen.value = mon.ten;
    oGia.value = mon.gia;
    oLoai.value = mon.loai || 'Cà phê';
    oMoTa.value = mon.moTa || '';
    oXoaMon.hidden = false;

    fileAnhDangChon = null;
    xoaAnhHienTai = false;
    if (mon.anh) {
      hienThiAnhXemTruoc(mon.anh);
    } else {
      anAnhXemTruoc();
    }
    xoaLoiAnh();
  }

  function resetForm() {
    form.reset();
    oMonId.value = '';
    oLoai.value = 'Cà phê';
    oXoaMon.hidden = true;
    fileAnhDangChon = null;
    xoaAnhHienTai = false;
    anAnhXemTruoc();
    xoaLoiAnh();
  }
  document.getElementById('nut-tao-moi').addEventListener('click', resetForm);

  // --- Khu vực ảnh ---
  function hienThiAnhXemTruoc(src) {
    xemTruocAnh.src = src;
    xemTruocAnh.hidden = false;
    vungThaTrong.hidden = true;
    truongAnhNut.hidden = false;
  }
  function anAnhXemTruoc() {
    xemTruocAnh.hidden = true;
    xemTruocAnh.src = '';
    vungThaTrong.hidden = false;
    truongAnhNut.hidden = true;
  }
  function xoaLoiAnh() { loiAnh.hidden = true; loiAnh.textContent = ''; }
  function baoLoiAnh(noiDung) { loiAnh.hidden = false; loiAnh.textContent = noiDung; }

  function xuLyFileAnh(file) {
    xoaLoiAnh();
    if (!file) return;

    if (DINH_DANG_ANH_HOP_LE.indexOf(file.type) === -1) {
      baoLoiAnh('Chỉ chấp nhận ảnh JPG, PNG, WEBP dưới 2MB.');
      return;
    }
    if (file.size > DUNG_LUONG_ANH_TOI_DA) {
      baoLoiAnh('Chỉ chấp nhận ảnh JPG, PNG, WEBP dưới 2MB.');
      return;
    }

    fileAnhDangChon = file;
    xoaAnhHienTai = false;

    var doc = new FileReader();
    doc.onload = function () { hienThiAnhXemTruoc(doc.result); };
    doc.onerror = function () { baoLoiAnh('Không đọc được file ảnh này, hãy thử ảnh khác.'); };
    doc.readAsDataURL(file);
  }

  vungTha.addEventListener('click', function () { chonFileAnh.click(); });
  vungTha.addEventListener('keydown', function (e) {
    if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); chonFileAnh.click(); }
  });
  chonFileAnh.addEventListener('change', function () { xuLyFileAnh(chonFileAnh.files[0]); });

  ['dragenter', 'dragover'].forEach(function (ten) {
    vungTha.addEventListener(ten, function (e) {
      e.preventDefault();
      vungTha.classList.add('vung-tha-anh--dang-keo');
    });
  });
  ['dragleave', 'drop'].forEach(function (ten) {
    vungTha.addEventListener(ten, function (e) {
      e.preventDefault();
      vungTha.classList.remove('vung-tha-anh--dang-keo');
    });
  });
  vungTha.addEventListener('drop', function (e) {
    var file = e.dataTransfer.files && e.dataTransfer.files[0];
    xuLyFileAnh(file);
  });

  document.getElementById('nut-doi-anh').addEventListener('click', function () { chonFileAnh.click(); });
  document.getElementById('nut-xoa-anh').addEventListener('click', function () {
    fileAnhDangChon = null;
    xoaAnhHienTai = true;
    chonFileAnh.value = '';
    anAnhXemTruoc();
  });

  // --- Lưu / xóa món (gọi API) ---
  form.addEventListener('submit', function (e) {
    e.preventDefault();

    var ten = oTen.value.trim();
    var giaChu = oGia.value.trim();
    var gia = Number(giaChu);
    if (!ten) { hienToast('Vui lòng nhập tên món.', true); oTen.focus(); return; }
    if (giaChu === '') { hienToast('Vui lòng nhập giá món.', true); oGia.focus(); return; }
    if (!isFinite(gia) || gia < 0) { hienToast('Giá món không hợp lệ.', true); oGia.focus(); return; }

    var fd = new FormData();
    fd.append('ten', ten);
    fd.append('gia', String(Math.round(gia)));   // cột gia là decimal(12,0): gửi số nguyên
    fd.append('loai', oLoai.value);
    fd.append('mota', oMoTa.value.trim());
    if (fileAnhDangChon) fd.append('anh', fileAnhDangChon);

    var dangSua = oMonId.value !== '';
    var url, phuongThuc;

    if (dangSua) {
      url = '/api/sanpham/' + oMonId.value;
      phuongThuc = 'PUT';
      if (xoaAnhHienTai) fd.append('xoaAnh', 'true');
    } else {
      url = '/api/sanpham';
      phuongThuc = 'POST';
    }

    goiApi(phuongThuc, url, fd)
      .then(function () {
        hienToast(dangSua ? 'Đã cập nhật món "' + ten + '".' : 'Đã thêm món "' + ten + '".');
        return taiDanhSachMon();
      })
      .then(function () {
        renderDsMonNho();
        renderDanhSachDon();
        resetForm();
      })
      .catch(function (err) {
        hienToast(err.message || 'Không lưu được món.', true);
      });
  });

  oXoaMon.addEventListener('click', function () {
    var id = Number(oMonId.value);
    var mon = danhSachMon.find(function (m) { return m.id === id; });
    if (!mon) return;
    if (!window.confirm('Xóa món "' + mon.ten + '"?')) return;

    goiApi('DELETE', '/api/sanpham/' + id)
      .then(function () {
        hienToast('Đã xóa món "' + mon.ten + '".');
        return taiDanhSachMon();
      })
      .then(function () {
        renderDsMonNho();
        renderDanhSachDon();
        resetForm();
      })
      .catch(function (err) {
        hienToast(err.message || 'Không xóa được món.', true);
      });
  });

  // ========================================================
  // Header: menu điều hướng, avatar, các nút "đang phát triển"
  // ========================================================

  document.querySelectorAll('.dh-nav__muc').forEach(function (muc) {
    muc.addEventListener('click', function (e) {
      e.preventDefault();
      document.querySelectorAll('.dh-nav__muc').forEach(function (m) { m.classList.remove('dh-nav__muc--dang-chon'); });
      muc.classList.add('dh-nav__muc--dang-chon');
    });
  });

  document.getElementById('nut-chuong').addEventListener('click', function () {
    hienToast('Không có thông báo mới.');
  });

  var nutAvatar = document.getElementById('nut-avatar');
  var dropdownAvatar = document.getElementById('dropdown-avatar');
  nutAvatar.addEventListener('click', function () {
    var dangMo = !dropdownAvatar.hidden;
    dropdownAvatar.hidden = dangMo;
    nutAvatar.setAttribute('aria-expanded', String(!dangMo));
  });
  document.addEventListener('click', function (e) {
    if (!dropdownAvatar.hidden && !e.target.closest('.dh-header__phai')) {
      dropdownAvatar.hidden = true;
      nutAvatar.setAttribute('aria-expanded', 'false');
    }
  });
  dropdownAvatar.addEventListener('click', function (e) {
    var nut = e.target.closest('[data-hanh-dong]');
    if (!nut) return;
    if (nut.dataset.hanhDong === 'dang-xuat') {
      if (window.confirm('Đăng xuất khỏi trang quản trị?')) {
        location.href = '/';
      }
    }
    dropdownAvatar.hidden = true;
  });

  document.querySelectorAll('.nut-tac-vu[data-thong-bao]').forEach(function (nut) {
    nut.addEventListener('click', function () { hienToast(nut.dataset.thongBao); });
  });

  // ========================================================
  // MODAL: Sale theo ngày
  // ========================================================

  var modalSale = document.getElementById('modal-sale');
  var formSale = document.getElementById('form-sale');
  var oSaleId = document.getElementById('sale-id');
  var oSaleSpId = document.getElementById('sale-sp-id');
  var oSaleTenMon = document.getElementById('sale-ten-mon');
  var oSaleGiaGoc = document.getElementById('sale-gia-goc');
  var oSaleBatDau = document.getElementById('sale-bat-dau');
  var oSaleKetThuc = document.getElementById('sale-ket-thuc');
  var oSalePhanTram = document.getElementById('sale-phan-tram');
  var oSaleXemTruocGia = document.getElementById('sale-xem-truoc-gia');
  var oSaleMonDangChon = document.getElementById('sale-mon-dang-chon');
  var oNutXoaSale = document.getElementById('nut-xoa-sale');

  var danhSachSale = [];
  var saleSpDangChon = null; // object món đang được chọn

  function dinhDangThoiGianLocal(dt) {
    // Chuyển Date thành chuỗi "YYYY-MM-DDTHH:mm:ss" phù hợp datetime-local
    var pad = function(n) { return n < 10 ? '0' + n : '' + n; };
    return dt.getFullYear() + '-' + pad(dt.getMonth() + 1) + '-' + pad(dt.getDate()) +
      'T' + pad(dt.getHours()) + ':' + pad(dt.getMinutes()) + ':' + pad(dt.getSeconds());
  }

  function taiDanhSachSale() {
    return goiApi('GET', '/api/sale').then(function (data) {
      danhSachSale = data;
    });
  }

  function moModalSale() {
    modalSale.hidden = false;
    resetFormSale();

    // Render ngay nếu danhSachMon đã có sẵn (tải lúc khởi động)
    if (danhSachMon.length > 0) {
      renderSaleMonDs();
    }

    // Tải danh sách món (nếu chưa có)
    var pMon = danhSachMon.length > 0
      ? Promise.resolve()
      : taiDanhSachMon().catch(function (err) {
          console.error('Lỗi tải danh sách món:', err);
        });

    // Tải danh sách sale — riêng biệt, lỗi không làm mất danh sách món
    var pSale = taiDanhSachSale().catch(function (err) {
      console.error('Lỗi tải danh sách sale:', err);
      danhSachSale = [];
    });

    pMon.then(function () {
      renderSaleMonDs(); // render món ngay sau khi món về
    });

    Promise.all([pMon, pSale]).then(function () {
      renderSaleMonDs();
      renderSaleDs();
    });
  }

  function dongModalSale() { modalSale.hidden = true; }

  document.getElementById('nut-sale-theo-ngay').addEventListener('click', moModalSale);
  document.getElementById('nut-dong-modal-sale').addEventListener('click', dongModalSale);
  modalSale.addEventListener('click', function (e) { if (e.target === modalSale) dongModalSale(); });
  document.addEventListener('keydown', function (e) {
    if (e.key === 'Escape' && !modalSale.hidden) dongModalSale();
  });

  function resetFormSale() {
    formSale.reset();
    oSaleId.value = '';
    oSaleSpId.value = '';
    saleSpDangChon = null;
    oSaleMonDangChon.hidden = true;
    oNutXoaSale.hidden = true;

    // Điền giờ mặc định: bắt đầu = bây giờ, kết thúc = +1 ngày
    var bayGio = new Date();
    var maiNgay = new Date(bayGio.getTime() + 86400000);
    oSaleBatDau.value = dinhDangThoiGianLocal(bayGio);
    oSaleKetThuc.value = dinhDangThoiGianLocal(maiNgay);
    oSaleXemTruocGia.textContent = '';
  }

  document.getElementById('nut-tao-sale-moi').addEventListener('click', function () {
    resetFormSale();
    // Bỏ chọn tất cả các món
    document.querySelectorAll('#sale-mon-ds .mon-the-nho').forEach(function (el) {
      el.classList.remove('mon-the-nho--dang-chon');
    });
    // Bỏ chọn tất cả các sale the
    document.querySelectorAll('#sale-ds-the .sale-the').forEach(function (el) {
      el.classList.remove('sale-the--dang-chon');
    });
  });

  function renderSaleMonDs() {
    var vung = document.getElementById('sale-mon-ds');
    vung.innerHTML = danhSachMon.map(function (mon) {
      var dangChon = saleSpDangChon && saleSpDangChon.id === mon.id ? ' mon-the-nho--dang-chon' : '';
      return '<button type="button" class="mon-the-nho' + dangChon + '" data-sale-mon-id="' + mon.id + '">' +
        '<img class="mon-the-nho__anh" src="' + thoatHtml(anhHienThiCuaMon(mon)) + '" alt="' + thoatHtml(mon.ten) + '" />' +
        thoatHtml(mon.ten) + '</button>';
    }).join('');
  }

  document.getElementById('sale-mon-ds').addEventListener('click', function (e) {
    var nut = e.target.closest('[data-sale-mon-id]');
    if (!nut) return;
    var mon = danhSachMon.find(function (m) { return m.id === Number(nut.dataset.saleMonId); });
    if (!mon) return;

    saleSpDangChon = mon;
    oSaleSpId.value = mon.id;
    oSaleTenMon.textContent = mon.ten;
    oSaleGiaGoc.textContent = 'Giá gốc: ' + dinhDangTien(mon.gia);
    oSaleMonDangChon.hidden = false;

    // Làm nổi nút được chọn
    document.querySelectorAll('#sale-mon-ds .mon-the-nho').forEach(function (el) {
      el.classList.remove('mon-the-nho--dang-chon');
    });
    nut.classList.add('mon-the-nho--dang-chon');

    capNhatXemTruocGia();
  });

  function capNhatXemTruocGia() {
    if (!saleSpDangChon) return;
    var pct = parseFloat(oSalePhanTram.value);
    if (isNaN(pct) || pct <= 0 || pct > 100) {
      oSaleXemTruocGia.textContent = '';
      return;
    }
    var giaSau = Math.round(saleSpDangChon.gia * (1 - pct / 100));
    oSaleXemTruocGia.textContent = '→ ' + dinhDangTien(giaSau);
  }

  oSalePhanTram.addEventListener('input', capNhatXemTruocGia);

  function renderSaleDs() {
    var vung = document.getElementById('sale-ds-the');
    var khu = document.getElementById('sale-ds-hien-tai');
    if (!danhSachSale || danhSachSale.length === 0) {
      khu.hidden = true;
      return;
    }
    khu.hidden = false;

    var nhanTrangThai = function(tt) {
      switch(tt) {
        case 'dang_chay': return '<span class="sale-the__tag sale-the__tag--dang">Đang chạy</span>';
        case 'chua_bat_dau': return '<span class="sale-the__tag sale-the__tag--chua">Chưa bắt đầu</span>';
        default: return '<span class="sale-the__tag sale-the__tag--xong">Đã kết thúc</span>';
      }
    };

    var dinhDangNgayGio = function(isoStr) {
      if (!isoStr) return '';
      var d = new Date(isoStr);
      var pad = function(n) { return n < 10 ? '0' + n : '' + n; };
      return pad(d.getDate()) + '/' + pad(d.getMonth() + 1) + '/' + d.getFullYear() +
        ' ' + pad(d.getHours()) + ':' + pad(d.getMinutes());
    };

    vung.innerHTML = danhSachSale.map(function (s) {
      return '<div class="sale-the" data-sale-id="' + s.id + '">' +
        '<img class="sale-the__anh" src="' + thoatHtml(s.anhSanPham || '') + '" alt="' + thoatHtml(s.tenSanPham) + '" />' +
        '<div class="sale-the__info">' +
          '<div class="sale-the__ten">' + thoatHtml(s.tenSanPham) + ' — giảm ' + s.phanTramGiam + '%</div>' +
          '<div class="sale-the__meta">' + dinhDangNgayGio(s.thoiGianBatDau) + ' → ' + dinhDangNgayGio(s.thoiGianKetThuc) + '</div>' +
        '</div>' +
        nhanTrangThai(s.trangThai) +
        '</div>';
    }).join('');
  }

  document.getElementById('sale-ds-the').addEventListener('click', function (e) {
    var the = e.target.closest('.sale-the');
    if (!the) return;
    var saleItem = danhSachSale.find(function (s) { return s.id === Number(the.dataset.saleId); });
    if (!saleItem) return;

    napSaleVaoForm(saleItem);

    document.querySelectorAll('#sale-ds-the .sale-the').forEach(function (el) {
      el.classList.remove('sale-the--dang-chon');
    });
    the.classList.add('sale-the--dang-chon');
  });

  function napSaleVaoForm(sale) {
    oSaleId.value = sale.id;
    oSaleSpId.value = sale.idSanPham;
    oSalePhanTram.value = sale.phanTramGiam;

    var mon = danhSachMon.find(function (m) { return m.id === sale.idSanPham; });
    saleSpDangChon = mon || { id: sale.idSanPham, ten: sale.tenSanPham, gia: sale.giaSanPham };

    oSaleTenMon.textContent = sale.tenSanPham;
    oSaleGiaGoc.textContent = 'Giá gốc: ' + dinhDangTien(sale.giaSanPham);
    oSaleMonDangChon.hidden = false;
    oNutXoaSale.hidden = false;

    // Chuyển ISO string sang local datetime-local
    var toLocal = function (iso) {
      if (!iso) return '';
      var d = new Date(iso);
      return dinhDangThoiGianLocal(d);
    };
    oSaleBatDau.value = toLocal(sale.thoiGianBatDau);
    oSaleKetThuc.value = toLocal(sale.thoiGianKetThuc);

    capNhatXemTruocGia();

    // Đánh dấu món trong danh sách
    document.querySelectorAll('#sale-mon-ds .mon-the-nho').forEach(function (el) {
      var id = Number(el.dataset.saleMonId);
      if (id === sale.idSanPham) el.classList.add('mon-the-nho--dang-chon');
      else el.classList.remove('mon-the-nho--dang-chon');
    });
  }

  // Lưu sale
  formSale.addEventListener('submit', function (e) {
    e.preventDefault();

    var spId = Number(oSaleSpId.value);
    if (!spId) { hienToast('Vui lòng chọn một món để sale.', true); return; }

    var pct = parseFloat(oSalePhanTram.value);
    if (isNaN(pct) || pct <= 0 || pct > 100) { hienToast('Phần trăm giảm phải từ 1 đến 100.', true); return; }

    var bd = oSaleBatDau.value;
    var kt = oSaleKetThuc.value;
    if (!bd || !kt) { hienToast('Vui lòng chọn thời gian bắt đầu và kết thúc.', true); return; }
    if (new Date(bd) >= new Date(kt)) { hienToast('Thời gian bắt đầu phải trước kết thúc.', true); return; }

    var payload = {
      idSanPham: spId,
      phanTramGiam: pct,
      thoiGianBatDau: new Date(bd).toISOString(),
      thoiGianKetThuc: new Date(kt).toISOString()
    };

    var dangSua = oSaleId.value !== '';
    var url = dangSua ? '/api/sale/' + oSaleId.value : '/api/sale';
    var phuongThuc = dangSua ? 'PUT' : 'POST';

    goiApi(phuongThuc, url, payload)
      .then(function () {
        hienToast(dangSua ? 'Đã cập nhật chương trình sale.' : 'Đã tạo chương trình sale mới.');
        return taiDanhSachSale();
      })
      .then(function () {
        renderSaleDs();
        resetFormSale();
        document.querySelectorAll('#sale-mon-ds .mon-the-nho').forEach(function (el) {
          el.classList.remove('mon-the-nho--dang-chon');
        });
      })
      .catch(function (err) {
        hienToast(err.message || 'Không lưu được sale.', true);
      });
  });

  // Xóa sale
  oNutXoaSale.addEventListener('click', function () {
    var id = Number(oSaleId.value);
    if (!id) return;
    if (!window.confirm('Xóa chương trình sale này?')) return;
    goiApi('DELETE', '/api/sale/' + id)
      .then(function () {
        hienToast('Đã xóa chương trình sale.');
        return taiDanhSachSale();
      })
      .then(function () {
        renderSaleDs();
        resetFormSale();
      })
      .catch(function (err) {
        hienToast(err.message || 'Không xóa được sale.', true);
      });
  });

  // ========================================================
  // Khởi động: tải dữ liệu thật từ server
  // ========================================================
  Promise.all([taiDanhSachDon(), taiDanhSachMon()])
    .then(function () {
      renderDanhSachDon();
    })
    .catch(function (err) {
      hienToast('Không tải được dữ liệu từ server.', true);
      console.error(err);
    });
})();
