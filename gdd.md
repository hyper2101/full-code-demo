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


# Mewtations: Dogma — Master GDD V2

## 1. High Concept

Mewtations: Dogma là game quản lý và xây dựng đội hình theo dạng card sandbox.

Người chơi quản lý một cộng đồng mèo nhỏ đang cố gắng sinh tồn trong một xã hội lạnh lẽo và đầy áp lực. Người chơi sẽ:

* nuôi dưỡng mèo
* chế tạo trang bị
* xây dựng đội hình
* đưa mèo đi thám hiểm
* đối mặt với các phe chó
* tìm kiếm cổ vật và vùng đất mới
* tạo ra những đội hình dị thường nhờ các mèo đột biến.

Toàn bộ thế giới đều tồn tại dưới dạng card.

Game tập trung vào:

* cảm giác kéo thả card
* chuẩn bị trước chiến đấu
* quản lý sức bền của mèo
* xây dựng đội hình
* khám phá các build kỳ lạ
* tiến triển dần từ nghèo khó sang ổn định.

Combat là tự động.
Người chơi chiến thắng chủ yếu nhờ:

* chuẩn bị
* vị trí đội hình
* trang bị
* phối hợp mèo
* hiểu cơ chế.

---

# 2. Trụ Cột Gameplay

## 2.1. Kéo Thả Và Tổ Chức Board

Cảm giác chính của game là:

* kéo card
* sắp xếp card
* gom tài nguyên
* quản lý không gian
* tối ưu quy trình.

Board lúc đầu khá chật và hỗn loạn.
Theo tiến độ game:

* kho chứa
* relic
* công trình
  sẽ dần giảm thao tác lặp lại.

---

## 2.2. Chuẩn Bị Quan Trọng Hơn Điều Khiển

Người chơi không điều khiển trực tiếp trong combat.

Sức mạnh đội hình đến từ:

* cách sắp xếp
* lựa chọn vũ khí
* kỹ năng
* lượng nộ
* mèo hỗ trợ
* chuẩn bị trước trận.

---

## 2.3. Sức Bền Và Kiệt Sức

Mỗi mèo có thanh stamina riêng.

Stamina giảm:

* theo thời gian
* theo combat.

Nếu stamina xuống thấp:

* mèo bị giảm hiệu quả chiến đấu
* bị kiệt sức
* cuối cùng bị tê liệt nếu tiếp tục bị ép hoạt động.

Đồ ăn không còn là upkeep theo ngày.
Đồ ăn chủ yếu dùng để:

* hồi phục
* duy trì hoạt động dài hạn
* hỗ trợ thám hiểm.

---

## 2.4. Mèo Đột Biến

Mèo đột biến là nguồn tạo build đặc biệt.

Chúng:

* hiếm
* mạnh
* có hướng build riêng
* thay đổi cách xây dựng đội hình.

Mỗi mèo đột biến đều có hình ảnh riêng.

---

## 2.5. Tiến Triển Từ Hỗn Loạn Sang Ổn Định

Đầu game:

* nghèo tài nguyên
* thiếu chỗ chứa
* ít mèo
* combat khó.

Giữa và cuối game:

* có thêm storage
* relic hỗ trợ
* quy trình ổn định hơn
* build phức tạp hơn.

---

## 2.6. Xung Đột Giữa Chó Và Mèo

Game không xây dựng phe thiện và phe ác rõ ràng.

Chó và mèo đại diện cho:

* cách sống
* tư tưởng
* cách nhìn xã hội khác nhau.

Người chơi chỉ nhìn thế giới từ phía mèo.

---

# 3. Gameplay Loop Chính

Gameplay chính xoay quanh chu kỳ sinh tồn và thám hiểm (Core Loop), kết hợp chặt chẽ giữa quản lý tài nguyên và chiến đấu chiến thuật:

**Giai đoạn 1: Chuẩn bị ở Trại (Base Management)**
1. **Thu thập tài nguyên:** Kéo thả mèo để khai thác tài nguyên từ các điểm nút hoặc công trình.
2. **Chế tạo đồ:** Ghép các thẻ nguyên liệu để chế tạo vũ khí, áo giáp, thức ăn và vật phẩm hỗ trợ.
3. **Hồi phục mèo:** Quản lý sức bền (stamina). Cho mèo ăn, chữa bệnh ở Trạm Y Tế (Dog Hospital) hoặc Đài Tẩy Tủy để phục hồi trạng thái chiến đấu.
4. **Xây đội hình (Team Building):** Trang bị vũ khí, kỹ năng cho Mèo và tối ưu hóa vị trí dựa trên Archetype.

