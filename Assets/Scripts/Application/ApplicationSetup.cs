using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class ApplicationSetup : MonoBehaviour
{
    public static ApplicationSetup Instance = null;

    public static bool RaycastDetected = false;

    public static bool InteractionMode
    {
        get => _interactionMode; 
        set 
        {
            _interactionMode = value;

            if (!value) 
                Win32API.SetWindowLongA(hWnd, GWL_EXSTYLE, WS_EX_LAYERED | WS_EX_TRANSPARENT);
        }
    }
    private static bool _interactionMode = true;

    const int GWL_EXSTYLE = -20;

    const long WS_EX_LAYERED = 0x00080000;
    const long WS_EX_TRANSPARENT = 0x00000020L;

    public static IntPtr hhook_kbinput = IntPtr.Zero;
    public static IntPtr hhook_mbinput = IntPtr.Zero;
    public static IntPtr hWnd { get; private set; }

    public static System.Diagnostics.Process proc { get; private set; }

    private GraphicRaycaster raycaster;
    public static List<RaycastResult> RaycastResults = new List<RaycastResult>();

    public List<UtilityAppBase> UtilityApps = new List<UtilityAppBase>();

    // Start is called before the first frame update
    private void Awake()
    {
        Instance ??= this;

        proc = System.Diagnostics.Process.GetCurrentProcess();
        //proc = Win32API.GetCurrentProcess();

        Application.runInBackground = true;
        Application.targetFrameRate = 60;

        raycaster = GetComponent<GraphicRaycaster>();

#if !UNITY_EDITOR

        hWnd = Win32API.GetActiveWindow();

        Win32API.Margin margin = new Win32API.Margin { cx_left = -20 };
        Win32API.DwmExtendFrameIntoClientArea(hWnd, ref margin);
        
        Win32API.SetWindowLongA(hWnd, GWL_EXSTYLE, WS_EX_LAYERED);

        Win32API.SetWindowPos(hWnd, new IntPtr(-1), 0, 0, 0, 0, 0);
#endif
    }

    public void Wakeup()
    {
        Win32API.BringWindowToTop(proc.MainWindowHandle);
        Win32API.SetActiveWindow(proc.MainWindowHandle);
        Win32API.AllowSetForegroundWindow(proc.Id);
        Win32API.SetForegroundWindow(proc.MainWindowHandle);
        Win32API.SetFocus(proc.MainWindowHandle);
    }

    // Update is called once per frame
    void Update()
    {
        if (!InteractionMode) 
            return;

        Cursor.lockState = (Application.isFocused) 
            ? CursorLockMode.Confined
            : CursorLockMode.None;

        PointerEventData ped = new PointerEventData(null);
        ped.position = Mouse.current.position.ReadValue();

        raycaster.Raycast(ped, RaycastResults);

        if (RaycastResults.Count > 0 || RaycastDetected)
        {
            // Bring window focus to the top
            Win32API.SetWindowLongA(hWnd, GWL_EXSTYLE, WS_EX_LAYERED);
        }
        else
        {
            Win32API.SetWindowLongA(hWnd, GWL_EXSTYLE, WS_EX_LAYERED | WS_EX_TRANSPARENT);
        }
        RaycastResults.Clear();
    }
}
