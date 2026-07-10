using UnityEngine;

/// <summary>
/// MonoBehaviour 单例基类。子类可重写 PersistAcrossScenes 以跨场景常驻。
/// </summary>
public class BaseMonoManager<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;

    public static T Instance
    {
        get
        {
            if (instance != null)
            {
                return instance;
            }

            return null;
        }
    }

    /// <summary>
    /// 是否跨场景保留。为 true 时自动 DontDestroyOnLoad，并销毁重复实例。
    /// </summary>
    protected virtual bool PersistAcrossScenes => false;

    protected virtual void Awake()
    {
        if (PersistAcrossScenes)
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
        }

        instance = this as T;
    }

    protected virtual void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
