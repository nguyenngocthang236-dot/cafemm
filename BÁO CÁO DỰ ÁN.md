# **BÁO CÁO DỰ ÁN**

## **KHẢO SÁT NGƯỜI DÙNG VÀ XÂY DỰNG SẢN PHẨM WEB QUẢN LÝ QUÁN CAFE**

*Từ nhu cầu người dùng đến User Story, Acceptance Criteria và Product Backlog*

### **Thông tin chung**

* **Tên dự án:** Cafe Manager – Website quản lý hoạt động quán cafe  
* **Nhóm thực hiện:** \[Điền tên nhóm của bạn\]  
* **Thành viên:** \[Điền tên thành viên\]  
* **Môn học / Giảng viên:** \[Điền tên môn học và giảng viên\]  
* **Thời gian thực hiện:** 2026

## **BƯỚC 1 – KHẢO SÁT NGƯỜI DÙNG**

**Mục tiêu:** Tìm hiểu cách quán cafe tiếp nhận món, quản lý bàn, thanh toán, kiểm soát nguyên liệu và theo dõi doanh thu; xác định công cụ đang dùng, tình huống bất tiện và nhu cầu cải thiện thực tế từ người dùng.

### **1.1. Kế hoạch khảo sát**

* **Đối tượng:** Chủ quán, quản lý, nhân viên phục vụ, pha chế và khách hàng trải nghiệm thực tế.  
* **Số lượng thực tế:** $6$ người tham gia khảo sát trực tuyến qua biểu mẫu.  
* **Phương pháp:** Khảo sát trực tuyến qua Google Forms.  
* **Đầu ra:** Bảng thống kê phản hồi thực tế từ biểu đồ khảo sát.

### **1.2. Thống kê kết quả khảo sát thực tế**

* **Vai trò người tham gia:** Khách hàng chiếm 66.7% (4 người), Quản lý quán chiếm 16.7% (1 người), và Khác (nhân viên/pha chế) chiếm 16.7% (1 người).  
* **Quy mô quán:** Dưới 10 bàn (33.3%), 10-20 bàn (33.3%), và Chủ yếu bán mang về \- Take-away (33.3%).  
* **Phương pháp quản lý hiện tại:** Đa số sử dụng Excel/Google Sheets (50%) hoặc kết hợp nhiều phương pháp (33.3%), chỉ có 16.7% từng sử dụng phần mềm quản lý chuyên dụng.  
* **Thiết bị sử dụng chính:** Máy tính chiếm 50%, kết hợp với thiết bị di động trong quá trình quản lý và vận hành.

### **1.3. Tổng hợp tỷ lệ vấn đề và nhu cầu thực tế** 

* **Nhầm lẫn đơn hàng / Khó ghi nhận đơn:** 3/6 người gặp phải (50.0%).  
* **Khó theo dõi bàn trống / Quản lý bàn:** 2/6 người phản hồi là khó khăn trực tiếp, và 66.7% có nhu cầu tích hợp tính năng quản lý bàn.  
* **Tốn thời gian cho Thanh toán / Hóa đơn:** 5/6 người lựa chọn là tính năng cấp thiết nhất (83.3%).  
* **Tốn thời gian kiểm kê kho nguyên liệu:** 2/6 người đánh giá đây là công việc mất thời gian nhất (33.3%).  
* **Nhu cầu về Menu và Bán hàng:** 4/6 người yêu cầu xây dựng tính năng quản lý menu và tạo đơn hàng (66.7%).

## **BƯỚC 2 – XÁC ĐỊNH VẤN ĐỀ**

Dựa trên số liệu khảo sát thực tế ($n=6$), nhóm xác định 3 vấn đề trọng tâm cần giải quyết trong phiên bản đầu tiên của sản phẩm:

* **I1 – Ghi nhận đơn hàng và hạn chế nhầm lẫn:** 50% người dùng gặp khó khăn trong việc ghi nhận đơn hàng dẫn đến nhầm lẫn. Cần một giao diện chọn món và tạo đơn trực quan.  
* **I2 – Quản lý bàn trống và sơ đồ quán:** Phù hợp với các quán có quy mô dưới 20 bàn hoặc take-away, giúp nhân viên nắm bắt trạng thái bàn nhanh chóng (66.7% nhu cầu).  
* **I3 – Thanh toán và xuất hóa đơn nhanh chóng:** Là tính năng được quan tâm nhất với 83.3% tỷ lệ mong muốn từ người dùng, giúp tối ưu hóa khâu thu ngân và tránh thất thoát.

