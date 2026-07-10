using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 关卡成就列表中的单行 View：显示关卡名、锁态、进入按钮。
/// 三星区域默认隐藏，扩展 Phase 再启用。
/// </summary>
public class LevelAchievementItemView : MonoBehaviour
{
    [Header("显示")]
    [SerializeField] private Text titleText;
    [SerializeField] private GameObject lockedOverlay;
    [SerializeField] private Button enterButton;

    [Header("星级（扩展 Phase 前默认隐藏）")]
    [SerializeField] private GameObject starsRoot;
    [SerializeField] private Image[] starImages = new Image[3];
    [SerializeField] private Sprite filledStarSprite;
    [SerializeField] private Sprite emptyStarSprite;

    private int levelIndex;
    private bool isUnlocked;

    /// <summary>点击「进入关卡」时触发，参数为关卡索引。</summary>
    public event Action<int> OnEnterRequested;

    private void Awake()
    {
        if (enterButton != null)
        {
            enterButton.onClick.AddListener(OnEnterButtonClicked);
        }

        if (starsRoot != null)
        {
            starsRoot.SetActive(false);
        }
    }

    /// <summary>绑定该行对应的关卡索引（0 起）。</summary>
    public void SetLevelIndex(int index)
    {
        levelIndex = index;
    }

    /// <summary>刷新单行显示。</summary>
    public void Apply(
        bool unlocked,
        string displayName,
        int starCount = 0,
        bool showStars = false)
    {
        isUnlocked = unlocked;

        if (titleText != null)
        {
            titleText.text = displayName ?? string.Empty;
        }

        if (lockedOverlay != null)
        {
            lockedOverlay.SetActive(!unlocked);
        }

        if (enterButton != null)
        {
            enterButton.interactable = unlocked;
        }

        ApplyStars(starCount, showStars);
    }

    private void ApplyStars(int starCount, bool showStars)
    {
        if (starsRoot != null)
        {
            starsRoot.SetActive(showStars);
        }

        if (!showStars || starImages == null)
        {
            return;
        }

        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] == null)
            {
                continue;
            }

            bool filled = i < starCount;
            if (filled && filledStarSprite != null)
            {
                starImages[i].sprite = filledStarSprite;
                starImages[i].enabled = true;
            }
            else if (!filled && emptyStarSprite != null)
            {
                starImages[i].sprite = emptyStarSprite;
                starImages[i].enabled = true;
            }
            else
            {
                starImages[i].enabled = false;
            }
        }
    }

    private void OnEnterButtonClicked()
    {
        if (!isUnlocked)
        {
            return;
        }

        AudioManager.Instance?.PlayButtonClick();
        OnEnterRequested?.Invoke(levelIndex);
    }

#if UNITY_EDITOR
    [ContextMenu("Validate References")]
    private void ValidateReferences()
    {
        if (titleText == null || enterButton == null)
        {
            Debug.LogError("[LevelAchievementItemView] 引用未配齐，请拖入 titleText 与 enterButton。", this);
        }
    }
#endif
}