**Giai đoạn 2: Viễn Chinh & Chiến Đấu (Expedition & Combat)**
5. **Đi thám hiểm (Expedition):** Gửi Cổng Viễn Chinh (Gateway) để mở bản đồ. Tại đây, hệ thống sẽ ngẫu nhiên (Randomization) sinh ra các Encounter (Sự kiện, Quái vật, Thương nhân). Đáng chú ý: **Mèo không bị nhốt vào balo hay biến mất khỏi Board khi đi Viễn Chinh**. Chúng vẫn ở nhà.
6. **Chạm trán Kẻ Địch (Dog Enemy & Encounter):** 
   - Hệ thống tự động nạp dữ liệu Enemy vào hệ thống thông qua `EncounterManager` với các Đội Hình Cố Định (Fixed Squad Formations) nhằm đảm bảo tính ổn định và chiến thuật cho các sự kiện như Thuế Chó (Dog Tax), Boss Blackaltar, và quái Expedition.
7. **Chuẩn Bị & Chiến đấu Chiến thuật (PreCombat & Turn-Based Combat):** 
   - Khi có combat, UI **PreCombatScreen** luôn được kích hoạt. Nhờ cơ chế "All Hands On Deck", người chơi có thể kéo thả **BẤT KỲ chú Mèo nào** còn khỏe mạnh đang có mặt trên Board vào lưới Grid 3x3 để tham chiến, không phân biệt là đang thám hiểm hay ở nhà.
   - Chiến đấu diễn ra tự động thông qua Event Stream và Reaction Chain, người chơi không trực tiếp can thiệp. Mèo tham chiến thực tế mới nhận được phần thưởng trận đánh.

**Giai đoạn 3: Rút lui & Phát triển (Retreat & Progression)**
8. **Mang tài nguyên về:** Nếu chiến thắng combat Viễn Chinh, đội không nhận EXP tu vi mà nhận trực tiếp tài nguyên, đồ vật. Mèo tham chiến sẽ ghi dấu ấn vào hệ thống **Hồi Ký (Memoir)**. Cần tránh giới hạn Insured Slots để không mất đồ khi rút lui thất bại.
9. **Mở khóa tiến triển mới:** Sử dụng tài nguyên mới để xây công trình (Shrine, Relic), giải mã các bí ẩn cốt truyện (Lore Hints) và chế tạo trang bị cao cấp.
10. **Lặp lại vòng lặp** với độ khó (Threat/Corruption) cao hơn.

---

# 4. Board Và Card

## 4.1. Toàn Bộ Thế Giới Là Card

Mọi thứ đều tồn tại dưới dạng card:

* mèo
* tài nguyên
* công trình
* relic
* đồ ăn
* trang bị
* sự kiện
* expedition.

---

## 4.2. Board Chính

Game chủ yếu diễn ra trên board chính.

Combat không chuyển sang màn riêng.
Thay vào đó:

* combat tạo layer mới đè lên board
* board chính bị tạm dừng trong lúc chiến đấu.

---

## 4.3. Không Gian Board

Board có giới hạn.

Người chơi có thể:

* mở rộng board
* xây storage
* mở thêm slot.

Áp lực không gian là một phần quan trọng của đầu game.

---

# 5. Hệ Thống Mèo

## 5.1. Mèo Thường

Mèo thường có chỉ số cơ bản giống nhau.

Sự khác biệt chủ yếu đến từ:

* trang bị
* kỹ năng
* build
* vị trí
* mèo đột biến.

---

## 5.2. Mèo Đột Biến

Mèo đột biến rất hiếm.

Nguồn nhận:

* boss tiến độ
* tỉ lệ thấp từ Miệng Thần Mèo.

Mỗi mèo đột biến:

* có hình riêng
* có hướng build riêng
* thay đổi đội hình mạnh.

Một đội mạnh thường xoay quanh:

* 1 mèo gây sát thương chính
* nhiều mèo hỗ trợ phía sau.

---

## 5.3. Stamina

Mỗi mèo có:

* stamina chính
* stamina kiệt sức.

Ví dụ:

* stamina chính: 50
* stamina kiệt sức: 20.

Mỗi ngày:

* stamina giảm dần.

Trong combat:

* mỗi cycle tiếp tục giảm stamina.

Khi hết stamina chính:

* mèo bị debuff kiệt sức
* giảm hiệu quả chiến đấu.

Khi hết stamina kiệt sức:

* mèo bị tê liệt
* không thể hành động
* cần thực phẩm hoặc vật phẩm đặc biệt để hồi phục.

---

## 5.4. Sẹo

Một số thất bại sẽ tạo sẹo.

Sẹo có thể:

