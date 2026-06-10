using UnityEngine;
using System.Collections.Generic;

// Phase 4: Seed Progress Decoupling
// Hạt giống sẽ tự chạy Timer của nó. Nếu nằm trên Farm/Garden, nó sẽ bắt đầu lớn.
// Được gắn vào GameCard của các hạt giống khi cần.
public class SeedRuntime : MonoBehaviour
{
    private GameCard _myGameCard;
    private float _growthTime = 120f; // Default, nên lấy từ config/BlueprintGrowth
    private string _resultCardId = "berrybush"; // Default

    private float _currentProgress = 0f;
    private bool _isGrowing = false;

    public void Initialize(string resultCardId, float time)
    {
        _myGameCard = GetComponent<GameCard>();
        _resultCardId = resultCardId;
        _growthTime = time;
    }

    public void StartGrowing()
    {
        _isGrowing = true;
        // Bật Timer bar UI của Stacklands (TimerAction giả)
        _myGameCard.StartTimer(_growthTime - _currentProgress, new TimerAction(OnGrowthComplete), "Đang phát triển...", "GrowSeed", true, false, false);
    }

    public void StopGrowing()
    {
        _isGrowing = false;
        // Tắt Timer nhưng lưu Progress
        _currentProgress = _growthTime - _myGameCard.CurrentTimerTime;
        if (_currentProgress < 0) _currentProgress = 0;
        _myGameCard.CancelTimer("GrowSeed");
    }

    private void OnGrowthComplete()
    {
        _isGrowing = false;
        
        // Spawn cây trưởng thành
        CardData maturePlant = WorldManager.instance.CreateCard(transform.position, _resultCardId, true, false, true);
        
        // Gửi cây ra ngoài (hoặc thay thế seed trong slot)
        Vector3 spawnPos = transform.position + new Vector3(0, 0, 1.5f);
        maturePlant.MyGameCard.SendToPosition(spawnPos);
        
        // Hủy hạt giống
        _myGameCard.DestroyCard(true, true);
    }
}
