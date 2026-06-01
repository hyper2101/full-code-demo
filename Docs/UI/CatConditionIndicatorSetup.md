# Hướng dẫn thiết lập Icon Trạng Thái Mèo trên Unity

Tuyệt vời! Toàn bộ logic code đã được hoàn thiện đúng theo kiến trúc chuẩn mà chúng ta đã thống nhất: Event-driven, tách biệt UI và Logic, cũng như giữ `IsExhausted`/`IsParalyzed` làm chân lý tuyệt đối.

Khi bạn mở dự án bằng Unity Editor, hãy làm theo các bước sau để mọi thứ hoạt động trơn tru:

## 1. Cập nhật SpriteManager
Mở scene chứa GameObject `SpriteManager` (thường nằm ở `_Manager` hoặc `GameManager`):
- Mở script `SpriteManager` trong Inspector.
- Kéo xuống phần **Cat Condition Icons**.
- Kéo thả 5 sprite bạn đã chuẩn bị cho các mục:
  - `Stamina High Icon`
  - `Stamina Medium Icon`
  - `Stamina Low Icon`
  - `Exhausted Icon`
  - `Paralyzed Icon`

## 2. Thiết lập Prefab Mèo (CatCard)
Mở Prefab thẻ Mèo của bạn (Ví dụ: `CatCardPrefab`):
- Click phải vào Prefab và chọn **Create Empty** để tạo một GameObject con. Đặt tên nó là `ConditionIndicator`.
- Thêm Component `SpriteRenderer` cho object này và chỉnh kích thước/vị trí hiển thị (góc thẻ, trên đầu, tuỳ ý).
- Thêm Component `Cat Condition Indicator` vào object này.
- Trong Component `Cat Condition Indicator`:
  - Kéo chính object này vào trường `Condition Icon`.
  - Component sẽ tự động tìm `GameCard` cha và kết nối Event.

## 3. Localization Terms
Hệ thống Status Effect sẽ tự động render icon nhỏ trong trường hợp Mèo bị kiệt sức hoặc tê liệt (để người chơi xem tooltip chi tiết). Hãy vào bảng Localization của bạn và thêm 6 dòng (Term):
- `statuseffect_exhausted_name`: "Kiệt Sức"
- `statuseffect_exhausted_description`: "Mèo đã cạn kiệt thể lực, tốc độ và năng suất giảm nghiêm trọng."
- `statuseffect_exhausted_a` và `statuseffect_exhausted_b`: Tên 2 màu (ví dụ `black` và `grey`) dùng cho UI.
- `statuseffect_paralyzed_name`: "Tê Liệt"
- `statuseffect_paralyzed_description`: "Mèo đang bị tê liệt, không thể làm việc hay tham chiến."
- `statuseffect_paralyzed_a` và `statuseffect_paralyzed_b`: Tên 2 màu cho UI.

---

> [!TIP]
> **Cách kiểm tra:**
> Chạy game và spawn một con Mèo. Bạn có thể ép Mèo làm việc liên tục (`ConsumeLaborStamina`) để thấy Icon góc thẻ chuyển đổi qua các mức Thể Lực. Khi tới 0, biểu tượng Kiệt sức sẽ hiện trên thẻ và đồng thời hiện một dấu hiệu Status Effect nhỏ để cho phép hover xem Tooltip!
