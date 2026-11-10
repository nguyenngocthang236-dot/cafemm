/* =========================================================
   ho-tro-khach.js - Nút "Hỗ trợ" + khung chat với nhân viên (giao diện khách)
   ĐẶT VÀO: wwwroot/js/ho-tro-khach.js   (file mới)

   Cách hoạt động: cứ vài giây hỏi máy chủ xem có tin mới không (không cần SignalR hay thư viện ngoài).
     - Khung chat đang mở : hỏi mỗi 2,5 giây
     - Khung chat đang đóng: hỏi mỗi 8 giây, chỉ để cập nhật số tin chưa đọc trên nút
   ========================================================= */
(function () {
  'use strict';

  var nut = document.querySelector('[data-ho-tro-nut]');
  var nen = document.querySelector('[data-ho-tro-nen]');
  if (!nut || !nen) return; // trang không có nút hỗ trợ (chưa quét QR)

  var dsTin = nen.querySelector('[data-ho-tro-ds]');
  var elTrong = nen.querySelector('[data-ho-tro-trong]');
  var elLoi = nen.querySelector('[data-ho-tro-loi]');
  var form = nen.querySelector('[data-ho-tro-form]');
  var oNhap = nen.querySelector('[data-ho-tro-nhap]');
  var nutGui = nen.querySelector('[data-ho-tro-gui]');
  var nutDong = nen.querySelector('[data-ho-tro-dong]');
  var elSo = nut.querySelector('[data-ho-tro-so]');

  var URL_TIN = nut.dataset.urlTin;
  var URL_GUI = nut.dataset.urlGui;

  var KHOANG_MO = 2500;     // khung chat đang mở
  var KHOANG_DONG = 8000;   // khung chat đang đóng
  var KHOANG_AN = 20000;    // tab đang bị ẩn
  var KHOANG_LOI = 15000;   // mất mạng / hết phiên: thử lại thưa hơn

  var daVe = {};            // id các tin đã hiện trên màn hình (tránh hiện trùng)
  var soTinDaVe = 0;
  var idCuoi = 0;           // id tin lớn nhất đã lấy về từ máy chủ
  var dangMo = false;
  var dangGui = false;
  var dangHoi = false;      // đang có một lượt hỏi chưa xong
  var hoiLai = false;       // trong lúc đó có yêu cầu hỏi lại ngay (vd. vừa mở khung chat)
  var henHoi = null;
  var henLoi = null;
  var laCamUng = !!(window.matchMedia && window.matchMedia('(pointer: coarse)').matches);

  function doiJson(res) {
    return res.text().then(function (text) {
      try { return JSON.parse(text); } catch (e) { return null; }
    });
  }

  // ---------- Hiển thị ----------
  function veTin(t) {
    var hang = document.createElement('div');
    hang.className = 'ho-tro-hang ho-tro-hang--' + (t.tu === 'khach' ? 'minh' : 'nhanvien');
    var bong = document.createElement('div');
    bong.className = 'ho-tro-bong';
    var nd = document.createElement('div');
    nd.className = 'ho-tro-bong__nd';
    nd.textContent = t.nd;           // textContent: nội dung khách gõ không bao giờ chạy như HTML
    var gio = document.createElement('div');
    gio.className = 'ho-tro-bong__gio';
    gio.textContent = t.luc;
    bong.appendChild(nd);
    bong.appendChild(gio);
    hang.appendChild(bong);
    return hang;
  }

  function dangONhauDay() {
    return dsTin.scrollHeight - dsTin.scrollTop - dsTin.clientHeight < 80;
  }
  function cuonXuongDay() { dsTin.scrollTop = dsTin.scrollHeight; }

  // Thêm tin vào màn hình (bỏ qua tin đã có). Trả về true nếu có tin mới.
  function themTin(ds) {
    var coMoi = false;
    ds.forEach(function (t) {
      if (daVe[t.id]) return;
      daVe[t.id] = true;
      soTinDaVe += 1;
      dsTin.appendChild(veTin(t));
      coMoi = true;
    });
    if (coMoi) elTrong.hidden = soTinDaVe > 0;
    return coMoi;
  }

  function capNhatSo(n) {
    elSo.hidden = n <= 0;
    elSo.textContent = n > 9 ? '9+' : String(n);
    nut.setAttribute('aria-label', n > 0
      ? 'Mở khung chat hỗ trợ, có ' + n + ' tin nhắn mới'
      : 'Mở khung chat hỗ trợ');
  }

  function baoLoi(noiDung) {
    elLoi.textContent = noiDung;
    elLoi.hidden = false;
    clearTimeout(henLoi);
    henLoi = setTimeout(function () { elLoi.hidden = true; }, 5000);
  }

  function chinhCaoONhap() {
    oNhap.style.height = 'auto';
    oNhap.style.height = Math.min(oNhap.scrollHeight, 120) + 'px';
  }

  // ---------- Hỏi máy chủ ----------
  function hoi() {
    clearTimeout(henHoi);
    if (dangHoi) { hoiLai = true; return; }
    dangHoi = true;
    hoiLai = false;

    var url = URL_TIN + '?sauId=' + idCuoi + '&dangMo=' + (dangMo ? 'true' : 'false');
    fetch(url, { cache: 'no-store' })
      .then(doiJson)
      .then(function (d) {
        // Hết phiên bàn thì máy chủ trả về trang "quét lại mã QR" chứ không phải JSON
        if (!d || !Array.isArray(d.tin)) throw new Error('khong-hop-le');

        var gan = dangONhauDay();
        d.tin.forEach(function (t) { if (t.id > idCuoi) idCuoi = t.id; });
        var coMoi = themTin(d.tin);
        if (coMoi && dangMo && gan) cuonXuongDay();

        capNhatSo(dangMo ? 0 : (Number(d.chuaDoc) || 0));
        return document.hidden ? KHOANG_AN : (dangMo ? KHOANG_MO : KHOANG_DONG);
      })
      .catch(function () { return KHOANG_LOI; })
      .then(function (cho) {
        dangHoi = false;
        henHoi = setTimeout(hoi, hoiLai ? 0 : cho);
      });
  }

  // ---------- Gửi tin ----------
  function gui() {
    var noiDung = oNhap.value.trim();
    if (!noiDung || dangGui) return;

    dangGui = true;
    nutGui.disabled = true;

    fetch(URL_GUI, { method: 'POST', body: new URLSearchParams({ noiDung: noiDung }) })
      .then(doiJson)
      .then(function (kq) {
        if (!kq) throw new Error('Phiên của bàn đã kết thúc hoặc mất kết nối. Hãy tải lại trang hoặc quét lại mã QR ở bàn.');
        if (!kq.ok) throw new Error(kq.thongBao || 'Chưa gửi được tin nhắn.');
        oNhap.value = '';
        chinhCaoONhap();
        // Tin vừa gửi hiện ngay; nếu lượt hỏi sau trả về đúng tin này thì bị bỏ qua nhờ daVe
        themTin([kq.tin]);
        cuonXuongDay();
      })
      .catch(function (err) {
        baoLoi(err.message && err.message.indexOf('Failed') === -1
          ? err.message
          : 'Chưa gửi được tin nhắn. Bạn thử lại nhé.');
      })
      .then(function () {
        dangGui = false;
        nutGui.disabled = false;
        if (!laCamUng) oNhap.focus();
      });
  }

  // ---------- Mở / đóng ----------
  // Bàn phím ảo làm màn hình điện thoại bị thu nhỏ: ép khung chat theo vùng nhìn thấy để ô nhập không bị che
  function chinhTheoVungNhin() {
    if (!dangMo) return;
    var vv = window.visualViewport;
    nen.style.top = vv ? vv.offsetTop + 'px' : '';
    nen.style.height = vv ? vv.height + 'px' : '';
  }

  function mo() {
    nen.hidden = false;
    dangMo = true;
    document.documentElement.classList.add('ho-tro-khoa');
    capNhatSo(0);
    chinhTheoVungNhin();
    cuonXuongDay();
    if (!laCamUng) oNhap.focus();   // điện thoại: không tự bật bàn phím che mất tin nhắn
    hoi();                          // hỏi ngay để đánh dấu "đã đọc"
  }

  function dong() {
    nen.hidden = true;
    dangMo = false;
    nen.style.top = '';
    nen.style.height = '';
    document.documentElement.classList.remove('ho-tro-khoa');
    nut.focus();
  }

  nut.addEventListener('click', mo);
  nutDong.addEventListener('click', dong);
  // mousedown (không phải click): kéo bôi đen chữ trong ô nhập rồi thả chuột ra ngoài sẽ không làm đóng khung
  nen.addEventListener('mousedown', function (e) { if (e.target === nen) dong(); });
  document.addEventListener('keydown', function (e) { if (e.key === 'Escape' && dangMo) dong(); });

  form.addEventListener('submit', function (e) { e.preventDefault(); gui(); });
  oNhap.addEventListener('input', chinhCaoONhap);
  oNhap.addEventListener('keydown', function (e) {
    // Máy tính: Enter gửi, Shift+Enter xuống dòng. Điện thoại: Enter xuống dòng, bấm nút gửi.
    if (e.key === 'Enter' && !e.shiftKey && !e.isComposing && !laCamUng) {
      e.preventDefault();
      gui();
    }
  });

  if (window.visualViewport) {
    window.visualViewport.addEventListener('resize', chinhTheoVungNhin);
    window.visualViewport.addEventListener('scroll', chinhTheoVungNhin);
  }
  document.addEventListener('visibilitychange', function () { if (!document.hidden) hoi(); });

  // Lần hỏi đầu tiên: tải sẵn lịch sử + số tin chưa đọc (chờ chút cho trang tải xong)
  henHoi = setTimeout(hoi, 800);
})();
