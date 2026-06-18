using UnityEngine;
using static GlobalInputSystem;
using DeviceType = GlobalInputSystem.DeviceType;
using KeyCode = SharpHook.Native.KeyCode;

public class GlobalAppController : UtilityAppBase
{
    public static GlobalAppController Instance { get; private set; }

    public GlobalAppControllerHelper Helper;

    public ScreenCanvas App_ScreenCanvas;
    public CardTable App_CardTable;
    public MinimapCanvas App_MinimapCanvas;
    public PlayersNamePanel App_PlayersNamePanel;
    public ScreenVeil App_ScreenVeil;
    public MinimapOverlay App_MinimapOverlay;

    public float timeToHoldToQuit = 2.0f; // Time to hold backspace to quit the app
    private float timer_quit = 0.0f;

    public void ToggleApp_ScreenCanvas()
    {
        var isActive = App_ScreenCanvas.gameObject.activeSelf;
        App_ScreenCanvas.gameObject.SetActive(!isActive);

        if (App_ScreenCanvas.gameObject.activeSelf)
        {
            App_MinimapCanvas.gameObject.SetActive(false);
        }
    }
    public void ToggleApp_CardTable()
    {
        var isActive = App_CardTable.gameObject.activeSelf;
        App_CardTable.gameObject.SetActive(!isActive);
    }
    public void ToggleApp_MinimapCanvas()
    {
        var isActive = App_MinimapCanvas.gameObject.activeSelf;
        App_MinimapCanvas.gameObject.SetActive(!isActive);

        if (App_MinimapCanvas.gameObject.activeSelf)
        {
            App_ScreenCanvas.gameObject.SetActive(false);
        }
    }
    public void ToggleApp_PlayersNamePanel()
    {
        var isActive = App_PlayersNamePanel.gameObject.activeSelf;
        App_PlayersNamePanel.gameObject.SetActive(!isActive);
    }
    public void ToggleApp_ScreenVeil()
    {
        var isActive = App_ScreenCanvas.gameObject.activeSelf;
        App_ScreenVeil.gameObject.SetActive(!isActive);
    }

    public void ToggleApp_MinimapOverlay()
    {
        var isActive = App_MinimapOverlay.gameObject.activeSelf;
        App_MinimapOverlay.gameObject.SetActive(!isActive);
    }
    protected void Awake()
    {
        Instance ??= this;
    }

    protected override void Start()
    {
        base.Start();
    }

    public override void InitializeInputs()
    {
        #region Toggle Apps
        AddInputCmd(
            DeviceType.Keyboard, (uint)KeyCode.VcF1,
            InputState.Pressed,
            (self) =>
            {
                ToggleApp_CardTable();
            }
        );
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
            (self) =>
            {
                ToggleApp_MinimapCanvas();
            }
        );
        AddInputCmd(
            DeviceType.Keyboard, (uint)KeyCode.VcF4,
            InputState.Pressed,
            (self) =>
            {
                ToggleApp_PlayersNamePanel();
            }
        );
        AddInputCmd(
            DeviceType.Keyboard, (uint)KeyCode.VcF5,
            InputState.Pressed,
            (self) =>
            {
                ToggleApp_MinimapOverlay();
            }
        );
        #endregion Toggle Apps

        AddInputCmd(
            DeviceType.Keyboard, (uint)KeyCode.VcF8,
            InputState.Pressed,
            (self) =>
            {
                Helper.gameObject.SetActive(!Helper.gameObject.activeSelf);
            }
        );

        #region Exit Command
        AddInputCmd(
            DeviceType.Keyboard, (uint)KeyCode.VcBackspace,
            InputState.Released,
            (self) =>
            {
                timer_quit = 0.0f;
            }
        );
        AddInputCmd(
            DeviceType.Keyboard, (uint)KeyCode.VcBackspace,
            InputState.Hold,
            (self) =>
            {
                timer_quit += Time.deltaTime;

                if (timer_quit >= timeToHoldToQuit)
                    Application.Quit();
            }
        );
        #endregion Exit Command
    }
}
