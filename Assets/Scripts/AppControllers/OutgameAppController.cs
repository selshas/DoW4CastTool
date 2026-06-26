using UnityEngine;
using static GlobalInputSystem;
using DeviceType = GlobalInputSystem.DeviceType;
using KeyCode = SharpHook.Data.KeyCode;

public class OutgameAppController : AppController
{
    public static OutgameAppController Instance { get; private set; }

    [SerializeField] private GameObject terminationOverlay;
    private RingGauge terminationGauge;

    public float timeToHoldToQuit = 2.0f;
    protected float timer_quit = 0.0f;

    /// <summary>
    /// Registers the singleton instance, initializes overlays, and wires toggle listeners.
    /// </summary>
    protected void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        terminationOverlay.SetActive(false);
        terminationGauge = terminationOverlay.GetComponentInChildren<RingGauge>(true);
        InitializeToggles();
    }

    /// <summary>
    /// Registers hotkey bindings for outgame app toggling and quit.
    /// </summary>
    public override void InitializeInputs()
    {
        AddInputCmd(
            DeviceType.Keyboard, (uint)KeyCode.VcF1,
            InputState.Pressed,
            (self) => ToggleApp<MatchSetup>()
        );

        AddInputCmd(
            DeviceType.Keyboard, (uint)KeyCode.VcF2,
            InputState.Pressed,
            (self) => ToggleApp<KnownPlayerListManager>()
        );

        #region Exit Command
        AddInputCmd(
            DeviceType.Keyboard, (uint)KeyCode.VcBackspace,
            InputState.Released,
            (self) =>
            {
                timer_quit = 0.0f;
                terminationOverlay.SetActive(false);
            }
        );
        AddInputCmd(
            DeviceType.Keyboard, (uint)KeyCode.VcBackspace,
            InputState.Hold,
            (self) =>
            {
                timer_quit += Time.deltaTime;

                terminationOverlay.SetActive(true);
                terminationGauge.FillAmount = timer_quit / timeToHoldToQuit;

                if (timer_quit >= timeToHoldToQuit)
                {
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                }
            }
        );
        #endregion Exit Command
    }

    /// <summary>
    /// Clears the singleton instance on destroy.
    /// </summary>
    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
