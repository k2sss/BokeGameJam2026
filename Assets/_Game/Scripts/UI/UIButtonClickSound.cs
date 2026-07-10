using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 挂到任意 Button 上，点击时播放 UI 音效。可选覆盖音频名。
/// </summary>
[RequireComponent(typeof(Button))]
public class UIButtonClickSound : MonoBehaviour
{
    [SerializeField] private string audioName = GameConstants.AudioNames.ButtonClick;
    [SerializeField] private bool playOnAwakeBind = true;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (playOnAwakeBind && button != null)
        {
            button.onClick.AddListener(PlayClickSound);
        }
    }

    public void PlayClickSound()
    {
        if (AudioManager.Instance == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(audioName)
            || audioName == GameConstants.AudioNames.ButtonClick)
        {
            AudioManager.Instance.PlayButtonClick();
        }
        else
        {
            AudioManager.Instance.PlaySfx(audioName);
        }
    }
}
