# Mewtations: Dogma - Expedition GDD (V1 Finalized)

Tài liệu này lưu trữ các quyết định thiết kế cuối cùng cho hệ thống Viễn Chinh (Expedition V1) và các cơ chế Sinh Tồn Đầu Game. Bất kỳ thay đổi nào cũng phải tuân thủ nghiêm ngặt các nguyên tắc dưới đây.

---

## 1. Core Architecture & Stack Logic

- **Board Freeze:** Khi tham gia Expedition, Board chính sẽ bị đóng băng (thông qua `WorldManager.WorldSimulationPaused`), nhưng môi trường Expedition (UI, Combat, Animation) vẫn tiếp tục chạy độc lập.
- **Restore Stack Logic:**
  - **Khởi hành:** Nếu mèo kẹp giữa stack, khi rời đi sẽ tự động nối stack lại (`Parent.SetChild(Child)`). Không sử dụng Persistence Manager.
  - **Trở về:** Mèo khỏe mạnh sẽ tự nối lại vào Parent cũ (thông qua `ParentCardUniqueId`).
  - **Thương vong:** Mèo bị Paralyzed hoặc Exhausted sẽ bị đẩy ra Board độc lập, không nối lại stack.

---

## 2. Ordering Storage Progression & Insured Slots

Ordering Storage (Nhẫn trữ vật) là công cụ chứa đồ mang về duy nhất.
- **Cơ bản:** Bắt đầu với 10 slot. 1 card = 1 slot. Không stack tài nguyên (Wood, Scrap, Food, Reward Card đều chiếm 1 slot riêng biệt).
- **Progression:** Có thể được nâng cấp trực tiếp bằng material để tăng giới hạn chứa (Capacity).
- **Insured Slots (Ô Bảo Hiểm):** Một số nâng cấp sẽ mở khóa "Insured Slots". 
  - Khi Retreat hoặc Party Wipe (chết sạch), player sẽ mất 50% đồ đạc.
  - Các item nằm trong Insured Slots (từ index `0` đến `InsuredSlots - 1`) sẽ luôn được giữ lại an toàn. Đây là cơ chế cốt lõi bắt buộc.

---

## 3. Bản Đồ Đặc Biệt (Special Map Pool)

- **Cơ chế Pity:** Hoạt động dựa trên `ExpeditionSpecialMapPityCounter` trong SaveGame. 
- **Map Pool:** Có một danh sách các Special Map (Ví dụ: Đền cổ, Trại tị nạn...). Khi Pity kích hoạt, hệ thống sẽ roll ngẫu nhiên 1 map trong Pool.
- **Loại trừ map cũ:** Nếu một map đã từng được đi qua, hệ thống sẽ giảm mạnh tỉ lệ roll trúng nó, nhưng không cấm vĩnh viễn, đảm bảo nội dung luân chuyển liên tục.

---

## 4. UI Nhận Thưởng (Reward Selection UI)

Hệ thống UI chung cho mọi Node thưởng (Combat, Event, Dialogue, Reward Node).
- **Reward Screen:** Giao diện hiển thị danh sách Card phần thưởng.
  - **Chuột trái:** Chọn Card đưa vào Ordering.
  - **Chuột phải:** Xem thông tin chi tiết của Card.
- **Mở Ordering Chủ Động:** Player có thể mở UI Ordering bất cứ lúc nào trong chuyến viễn chinh để xem hoặc dọn đồ. Nếu nhặt thưởng mà Ordering đầy, UI Ordering sẽ tự động bật lên.
- **Trash Slot (Ô Ném Đồ):** Nằm trực tiếp trong UI Ordering. 
  - *Lưu ý:* Trash Slot hoàn toàn khác biệt với Trash Pile (Đống rác đào đồ đầu game).
  - Cho phép kéo-thả (Drag & Drop). Khi kéo thả Card vào ô này, Card sẽ bị xóa vĩnh viễn để giải phóng chỗ. Kéo trượt ra ngoài sẽ trả Card về vị trí cũ.

---

## 5. Randomization & Node Interactions

- **Camp Merchant:** Không sử dụng phần thưởng gán cứng. Thương nhân sử dụng `LootProfile` để trả về các phần thưởng ngẫu nhiên tương ứng với giá trị giao dịch, mang lại yếu tố bất ngờ.
- **Camp Healer:** Chỉ hỗ trợ hồi máu (thông qua Healing Pool 100 HP chia đều) và xóa Debuff. Tuyệt đối KHÔNG sinh Debt Note, KHÔNG dính líu đến Threat System.

---

## 6. Sinh Tồn Trại Chính (Dog Hospital & Debt Note)

- **Dog Hospital:** Điểm duy nhất trên Base điều trị Tê Liệt (Paralyzed). 
  - Điều trị tốn 60 giây và đưa Stamina của mèo về 0.
  - **Luôn luôn sinh Debt Note** sau khi chữa trị thành công.
- **Debt Note (Giấy Nợ):**
  - Giấy Nợ là một loại Card chỉ tồn tại trên Base chính, không được kéo vào Expedition. Không chiếm slot Ordering.
  - Có thể cấu hình mức phạt (VD: đòi 10 Pate, 5 Cá Khô). 
  - Khởi nguồn từ Thuế (Tax System), Khủng hoảng (Threat System) hoặc phí bệnh viện (Dog Hospital). Nếu không trả nợ sẽ bị phạt.

---

## 7. Localization

- **Ngôn ngữ:** Tất cả text, thông báo liên quan đến Expedition (như "Túi đồ đầy", tên Node...) đều được đăng ký dưới dạng Key trong file `.tsv` đơn giản của hệ thống Mewtations (`GameScripts/Core/Systems/MewtationsLocTable.tsv`).
- Không sử dụng hệ thống localization cũ của Stacklands, không hardcode string trong file scripts.