* giảm chỉ số
* khóa slot trang bị
* khóa kỹ năng.

---

# 6. Combat

## 6.1. Đội Hình

Combat sử dụng grid 3x3.

Mỗi phe:

* tối đa 5 mèo.

Người chơi được tự do sắp xếp vị trí.

---

## 6.2. Turn Order

Combat hoạt động theo tốc độ.

Các mèo hành động tuần tự.

Sau mỗi cycle:

* lượt sẽ reset.

---

## 6.3. Rage

Mỗi mèo có thanh nộ.

Nộ nhận được chủ yếu từ:

* đánh thường
* một số kỹ năng.

Khi dùng kỹ năng:

* toàn bộ nộ bị reset.

---

## 6.4. Kỹ Năng

Kỹ năng:

* thay thế đòn đánh thường
* dùng khi tới lượt
* tiêu hao toàn bộ nộ.

Skill system sẽ mở rộng mạnh hơn theo tiến độ game.

---

## 6.5. Targeting

Targeting không ngẫu nhiên.

Ưu tiên mục tiêu dựa theo:

* hàng
* kiểu tấn công
* vũ khí.

---

## 6.6. Vũ Khí

Vũ khí quyết định:

* kiểu đánh
* hướng build
* cách gây sát thương.

Có:

* vũ khí craft được
* vũ khí unique.

---

## 6.7. Hiệu Ứng

Combat có các hiệu ứng như:

* burn
* poison
* bleed
* stun.

Một số build xoay quanh stacking debuff.

---

## 6.8. Sát Thương Đồng Minh

Một số build có thể:

* gây sát thương lên đồng minh
  để đổi lấy lượng sát thương lớn hơn lên địch.

Các build này sẽ có role riêng.

---

## 6.9. Boss

Boss thường là nhân vật có tư tưởng đối lập mạnh.

Combat boss tập trung vào:

* cơ chế riêng
* cách build đội hình đúng.

Boss không quá khó nếu đội hình được chuẩn bị phù hợp.

---

# 7. Trang Bị

## 7.1. Slot Trang Bị

Mỗi mèo có:

* 1 slot vũ khí
* 2 slot bùa.

Sau khi tiến triển:

* mở thêm slot đan
* slot thức ăn
* slot passive.

---

## 7.2. Độ Hiếm

Trang bị có rarity.

---

## 7.3. Tăng Tiến

Trang bị mạnh dần theo tiến độ game.

Ngoài chỉ số:

* passive của trang bị cũng tăng theo.

---

## 7.4. Data Ownership & Kiến trúc Kéo thả (EquipmentInstance)

Triết lý cốt lõi của hệ thống trang bị là: **"Mèo giữ Data, Bàn cờ giữ Object"**.
* Khi kéo thả một thẻ trang bị vật lý vào ô đồ của Mèo thông qua giao diện `CharacterPanelUI`, thẻ vật lý trên bàn cờ sẽ **bị hủy bỏ hoàn toàn (Destroy)**.
* Dữ liệu (ID, UpgradeLevel) và toàn bộ chỉ số gốc (CombatStats) của trang bị được Deep Copy thành một object `EquipmentInstance` lưu ngầm bên trong thẻ Mèo.
* Thao tác này giúp dọn dẹp không gian Board, tối ưu hóa hiệu năng và ngăn chặn triệt để các rủi ro phát sinh từ Reference Leak (ví dụ: các buff/debuff làm biến đổi chỉ số thẻ gốc).
* Khi người chơi tháo đồ (Unequip), hệ thống sẽ đọc lại `EquipmentInstance`, khôi phục dữ liệu và **tự động spawn (nặn) ra một thẻ vật lý mới** văng ra ngoài bàn chơi.

---

## 7.5. Tính toán chỉ số & Compatibility Logic
* Việc tính toán chỉ số (Combat Stats) được tách bạch khỏi thẻ vật lý, xử lý qua một hàm độc lập `GetEquipmentStats()` để dễ dàng tái sử dụng cho UI Tooltip, Preview hay AI.
* Để duy trì khả năng tương thích (Compatibility) cho các cơ chế cũ (Quests, Talismans, Combat Factory) vốn yêu cầu quét thẻ vật lý, Mèo sẽ tự động sinh ra một danh sách thẻ gốc (Base Prefab ảo) chỉ để đọc. Tuy nhiên, hệ thống tính điểm cốt lõi sẽ bỏ qua danh sách này nhằm chặn đứng lỗi cộng dồn điểm (Double Stat).

---

# 8. Expedition

## 8.1. Tổng Quan

Expedition là nguồn chính để:

* lấy relic
* lấy tài nguyên hiếm
* lấy vật phẩm unique
* gặp boss.

