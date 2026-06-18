using SharpHook.Native;
using static GlobalInputSystem;

public class ScreenCanvas : DrawableCanvas
{
    private void OnEnable()
    {
        Clear();
    }

    private void OnDisable()
    {
    }

    public override void InitializeInputs()
    {
        base.InitializeInputs();

        AddInputCmd(
            DeviceType.Keyboard, (uint)KeyCode.VcTab, 
            InputState.Pressed, 
            (self) =>
            {
                GlobalAppController.Instance.ToggleApp_ScreenCanvas();
                GlobalAppController.Instance.ToggleApp_MinimapCanvas();
            }
        );
    }
}