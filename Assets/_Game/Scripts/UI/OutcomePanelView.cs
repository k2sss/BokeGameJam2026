using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 单页胜负面板视图：管理显隐与继续/重试按钮引用。
/// 保留预制体上的尺寸与缩放，仅将位置居中。
/// </summary>
public class OutcomePanelView : MonoBehaviour
{
    private const string VictoryButtonName = "btn_Goon";
    private const string DefeatButtonName = "btn_Restart";

    [Header("按钮（留空则按子节点名自动查找）")]
    [SerializeField] private Button actionButton;

    public Button ActionButton => actionButton;

    public bool IsVictoryPanel { get; private set; }

    private void Awake()
    {
        ResolveReferences();
        ApplyCenteredPosition();
        Hide();
    }

    public void Show()
    {
        ResolveReferences();
        ApplyCenteredPosition();
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 将面板固定在父节点中心，保留预制体的 sizeDelta 与 localScale。
    /// </summary>
    public void ApplyCenteredPosition()
    {
        CenterInParent(transform as RectTransform);
    }

    public static void CenterInParent(RectTransform rect)
    {
        if (rect == null)
        {
            return;
        }

        Vector2 sizeDelta = rect.sizeDelta;
        Vector3 localScale = rect.localScale;

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = sizeDelta;
        rect.localScale = localScale;
    }

    private void ResolveReferences()
    {
        if (actionButton != null)
        {
            return;
        }

        Transform victoryButton = FindChildRecursive(transform, VictoryButtonName);
        if (victoryButton != null)
        {
            actionButton = victoryButton.GetComponent<Button>();
            IsVictoryPanel = true;
            return;
        }

        Transform defeatButton = FindChildRecursive(transform, DefeatButtonName);
        if (defeatButton != null)
        {
            actionButton = defeatButton.GetComponent<Button>();
            IsVictoryPanel = false;
        }
    }

    private static Transform FindChildRecursive(Transform parent, string childName)
    {
        if (parent.name == childName)
        {
            return parent;
        }

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform found = FindChildRecursive(parent.GetChild(i), childName);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }
}
