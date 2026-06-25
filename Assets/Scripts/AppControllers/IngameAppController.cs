using UnityEngine;
using UnityEngine.SceneManagement;
using static GlobalInputSystem;
using DeviceType = GlobalInputSystem.DeviceType;
using KeyCode = SharpHook.Native.KeyCode;

public class IngameAppController : UtilityAppBase
{
    public static IngameAppController Instance { get; private set; }

    public GameObject Helper;

    public ScreenCanvas App_ScreenCanvas;
    public MinimapCanvas App_MinimapCanvas;
    public PlayersNamePanel App_PlayersNamePanel;
    public ScreenVeil App_ScreenVeil;
    public MinimapOverlay App_MinimapOverlay;

    public float timeToHoldToQuit = 2.0f;
    private float timer_quit = 0.0f;

    /// <summary>
    /// Toggles the ScreenCanvas, disabling MinimapCanvas if activated.
    /// </summary>
    public void ToggleApp_ScreenCanvas()
    {
        var isActive = App_ScreenCanvas.gameObject.activeSelf;
        App_ScreenCanvas.gameObject.SetActive(!isActive);

        if (App_ScreenCanvas.gameObject.activeSelf)
            App_MinimapCanvas.gameObject.SetActive(false);
    }

    /// <summary>
    /// Toggles the MinimapCanvas, disabling ScreenCanvas if activated.
    /// </summary>
    public void ToggleApp_MinimapCanvas()
    {
        var isActive = App_MinimapCanvas.gameObject.activeSelf;
        App_MinimapCanvas.gameObject.SetActive(!isActive);

        if (App_MinimapCanvas.gameObject.activeSelf)
            App_ScreenCanvas.gameObject.SetActive(false);
    }

    /// <summary>
    /// Toggles the PlayersNamePanel visibility.
    /// </summary>
    public void ToggleApp_PlayersNamePanel()
    {
        var isActive = App_PlayersNamePanel.gameObject.activeSelf;
        App_PlayersNamePanel.gameObject.SetActive(!isActive);
    }

    /// <summary>
    /// Toggles the ScreenVeil visibility.
    /// </summary>
    public void ToggleApp_ScreenVeil()
    {
        var isActive = App_ScreenVeil.gameObject.activeSelf;
        App_ScreenVeil.gameObject.SetActive(!isActive);
    }

    /// <summary>
    /// Toggles the MinimapOverlay visibility.
    /// </summary>
    public void ToggleApp_MinimapOverlay()
    {
        var isActive = App_MinimapOverlay.gameObject.activeSelf;
        App_MinimapOverlay.gameObject.SetActive(!isActive);
    }

    /// <summary>
    /// Registers the singleton instance.
    /// </summary>
    protected void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    protected override void Start()
    {
        base.Start();
    }

    /// <summary>
    /// Registers hotkey bindings for ingame app toggling, helper, and quit.
    /// </summary>
    public override void InitializeInputs()
    {
        AddInputCmd(
            DeviceType.Keyboard, (uint)KeyCode.VcF2,
            InputState.Pressed,
            (self) =>
            {
                ToggleApp_ScreenCanvas();
                App_ScreenCanvas.Clear();
            }
        );
        AddInputCmd(
            DeviceType.Keyboard, (uint)KeyCode.VcF3,
            InputState.Pressed,
            (self) => ToggleApp_MinimapCanvas()
        );
        AddInputCmd(
            DeviceType.Keyboard, (uint)KeyCode.VcF4,
            InputState.Pressed,
            (self) => ToggleApp_PlayersNamePanel()
        );
        AddInputCmd(
            DeviceType.Keyboard, (uint)KeyCode.VcF5,
            InputState.Pressed,
            (self) => ToggleApp_MinimapOverlay()
        );

        AddInputCmd(
            DeviceType.Keyboard, (uint)KeyCode.VcF8,
            InputState.Pressed,
            (self) =>
            {
                if (Helper != null)
                    Helper.SetActive(!Helper.activeSelf);
            }
        );

        #region Exit Command
        AddInputCmd(
            DeviceType.Keyboard, (uint)KeyCode.VcBackspace,
            InputState.Released,
            (self) => timer_quit = 0.0f
        );
        AddInputCmd(
            DeviceType.Keyboard, (uint)KeyCode.VcBackspace,
            InputState.Hold,
            (self) =>
            {
                timer_quit += Time.deltaTime;

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