---

## 8.2. Cách Hoạt Động

Expedition diễn ra trên board chính nhưng có layer riêng.

Người chơi chỉ mang theo:

* đội combat
* trang bị nằm trong chest expedition.

---

## 8.3. Chest Expedition

Chest expedition có giới hạn slot.

Chest dùng để:

* mang trang bị đi
* mang loot về.

Nếu retreat:

* mất ngẫu nhiên 50% đồ trong chest.

---

## 8.4. Node

Mỗi expedition có pool node riêng.

Node có thể gồm:

* combat
* elite
* boss
* event
* Q&A
* hint.

Node được random trong phạm vi pool của expedition đó.

---

## 8.5. Retreat

Người chơi có thể retreat thường xuyên.

Retreat là một phần chiến thuật quan trọng.

---

# 9. Event

## 9.1. Event Thường

Event có thể:

* cho phần thưởng
* tạo risk/reward
* tạo lựa chọn khó.

---

## 9.2. Event Hỏi Đáp

Một số event yêu cầu:

* nhớ hint
* nhớ lore
* suy luận.

Trả lời đúng sẽ mở thêm:

* phần thưởng
* lore
* hướng đi mới.

---

## 9.3. Event Chó

Một số event liên quan tới:

* thu thuế
* bị chó tấn công
* tranh chấp tài nguyên.

---

# 10. Tài Nguyên Và Crafting

## 10.1. Linh Thảo

Linh thảo là nhóm tài nguyên chính dùng cho:

* thức ăn
* đan
* linh dược.

### Hệ Dược Điền (Spirit Field)
Hệ thống trồng linh thảo hoạt động độc lập, không sử dụng cơ chế transform stack của Stacklands.
- **Thực thể Runtime Nội Bộ:** Hạt giống trồng trong Dược Điền không phải là các thẻ bài (Card) vật lý nằm chồng lên nhau. Khi gieo hạt, hạt giống bị tiêu hủy và chuyển thành một tiến trình `PlantRuntimeState` ẩn bên trong Dược Điền.
- **Linh Trì (Water Pool):** Dược Điền sở hữu một bể chứa linh thủy. Người chơi nạp linh thủy vào để tăng tốc độ sinh trưởng của linh thảo.
- **Cơ chế Tier:** Mỗi hạt giống yêu cầu Dược Điền có bậc (Tier) tương ứng (Ví dụ Huyết Tâm Liên Tier 2 không thể trồng ở Dược Điền Tier 1).
- **Auto-Eject:** Khi linh thảo trưởng thành, nó tự động nảy ra thành một thẻ bài vật lý trên bàn cờ, không cần tương tác thu hoạch thủ công. Nếu bàn cờ quá chật, linh thảo sẽ đợi sẵn trong Dược Điền.

---

## 10.2. Linh Khoáng

Linh khoáng dùng cho:

* công trình
* trang bị.

---

## 10.3. Thuộc Tính

Một vật phẩm có thể mang tag:

* Kim
* Mộc
* Thủy
* Hỏa
* Thổ.

Hệ không giữ vai trò quá lớn trong combat.
Nó chủ yếu:

* tạo variation
* mở thêm hướng build.

Có vòng tương tác nhẹ giữa các hệ.

---

## 10.4. Tiền Tệ

Linh thạch là tiền tệ chính.

Linh lực là tài nguyên đi kèm.

---

# 11. Shrine Và Relic

## 11.1. Shrine

Shrine là công trình unique.

Shrine:

* không liên quan tới Miệng Thần Mèo
* dùng để tôn thờ relic và cổ vật.

---

## 11.2. Relic

Relic chủ yếu là hỗ trợ thụ động.

Ví dụ:

* tăng tỉ lệ rơi đồ
* nhân đôi sản xuất
* giảm thao tác lặp lại.

Relic giúp người chơi:

* ổn định quy trình
* giảm micromanagement.

---

## 11.3. Tiến Triển Shrine

Shrine có thể:

* tăng slot relic
* mở utility mới.

---

# 12. Miệng Thần Mèo (Cat God Mouth)

Miệng Thần Mèo là "Hệ thống hiến tế cống phẩm tà đạo" cốt lõi, không còn là máy gacha tổng hợp. Nó đóng vai trò tiêu hủy vật phẩm dư thừa và chuyển hóa tài nguyên thành tiến trình nghi lễ (ritual progression) với rủi ro cao nếu báng bổ thần linh.

## 12.1. Cấu Trúc Slot & Hiến Tế

