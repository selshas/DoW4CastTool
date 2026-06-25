using System.Collections.Generic;
using UnityEngine;

using SharpHook;
using System.Linq;

public partial class GlobalInputSystem : MonoBehaviour
{
    public static GlobalInputSystem Instance = null;

    public Dictionary<DeviceType, Dictionary<uint, InputState>> InputStates = new Dictionary<DeviceType, Dictionary<uint, InputState>>();
    public Dictionary<DeviceType, Dictionary<uint, InputState>> InputStates_prev = new Dictionary<DeviceType, Dictionary<uint, InputState>>();

    public delegate void InputCallback(UtilityAppBase self);

    public Dictionary<DeviceType, List<uint>> RegisteredInput = new Dictionary<DeviceType, List<uint>>();

    private TaskPoolGlobalHook hooker;

    private async void Awake()
    {
        Instance ??= this;

        hooker = new TaskPoolGlobalHook();
        hooker.MousePressed += Hook_MousePressed;
        hooker.MouseReleased += Hook_MouseReleased;
        hooker.KeyPressed += Hook_KeyboardPressed;
        hooker.KeyReleased += Hook_KeyboardReleased;

        Debug.Log("SharpHookAgent Initialized");

        await hooker.RunAsync();
    }

    void OnDestroy()
    {
        hooker.MousePressed -= Hook_MousePressed;
        hooker.MouseReleased -= Hook_MouseReleased;
        hooker.KeyPressed -= Hook_KeyboardPressed;
        hooker.KeyReleased -= Hook_KeyboardReleased;

        hooker.Dispose();

        Debug.Log("SharpHookAgent Destroyed");
    }

    private void Hook_KeyboardPressed(object sender, KeyboardHookEventArgs e)
    {
        var keyCode = e.Data.KeyCode;

        var inputStates = InputStates[DeviceType.Keyboard];
        if (
            inputStates[(uint)keyCode] == InputState.Pressed
            || inputStates[(uint)keyCode] == InputState.Hold
        )
            return;

        inputStates[(uint)keyCode] = InputState.Pressed;
    }
    private void Hook_KeyboardReleased(object sender, KeyboardHookEventArgs e)
    {
        var keyCode = e.Data.KeyCode;

        var inputStates = InputStates[DeviceType.Keyboard];
        inputStates[(uint)keyCode] = InputState.Released;
    }

    private void Hook_MousePressed(object sender, MouseHookEventArgs e)
    {
        var mouseCode = e.Data.Button;

        var inputStates = InputStates[DeviceType.Mouse];
        inputStates[(uint)mouseCode] = InputState.Pressed;
    }

    private void Hook_MouseReleased(object sender, MouseHookEventArgs e)
    {
        var mouseCode = e.Data.Button;

        var inputStates = InputStates[DeviceType.Mouse];
        inputStates[(uint)mouseCode] = InputState.Released;
    }

    /// <summary>
    /// Forces the input state of the specified key/button to Idle, cancelling any in-progress press or hold.
    /// </summary>
    public void ForceIdle(DeviceType deviceType, uint code)
    {
        if (InputStates.TryGetValue(deviceType, out var states) && states.ContainsKey(code))
            states[code] = InputState.Idle;

        if (InputStates_prev.TryGetValue(deviceType, out var prevStates) && prevStates.ContainsKey(code))
            prevStates[code] = InputState.Idle;
    }

    void Update()
    {
        foreach ((var device, var inputs) in RegisteredInput)
        {
            foreach (var code in inputs)
            {
                if (InputStates_prev[device][code] == InputState.Pressed && InputStates[device][code] == InputState.Pressed)
                    InputStates[device][code] = InputState.Hold;
                else if (InputStates_prev[device][code] == InputState.Released && InputStates[device][code] == InputState.Released)
                    InputStates[device][code] = InputState.Idle;
            }
        }

        InputStates_prev = InputStates.ToDictionary(x => x.Key, y => y.Value.ToDictionary(i => i.Key, j => j.Value));
    }
}
