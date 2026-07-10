using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 过关/失败面板控制器：懒加载 VandDPanel，按关卡索引显示对应预制体。
/// 挂于 SceneFlowManager 节点。
/// </summary>
public class OutcomePanelController : MonoBehaviour
{
    private const string DefaultVandDPanelPath = "Assets/_Game/Prefabs/UI/PanelsSum/VandDPanel.prefab";

    [Header("面板根预制体（含 VictoryPanel_1~4、DefeatPanel_2~4）")]
    [SerializeField] private GameObject vandDPanelPrefab;

    [Header("UI")]
    [SerializeField] private int canvasSortOrder = 180;

    private readonly Dictionary<string, OutcomePanelView> panelViews = new Dictionary<string, OutcomePanelView>();
    private GameObject canvasRoot;
    private bool defeatButtonsBound;
    private bool victoryConfirmed;
    private SceneTransitionUI transitionUI;

    public bool IsShowingVictory { get; private set; }
    public bool IsShowingDefeat { get; private set; }

    private void Awake()
    {
#if UNITY_EDITOR
        EnsurePrefabReference();
#endif
        transitionUI = GetComponent<SceneTransitionUI>();
    }

    public IEnumerator ShowVictoryAndWait(int levelIndex)
    {
        OutcomePanelView panel = ResolvePanel(GetVictoryPanelName(levelIndex));
        if (panel == null)
        {
            Debug.LogWarning(
                $"[OutcomePanelController] 关卡 {levelIndex} 无胜利面板，将直接继续。");
            SceneFlowManager.Instance?.LoadNextLevel();
            yield break;
        }

        HideAll();
        IsShowingVictory = true;
        victoryConfirmed = false;

        transitionUI?.SuppressOverlayForContent();
        panel.Show();
        BindVictoryButton(panel);

        while (!victoryConfirmed)
        {
            yield return null;
        }

        panel.Hide();
        IsShowingVictory = false;
    }

    public bool ShowDefeat(int levelIndex)
    {
        if (levelIndex <= 0)
        {
            Debug.LogWarning("[OutcomePanelController] 教学关不显示失败面板。");
            return false;
        }

        OutcomePanelView panel = ResolvePanel(GetDefeatPanelName(levelIndex));
        if (panel == null)
        {
            Debug.LogWarning(
                $"[OutcomePanelController] 关卡 {levelIndex} 无失败面板，回退 GameOverUI。");
            return false;
        }

        HideAll();
        IsShowingDefeat = true;

        transitionUI?.SuppressOverlayForContent();
        panel.Show();
        BindDefeatButtons();
        return true;
    }

    public void HideAll()
    {
        foreach (KeyValuePair<string, OutcomePanelView> pair in panelViews)
        {
            if (pair.Value != null)
            {
                pair.Value.Hide();
            }
        }

        IsShowingVictory = false;
        IsShowingDefeat = false;
    }

    private void BindVictoryButton(OutcomePanelView panel)
    {
        Button button = panel.ActionButton;
        if (button == null)
        {
            Debug.LogWarning("[OutcomePanelController] 胜利面板未找到 btn_Goon。");
            return;
        }

        button.onClick.RemoveListener(OnVictoryContinueClicked);
        button.onClick.AddListener(OnVictoryContinueClicked);
    }

    private void BindDefeatButtons()
    {
        if (defeatButtonsBound)
        {
            return;
        }

        foreach (KeyValuePair<string, OutcomePanelView> pair in panelViews)
        {
            if (!pair.Key.StartsWith("DefeatPanel_"))
            {
                continue;
            }

            Button button = pair.Value.ActionButton;
            if (button == null)
            {
                continue;
            }

            button.onClick.RemoveListener(OnDefeatRestartClicked);
            button.onClick.AddListener(OnDefeatRestartClicked);
        }

        defeatButtonsBound = true;
    }