## **BƯỚC 3 – CÁC TÌNH HUỐNG THỰC TẾ (MICRO-SCENARIOS)**

* **Tình huống 1 (Từ vấn đề nhầm đơn \- 50%):** Vào giờ cao điểm, nhân viên thao tác ghi nhận đơn hàng thủ công trên Excel hoặc giấy dễ dẫn đến nhầm lẫn món của khách, gây mất thời gian xử lý lại.  
* **Tình huống 2 (Từ nhu cầu quản lý bàn \- 66.7%):** Quán có quy mô vừa và nhỏ nhưng việc theo dõi bàn trống, bàn đang phục vụ còn rời rạc, khiến việc xếp chỗ cho khách mới gặp lúng túng.  
* **Tình huống 3 (Từ nhu cầu thanh toán \- 83.3%):** Khâu tính tiền và xuất hóa đơn chiếm nhiều thời gian do thiếu một công cụ tập trung, là điểm đau lớn nhất được 83.3% người tham gia khảo sát kỳ vọng được giải quyết.

## **BƯỚC 4 – XÂY DỰNG PERSONA**

* **Persona 1: Nhân viên Thu ngân / Phục vụ**  
  * *Mục tiêu:* Tạo đơn nhanh, không nhầm món, thanh toán chính xác.  
  * *Khó khăn:* Thao tác trên công cụ thủ công dễ nhầm lẫn, tốn thời gian giờ cao điểm.  
* **Persona 2: Chủ quán / Quản lý**  
  * *Mục tiêu:* Nắm bắt doanh thu, kiểm soát hoạt động và quản lý menu/bàn hiệu quả.  
  * *Khó khăn:* Dữ liệu phân tán, khó theo dõi sát sao tình hình hoạt động của quán.

## **BƯỚC 5 – XÂY DỰNG SCENARIO**

* **Scenario 1:** Mở phần mềm $\rightarrow$ Chọn bàn $\rightarrow$ Thêm món từ Menu $\rightarrow$ Kiểm tra giỏ hàng $\rightarrow$ Xác nhận tạo đơn.  
* **Scenario 2:** Chọn đơn hàng cần thanh toán $\rightarrow$ Hệ thống tự động tính tổng tiền $\rightarrow$ Xác nhận thanh toán $\rightarrow$ Giải phóng trạng thái bàn về "Trống".

## **BƯỚC 6 – CHUYỂN THÀNH USER STORY (KÈM THANG ƯU TIÊN MoSCoW)**

| ID | User Story | Ưu tiên (MoSCoW) | Nhóm |
| :---- | :---- | :---- | :---- |
| **US01** | Với vai trò thu ngân, tôi muốn tạo đơn hàng và thêm món vào đơn để ghi nhận yêu cầu của khách nhanh, chính xác. | Must | Bán hàng |
| **US02** | Với vai trò thu ngân, tôi muốn sửa số lượng, xóa món và thêm ghi chú trước khi xác nhận để xử lý yêu cầu thay đổi của khách. | Must | Bán hàng |
| **US03** | Dengan vai trò nhân viên, tôi muốn xem sơ đồ bàn để biết bàn nào trống hoặc đang phục vụ. | Must | Quản lý bàn |
| **US04** | Với vai trò thu ngân, tôi muốn ghi nhận thanh toán và lưu hóa đơn để hoàn tất giao dịch. | Must | Thanh toán |
| **US05** | Với vai trò quản lý, tôi muốn tìm đơn theo mã, ngày và trạng thái để kiểm tra giao dịch khi cần. | Should | Đơn hàng |
| **US06** | Với vai trò quản lý, tôi muốn xem tổng doanh thu theo thời gian để theo dõi tình hình kinh doanh. | Should | Báo cáo |
| **US07** | Với vai trò quản lý, tôi muốn ghi nhận nhập kho và xem số lượng tồn nguyên liệu. | Should | Kho |
| **US08** | Với vai trò quản lý, tôi muốn thiết lập mức tồn tối thiểu để nhận biết nguyên liệu cần nhập thêm. | Could | Kho |
| **US09** | Với vai trò quản lý, tôi muốn thêm, sửa, ẩn món và cập nhật giá bán để menu luôn đồng bộ. | Must | Menu |
| **US10** | Với vai trò quản lý, tôi muốn phân quyền tài khoản để mỗi nhân viên truy cập đúng chức năng. | Must | Tài khoản |
| **US11** | Với vai trò nhân viên, tôi muốn chuyển đơn sang bàn trống khác khi khách đổi chỗ. | Could | Quản lý bàn |

