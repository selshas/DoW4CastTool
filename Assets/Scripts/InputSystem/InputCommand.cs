using static GlobalInputSystem;

public class InputCommand
{
    public DeviceType DeviceType = DeviceType.None;
    public InputState StateCondition = InputState.Idle;
    public uint inputCode = 0;

    public InputCallback Callback;

    public InputCommand(DeviceType deviceType, uint inputCode, InputState stateCondition, InputCallback callback)
    {
        DeviceType = deviceType;
        this.inputCode = inputCode;

        StateCondition = stateCondition;

        Callback = callback;
    }
}
