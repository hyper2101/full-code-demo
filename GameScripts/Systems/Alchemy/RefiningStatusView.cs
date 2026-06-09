using UnityEngine;
using UnityEngine.UI;

namespace Mewtations.Systems.Alchemy
{
    public class RefiningStatusView : MonoBehaviour
    {
        public AlchemyFurnaceRuntime FurnaceRuntime;

        [Header("UI Elements")]
        public GameObject RootContainer;
        public Text RecipeNameText;
        public Image ProgressBar;

        private void OnEnable()
        {
            if (FurnaceRuntime != null)
            {
                FurnaceRuntime.OnRecipeValid += HandleRecipeValid;
                FurnaceRuntime.OnProgressUpdated += HandleProgressUpdated;
                FurnaceRuntime.OnRefiningStarted += HandleRefiningStarted;
                FurnaceRuntime.OnRefiningStopped += HandleRefiningStopped;
            }
        }

        private void OnDisable()
        {
            if (FurnaceRuntime != null)
            {
                FurnaceRuntime.OnRecipeValid -= HandleRecipeValid;
                FurnaceRuntime.OnProgressUpdated -= HandleProgressUpdated;
                FurnaceRuntime.OnRefiningStarted -= HandleRefiningStarted;
                FurnaceRuntime.OnRefiningStopped -= HandleRefiningStopped;
            }
        }

        private void Start()
        {
            RootContainer.SetActive(false);
        }

        private void HandleRecipeValid(AlchemyRecipe recipe)
        {
            if (FurnaceRuntime.IsRefining) return;

            if (recipe != null)
            {
                RootContainer.SetActive(true);
                string localizedName = MewtationsLoc.Translate(recipe.ResultCardId + "_name");
                if (string.IsNullOrEmpty(localizedName)) localizedName = recipe.ResultCardId;
                
                RecipeNameText.text = $"[Preview] {localizedName}";
                if (ProgressBar != null) ProgressBar.fillAmount = 0f;
            }
            else
            {
                RootContainer.SetActive(false);
            }
        }

        private void HandleRefiningStarted()
        {
            RootContainer.SetActive(true);
            if (FurnaceRuntime.CurrentValidRecipe != null)
            {
                string localizedName = MewtationsLoc.Translate(FurnaceRuntime.CurrentValidRecipe.ResultCardId + "_name");
                if (string.IsNullOrEmpty(localizedName)) localizedName = FurnaceRuntime.CurrentValidRecipe.ResultCardId;
                
                RecipeNameText.text = $"Luyện: {localizedName}";
            }
        }

        private void HandleProgressUpdated(float normalizedProgress)
        {
            if (ProgressBar != null)
            {
                ProgressBar.fillAmount = normalizedProgress;
            }
        }

        private void HandleRefiningStopped()
        {
            RootContainer.SetActive(false);
        }
    }
}
