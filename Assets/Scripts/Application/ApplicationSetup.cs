using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
                SetClickThrough(true);
        }
    }
    private static bool _interactionMode = true;

    private const int GWL_EXSTYLE = -20;

    private const long WS_EX_LAYERED = 0x00080000;
    private const long WS_EX_TRANSPARENT = 0x00000020L;

    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOSIZE = 0x0001;

    public static IntPtr hhook_kbinput = IntPtr.Zero;
    public static IntPtr hhook_mbinput = IntPtr.Zero;
    public static IntPtr hWnd { get; private set; }

    public static System.Diagnostics.Process proc { get; private set; }

    private GraphicRaycaster raycaster;
    public static List<RaycastResult> RaycastResults = new List<RaycastResult>();

    public bool IsLoaded { get; private set; } = false; 
    public List<UtilityAppBase> UtilityApps = new List<UtilityAppBase>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        proc = System.Diagnostics.Process.GetCurrentProcess();

        Application.runInBackground = true;
        Application.targetFrameRate = 60;

#if !UNITY_EDITOR

        hWnd = Win32API.GetActiveWindow();

        Win32API.Margin margin = new Win32API.Margin { cx_left = -1 };
        Win32API.DwmExtendFrameIntoClientArea(hWnd, ref margin);

        long style = Win32API.GetWindowLongA(hWnd, GWL_EXSTYLE);
        Win32API.SetWindowLongA(hWnd, GWL_EXSTYLE, style | WS_EX_LAYERED);

        Win32API.SetWindowPos(hWnd, new IntPtr(-1), 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
#endif

        DontDestroyOnLoad(gameObject);
        
        IsLoaded = true;
    }

    private static void SetClickThrough(bool clickThrough)
    {
#if !UNITY_EDITOR
        long style = Win32API.GetWindowLongA(hWnd, GWL_EXSTYLE);
        if (clickThrough)
            style |= WS_EX_TRANSPARENT;
        else
            style &= ~WS_EX_TRANSPARENT;
        Win32API.SetWindowLongA(hWnd, GWL_EXSTYLE, style);
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

    private void Update()
    {
        if (!InteractionMode)
            return;

        if (raycaster == null)
        {
            var canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null)
                return;

            raycaster = canvas.GetComponent<GraphicRaycaster>();
        }

        Cursor.lockState = (Application.isFocused)
            ? CursorLockMode.Confined
            : CursorLockMode.None;

        Win32API.GetCursorPos(out Win32API.POINT cursorPos);

        PointerEventData ped = new PointerEventData(null);
        ped.position = new Vector2(cursorPos.x, Screen.height - cursorPos.y);


        raycaster.Raycast(ped, RaycastResults);

        if (RaycastResults.Count > 0 || RaycastDetected)
        {
            SetClickThrough(false);
        }
        else
        {
            SetClickThrough(true);
        }
        RaycastResults.Clear();

    }
}
