/* Trang "Mã QR đặt món theo bàn": vẽ mã QR cho từng bàn ngay trên trình duyệt (không cần Internet).
   Mỗi mã QR trỏ tới  <địa chỉ gốc>/Ban/Quet/<idban>  -> BanController.Quet tạo phiên cho bàn đó
   rồi chuyển khách tới trang thực đơn.
   Danh sách bàn lấy từ dữ liệu khởi tạo của trang, có thể thêm/xóa ngay tại đây (gọi BanController). */
(function () {
  'use strict';

  var oDiaChi = document.getElementById('dia-chi-goc');
  var oCanhBao = document.getElementById('canh-bao');
  var oLuoi = document.getElementById('luoi-qr');
  var oChuaCoBan = document.getElementById('chua-co-ban');
  var mauThe = document.getElementById('mau-the-qr');
  var KHOA_LUU = 'quancafe.qr.diaChiGoc';

  var danhSachBan = JSON.parse(document.getElementById('du-lieu-ban-ban-dau').textContent || '[]');

  function docLuu() {
    try { return window.localStorage.getItem(KHOA_LUU) || ''; } catch (e) { return ''; }
  }
  function ghiLuu(v) {
    try { window.localStorage.setItem(KHOA_LUU, v); } catch (e) { /* bỏ qua */ }
  }

  function laMayNoiBo(host) {
    return host === 'localhost' || host === '127.0.0.1' || host === '[::1]' || host === '::1';
  }

  // Chấp nhận "192.168.1.10:5213" (tự thêm http://), bỏ dấu "/" cuối, bỏ query/hash.
  function chuanHoaGoc(chu) {
    chu = (chu || '').trim();
    if (!chu) return null;
    if (!/^https?:\/\//i.test(chu)) chu = 'http://' + chu;
    try {
      var u = new URL(chu);
      return { goc: u.origin + u.pathname.replace(/\/+$/, ''), host: u.hostname };
    } catch (e) {
      return null;
    }
  }

  function taoSvg(noiDung) {
    var qr = new QRCode(-1, QRCode.ErrorCorrectLevel.M);   // -1: tự chọn kích thước nhỏ nhất đủ chứa
    qr.addData(noiDung);
    qr.make();
    var n = qr.getModuleCount();
    var le = 4;                                            // vùng trắng quanh mã (chuẩn QR: 4 ô)
    var tong = n + le * 2;
    var d = '';
    for (var r = 0; r < n; r++) {
      var c = 0;
      while (c < n) {
        if (qr.isDark(r, c)) {
          var dau = c;
          while (c < n && qr.isDark(r, c)) c++;
          d += 'M' + (dau + le) + ' ' + (r + le) + 'h' + (c - dau) + 'v1h-' + (c - dau) + 'z';
        } else {
          c++;
        }
      }
    }
    return '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 ' + tong + ' ' + tong + '"' +
           ' shape-rendering="crispEdges" role="img" aria-label="Mã QR">' +
           '<rect width="' + tong + '" height="' + tong + '" fill="#fff"/>' +
           '<path d="' + d + '" fill="#000"/></svg>';
  }

  function hienCanhBao(chu) {
    if (!oCanhBao) return;
    if (chu) { oCanhBao.innerHTML = chu; oCanhBao.hidden = false; }
    else { oCanhBao.textContent = ''; oCanhBao.hidden = true; }
  }

  // ---- Vẽ danh sách thẻ bàn từ mảng danhSachBan (mỗi phần tử: { id, maBan }) ----
  function veLuoiThe() {
    oLuoi.innerHTML = '';
    oChuaCoBan.hidden = danhSachBan.length !== 0;
    danhSachBan.forEach(function (ban) {
      var the = mauThe.content.firstElementChild.cloneNode(true);
      the.setAttribute('data-idban', ban.id);
      the.querySelector('[data-ten-ban]').textContent = 'Bàn ' + ban.maBan;
      oLuoi.appendChild(the);
    });
    veMaTatCa();
  }

  // ---- Chỉ vẽ lại phần mã QR (không dựng lại khung thẻ) khi đổi địa chỉ website ----
  function veMaTatCa() {
    var kq = chuanHoaGoc(oDiaChi.value);
    var theTrenTrang = oLuoi.querySelectorAll('.the-qr');

    if (!kq) {
      hienCanhBao('Địa chỉ website chưa hợp lệ. Ví dụ đúng: <code>http://192.168.1.10:5213</code>');
      theTrenTrang.forEach(function (the) {
        the.querySelector('[data-qr]').innerHTML = '';
        the.querySelector('[data-url]').textContent = '';
      });
      return;
    }

    hienCanhBao('');
    theTrenTrang.forEach(function (the) {
      var url = kq.goc + '/Ban/Quet/' + the.getAttribute('data-idban');
      the.querySelector('[data-qr]').innerHTML = taoSvg(url);
      var lienKet = the.querySelector('[data-url]');
      lienKet.textContent = url;
      lienKet.href = url;
    });
  }

  // ---- Gọi API (dùng FormData, giống các API khác trong trang quản lý) ----
  function goiApi(url, tuyChon) {
    return fetch(url, tuyChon).then(function (res) {
      // Đọc dạng chữ rồi tự parse: tránh lỗi "Unexpected token..." khi server trả về rỗng/HTML.
      return res.text().then(function (text) {
        var data = null;
        if (text) {
          try { data = JSON.parse(text); } catch (e) { data = null; }
        }
        if (!res.ok) {
          throw new Error((data && data.loi) || ('Lỗi máy chủ (HTTP ' + res.status + ').'));
        }
        return data;
      });
    });
  }

  function hienLoi(chu) {
    window.alert(chu);
  }

  // ---- Thêm bàn mới ----
  var oMaBanMoi = document.getElementById('ma-ban-moi');
  var oNutThemBan = document.getElementById('nut-them-ban');

  function themBan() {
    var maBan = oMaBanMoi.value.trim();
    if (!maBan) { hienLoi('Vui lòng nhập mã bàn.'); oMaBanMoi.focus(); return; }

    oNutThemBan.disabled = true;
    var fd = new FormData();
    fd.append('maBan', maBan);

    goiApi('/api/ban', { method: 'POST', body: fd })
      .then(function (ban) {
        danhSachBan.push({ id: ban.id, maBan: ban.maBan });
        danhSachBan.sort(function (a, b) { return a.maBan.localeCompare(b.maBan, 'vi'); });
        veLuoiThe();
        oMaBanMoi.value = '';
        oMaBanMoi.focus();
      })
      .catch(function (err) { hienLoi(err.message); })
      .finally(function () { oNutThemBan.disabled = false; });
  }

  oNutThemBan.addEventListener('click', themBan);
  oMaBanMoi.addEventListener('keydown', function (e) { if (e.key === 'Enter') { e.preventDefault(); themBan(); } });

  // ---- Xóa bàn (nút × trên mỗi thẻ) ----
  oLuoi.addEventListener('click', function (e) {
    var nutXoa = e.target.closest('.the-qr__xoa');
    if (!nutXoa) return;
    var the = nutXoa.closest('.the-qr');
    var idban = Number(the.getAttribute('data-idban'));
    var ban = danhSachBan.find(function (b) { return b.id === idban; });
    var tenBan = ban ? ban.maBan : idban;

    if (!window.confirm('Xóa bàn ' + tenBan + '? Mã QR của bàn này sẽ không dùng được nữa.')) return;

    goiApi('/api/ban/' + idban, { method: 'DELETE' })
      .then(function () {
        danhSachBan = danhSachBan.filter(function (b) { return b.id !== idban; });
        veLuoiThe();
      })
      .catch(function (err) { hienLoi(err.message); });
  });

  // Địa chỉ mặc định: ?goc=... > địa chỉ đang mở trang này > địa chỉ đã lưu (nếu đang mở bằng localhost)
  var thamSo = new URLSearchParams(window.location.search).get('goc');
  var hienTai = window.location.origin;
  var macDinh = thamSo || hienTai;
  if (!thamSo && laMayNoiBo(window.location.hostname) && docLuu()) macDinh = docLuu();
  oDiaChi.value = macDinh;

  document.getElementById('nut-tao-lai').addEventListener('click', function () {
    ghiLuu(oDiaChi.value.trim());
    veMaTatCa();
  });
  oDiaChi.addEventListener('keydown', function (e) {
    if (e.key === 'Enter') { e.preventDefault(); ghiLuu(oDiaChi.value.trim()); veMaTatCa(); }
  });
  document.getElementById('nut-in').addEventListener('click', function () { window.print(); });

  veLuoiThe();
})();
