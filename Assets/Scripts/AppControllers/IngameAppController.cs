using UnityEngine;
using UnityEngine.SceneManagement;
using static GlobalInputSystem;
using DeviceType = GlobalInputSystem.DeviceType;
using KeyCode = SharpHook.Data.KeyCode;

public class IngameAppController : AppController
{
    public static IngameAppController Instance { get; private set; }

    [SerializeField] private GameObject closingMatchOverlay;
    [SerializeField] private RectTransform closingMatchGauge;

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
        closingMatchOverlay.SetActive(false);
        InitializeToggles();
    }

    /// <summary>
    /// Registers hotkey bindings for ingame app toggling and quit.
    /// </summary>
    public override void InitializeInputs()
    {
        AddInputCmd(
            DeviceType.Keyboard, (uint)KeyCode.VcF1,
            InputState.Pressed,
            (self) =>
            {
                ToggleApp<ScreenCanvas>();
                GetApp<ScreenCanvas>().Clear();
            }
        );
        AddInputCmd(
            DeviceType.Keyboard, (uint)KeyCode.VcF2,
            InputState.Pressed,
            (self) => ToggleApp<MinimapCanvas>()
        );
        AddInputCmd(
            DeviceType.Keyboard, (uint)KeyCode.VcF3,
            InputState.Pressed,
            (self) => ToggleApp<MatchPanel>()
        );

        #region Exit Command
        AddInputCmd(
            DeviceType.Keyboard, (uint)KeyCode.VcBackspace,
            InputState.Released,
            (self) =>
            {
                timer_quit = 0.0f;
                closingMatchOverlay.SetActive(false);
            }
        );
        AddInputCmd(
            DeviceType.Keyboard, (uint)KeyCode.VcBackspace,
            InputState.Hold,
            (self) =>
            {
                timer_quit += Time.deltaTime;

                var fill = Mathf.Clamp01(timer_quit / timeToHoldToQuit);

                closingMatchOverlay.SetActive(true);
                closingMatchGauge.anchorMax = new Vector2(fill, 1f);

                if (timer_quit >= timeToHoldToQuit)
                {
                    GlobalInputSystem.Instance.ForceIdle(DeviceType.Keyboard, (uint)KeyCode.VcBackspace);
                    SceneManager.LoadScene(SceneNames.OutgameOverlay);
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