## **BƯỚC 7 – ACCEPTANCE CRITERIA VÀ QUY TẮC NGHIỆP VỤ**

* **US01:** *Given* đã đăng nhập, *When* chọn bàn và thêm món còn bán, *Then* hệ thống tạo mã đơn duy nhất và hiển thị tổng tiền đúng, món ngừng bán không thêm được.  
* **US04:** *Given* đơn có tổng tiền \> 0 và chưa thanh toán, *When* xác nhận thanh toán thành công, *Then* chuyển trạng thái sang "Đã thanh toán", không cho phép thanh toán trùng lặp đơn đã hoàn thành.  
* **US09:** Tên món không trùng, giá \> 0, bắt buộc chọn danh mục.  
* **US10:** Nhân viên cố ý gõ đường dẫn trang quản lý bằng URL vẫn bị hệ thống từ chối truy cập.

### **Bảng quy tắc nghiệp vụ cốt lõi:**

* **Hủy đơn:** Chỉ hủy được đơn chưa thanh toán; đơn hủy không tính vào doanh thu.  
* **Giải phóng bàn:** Bàn tự động về trạng thái "Trống" ngay khi đơn hàng được thanh toán thành công.  
* **Làm tròn tiền:** Giá trị tiền tệ tính theo số nguyên đồng Việt Nam (VNĐ).

## **BƯỚC 8 – TỪ USER STORY ĐẾN TÍNH NĂNG**

* **I1 (Nhầm đơn/Ghi đơn chậm):** Liên kết US01, US02 $\rightarrow$ Tính năng màn hình bán hàng POS, giỏ hàng, tùy chỉnh ghi chú món.  
* **I2 (Khó theo dõi bàn):** Liên kết US03, US11 $\rightarrow$ Tính năng sơ đồ trực quan, cập nhật trạng thái bàn thời gian thực, chuyển bàn.  
* **I3 (Thanh toán & Đối soát):** Liên kết US04, US05, US06 $\rightarrow$ Module thanh toán, lưu trữ hóa đơn, lịch sử giao dịch và báo cáo doanh thu.

## **BƯỚC 9 – PRODUCT BACKLOG VÀ KẾ HOẠCH SPRINT**

**9.1. Product Backlog phiên bản hoàn thiện** 

| Thứ tự  | ID  | Hạng mục  | Ưu tiên  | Lý do  | Dự kiến Sprint |
| :---- | :---- | :---- | :---- | :---- | ----- |
| 1  | US01  | Tạo đơn, chọn bàn/mang đi và thêm món  | Must  | Cốt lõi để phát sinh đơn hàng; giá trị sử dụng trực tiếp.  |  S3-S4 |
| 2  | US02  | Sửa số lượng, xóa món, ghi chú và tính lại tiền  | Must  | Giảm sai sót khi khách thay đổi yêu cầu.  |  S4 |
| 3  | US03  | Sơ đồ bàn và trạng thái bàn  | Must  | Giúp phục vụ và thu ngân phối hợp.  |  S4 |
| 4  | US04  | Thanh toán, lưu hóa đơn  | Must  | Hoàn tất vòng đời đơn và lưu dữ liệu giao dịch.  |  S4 |
| 5  | US09  | Thêm/sửa/ẩn món, danh mục và giá  | Must  | Menu là dữ liệu đầu vào cho bán hàng.  | S3 |
| 6  | US05  | Tra cứu/lọc lịch sử đơn  | Should  | Hỗ trợ kiểm tra giao dịch và xử lý thắc mắc.  | S5 |
| 7  | US10  | Đăng nhập và phân quyền cơ bản  | Must  | Bảo vệ dữ liệu và giới hạn thao tác theo vai trò.  | S3 |
| 8  | US06  | Báo cáo doanh thu theo thời gian  | Should  | Giúp quản lý theo dõi kết quả kinh doanh.  | S5 |
| 9  | US07  | Nhập kho và xem tồn nguyên liệu  | Should  | Theo dõi nguyên liệu, giảm phụ thuộc vào ghi chép rời rạc.  | S5 |
| 10  | US08  | Cảnh báo tồn kho thấp  | Could  | Hữu ích nhưng có thể làm sau khi dữ liệu kho ổn định.  | S5 |
| 11 | US11 | Chuyển đơn sang bàn trống khi khách đổi chỗ  | Could  | Hỗ trợ nghiệp vụ phát sinh tại quán.  | S4-S5  |

