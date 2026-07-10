using UnityEngine;

/// <summary>
/// 单页介绍面板视图：管理本页显隐与 img_Closetip 继续提示。
/// 挂于 Panel0~Panel4 各自独立的预制体根节点。
/// </summary>
public class LevelIntroductionView : MonoBehaviour
{
    [Header("继续提示（留空则按子节点 img_Closetip 自动查找）")]
    [SerializeField] private GameObject closeTip;

    private void Awake()
    {
        ResolveReferences();
        Hide();
    }

    public void Show()
    {
        ResolveReferences();
        gameObject.SetActive(true);
        SetCloseTipVisible(false);
    }

    public void SetCloseTipVisible(bool visible)
    {
        if (closeTip != null)
        {
            closeTip.SetActive(visible);
        }
    }

    public void Hide()
    {
        SetCloseTipVisible(false);
        gameObject.SetActive(false);
    }

    private void ResolveReferences()
    {
        if (closeTip != null)
        {
            return;
        }

        Transform tipTransform = transform.Find("img_Closetip");
        if (tipTransform != null)
        {
            closeTip = tipTransform.gameObject;
        }
    }
}