    private void OnVictoryContinueClicked()
    {
        if (victoryConfirmed)
        {
            return;
        }

        AudioManager.Instance?.PlayButtonClick();
        victoryConfirmed = true;
        HideAll();

        if (SceneFlowManager.Instance == null)
        {
            Debug.LogWarning("[OutcomePanelController] SceneFlowManager 不存在，无法加载下一关。");
            return;
        }

        SceneFlowManager.Instance.LoadNextLevel();
    }

    private void OnDefeatRestartClicked()
    {
        AudioManager.Instance?.PlayButtonClick();
        HideAll();

        if (SceneFlowManager.Instance != null)
        {
            SceneFlowManager.Instance.ReloadCurrentLevel();
            return;
        }

        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.ResetToPlaying();
        }
        else
        {
            Time.timeScale = 1f;
        }

        AudioManager.Instance?.PlayMusicForActiveScene();
        Debug.LogWarning("[OutcomePanelController] SceneFlowManager 不存在，重试未重载场景。");
    }

    private static string GetVictoryPanelName(int levelIndex)
    {
        return $"VictoryPanel_{levelIndex + 1}";
    }

    private static string GetDefeatPanelName(int levelIndex)
    {
        return $"DefeatPanel_{levelIndex + 1}";
    }

    private OutcomePanelView ResolvePanel(string panelName)
    {
        if (panelViews.TryGetValue(panelName, out OutcomePanelView cached) && cached != null)
        {
            return cached;
        }

        EnsurePanelsBuilt();
        panelViews.TryGetValue(panelName, out cached);
        return cached;
    }

    private void EnsurePanelsBuilt()
    {
        if (canvasRoot != null)
        {
            return;
        }

        if (vandDPanelPrefab == null)
        {
            Debug.LogError("[OutcomePanelController] 未配置 VandDPanel 预制体。");
            return;
        }

        EnsureEventSystem();

        canvasRoot = new GameObject("OutcomePanelCanvas");
        canvasRoot.transform.SetParent(transform, false);

        Canvas canvas = canvasRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = canvasSortOrder;

        CanvasScaler scaler = canvasRoot.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasRoot.AddComponent<GraphicRaycaster>();

        RectTransform canvasRect = canvasRoot.GetComponent<RectTransform>();
        if (canvasRect != null)
        {
            canvasRect.anchorMin = Vector2.zero;
            canvasRect.anchorMax = Vector2.one;
            canvasRect.offsetMin = Vector2.zero;
            canvasRect.offsetMax = Vector2.zero;
        }

        GameObject panelRoot = Instantiate(vandDPanelPrefab, canvasRoot.transform);
        panelRoot.name = vandDPanelPrefab.name;

        RectTransform panelRect = panelRoot.GetComponent<RectTransform>();
        if (panelRect != null)
        {
            OutcomePanelView.CenterInParent(panelRect);
        }

        CachePanelViews(panelRoot.transform);
        HideAll();
    }

    private void CachePanelViews(Transform root)
    {
        panelViews.Clear();

        OutcomePanelView[] views = root.GetComponentsInChildren<OutcomePanelView>(true);
        foreach (OutcomePanelView view in views)
        {
            panelViews[view.gameObject.name] = view;
            view.ApplyCenteredPosition();
            view.Hide();
            continue;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);
            if (panelViews.ContainsKey(child.name))
            {
                continue;
            }

            OutcomePanelView view = child.GetComponent<OutcomePanelView>();
            if (view == null)
            {
                view = child.gameObject.AddComponent<OutcomePanelView>();
            }

            panelViews[child.name] = view;
            view.ApplyCenteredPosition();
            view.Hide();
        }
    }

    private static void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<StandaloneInputModule>();
    }

#if UNITY_EDITOR
    private void EnsurePrefabReference()
    {
        if (vandDPanelPrefab != null)
        {
            return;
        }

        vandDPanelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DefaultVandDPanelPath);
    }
#endif
}
