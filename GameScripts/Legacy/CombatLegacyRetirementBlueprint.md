# Combat Legacy Retirement Blueprint

Tài liệu này đóng vai trò là Blueprint chuẩn để dọn dẹp và khai tử (deprecate) toàn bộ hệ thống Enemy vật lý kế thừa từ Stacklands cũ, hướng đến kiến trúc Combat Turn-based thuần túy của Mewtations.

## 1. Phân định Vai trò (Domain Roles)

Hệ thống mới yêu cầu phân định rạch ròi các ranh giới:
| Hệ thống | Vai trò | Tình trạng |
| --- | --- | --- |
| **`Threat`** | Thẻ sự kiện/áp lực nằm trên board, buộc người chơi chuẩn bị. | **Active** |
| **`Encounter`** | Dữ liệu khởi tạo phiên combat, cấp quyền cho Combat Manager. | **Active** |
| **`DogEnemyDefinition`** | Template định nghĩa chỉ số, kỹ năng của quái. | **Active** |
| **`Enemy` / `Mob`** | Quái vật vật lý tồn tại trực tiếp như một Card trên board. | **LEGACY (Bị loại bỏ)** |

> Mọi thực thể combat giờ đây là "Instance-based" (Tách biệt khỏi board), KHÔNG CÒN là "Board entity-based".

---

## 2. Combat Legacy Boundary (Ranh giới Legacy)

Từ thời điểm tài liệu này có hiệu lực, **MỌI TÍNH NĂNG GAMEPLAY MỚI BẮT BUỘC TUÂN THỦ:**
- **KHÔNG ĐƯỢC** kế thừa từ class `Enemy` hay `Mob`.
- **KHÔNG ĐƯỢC** gọi lệnh spawn thẻ có ID dạng `mob_*` (VD: `WorldManager.instance.CreateCard(pos, "mob_slime")`).
- **KHÔNG CHO PHÉP** quái vật tồn tại dưới dạng Board Card vật lý có khả năng di chuyển và tương tác vật lý (bumping) với thẻ khác trên mặt bàn.

---

## 3. Các Giai đoạn Thực thi (5 Phases of Deprecation)

### Phase 1 — Đánh dấu Obsolete & Dò Dependency
1. Thêm attribute `[System.Obsolete("Sử dụng Encounter System thay thế.")]` và custom `[LegacySystem(LegacyOrigin.Stacklands, "Physical board enemies replaced by Encounter system.")]` vào các class `Enemy`, `Mob` và các subclass (`Slime`, `Demon`, `Kraken`, v.v.).
2. **Audit Chuyên sâu:** 
   - Đặc biệt chú ý đến class `Mob` vì nó có thể chứa root AI tick của Stacklands. 
   - Rà soát các phụ thuộc tại `Combatable`, `BossEntityCardData`, hệ thống Reward/Loot và UI Preview.

### Phase 2 — Runtime Guard (Chặn Spawn)
Thêm chốt chặn trung tâm tại `WorldManager.CreateCard`:
```csharp
if(cardId.StartsWith("mob_")) {
    Debug.LogWarning($"[Legacy Guard] Bị chặn: Spawn thẻ {cardId}. Board Enemy đã bị loại bỏ.");
    return null; // Chặn triệt để runtime spawn từ cutscene, mod, event cũ...
}
```

### Phase 3 — Save Migration (An toàn dữ liệu)
Không cần over-engineering. Nếu hệ thống Load Save đọc thấy các thẻ cũ (VD: `mob_slime`, `mob_demon`...):
- **Hành động:** Destroy silently (Xóa âm thầm).
- Lý do: Loại bỏ entity hỏng để tránh gãy AI loop và null behavior trên board. Chưa cần thiết phải convert thành rác/vật phẩm phụ.

### Phase 4 — Cắt đứt hoàn toàn Dependencies
- Gỡ bỏ vòng lặp AI cũ (các hàm Update, Tick của `Mob` hoặc `Enemy`).
- Xóa bỏ logic rớt đồ phụ thuộc vào việc Enemy chết trên board.
- Dọn dẹp Animation Hooks của các thực thể di chuyển vật lý này.

### Phase 5 — Hard Delete
Khi:
- Compile không còn bất kỳ warning nào ngoại trừ nội bộ file legacy.
- Không còn gãy log khi load save game cũ.
Lúc này, toàn bộ file `.cs` liên quan đến `Enemy`/`Mob` cũ có thể an tâm xóa thẳng khỏi source code.
