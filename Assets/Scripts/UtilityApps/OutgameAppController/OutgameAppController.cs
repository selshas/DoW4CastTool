using UnityEngine;
using static GlobalInputSystem;
using DeviceType = GlobalInputSystem.DeviceType;
using KeyCode = SharpHook.Native.KeyCode;

public class OutgameAppController : UtilityAppBase
{
    public static OutgameAppController Instance { get; private set; }

    public GameObject Helper;

    public MatchSetup App_MatchSetup;

    public float timeToHoldToQuit = 2.0f;
    private float timer_quit = 0.0f;

    /// <summary>
    /// Toggles the MatchSetup visibility.
    /// </summary>
    public void ToggleApp_MatchSetup()
    {
        var isActive = App_MatchSetup.gameObject.activeSelf;
        App_MatchSetup.gameObject.SetActive(!isActive);
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
    /// Registers hotkey bindings for outgame app toggling, helper, and quit.
    /// </summary>
    public override void InitializeInputs()
    {
        AddInputCmd(
            DeviceType.Keyboard, (uint)KeyCode.VcF1,
            InputState.Pressed,
            (self) => ToggleApp_MatchSetup()
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
                    Application.Quit();
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