* Must – Bắt buộc: cần có để hoàn thành luồng bán hàng và bảo đảm hệ thống sử dụng được.   
* Should – Nên có: mang lại giá trị cao nhưng có thể triển khai sau luồng cốt lõi.   
* Could – Có thể có: hữu ích, nhưng không làm gián đoạn mục tiêu phiên bản đầu nếu chưa thực hiện. 

**9.2. Kế hoạch phát triển đề xuất** 

| Sprint  | Phạm vi  | Kết quả bàn giao có thể kiểm tra  |
| :---- | :---- | :---- |
| Sprint 1  | Xác nhận nhu cầu, User Story, Acceptance Criteria  | Bảng khảo sát thực tế; danh sách vấn đề; backlog được nhóm xác nhận.  |
| Sprint 2  | Thiết kế giao diện, database và phân quyền  | Wireframe/Figma; ERD; mô hình dữ liệu và luồng nghiệp vụ.  |
| Sprint 3  | Đăng nhập, quản lý menu, dữ liệu bàn  | Có thể đăng nhập theo vai trò; CRUD menu; xem sơ đồ bàn.  |
| Sprint 4  | Bán hàng, đơn hàng, thanh toán, hóa đơn  | Chạy được luồng từ tạo đơn đến thanh toán và tra cứu hóa đơn.  |
| Sprint 5  | Báo cáo, quản lý kho, lịch sử đơn  | Có báo cáo theo thời gian; ghi nhận nhập kho; lọc lịch sử đơn.  |
| Sprint 6  | Kiểm thử, sửa lỗi, hoàn thiện demo  | Biên bản kiểm thử theo Acceptance Criteria; dữ liệu demo; tài liệu hướng dẫn.  |


**KẾT LUẬN**   
Qua quy trình thực hiện từ khảo sát thực tế người dùng đến khi xây dựng Product Backlog, dự án Cafe Manager được định hướng giải quyết ba nhu cầu cốt lõi: ghi nhận đơn hàng chính xác (tránh nhầm lẫn), theo dõi trạng thái bàn trực quan và thanh toán/đối soát giao dịch nhanh chóng.

Các nhu cầu thực tế này đã được chuyển hóa thành hệ thống User Story, đi kèm các tiêu chí nghiệm thu (Acceptance Criteria) có thể kiểm thử, đảm bảo khả năng truy vết tính năng và được sắp xếp vào Product Backlog theo độ ưu tiên hợp lý. Các tính năng bổ trợ như quản lý menu, phân quyền tài khoản, báo cáo doanh thu và quản lý kho được tích hợp nhằm hoàn thiện quy trình vận hành toàn diện cho quán.

Báo cáo này đã phản ánh đúng thực trạng người dùng thông qua việc tổng hợp dữ liệu khảo sát thực tế từ biểu mẫu Google Forms, điều chỉnh danh mục vấn đề theo bằng chứng thu thập được, đồng thời chuẩn hóa các quy tắc nghiệp vụ cốt lõi làm cơ sở cho quá trình phát triển sản phẩm trong các Sprint tiếp theo.

