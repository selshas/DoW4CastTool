/// <summary>
/// Abstract base for singleton MonoBehaviours that persist across scene changes.
/// Duplicates self-destruct in Awake. OnDestroy cleanup only runs for the true Instance.
/// </summary>
public abstract class SingletonBehaviour<T> : SceneBehaviour<T> where T : SceneBehaviour<T>
{
    /// <summary>
    /// Sets up the singleton instance with DontDestroyOnLoad. Duplicates are destroyed immediately.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }
}
