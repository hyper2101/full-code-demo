```md
# Mewtations: Dogma — Current Gameplay Gaps & Design Warnings

> Mục đích file này:
> ghi nhớ các vấn đề gameplay còn thiếu sau giai đoạn purge kiến trúc Stacklands.
>
> Đây KHÔNG phải bug list.
> Đây là danh sách những phần còn thiếu để project trở thành:
>
> “một game Mewtations hoàn chỉnh”
>
> thay vì:
>
> “combat framework + cleaned runtime”.

---

# Current Project State

## Đã làm tốt

### Architecture

- Combat authority đã tách khỏi realtime Stacklands combat
- `Combatable` đã passive hóa
- `WorldManager` đã giảm dependency legacy mạnh
- Legacy systems đã bị quarantine bằng Hooks + RuntimeFlags
- Runtime ownership đã rõ ràng hơn
- Hidden combat coroutine đã bị loại bỏ
- Realtime attack flow không còn active

### Technical Foundation

- Turn-based combat foundation hoạt động
- Save/load nền tảng đã tồn tại
- Time flow và moon cycle đã có framework
- Tactical combat đang hình thành

---

# Gameplay Gaps (Quan trọng)

# 1. Expedition Gameplay Chưa Đủ Mạnh

## Vấn đề

Hiện tại game:
- có combat,
- có map/board,
- có exploration framework,

nhưng chưa tạo được cảm giác:

- đi xa nguy hiểm,
- tài nguyên cạn dần,
- càng tham càng rủi ro,
- phải quyết định rút lui,
- expedition có hậu quả thật.

Hiện combat tồn tại nhưng “adventure tension” còn yếu.

### Các Tính Năng Thiếu Sót Cần Bổ Sung Gấp (Cập nhật tiến độ)
- [x] **Encounter System cho Miệng Thần Mèo:** Đã tích hợp ThreatCardComponent và dùng tạm DogTaxEncounter thay thế cơ chế gọi quái vật lý. Đã dọn dẹp sạch `CatGodAnger` và `mob_void_spirit`.
- [x] **Special Map Pool (Viễn Chinh):** Đã sửa xử lý node SpecialMap để không gây kẹt.
- [ ] **LootProfile cho Thương Nhân:** Cần cấu hình LootProfile ngẫu nhiên theo giá trị giao dịch cho Camp Merchant thay vì gán cứng 1 Food/1 Gold như hiện tại.
- [x] **Debt Note tại Bệnh Viện Chó:** Đã tích hợp `AddDebtAmount` để tránh spam giấy nợ, gọi đúng qua hệ thống DogTax.
- [x] **Localization Hồi Ký (Memoir):** Đã chuyển toàn bộ text cứng (trả về, kiệt sức, bệnh viện) sang Localization ID.
- [x] **Reset Timer Đột Phá:** Đã sửa logic Timer trận pháp, tự động reset dựa vào số lượng thẻ đang stack (`_lastChildCount`).
- [x] **Sửa Lại Workflow Rút Lui (ReturnToBase):** Đã gỡ bỏ lệnh `RemoveFromStack()` để không phá vỡ hoạt động tại nhà khi mèo đi Viễn chinh.

---

# Danh Sách Tàn Dư Legacy Cần Dọn Dẹp (Chưa Gỡ Bỏ Ngay)

Đây là các hệ thống không còn giá trị sử dụng nhưng vẫn đang nằm rải rác trong code, cần được note lại để có kế hoạch dọn dẹp hoặc tái cơ cấu sau:

1. **Trọn bộ DLC "Greed" của Stacklands (Cursed Worlds)**: 
   - Không liên quan đến GDD hiện tại, chứa các class đòi cống nạp đồ và sinh quái (`GreedCutscenes`, `DemandManager`, cấu hình túi thẻ `Greed_...`).
   - Cần gỡ bỏ hoàn toàn khỏi `GameScreen.cs`, `RunOptions.cs` và `GameDataLoader.cs` để dọn dẹp kiến trúc.

2. **Chỉ số Greed & Corruption gắn với hệ quả vật lý trong Expedition**:
   - Hệ thống Viễn chinh vẫn đang tính toán `GreedLevel` và `CorruptionLevel` (ví dụ: cộng dồn dựa trên số xác mèo trên bàn cờ, cộng dồn khi chọn sai sự kiện).
   - Tuy nhiên, hệ quả trước đây (như spawn ra `mob_void_spirit` ở mốc Greed 80) đã bị vô hiệu hóa vì quái vật lý đã xóa sổ. Cần thiết kế lại hệ quả (Consequence) cho các chỉ số này bằng hệ thống Threat mới.


## Cần đạt được

Player phải cảm thấy:

- “mình nên quay về chưa?”
- “nếu đi tiếp có thể mất hết”
- “đồ loot này đáng để mạo hiểm không?”
- “mèo của mình đang kiệt sức”

## Cảnh báo

Nếu expedition không tạo tension:
- combat sẽ trở thành minigame tách rời gameplay chính.

---

# 2. Mutation Chưa Là Gameplay Identity

## Vấn đề

Lore hiện tại:
- mutation = tự do,
- individuality,
- khác biệt,
- sức mạnh + hậu quả.

Nhưng gameplay hiện chưa phản ánh điều đó.

Mutation hiện có nguy cơ trở thành:
- stat buff đơn giản,
- passive modifier nhẹ.

## Cần đạt được

Mutation phải:

- thay đổi cách chơi
- tạo build khác nhau
- có tradeoff
- ảnh hưởng chiến thuật
- ảnh hưởng role của mèo
- có hậu quả lâu dài

## Ví dụ đúng hướng

- mutation mạnh nhưng khó kiểm soát
- mutation gây corruption
- mutation mở skill mới nhưng mất stability
- mutation ảnh hưởng social/faction reaction
- mutation làm mèo unique

## Cảnh báo

Nếu mutation chỉ là stat buff:
- core fantasy của “Mewtations” sẽ không tồn tại trong gameplay.

---

# 3. Người Chơi Chưa Gắn Bó Với Từng Con Mèo

## Vấn đề

Hiện mèo vẫn có nguy cơ mang cảm giác:
- worker/villager,
- resource unit,
- pawn generic.

## Cần đạt được

Player cần:

- nhớ từng mèo
- build từng mèo
- sợ mất mèo
- quan tâm injury/fatigue
- cảm thấy mỗi mèo khác nhau

## Các yếu tố còn thiếu / Đã bắt đầu giải quyết

- [x] personality/gameplay traits & lịch sử cá nhân (Đã giải quyết bằng hệ thống Hồi Ký - Memoir System. Mỗi mèo giờ đây có tiểu sử lưu lại các trận sinh tử, đột phá và sự kiện riêng).
- role identity rõ ràng
- long-term injury
- fatigue impact
- mutation individuality
- unique combat utility

## Cảnh báo

Nếu mèo không có individuality:
- tactical combat sẽ mất cảm xúc.

---

# 4. Settlement / Colony Chưa Có Vai Trò Rõ

## Vấn đề

Hiện chưa rõ:
- base tồn tại để làm gì ngoài hồi phục/craft.

## Cần xác định

Settlement là:

### Option A — Safe Recovery Layer
- nơi nghỉ ngơi
- heal
- craft
- prep expedition
- ritual

### Option B — Continuous Survival Pressure
- food shortage
- internal crisis
- faction pressure
- corruption spread
- colony instability

## Cảnh báo

Nếu settlement không có gameplay identity:
- toàn bộ macro loop sẽ yếu.

---

# 5. Survival Pressure Chưa Đủ Sâu

## Hiện có

- stamina
- food recovery
- basic consumption

## Nhưng còn thiếu

- exhaustion pressure
- expedition attrition
- recovery pacing
- resource scarcity
- difficult tradeoffs
- injury persistence

## Mục tiêu

Player phải:
- thiếu tài nguyên thật,
- phải ưu tiên,
- phải hy sinh,
- phải cân nhắc risk/reward.

## Cảnh báo

Nếu survival pressure quá nhẹ:
- expedition sẽ mất ý nghĩa.

Nếu survival pressure quá nặng:
- game sẽ thành grind simulator.

---

# 6. Progression Direction Chưa Khóa

## Vấn đề

Hiện chưa rõ:
player đang mạnh lên bằng cái gì.

## Có thể là

- mutation
- ritual
- relic
- spiritual energy
- gear
- colony growth
- faction influence
- social ascension

## Cần khóa “north star”

Toàn bộ systems cần xoay quanh:
- một fantasy progression chính.

## Cảnh báo

Nếu progression direction không rõ:
- systems sẽ pull theo nhiều hướng khác nhau,
- gameplay sẽ rời rạc.

---

# 7. Lore Chưa Gắn Chặt Với Gameplay

## Vấn đề

Lore hiện mạnh hơn gameplay.

Các theme hiện có:
- freedom
- Dogma
- greed
- individuality
- spiritual hierarchy

Nhưng gameplay chưa phản ánh đủ.

## Cần đạt được

Gameplay phải thể hiện:

- freedom có giá phải trả
- mutation có hậu quả
- individuality phá optimization
- corruption temptation
- expedition greed
- social suppression của Dogma

## Cảnh báo

Nếu lore và gameplay tách rời:
- story sẽ chỉ tồn tại trong text/dialogue.

---

# Important Reminder

## Project đã vượt phase:

“Stacklands fork cleanup”

## Project đang bước vào phase:

“Xây gameplay identity thật sự cho Mewtations”

---

# Current Biggest Risk

Hiện tại nguy cơ lớn nhất KHÔNG còn là:
- legacy code,
- realtime combat,
- architecture debt.

Nguy cơ lớn nhất hiện tại là:

- gameplay loop không đủ tension
- systems đẹp nhưng không có cảm xúc
- combat không gắn với survival
- mutation không ảnh hưởng gameplay thật
- expedition không tạo risk/reward mạnh

---

# Recommended Priority Order

## Critical

1. Expedition risk/reward loop
2. Survival pressure balancing
3. Tactical combat stability
4. Cat individuality & attachment

## Medium

5. Mutation depth
6. Settlement gameplay identity
7. Progression direction

## Lower Priority

8. Environmental systems
9. Dynamic simulation
10. Advanced world ecosystem

---

# Final Goal

Mục tiêu cuối cùng KHÔNG phải:

- combat đẹp hơn
- nhiều systems hơn
- nhiều content hơn

Mà là:

Player thật sự cảm thấy:

- nguy hiểm,
- tham lam,
- gắn bó với mèo,
- áp lực sinh tồn,
- tự do nhưng có hậu quả,
- expedition đáng nhớ,
- mutation vừa hấp dẫn vừa đáng sợ.

Đó mới là “Mewtations: Dogma”.
```
