/* =========================================================
   ho-tro-quan-ly.js - Nút "Yêu cầu hỗ trợ" + chat với từng bàn (Dashboard quản lý)
   ĐẶT VÀO: wwwroot/js/ho-tro-quan-ly.js   (file mới)
   Nạp SAU dashboard.js. File này tự chứa mọi thứ nó cần, không phụ thuộc hàm trong dashboard.js.

   Hỏi máy chủ định kỳ (không cần SignalR):
     - Danh sách bàn + số chưa đọc: 4 giây/lần khi popup mở, 6 giây/lần khi đóng (để số đỏ trên nút luôn mới)
     - Khung chat của một bàn      : 2 giây/lần
   ========================================================= */
(function () {
  'use strict';

  var nut = document.getElementById('nut-ho-tro');
  var nen = document.getElementById('modal-ho-tro');
  if (!nut || !nen) return;

  var URL_BAN = '/api/hotro/ban';
  var KHOANG_DS_MO = 4000;
  var KHOANG_DS_DONG = 6000;
  var KHOANG_CHAT = 2000;

  var manBan = document.getElementById('ho-tro-man-ban');
  var manChat = document.getElementById('ho-tro-man-chat');
  var luoi = document.getElementById('ho-tro-luoi');
  var dsTin = document.getElementById('ho-tro-tin');
  var ghiChu = document.getElementById('ho-tro-ghi-chu');
  var form = document.getElementById('ho-tro-form');
  var oNhap = document.getElementById('ho-tro-nhap');
  var nutGui = document.getElementById('ho-tro-gui');
  var nutDong = document.getElementById('nut-dong-modal-ho-tro');
  var nutQuayLai = document.getElementById('nut-ho-tro-quay-lai');
  var tieuDe = document.getElementById('ho-tro-tieu-de');

  var TIEU_DE_TRANG = document.title;
  var TIEU_DE_MAC_DINH = 'Yêu cầu hỗ trợ';

  var danhSachBan = [];       // [{id, maBan, coKhach, chuaDoc}]
  var dauDanhSach = '';       // "dấu vân tay" của lần vẽ trước, chỉ vẽ lại lưới khi dữ liệu đổi
  var dangMo = false;
  var banDangChat = null;     // {id, maBan, coKhach} hoặc null khi đang ở màn danh sách
  var luot = 0;               // tăng mỗi lần đổi bàn / thoát chat: bỏ qua kết quả của lượt hỏi cũ
  var daVe = {};
  var soTinDaVe = 0;
  var idCuoi = 0;
  var dangGui = false;
  var henDs = null, henChat = null, henGhiChu = null;
  var dangHoiDs = false, hoiDsLai = false;   // chặn chạy 2 chuỗi hỏi danh sách song song
  var elTrong = null;         // dòng "Chưa có tin nhắn nào"

  // Số nhỏ viền đỏ trên biểu tượng của nút "Yêu cầu hỗ trợ"
  var cham = document.createElement('span');
  cham.className = 'ho-tro-so';
  cham.hidden = true;
  nut.appendChild(cham);

  // ---------- Gọi API ----------
  function goi(phuongThuc, url, body) {
    var tuyChon = { method: phuongThuc, cache: 'no-store' };
    if (body) tuyChon.body = body;
    return fetch(url, tuyChon).then(function (res) {
      return res.text().then(function (text) {
        var data = null;
        if (text) { try { data = JSON.parse(text); } catch (e) { data = null; } }
        if (!res.ok) {
          throw new Error((data && (data.loi || data.title)) || ('Lỗi máy chủ (HTTP ' + res.status + ').'));
        }
        return data;
      });
    });
  }

  function dinhDangSo(n) { return n > 99 ? '99+' : String(n); }

  function tao(the, lop, chu) {
    var el = document.createElement(the);
    if (lop) el.className = lop;
    if (chu != null) el.textContent = chu;
    return el;
  }

  // ---------- Số chưa đọc trên nút + tiêu đề tab ----------
  function capNhatCham() {
    var tong = 0;
    danhSachBan.forEach(function (b) { tong += b.chuaDoc; });
    cham.hidden = tong <= 0;
    cham.textContent = dinhDangSo(tong);
    document.title = tong > 0 ? '(' + tong + ') ' + TIEU_DE_TRANG : TIEU_DE_TRANG; // thấy ngay cả khi đang ở tab khác
  }

  // ---------- Màn 1: lưới các bàn ----------
  function veDanhSachBan(ep) {
    var dau = JSON.stringify(danhSachBan);
    if (!ep && dau === dauDanhSach) return;   // không đổi gì thì khỏi vẽ lại (giữ nguyên vị trí con trỏ / hover)
    dauDanhSach = dau;

    luoi.textContent = '';
    danhSachBan.forEach(function (b) {
      var o = tao('button', 'ho-tro-ban' + (b.coKhach ? ' ho-tro-ban--co-khach' : ''));
      o.type = 'button';
      o.appendChild(tao('span', 'ho-tro-ban__ma', b.maBan));
      o.appendChild(tao('span', 'ho-tro-ban__tt', b.coKhach ? 'Có khách' : 'Trống'));
      if (b.chuaDoc > 0) {
        var so = tao('span', 'ho-tro-so', dinhDangSo(b.chuaDoc));
        so.setAttribute('aria-label', b.chuaDoc + ' tin chưa đọc');
        o.appendChild(so);
      }
      o.addEventListener('click', function () { moChat(b); });
      luoi.appendChild(o);
    });
  }

  function hoiDanhSach() {
    clearTimeout(henDs);
    // Đang có một lượt hỏi chưa xong (vd. vừa mở/đóng popup liên tục): chỉ ghi nhớ "hỏi lại ngay"
    // chứ không mở thêm chuỗi hỏi thứ hai, nếu không số lượt hỏi sẽ nhân đôi dần.
    if (dangHoiDs) { hoiDsLai = true; return; }
    dangHoiDs = true;
    hoiDsLai = false;

    goi('GET', URL_BAN)
      .then(function (d) {
        danhSachBan = (d && d.ban) || [];
        capNhatCham();
        if (dangMo && !banDangChat) veDanhSachBan();
      })
      .catch(function () { /* mất mạng tạm thời: lần sau thử lại */ })
      .then(function () {
        dangHoiDs = false;
        henDs = setTimeout(hoiDanhSach, hoiDsLai ? 0 : (dangMo ? KHOANG_DS_MO : KHOANG_DS_DONG));
      });
  }

  // ---------- Màn 2: chat với một bàn ----------
  function veTin(t) {
    var hang = tao('div', 'ho-tro-hang ho-tro-hang--' + (t.tu === 'quanly' ? 'minh' : 'khach'));
    var bong = tao('div', 'ho-tro-bong');
    bong.appendChild(tao('div', 'ho-tro-bong__nd', t.nd));   // textContent: không chạy HTML
    bong.appendChild(tao('div', 'ho-tro-bong__gio', t.luc));
    hang.appendChild(bong);
    return hang;
  }

  function dangONhauDay() { return dsTin.scrollHeight - dsTin.scrollTop - dsTin.clientHeight < 80; }
  function cuonXuongDay() { dsTin.scrollTop = dsTin.scrollHeight; }

  function themTin(ds) {
    var coMoi = false;
    ds.forEach(function (t) {
      if (daVe[t.id]) return;
      daVe[t.id] = true;
      soTinDaVe += 1;
      if (elTrong && elTrong.parentNode) { dsTin.removeChild(elTrong); }
      dsTin.appendChild(veTin(t));
      coMoi = true;
    });
    return coMoi;
  }

  function capNhatOGui() {
    var coKhach = !!(banDangChat && banDangChat.coKhach);
    oNhap.disabled = !coKhach;
    nutGui.disabled = !coKhach || dangGui;
    clearTimeout(henGhiChu);
    ghiChu.classList.remove('ho-tro-ghi-chu--loi');
    if (!coKhach) {
      ghiChu.textContent = 'Bàn này hiện chưa có khách (phiên đã đóng) nên chưa thể nhắn.';
      ghiChu.hidden = false;
    } else {
      ghiChu.hidden = true;
    }
  }

  function baoLoi(noiDung) {
    ghiChu.textContent = noiDung;
    ghiChu.classList.add('ho-tro-ghi-chu--loi');
    ghiChu.hidden = false;
    clearTimeout(henGhiChu);
    henGhiChu = setTimeout(capNhatOGui, 4500);
  }

  function chinhCaoONhap() {
    oNhap.style.height = 'auto';
    oNhap.style.height = Math.min(oNhap.scrollHeight, 120) + 'px';
  }

  function hoiChat() {
    clearTimeout(henChat);
    if (!banDangChat) return;
    var luotNay = luot;
    var ban = banDangChat;

    goi('GET', URL_BAN + '/' + ban.id + '/tin?sauId=' + idCuoi + '&dangMo=true')
      .then(function (d) {
        if (luotNay !== luot) return;            // đã đổi bàn / thoát chat trong lúc chờ
        var tin = (d && d.tin) || [];

        if (ban.coKhach !== !!(d && d.coKhach)) {
          ban.coKhach = !!(d && d.coKhach);
          capNhatOGui();
        }

        var gan = dangONhauDay();
        tin.forEach(function (t) { if (t.id > idCuoi) idCuoi = t.id; });
        if (themTin(tin) && gan) cuonXuongDay();

        // Máy chủ vừa đánh dấu tin của khách là đã đọc -> hạ số đỏ ngay, khỏi chờ lượt hỏi danh sách
        var cap = danhSachBan.filter(function (b) { return b.id === ban.id; })[0];
        if (cap && cap.chuaDoc) { cap.chuaDoc = 0; capNhatCham(); }
      })
      .catch(function () { /* thử lại ở lượt sau */ })
      .then(function () {
        if (luotNay === luot && banDangChat) henChat = setTimeout(hoiChat, KHOANG_CHAT);
      });
  }

  function moChat(b) {
    luot += 1;
    banDangChat = { id: b.id, maBan: b.maBan, coKhach: b.coKhach };
    daVe = {};
    soTinDaVe = 0;
    idCuoi = 0;
    dangGui = false;

    dsTin.textContent = '';
    elTrong = tao('p', 'ho-tro-trong', 'Chưa có tin nhắn nào. Khách nhắn là sẽ hiện ở đây.');
    dsTin.appendChild(elTrong);
    oNhap.value = '';
    chinhCaoONhap();

    manBan.hidden = true;
    manChat.hidden = false;
    nutQuayLai.hidden = false;
    tieuDe.textContent = 'Bàn ' + b.maBan;
    capNhatOGui();
    if (banDangChat.coKhach) oNhap.focus();
    hoiChat();
  }

  function veDanhSach() {
    luot += 1;                 // các lượt hỏi chat đang chờ sẽ bị bỏ qua
    clearTimeout(henChat);
    banDangChat = null;
    manChat.hidden = true;
    manBan.hidden = false;
    nutQuayLai.hidden = true;
    tieuDe.textContent = TIEU_DE_MAC_DINH;
    veDanhSachBan(true);
    hoiDanhSach();
  }

  function gui() {
    if (!banDangChat || dangGui || !banDangChat.coKhach) return;
    var noiDung = oNhap.value.trim();
    if (!noiDung) return;

    var luotNay = luot;
    var idBan = banDangChat.id;
    dangGui = true;
    nutGui.disabled = true;

    goi('POST', URL_BAN + '/' + idBan + '/gui', new URLSearchParams({ noiDung: noiDung }))
      .then(function (kq) {
        if (luotNay !== luot) return;
        oNhap.value = '';
        chinhCaoONhap();
        // Hiện ngay; lượt hỏi sau trả về đúng tin này thì bị bỏ qua nhờ daVe
        themTin([kq.tin]);
        cuonXuongDay();
      })
      .catch(function (err) {
        if (luotNay === luot) baoLoi(err.message || 'Chưa gửi được tin nhắn.');
      })
      .then(function () {
        dangGui = false;
        if (luotNay === luot) {
          // Chỉ mở lại nút Gửi, không đụng tới dòng thông báo lỗi vừa hiện.
          // Nếu lỗi là "bàn hết khách", lượt hỏi chat kế tiếp sẽ tự khoá ô nhập.
          nutGui.disabled = !(banDangChat && banDangChat.coKhach);
          if (banDangChat && banDangChat.coKhach) oNhap.focus();
        }
      });
  }

  // ---------- Mở / đóng popup ----------
  function mo() {
    nen.hidden = false;
    dangMo = true;
    veDanhSach();
  }

  function dong() {
    nen.hidden = true;
    dangMo = false;
    luot += 1;
    clearTimeout(henChat);
    banDangChat = null;
    manChat.hidden = true;
    manBan.hidden = false;
    nutQuayLai.hidden = true;
    tieuDe.textContent = TIEU_DE_MAC_DINH;
    hoiDanhSach();            // đóng lại thì quay về nhịp hỏi thưa hơn
  }

  nut.addEventListener('click', mo);
  nutDong.addEventListener('click', dong);
  nutQuayLai.addEventListener('click', veDanhSach);
  // mousedown (không phải click) để kéo bôi đen chữ rồi thả chuột ra ngoài sẽ không làm đóng popup
  nen.addEventListener('mousedown', function (e) { if (e.target === nen) dong(); });
  document.addEventListener('keydown', function (e) {
    if (e.key !== 'Escape' || !dangMo) return;
    if (banDangChat) veDanhSach(); else dong();   // Esc: thoát chat về danh sách bàn trước, bấm lần nữa mới đóng hẳn
  });

  form.addEventListener('submit', function (e) { e.preventDefault(); gui(); });
  oNhap.addEventListener('input', chinhCaoONhap);
  oNhap.addEventListener('keydown', function (e) {
    if (e.key === 'Enter' && !e.shiftKey && !e.isComposing) { e.preventDefault(); gui(); }
  });

  hoiDanhSach();   // chạy ngay khi trang tải để số đỏ trên nút hiện sẵn
})();
