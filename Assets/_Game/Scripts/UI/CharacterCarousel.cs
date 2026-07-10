using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 主界面角色轮播：在 Inspector 绑定 displayImage，从 LevelDatabase 或手动 Sprite 列表取图。
/// </summary>
public class CharacterCarousel : MonoBehaviour
{
    [Header("显示")]
    [SerializeField] private Image displayImage;

    [Header("数据来源（二选一或同时使用，手动列表优先）")]
    [SerializeField] private LevelDatabaseSO levelDatabase;
    [SerializeField] private List<Sprite> manualSprites = new List<Sprite>();

    [Header("播放")]
    [SerializeField] private float intervalSeconds = 2f;
    [SerializeField] private bool playOnEnable = true;
    [SerializeField] private Color emptyPlaceholderColor = new Color(0.25f, 0.25f, 0.3f, 0.6f);

    private readonly List<Sprite> runtimeSprites = new List<Sprite>();
    private Coroutine carouselRoutine;
    private int currentIndex;

    private void OnEnable()
    {
        if (playOnEnable)
        {
            Initialize(levelDatabase);
        }
    }

    private void OnDisable()
    {
        StopCarousel();
    }

    /// <summary>注入关卡表并重新开始轮播。</summary>
    public void Initialize(LevelDatabaseSO database)
    {
        if (database != null)
        {
            levelDatabase = database;
        }

        RebuildSpriteList();
        RestartCarousel();
    }

    private void RebuildSpriteList()
    {
        runtimeSprites.Clear();

        if (manualSprites != null && manualSprites.Count > 0)
        {
            for (int i = 0; i < manualSprites.Count; i++)
            {
                if (manualSprites[i] != null)
                {
                    runtimeSprites.Add(manualSprites[i]);
                }
            }

            if (runtimeSprites.Count > 0)
            {
                return;
            }
        }

        if (levelDatabase == null)
        {
            return;
        }

        IReadOnlyList<LevelEntry> levels = levelDatabase.Levels;
        for (int i = 0; i < levels.Count; i++)
        {
            LevelEntry entry = levels[i];
            if (entry != null && entry.characterSprite != null)
            {
                runtimeSprites.Add(entry.characterSprite);
            }
        }
    }

    private void RestartCarousel()
    {
        StopCarousel();

        if (displayImage == null)
        {
            return;
        }

        if (runtimeSprites.Count == 0)
        {
            displayImage.sprite = null;
            displayImage.color = emptyPlaceholderColor;
            return;
        }

        currentIndex = 0;
        ApplySprite(runtimeSprites[currentIndex]);
        carouselRoutine = StartCoroutine(CarouselRoutine());
    }

    private IEnumerator CarouselRoutine()
    {
        var wait = new WaitForSecondsRealtime(intervalSeconds);

        while (true)
        {
            yield return wait;

            if (runtimeSprites.Count == 0)
            {
                continue;
            }

            currentIndex = (currentIndex + 1) % runtimeSprites.Count;
            ApplySprite(runtimeSprites[currentIndex]);
        }
    }

    private void ApplySprite(Sprite sprite)
    {
        displayImage.sprite = sprite;
        displayImage.color = sprite != null ? Color.white : emptyPlaceholderColor;
        displayImage.preserveAspect = true;
    }

    private void StopCarousel()
    {
        if (carouselRoutine != null)
        {
            StopCoroutine(carouselRoutine);
            carouselRoutine = null;
        }
    }
}
