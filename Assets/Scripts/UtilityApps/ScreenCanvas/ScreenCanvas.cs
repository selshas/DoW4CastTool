using SharpHook.Data;
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
                IngameAppController.Instance.ToggleApp_ScreenCanvas();
                IngameAppController.Instance.ToggleApp_MinimapCanvas();
            }
        );
    }
}