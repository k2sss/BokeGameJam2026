using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    public static GameOverUI Instance { get; private set; }

    [SerializeField] private GameObject panelRoot;

    private Font uiFont;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        EnsureEventSystem();
        EnsurePanel();
        Hide();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void Show()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }
    }

    public void Hide()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    public void OnRetryClicked()
    {
        if (SceneFlowManager.Instance != null)
        {
            SceneFlowManager.Instance.ReloadCurrentLevel();
            return;
        }

        Hide();

        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.ResetToPlaying();
        }
        else
        {
            Time.timeScale = 1f;
        }

        Debug.LogWarning("[GameOverUI] SceneFlowManager not found, retry did not reload scene.");
    }

    private void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<StandaloneInputModule>();
    }

    private void EnsurePanel()
    {
        if (panelRoot != null)
        {
            return;
        }

        GameObject canvasObject = new GameObject("GameOverCanvas");
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        canvasObject.AddComponent<GraphicRaycaster>();

        panelRoot = CreatePanel(canvasObject.transform);
        CreateTitle(panelRoot.transform);
        CreateRetryButton(panelRoot.transform);
    }

    private GameObject CreatePanel(Transform parent)
    {
        GameObject panel = new GameObject("GameOverPanel");
        panel.transform.SetParent(parent, false);

        Image image = panel.AddComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.72f);

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        return panel;
    }

    private void CreateTitle(Transform parent)
    {
        GameObject titleObject = new GameObject("Title");
        titleObject.transform.SetParent(parent, false);

        Text title = titleObject.AddComponent<Text>();
        title.font = uiFont;
        title.text = "Game Over";
        title.alignment = TextAnchor.MiddleCenter;
        title.fontSize = 56;
        title.color = Color.white;

        RectTransform rect = titleObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(600f, 100f);
        rect.anchoredPosition = new Vector2(0f, 80f);
    }

    private void CreateRetryButton(Transform parent)
    {
        GameObject buttonObject = new GameObject("RetryButton");
        buttonObject.transform.SetParent(parent, false);

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.2f, 0.55f, 0.95f, 1f);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(OnRetryClicked);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(260f, 70f);
        rect.anchoredPosition = new Vector2(0f, -40f);

        GameObject labelObject = new GameObject("Label");
        labelObject.transform.SetParent(buttonObject.transform, false);

        Text label = labelObject.AddComponent<Text>();
        label.font = uiFont;
        label.text = "Retry";
        label.alignment = TextAnchor.MiddleCenter;
        label.fontSize = 32;
        label.color = Color.white;

        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
    }
}