Miệng Thần Mèo sở hữu cấu trúc 2 slot ngang rành mạch:
* **Slot Nghi Lễ (Ritual Slot):** Chỉ nhận duy nhất 1 thẻ bài Nghi Lễ (`RitualCardData`). Thẻ này xác định yêu cầu và phần thưởng của nghi lễ.
* **Slot Hiến Tế:** Nơi thả các vật phẩm dâng lên thần linh. Nếu người chơi thả nguyên một cọc (stack), hệ thống sẽ tự động quét từ trên cùng xuống, nuốt các vật phẩm hợp lệ và đẩy các vật phẩm không hợp lệ (Mèo, Người...) văng ra ngoài. Cống phẩm thừa sau khi nghi lễ hoàn thành cũng tự động được hất ra để trả lại.

## 12.2. Hệ Thống Devotion & Blasphemy (Lòng Thành và Báng Bổ)

Mọi vật phẩm hiến tế đều mang 2 chỉ số ngầm:
* **Devotion (Lòng thành):** Đẩy nhanh tiến trình hoàn thành nghi lễ. (Linh khoáng sạch sẽ mang điểm này).
* **Blasphemy (Sự báng bổ):** Tích tụ khi dâng đồ ô uế (ví dụ: Thức ăn hôi thiu từ Bãi rác).

Nếu tổng tỷ lệ Blasphemy của các vật phẩm hiến tế vượt mức cho phép so với yêu cầu của nghi lễ:
* **20%:** Thần Mèo sẽ đưa ra các dòng text cảnh báo khó chịu.
* **40%:** Cơn thịnh nộ giáng xuống (`GodCatThreat`). Nghi lễ vẫn hoàn thành nhưng một khe nứt Hư Không sẽ mở ra, triệu hồi quái vật tấn công trực tiếp vào trại của người chơi.

## 12.3. Đài Tẩy Tủy (Cleansing Altar) & Chữa Bệnh

Các tính năng Y tế trước đây đã được bóc tách hoàn toàn khỏi Miệng Thần Mèo và chuyển sang cấu trúc riêng mang tên **Đài Tẩy Tủy**, bao gồm:
* **Nghi Lễ Hộ Mệnh Trị Liệu:** Khai thông kinh mạch, mở khóa an toàn mọi ô khe cắm (slot) bị kẹt do Mèo bị Tẩu Hỏa Nhập Ma.
* **Nghi Lễ Tẩy Tủy Sẹo:** Tẩy sạch vết sẹo vĩnh cửu với rủi ro 50% thất bại khiến Mèo bị phế mạch nặng hơn.

## 12.4. Phần Thưởng (GodCatPackCard)

Hoàn thành nghi lễ Miệng Thần Mèo không rớt đồ vung vãi như cũ. Hệ thống sẽ trả về một gói quà **GodCatPackCard**.
Người chơi có thể chủ động click mở gói này để nhận:
* Nguồn cung chính để thu thập **Linh Thạch** (Tiền tệ nền kinh tế mới).
* Tỷ lệ nhỏ rơi ra các loại vũ khí/trang bị hiếm.

---

# 13. Đột Phá

## 13.1. Tổng Quan

Đột phá là quá trình tăng tiến sức mạnh.

Nó không hoàn toàn là tu luyện.

Đột phá giúp:

* mở slot mới
* tăng chỉ số
* mở sức mạnh mới.

---

## 13.2. Real-time Setup

Đột phá diễn ra theo thời gian thực.

Người chơi có thể:

* thêm card hỗ trợ
* tháo card hỗ trợ.

Nếu thay đổi setup giữa chừng:

* timer reset lại từ đầu.

---

## 13.3. Hoàn Thành

Khi quá trình hoàn tất:

* không thể can thiệp nữa
* sét sẽ tấn công.

Kết quả phụ thuộc vào:

* setup
* hỗ trợ
* chuẩn bị trước đó.

---

## 13.4. Thất Bại

Thất bại có thể tạo:

* sẹo
* debuff
* hậu quả lâu dài.

---

# 14. Thế Giới

## 14.1. Xã Hội Chó Và Mèo

Thế giới tồn tại sự khác biệt lớn giữa:

* chó
* mèo.

Hai phe có:

* cách suy nghĩ
* cách sống
* cách tổ chức xã hội khác nhau.

Game không xác định phe thiện hay ác tuyệt đối.

---

## 14.2. Tone

Tone game:

* hơi lạnh
* thư giãn
* có châm biếm nhẹ
* không quá grimdark.

Humor tồn tại nhưng không quá lố.

---

## 14.3. Lore

Lore được mở dần thông qua:

* boss
* event
* hint
* expedition.

Người chơi có thể:

* hiểu rất ít
* hoặc đào sâu toàn bộ lore.

