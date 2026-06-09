using UnityEngine;
using UnityEngine.UI;
using Mewtations.Systems.Planting;

public class SpiritFieldUI : MonoBehaviour
{
    [Header("Dependencies")]
    public SpiritFieldRuntime Runtime;

    [Header("UI Fixed Slots (Tier 1 = 4 slots)")]
    public PlantSlotView[] SlotViews; // Assign exactly 4 views in inspector

    [Header("Water Pool UI")]
    public Image WaterPoolFillBar;
    // public Text WaterPoolText;

    private void Update()
    {
        if (Runtime == null || SlotViews == null) return;

        // Render Water Pool
        if (WaterPoolFillBar != null && Runtime.WaterPool != null)
        {
            WaterPoolFillBar.fillAmount = (float)Runtime.WaterPool.CurrentEssence / Runtime.WaterPool.MaxEssence;
        }

        // Render Fixed Slots
        for (int i = 0; i < SlotViews.Length; i++)
        {
            if (i < Runtime.Slots.Count)
            {
                StructureSlot slotData = Runtime.Slots[i];
                UpdateSlotView(SlotViews[i], slotData);
            }
            else
            {
                // Unused slots for this tier can be hidden
                SlotViews[i].RootObject.SetActive(false);
            }
        }
    }

    private void UpdateSlotView(PlantSlotView view, StructureSlot data)
    {
        if (view == null || view.RootObject == null) return;

        if (data.IsEmpty)
        {
            // Empty state
            view.RootObject.SetActive(true);
            view.EmptyStateObject.SetActive(true);
            view.GrowingStateObject.SetActive(false);
            view.MatureStateObject.SetActive(false);
            if (view.ProgressBar != null) view.ProgressBar.fillAmount = 0f;
            
            if (view.HoverInfoBox != null)
            {
                view.HoverInfoBox.enabled = false; // Tắt tooltip riêng khi ô trống
            }
            return;
        }

        view.RootObject.SetActive(true);
        view.EmptyStateObject.SetActive(false);

        if (data.IsComplete)
        {
            // Mature state
            view.GrowingStateObject.SetActive(false);
            view.MatureStateObject.SetActive(true);
            if (view.ProgressBar != null) view.ProgressBar.fillAmount = 1f;
        }
        else
        {
            // Growing state
            view.GrowingStateObject.SetActive(true);
            view.MatureStateObject.SetActive(false);
            
            if (view.ProgressBar != null && data.MaxProgress > 0)
            {
                view.ProgressBar.fillAmount = data.CurrentProgress / data.MaxProgress;
            }
        }

        // --- Tooltip Override (Mắt thần) ---
        if (view.HoverInfoBox != null)
        {
            view.HoverInfoBox.enabled = true;
            
            // Lấy tên thẻ từ bộ Localization (Quy ước: id + "_name" và "_description")
            // Hoặc có thể parse từ CardData nếu lưu sẵn. Tạm thời dùng Localization ID.
            string title = MewtationsLoc.Translate(data.StoredDataId + "_name");
            if (string.IsNullOrEmpty(title)) title = data.StoredDataId; // fallback
            
            view.HoverInfoBox.InfoBoxTitle = title;

            if (data.IsComplete)
            {
                view.HoverInfoBox.InfoBoxText = MewtationsLoc.Translate("plant_stage_mature"); // "Trưởng Thành"
            }
            else
            {
                int percent = data.MaxProgress > 0 ? Mathf.RoundToInt((data.CurrentProgress / data.MaxProgress) * 100f) : 0;
                view.HoverInfoBox.InfoBoxText = MewtationsLoc.Translate("plant_stage_young") + $" ({percent}%)";
            }
        }
    }
}

[System.Serializable]
public class PlantSlotView
{
    public GameObject RootObject;
    public GameObject EmptyStateObject;
    public GameObject GrowingStateObject;
    public GameObject MatureStateObject;
    public Image ProgressBar;
    public ShowInfoBox HoverInfoBox;
}
