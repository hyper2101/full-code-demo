using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class RecipeBookController : MonoBehaviour
{
    public static RecipeBookController Instance { get; private set; }

    [Header("UI References")]
    public GameObject CanvasRoot;
    public Transform TabContainer;
    public Transform RecipeGrid;
    public RecipeDetailPanel DetailPanel;
    public Text EmptyStateText;
    
    [Header("Prefabs")]
    public GameObject TabPrefab;
    public GameObject RecipeSlotPrefab;

    private Dictionary<RecipeCategory, List<Blueprint>> categoryCache = new Dictionary<RecipeCategory, List<Blueprint>>();
    private Dictionary<string, Blueprint> idCache = new Dictionary<string, Blueprint>();

    private RecipeCategory currentCategory;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        BuildCache();
    }

    private void BuildCache()
    {
        categoryCache.Clear();
        idCache.Clear();

        if (WorldManager.instance == null || WorldManager.instance.BlueprintPrefabs == null) return;

        List<Blueprint> allBlueprints = WorldManager.instance.BlueprintPrefabs;
        foreach (Blueprint bp in allBlueprints)
        {
            idCache[bp.Id] = bp;

            if (!categoryCache.ContainsKey(bp.CodexCategory))
            {
                categoryCache[bp.CodexCategory] = new List<Blueprint>();
            }
            categoryCache[bp.CodexCategory].Add(bp);
        }

        // Sort each category by CodexSortOrder
        foreach (var key in categoryCache.Keys.ToList())
        {
            categoryCache[key] = categoryCache[key].OrderBy(b => b.CodexSortOrder).ToList();
        }
    }

    public void OpenRecipeBook()
    {
        CanvasRoot.SetActive(true);
        GenerateTabs();
        
        // Select first category by default if available
        if (categoryCache.Keys.Count > 0)
        {
            SelectCategory(categoryCache.Keys.First());
        }
        else
        {
            ShowEmptyState(MewtationsLoc.Translate("term_no_recipes_found"));
        }
    }

    public void CloseRecipeBook()
    {
        CanvasRoot.SetActive(false);
    }

    private void GenerateTabs()
    {
        // Clear existing tabs
        foreach (Transform child in TabContainer)
        {
            Destroy(child.gameObject);
        }

        // Generate new tabs based on cached categories
        foreach (RecipeCategory category in categoryCache.Keys)
        {
            GameObject tabObj = Instantiate(TabPrefab, TabContainer);
            Button tabButton = tabObj.GetComponent<Button>();
            if (tabButton != null)
            {
                RecipeCategory catToSelect = category;
                tabButton.onClick.AddListener(() => SelectCategory(catToSelect));
            }
            Text tabText = tabObj.GetComponentInChildren<Text>();
            if (tabText != null)
            {
                tabText.text = MewtationsLoc.Translate("recipe_category_" + category.ToString().ToLower());
            }
        }
    }

    public void SelectCategory(RecipeCategory category)
    {
        currentCategory = category;
        PopulateGrid(category);
        if (DetailPanel != null)
        {
            DetailPanel.ClearDetails();
        }
    }

    private void PopulateGrid(RecipeCategory category)
    {
        // Clear existing slots
        foreach (Transform child in RecipeGrid)
        {
            Destroy(child.gameObject);
        }

        if (!categoryCache.ContainsKey(category) || categoryCache[category].Count == 0)
        {
            ShowEmptyState(MewtationsLoc.Translate("term_no_recipes_unlocked"));
            return;
        }

        List<Blueprint> blueprints = categoryCache[category];
        int visibleCount = 0;

        foreach (Blueprint bp in blueprints)
        {
            if (!bp.ShowInRecipeBook) continue;

            bool isUnlocked = SaveManager.instance.CurrentSave.UnlockedRecipeIds.Contains(bp.Id);
            
            if (bp.HiddenUntilUnlocked && !isUnlocked) continue;

            GameObject slotObj = Instantiate(RecipeSlotPrefab, RecipeGrid);
            RecipeSlotUI slotUI = slotObj.GetComponent<RecipeSlotUI>();
            if (slotUI != null)
            {
                slotUI.Init(bp, isUnlocked);
            }
            visibleCount++;
        }

        if (visibleCount == 0)
        {
            ShowEmptyState(MewtationsLoc.Translate("term_no_recipes_unlocked"));
        }
        else
        {
            if (EmptyStateText != null)
            {
                EmptyStateText.gameObject.SetActive(false);
            }
        }
    }

    private void ShowEmptyState(string message)
    {
        if (EmptyStateText != null)
        {
            EmptyStateText.text = message;
            EmptyStateText.gameObject.SetActive(true);
        }
    }

    public void OnRecipeSelected(Blueprint bp, RecipeSlotUI slotUI)
    {
        // Clear unread badge
        if (SaveManager.instance.CurrentSave.UnreadRecipeIds.Contains(bp.Id))
        {
            SaveManager.instance.CurrentSave.MarkRecipeRead(bp.Id);
            slotUI.RefreshNewBadge(false);
        }

        if (DetailPanel != null)
        {
            DetailPanel.ShowDetails(bp);
        }
    }
}