---

# 15. Art Direction

Visual gần với phong cách của entity["video_game","WitchHand","WitchHand by Jon Nielsen"] hơn.

Mục tiêu:

* dễ đọc
* hơi mềm mại
* có nét vẽ tay nhẹ.

Mèo đột biến:

* độc đáo
* lạ
* nhưng không quá kinh dị.

UI:

* mức độ vừa phải
* không quá dày.

---

# 16. Audio Direction

Nhạc nền:

* nhẹ nhàng
* hơi buồn
* thư giãn.

Combat sound:

* mạnh tay hơn
* rõ lực đánh.

---

# 17. Endgame

Game có nhiều ending.

Ví dụ:

* đánh boss cuối
* đánh boss cuối sau khi tìm đủ hint.

Người chơi có thể:

* chơi world rất lâu
* hoặc tiến nhanh bằng build tối ưu.

---

# 18. Triết Lý Thiết Kế

## 18.1. Không Quá Nhiều Thuật Ngữ

Gameplay phải dễ hiểu trước.
Lore và tên gọi chỉ là lớp phủ bên ngoài.

---

## 18.2. Build Quan Trọng Hơn Độ Hiếm

Một đội hình phối hợp tốt quan trọng hơn một mèo mạnh đơn lẻ.

---

## 18.3. Chuẩn Bị Quan Trọng Hơn Phản Xạ

Game không tập trung vào thao tác nhanh.

Người chơi thắng nhờ:

* hiểu game
* chuẩn bị đúng
* xây dựng đội hình hợp lý.

---

## 18.4. Sự Tiến Bộ Phải Cảm Nhận Được

Đầu game:

* nghèo
* chật chội
* thiếu thốn.

Cuối game:

* ổn định hơn
* có nhiều hỗ trợ tự động hơn
* build phức tạp hơn.

Người chơi phải cảm thấy mình thật sự xây dựng được một cộng đồng mèo mạnh dần theo thời gian.


---

# 19. Triết Lý Phát Triển & Quy Trình Làm Việc (Cập nhật Mới)

