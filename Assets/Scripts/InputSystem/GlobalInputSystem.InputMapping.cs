using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

using SharpKey = SharpHook.Native.KeyCode;
using SharpMouse = SharpHook.Native.MouseButton;

/// <summary>
/// SharpHook-to-InputSystem key/mouse mapping for focused input polling.
/// Unity 6 bug: WH_KEYBOARD_LL / WH_MOUSE_LL hooks don't fire while the window is focused.
/// SharpHook handles unfocused input; these mappings let PollFocusedInput fill the gap via Unity's Input System.
/// </summary>
public partial class GlobalInputSystem
{
    private static readonly Dictionary<uint, Key> sharpHookToInputSystemKey = new Dictionary<uint, Key>
    {
        { (uint)SharpKey.VcEscape, Key.Escape },

        { (uint)SharpKey.VcF1, Key.F1 },
        { (uint)SharpKey.VcF2, Key.F2 },
        { (uint)SharpKey.VcF3, Key.F3 },
        { (uint)SharpKey.VcF4, Key.F4 },
        { (uint)SharpKey.VcF5, Key.F5 },
        { (uint)SharpKey.VcF7, Key.F7 },
        { (uint)SharpKey.VcF8, Key.F8 },

        { (uint)SharpKey.VcBackspace, Key.Backspace },
        { (uint)SharpKey.VcTab, Key.Tab },
        { (uint)SharpKey.VcInsert, Key.Insert },
        { (uint)SharpKey.VcHome, Key.Home },
        { (uint)SharpKey.VcDelete, Key.Delete },

        { (uint)SharpKey.Vc1, Key.Digit1 },
        { (uint)SharpKey.Vc2, Key.Digit2 },
        { (uint)SharpKey.Vc3, Key.Digit3 },
        { (uint)SharpKey.Vc4, Key.Digit4 },
        { (uint)SharpKey.Vc5, Key.Digit5 },
    };

    /// <summary>
    /// Maps a SharpHook mouse button code to the corresponding Input System ButtonControl.
    /// </summary>
    private ButtonControl GetMouseButton(Mouse mouse, uint sharpHookCode)
    {
        if (sharpHookCode == (uint)SharpMouse.Button1)
            return mouse.leftButton;
        if (sharpHookCode == (uint)SharpMouse.Button2)
            return mouse.rightButton;
        if (sharpHookCode == (uint)SharpMouse.Button3)
            return mouse.middleButton;

        return null;
    }
}
