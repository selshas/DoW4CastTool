using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

using SharpHook;
using System.Linq;

public partial class GlobalInputSystem : SingletonBehaviour<GlobalInputSystem>
{
    public Dictionary<DeviceType, Dictionary<uint, InputState>> InputStates = new Dictionary<DeviceType, Dictionary<uint, InputState>>();
    public Dictionary<DeviceType, Dictionary<uint, InputState>> InputStates_prev = new Dictionary<DeviceType, Dictionary<uint, InputState>>();

    public delegate void InputCallback(UtilityAppBase self);

    public Dictionary<DeviceType, List<uint>> RegisteredInput = new Dictionary<DeviceType, List<uint>>();

    private TaskPoolGlobalHook hooker;

    /// <summary>
    /// Starts the global input hook and subscribes to scene change events.
    /// </summary>
    protected override async void OnInitialize()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        hooker = new TaskPoolGlobalHook(runAsyncOnBackgroundThread: false);
        hooker.MousePressed += Hook_MousePressed;
        hooker.MouseReleased += Hook_MouseReleased;
        hooker.KeyPressed += Hook_KeyboardPressed;
        hooker.KeyReleased += Hook_KeyboardReleased;

        Debug.Log("SharpHookAgent Initialized");

        await hooker.RunAsync();
    }

    /// <summary>
    /// Unsubscribes hook events and disposes the hook.
    /// </summary>
    protected override void OnDispose()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        hooker.MousePressed -= Hook_MousePressed;
        hooker.MouseReleased -= Hook_MouseReleased;
        hooker.KeyPressed -= Hook_KeyboardPressed;
        hooker.KeyReleased -= Hook_KeyboardReleased;

        hooker.Dispose();

        Debug.Log("SharpHookAgent Destroyed");
    }

    /// <summary>
    /// Clears all input registrations and state so the new scene's UtilityApps can re-register.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RegisteredInput.Clear();
        InputStates.Clear();
        InputStates_prev.Clear();
    }

    private void Hook_KeyboardPressed(object sender, KeyboardHookEventArgs e)
    {
        var keyCode = e.Data.KeyCode;

        if (!InputStates.TryGetValue(DeviceType.Keyboard, out var inputStates))
            return;
        if (!inputStates.TryGetValue((uint)keyCode, out var currentState))
            return;

        if (currentState == InputState.Pressed || currentState == InputState.Hold)
            return;

        inputStates[(uint)keyCode] = InputState.Pressed;
    }

    private void Hook_KeyboardReleased(object sender, KeyboardHookEventArgs e)
    {
        var keyCode = e.Data.KeyCode;

        if (!InputStates.TryGetValue(DeviceType.Keyboard, out var inputStates))
            return;
        if (!inputStates.ContainsKey((uint)keyCode))
            return;

        inputStates[(uint)keyCode] = InputState.Released;
    }

    private void Hook_MousePressed(object sender, MouseHookEventArgs e)
    {
        var mouseCode = e.Data.Button;

        if (!InputStates.TryGetValue(DeviceType.Mouse, out var inputStates))
            return;
        if (!inputStates.ContainsKey((uint)mouseCode))
            return;

        inputStates[(uint)mouseCode] = InputState.Pressed;
    }

    private void Hook_MouseReleased(object sender, MouseHookEventArgs e)
    {
        var mouseCode = e.Data.Button;

        if (!InputStates.TryGetValue(DeviceType.Mouse, out var inputStates))
            return;
        if (!inputStates.ContainsKey((uint)mouseCode))
            return;

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

    /// <summary>
    /// Polls Unity's Input System for registered keys when the application is focused.
    /// </summary>
    private void PollFocusedInput()
    {
        if (!Application.isFocused)
            return;

        var keyboard = Keyboard.current;
        if (keyboard != null && RegisteredInput.TryGetValue(DeviceType.Keyboard, out var kbInputs))
        {
            var kbStates = InputStates[DeviceType.Keyboard];
            foreach (var code in kbInputs)
            {
                if (!sharpHookToInputSystemKey.TryGetValue(code, out var key))
                    continue;

                var keyControl = keyboard[key];
                if (keyControl.wasPressedThisFrame)
                {
                    if (kbStates[code] != InputState.Pressed && kbStates[code] != InputState.Hold)
                        kbStates[code] = InputState.Pressed;
                }
                else if (keyControl.wasReleasedThisFrame)
                    kbStates[code] = InputState.Released;
            }
        }

        var mouse = Mouse.current;
        if (mouse != null && RegisteredInput.TryGetValue(DeviceType.Mouse, out var mouseInputs))
        {
            var mouseStates = InputStates[DeviceType.Mouse];
            foreach (var code in mouseInputs)
            {
                var buttonControl = GetMouseButton(mouse, code);
                if (buttonControl == null)
                    continue;

                if (buttonControl.wasPressedThisFrame)
                {
                    if (mouseStates[code] != InputState.Pressed && mouseStates[code] != InputState.Hold)
                        mouseStates[code] = InputState.Pressed;
                }
                else if (buttonControl.wasReleasedThisFrame)
                    mouseStates[code] = InputState.Released;
            }
        }
    }

    /// <summary>
    /// Processes input state transitions and snapshots previous state.
    /// </summary>
    private void Update()
    {
        PollFocusedInput();

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