## 19.1. Tách Biệt Logic và Text (Localization First)
- **Tuyệt đối không Hardcode:** Không bao giờ gán cứng (hardcode) các chuỗi văn bản hiển thị cho người chơi trực tiếp bên trong code (C# scripts). 
- **TSV Mapping:** Mọi đoạn hội thoại, tên vật phẩm, thông báo log đều phải được đăng ký thành lockey và trỏ về file GameScripts\Core\Systems\MewtationsLocTable.tsv.
- **An Toàn Logic:** Việc thiết kế sự kiện (như Expedition) phải tách biệt hoàn toàn giữa việc xử lý dữ liệu (trừ Vàng, cộng HP, random phần thưởng) và lớp hiển thị (Dialogue). Điều này đảm bảo khi thay đổi cốt truyện, logic hệ thống không bao giờ bị hỏng.
- **Bảo Tồn Nguyên Bản:** Các nội dung văn bản (lore) nguyên thủy của game mang tính định hình phong cách, phải luôn được ưu tiên bảo tồn nguyên vẹn và lồng ghép khéo léo thông qua hệ thống Loc thay vì lược bỏ.

## 19.2. Workflow Cốt Lõi: Expedition - Gateway - Ordering

Sự liên kết giữa 3 hệ thống này là xương sống của cơ chế Viễn Chinh:

1. **Khởi Chạy (Gateway):** 
   - Gateway là cánh cổng trung gian kết nối Board chính và không gian Expedition. 
   - Khi mèo được thả vào Gateway, hệ thống sẽ xác nhận trạng thái đội hình và kích hoạt ExpeditionManager.
   - Ngay lập tức, **Board chính bị đóng băng (Board Freeze)** để đảm bảo an toàn tài nguyên ở nhà, chuyển toàn bộ sự chú ý của người chơi vào chuyến thám hiểm.

2. **Quản Lý Balo (Ordering Storage):**
   - Ordering đóng vai trò là "Nhẫn Trữ Vật" duy nhất trong suốt chuyến đi.
   - Khi gặp các Node sự kiện (Event), hệ thống sẽ truy xuất trực tiếp vào Ordering để tiêu hao vật phẩm (ví dụ: cống nạp Vàng, Thức ăn) hoặc thêm phần thưởng (Linh Thạch, Cổ Vật).
   - Ordering tạo ra áp lực quản lý không gian: Người chơi phải liên tục ra quyết định giữ gì, bỏ gì bằng cơ chế **Trash Slot** (ném bỏ bài vĩnh viễn) khi túi đồ đầy.

3. **Kết Thúc & Phục Hồi (Return Workflow):**
   - Khi kết thúc chuyến đi (Retreat hoặc chết sạch đội hình - Party Wipe), Loot mang về từ Ordering sẽ được đổ ra Board chính (tùy thuộc vào việc có bị phạt rơi đồ do Party Wipe hay không, với ngoại lệ là các ô Insured Slots luôn được bảo hiểm).
   - Trạng thái Mèo bị thương (tê liệt/kiệt sức) trong combat sẽ chờ cấp cứu tại Dog Hospital, mô phỏng chân thực chấn thương sau viễn chinh.

## 19.3. Hệ Thống Hồi Ký (Memoir System)
Hệ thống **Memoir** thay thế cho việc theo dõi chỉ số khô khan. Mỗi khi một chú mèo trải qua sinh tử (BossKill, Combat, Đột biến, Khai mở Thiên Phú...), game sẽ ghi lại một dòng hồi ký. Điều này tạo tính nhập vai (Roleplay) cực lớn, biến mỗi chú mèo thành một nhân vật có câu chuyện cá nhân độc nhất, đồng thời giải thích rõ ràng nguồn gốc sức mạnh (hoặc sẹo/debuff) mà chúng sở hữu.

---

# 20. Coreloop Kịch Bản Lv1 → Lv9 (Early Game Foundation)

Mục tiêu của giai đoạn này:
* Dạy gameplay bằng hành động thay vì text dài
* Tạo vòng dopamine đều đặn
* Hình thành economy cơ bản
* Dẫn player từ:
  * combat đơn lẻ
  * → colony nhỏ
  * → cultivation economy
  * → chuẩn bị boss ritual

Giai đoạn này CHƯA có:
* God Cat Mouth
* mutation phức tạp
* corruption nặng
* faction war lớn
* automation sâu

Đây là:
**“Survival Colony Phase”**

---

## Tổng Trục Gameplay Lv1-9

Player sẽ dần học:

| Phase | Học gì |
| :--- | :--- |
| Lv1 | Combat + equip |
| Lv2 | Recovery + food |
| Lv3 | Gathering |
| Lv4 | Craft cơ bản |
| Lv5 | Expedition |
| Lv6 | Colony expansion |
| Lv7 | Cultivation |
| Lv8 | Economy + Recipe |
| Lv9 | Boss preparation |

---

## LOOP TỔNG

### Core Loop chính
1. Chuẩn bị mèo
2. Đi expedition/combat
3. Nhận reward
4. Hồi phục
5. Gather/craft/sell
6. Mua recipe/công trình
7. Mạnh hơn
8. Repeat

---

## LV1 — “First Blood”

### Mở game
Không có intro dài. Mở trực tiếp:

**Pre-combat Scene**
UI:
* phe địch đứng trước mặt
* 1 mèo player
* 1 weapon card

Ordering:
* kéo vũ khí vào mèo
* nhấn “Start Combat”

### Combat
Combat cực dễ.
Mục tiêu:
* cho player thắng nhanh
* thấy animation/combat flow

### Reward Screen
Reward cố định:
* Sacred Food x1
* Rusty Weapon x1
* Scrap Material x2

Ordering:
> “Mèo đang kiệt sức.”

---

## LV2 — “Survival”

### Board chính mở ra
Mèo:
* HP thấp
* stamina cạn

Ordering dạy:
* kéo Sacred Food vào mèo

Player học:
* drag/drop cơ bản
* recovery
* food consumption

### Pack Reward đầu tiên
Cho:
* berry
* wood scrap
* stone scrap

KHÔNG cho quá nhiều item.
Mục tiêu:
* board dễ đọc
* tránh overwhelm

### Resource Node đầu tiên
Spawn:
* Junk Pile
* Berry Bush

Ordering:
> “Cần nguyên liệu để sống sót.”

Player học:
* harvest node
* timed extraction
* resource spawn

---

## LV3 — “Primitive Craft”

Unlock:
**Primitive Recipe**
Ví dụ:
* Rough Armor
* Straw Hat
* Campfire

Ordering:
> “Trang bị tốt hơn sẽ giúp sống sót.”

### Player bắt đầu lựa chọn
Resource:
* bán?
* craft?
* giữ?

Đây là:
**decision economy đầu tiên**

### Campfire
Cho phép:
* nấu thức ăn

Player học:
* raw food hồi ít
* cooked food hồi mạnh hơn

Đây là:
**processing economy foundation**

---

## LV4 — “First Trade”

### Shop thật mở khóa
Không phải booster shop.
Là: **Merchant Caravan**

UI:
* recipe
* công cụ
* vật phẩm cơ bản

Ví dụ:
* Pickaxe Recipe
* Shelter Recipe
* Storage Basket

### Player học: bán hàng lấy linh thạch
Commodity item:
* berry
* wood
* scrap
(có giá trị thấp nhưng ổn định.)

### Dopamine Loop #1 chính thức hình thành
**Farm → Sell → Unlock Recipe**
Đây là progression ổn định.

---

## LV5 — “Second Expedition”

Ordering:
> “Có thể rời khỏi khu vực để tìm tài nguyên tốt hơn.”

### Expedition combat thứ hai
Vẫn dễ. Nhưng:
* cần armor
* cần food

Player bắt đầu:
* chuẩn bị trước combat

### Reward đặc biệt
Special Pack:
* mở ra mèo mới
Kèm:
* vài resource thường
* ít linh thạch

### Colony chính thức bắt đầu
Player giờ có:
* 2 mèo
* chia workload
Ví dụ: 1 gather, 1 recover/train.

---

## LV6 — “Temporary Shelter”

Unlock:
**Temporary Shelter**
Chức năng:
* recovery tốt hơn
* cultivation cơ bản

### Resource mới
Spawn:
* Cracked Ore Vein
Cho:
* spirit shard
* linh thạch vụn

Node này:
* hữu hạn lượt khai thác
* sẽ biến mất

Player học: **resource depletion**

### Work Specialization xuất hiện
Ví dụ:
* combat cat
* worker cat

---

## LV7 — “Cultivation”

Unlock:
**Linh Thạch System**
Player có thể:
* đặt mèo vào shelter
* bỏ linh thạch vào
* bắt đầu tu luyện

### Realm đầu tiên
Ví dụ: Mortal → Qi Gathering I
Buff nhẹ:
* HP
* stamina
* combat power

### Economy Split bắt đầu
Linh thạch giờ dùng cho:
* cultivation
* recipe
* crafting

Player bắt đầu phải: **ưu tiên tài nguyên**

---

## LV8 — “Settlement Economy”

### Recipe tier 2 bắt đầu xuất hiện
Ví dụ:
* furnace
* drying rack
* better weapon
* ore refinement

### Player hiểu: processing > raw selling
Ví dụ:
* raw ore bán rẻ
* refined ore giá cao hơn
* nhưng refined ore cũng cần cho craft

### Dopamine Loop #2 hoàn thiện
**Combat → Reward Pack**
Player bắt đầu:
* build preparation
* optimize gear
* tích food

### Hidden Recipe đầu tiên có thể xuất hiện
Ví dụ: từ event node, expedition reward, exploration.
KHÔNG giải thích rõ.
Mục tiêu: **tạo curiosity**

---

## LV9 — “Black Altar”

Ordering:
> “Có thứ gì đó đang thức tỉnh…”

### Unlock: Black Altar Recipe
Player có quyền:
* xây
* hoặc trì hoãn

Đây là: **voluntary escalation**

### Trước Boss
Player expected state:
* Có 2-3 mèo
* basic economy
* cooked food
* cultivation cơ bản
* vài công trình
* combat preparation habit

### Player đã hiểu:
| Hệ thống | Đã học |
| :--- | :--- |
| Combat | ✓ |
| Equip | ✓ |
| Food | ✓ |
| Recovery | ✓ |
| Gathering | ✓ |
| Crafting | ✓ |
| Selling | ✓ |
| Merchant | ✓ |
| Expedition | ✓ |
| Cultivation | ✓ |
| Multi-cat management | ✓ |

---

## Thiết Kế Reward Domain (Quan trọng)

**Combat Reward**
Chủ yếu: gear, combat consumable, combat pack.
Fantasy: > *chiến đấu*

**Merchant**
Chủ yếu: recipe, progression tool, infrastructure.
Fantasy: > *phát triển thuộc địa*

**Gathering**
Chủ yếu: raw economy.
Fantasy: > *sinh tồn*

**Hidden/Event Reward**
Chủ yếu: unique recipe, strange item, lore hint.
Fantasy: > *khám phá bí mật*

---

## Early Game Pressure

Giai đoạn này pressure nên:
* nhẹ
* liên tục
* không quá trừng phạt

Ví dụ: đói, fatigue, thiếu food, thiếu linh thạch, expedition risk nhẹ.

KHÔNG nên:
* permadeath sớm
* corruption nặng
* tax nặng

---

## Mục tiêu tâm lý của Early Game
Player phải cảm thấy:
**“Mình đang từ một con mèo sống sót…”**
→
**“xây dựng một thuộc địa mèo nhỏ.”**

Đây là fantasy cốt lõi của Lv1-9.
