using UnityEngine;

public class CatConditionIndicator : MonoBehaviour
{
    public SpriteRenderer ConditionIcon;
    public CatCardData CatData;
    private GameCard _gameCard;

    private void Awake()
    {
        _gameCard = GetComponentInParent<GameCard>();
        if (_gameCard != null && _gameCard.CardData is CatCardData catData)
        {
            CatData = catData;
        }
    }

    private void OnEnable()
    {
        if (CatData != null)
        {
            CatData.OnConditionStateChanged += UpdateIcon;
            CatData.RefreshConditionState(); // Ensure correct state is shown when enabled
        }
    }

    private void OnDisable()
    {
        if (CatData != null)
        {
            CatData.OnConditionStateChanged -= UpdateIcon;
        }
    }

    public void UpdateIcon(StaminaVisualState state)
    {
        if (ConditionIcon == null || SpriteManager.instance == null) return;

        switch (state)
        {
            case StaminaVisualState.High:
                ConditionIcon.sprite = SpriteManager.instance.StaminaHighIcon;
                ConditionIcon.gameObject.SetActive(true);
                break;
            case StaminaVisualState.Medium:
                ConditionIcon.sprite = SpriteManager.instance.StaminaMediumIcon;
                ConditionIcon.gameObject.SetActive(true);
                break;
            case StaminaVisualState.Low:
                ConditionIcon.sprite = SpriteManager.instance.StaminaLowIcon;
                ConditionIcon.gameObject.SetActive(true);
                break;
            case StaminaVisualState.Exhausted:
                ConditionIcon.sprite = SpriteManager.instance.ExhaustedIcon;
                ConditionIcon.gameObject.SetActive(true);
                break;
            case StaminaVisualState.Paralyzed:
                ConditionIcon.sprite = SpriteManager.instance.ParalyzedIcon;
                ConditionIcon.gameObject.SetActive(true);
                break;
            default:
                ConditionIcon.gameObject.SetActive(false);
                break;
        }
    }
}
