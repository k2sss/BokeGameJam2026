using System.Collections;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 叙事介绍控制器：按页索引从独立预制体懒加载面板，延迟显示继续提示并等待全屏点击。
/// 挂于 SceneFlowManager 节点。
/// </summary>
public class NarrativeIntroController : MonoBehaviour
{
    public const int NewGamePageIndex = 0;
    public const int FirstLevelPageIndex = 1;
    public const int PageCount = 5;

    private static readonly string[] DefaultPanelPrefabPaths =
    {
        "Assets/_Game/Prefabs/UI/PanelsSum/Panel0.prefab",
        "Assets/_Game/Prefabs/UI/PanelsSum/Panel1.prefab",
        "Assets/_Game/Prefabs/UI/PanelsSum/Panel2.prefab",
        "Assets/_Game/Prefabs/UI/PanelsSum/Panel3.prefab",
        "Assets/_Game/Prefabs/UI/PanelsSum/Panel4.prefab"
    };

    [Header("独立面板预制体（Panel0=新游戏，Panel1~4=各关）")]
    [SerializeField] private GameObject[] panelPrefabs = new GameObject[PageCount];

    [Header("交互")]
    [SerializeField] private float closeTipDelay = 1.5f;
    [SerializeField] private int canvasSortOrder = 150;

    private readonly LevelIntroductionView[] panelInstances = new LevelIntroductionView[PageCount];
    private int activePageIndex = -1;

    public bool IsShowing { get; private set; }

    private void Awake()
    {
#if UNITY_EDITOR
        EnsurePrefabReferences();
#endif
    }

    public IEnumerator ShowNewGameIntroAndWait()
    {
        yield return ShowPageAndWait(NewGamePageIndex);
    }

    public IEnumerator ShowLevelIntroAndWait(int levelIndex)
    {
        int pageIndex = FirstLevelPageIndex + levelIndex;
        if (!HasPanelPrefab(pageIndex))
        {
            Debug.LogWarning(
                $"[NarrativeIntroController] 关卡 {levelIndex} 无对应介绍预制体（page {pageIndex}），已跳过。");
            yield break;
        }

        yield return ShowPageAndWait(pageIndex);
    }

    public IEnumerator ShowPageAndWait(int pageIndex)
    {
        LevelIntroductionView panel = EnsurePanelInstance(pageIndex);
        if (panel == null)
        {
            Debug.LogWarning($"[NarrativeIntroController] 页面 {pageIndex} 无可用介绍面板，已跳过。");
            yield break;
        }

        HideActivePanel();
        IsShowing = true;
        activePageIndex = pageIndex;
        panel.Show();

        if (closeTipDelay > 0f)
        {
            yield return WaitUnscaled(closeTipDelay);
        }

        panel.SetCloseTipVisible(true);
        yield return WaitForScreenClick();
        panel.Hide();
        activePageIndex = -1;
        IsShowing = false;
    }

    private void HideActivePanel()
    {
        if (activePageIndex < 0)
        {
            return;
        }

        LevelIntroductionView panel = panelInstances[activePageIndex];
        if (panel != null)
        {
            panel.Hide();
        }

        activePageIndex = -1;
    }

    private bool HasPanelPrefab(int pageIndex)
    {
        return pageIndex >= 0 && pageIndex < PageCount && panelPrefabs[pageIndex] != null;
    }

    private LevelIntroductionView EnsurePanelInstance(int pageIndex)
    {
        if (!HasPanelPrefab(pageIndex))
        {
#if UNITY_EDITOR
            EnsurePrefabReferences();
#endif
        }

        if (!HasPanelPrefab(pageIndex))
        {
            return null;
        }

        if (panelInstances[pageIndex] != null)
        {
            return panelInstances[pageIndex];
        }

        GameObject panelObject = Instantiate(panelPrefabs[pageIndex], transform);
        panelObject.name = panelPrefabs[pageIndex].name;

        LevelIntroductionView panel = panelObject.GetComponent<LevelIntroductionView>();
        if (panel == null)
        {
            panel = panelObject.AddComponent<LevelIntroductionView>();
        }

        ConfigureCanvas(panelObject);
        panel.Hide();
        panelInstances[pageIndex] = panel;
        return panel;
    }

    private void ConfigureCanvas(GameObject panelObject)
    {
        Canvas canvas = panelObject.GetComponent<Canvas>();
        if (canvas == null)
        {
            return;
        }

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = canvasSortOrder;
    }

    private static IEnumerator WaitUnscaled(float seconds)
    {
        float elapsed = 0f;
        while (elapsed < seconds)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    private static IEnumerator WaitForScreenClick()
    {
        while (!WasScreenClickedThisFrame())
        {
            yield return null;
        }
    }

    private static bool WasScreenClickedThisFrame()
    {
        if (Input.GetMouseButtonDown(0))
        {
            return true;
        }

        return Input.touchCount > 0 &&
               Input.GetTouch(0).phase == TouchPhase.Began;
    }

#if UNITY_EDITOR
    private void EnsurePrefabReferences()
    {
        for (int i = 0; i < PageCount; i++)
        {
            if (panelPrefabs[i] != null)
            {
                continue;
            }

            panelPrefabs[i] = AssetDatabase.LoadAssetAtPath<GameObject>(DefaultPanelPrefabPaths[i]);
        }
    }
#endif
}
