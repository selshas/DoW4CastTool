using UnityEngine;

/// <summary>
/// Abstract base for singleton MonoBehaviours that lives with scene lifetime.
/// Duplicates self-destruct in Awake. OnDestroy cleanup only runs for the true Instance.
/// </summary>
public abstract class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
{
    public static T Instance { get; private set; }

    /// <summary>
    /// Sets up the singleton instance with DontDestroyOnLoad. Duplicates are destroyed immediately.
    /// </summary>
    protected virtual void Awake()
    {
        if (Instance != null)
        {
            Debug.Log($"Singleton '{nameof(T)}' already exists. Destroy new comer");

            Destroy(gameObject);
            return;
        }

        Instance = (T)this;
        OnInitialize();
    }

    /// <summary>
    /// Runs cleanup only when the singleton Instance is being destroyed, not duplicates.
    /// </summary>
    protected void OnDestroy()
    {
        if (Instance != this)
            return;

        Instance = null;
        OnDispose();
    }

    /// <summary>
    /// Called once after the singleton is established. Override for subclass-specific initialization.
    /// </summary>
    protected virtual void OnInitialize() { }

    /// <summary>
    /// Called when the singleton Instance is destroyed. Override for subclass-specific cleanup.
    /// </summary>
    protected virtual void OnDispose() { }
}
