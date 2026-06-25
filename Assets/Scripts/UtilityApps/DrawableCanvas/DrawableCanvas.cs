using SharpHook.Native;

using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

using static GlobalInputSystem;
using DeviceType = GlobalInputSystem.DeviceType;
using KeyCode = SharpHook.Native.KeyCode;

public abstract partial class DrawableCanvas : UtilityAppBase
{
    public Helper Helper;

    public RawImage RawImg_TargetCanvas = null;
    public Transform Transform_TargetCanvas;
    protected RectTransform rectTransform_targetCanvas;

    protected Vector2 cursorPos_curr;
    protected Vector2 cursorPos_prev;

    protected Transform transform_emulatedCursor;
    public RawImage RawImage_EmulatedCursor;

    public ChunkedCanvas Canvas => chunkedCanvas;
    protected ChunkedCanvas chunkedCanvas;

    public Material Mat_DrawSpot;
    public Material Mat_EraseSpot;

    private const float DRAW_RADIUS = 4f;
    private const float ERASE_RADIUS = 32f;

    public Dictionary<string, Color> PenColors = new Dictionary<string, Color>()
    {
        { "white",  new Color(1, 1, 1, 1) },
        { "red",  new Color(1, 0, 0, 1) },
        { "green",  new Color(0, 1, 0, 1) },
        { "blue",  new Color(0, 0, 1, 1) },
        { "yellow",  new Color(1, 1, 0, 1) },
    };

    public Color CurrentPenColor
    {
        get => currentPenColor;
        set
        {
            currentPenColor = value;
            RawImage_EmulatedCursor.color = value;
        }
    }
    protected Color currentPenColor;

    protected Vector2[] corners_screen = new Vector2[4];
    protected float xMin = 0.0f;
    protected float xMax = 0.0f;
    protected float yMin = 0.0f;
    protected float yMax = 0.0f;

    protected virtual void Awake()
    {
        currentPenColor = PenColors["white"];

        transform_emulatedCursor = RawImage_EmulatedCursor.transform;

        rectTransform_targetCanvas = Transform_TargetCanvas.GetComponent<RectTransform>();
        RawImg_TargetCanvas ??= Transform_TargetCanvas.GetComponent<RawImage>();
    }

    protected override void Start()
    {
        base.Start();

        cursorPos_prev = cursorPos_curr = ApplicationSetup.GetCursorViewportPosition();

        Mat_DrawSpot.SetVector("_Color", CurrentPenColor);

        var corners = new Vector3[4];
        rectTransform_targetCanvas.GetWorldCorners(corners);

        corners_screen = new Vector2[4]
        {
            RectTransformUtility.WorldToScreenPoint(null, corners[0]),
            RectTransformUtility.WorldToScreenPoint(null, corners[1]),
            RectTransformUtility.WorldToScreenPoint(null, corners[2]),
            RectTransformUtility.WorldToScreenPoint(null, corners[3]),
        };

        xMin = corners_screen[0].x;
        yMin = corners_screen[0].y;
        xMax = corners_screen[2].x;
        yMax = corners_screen[2].y;

        var canvasWidth = Mathf.RoundToInt(xMax - xMin);
        var canvasHeight = Mathf.RoundToInt(yMax - yMin);

        chunkedCanvas = new ChunkedCanvas(canvasWidth, canvasHeight, rectTransform_targetCanvas);
        RawImg_TargetCanvas.enabled = false;
    }

    protected bool IsCursorInsideCanvas()
    {
        Vector2 cursorPos = cursorPos_curr;

        return (
            xMin <= cursorPos.x && cursorPos.x <= xMax
            && yMin <= cursorPos.y && cursorPos.y <= yMax
        );
    }

    protected override void Update()
    {
        base.Update();

        cursorPos_prev = cursorPos_curr;
        cursorPos_curr = ApplicationSetup.GetCursorViewportPosition();
        transform_emulatedCursor.position = cursorPos_curr;

        transform_emulatedCursor.gameObject.SetActive(IsCursorInsideCanvas());
    }

    protected virtual void OnDestroy()
    {
        chunkedCanvas?.Release();
    }

    public void Clear()
    {
        chunkedCanvas?.Clear();
    }

    public void DrawSpot(Vector2 pos)
    {
        pos -= new Vector2(xMin, yMin);

        Mat_DrawSpot.SetVector("_Color", CurrentPenColor);

        chunkedCanvas.BlitSpot(pos, DRAW_RADIUS, Mat_DrawSpot);
    }

    public void EraseSpot(Vector2 pos)
    {
        pos -= new Vector2(xMin, yMin);

        chunkedCanvas.BlitSpot(pos, ERASE_RADIUS, Mat_EraseSpot);
    }

    public override void InitializeInputs()
    {
        AddInputCmd(
            DeviceType.Mouse, (uint)MouseButton.Button1,
            InputState.Pressed,
            (self) =>
            {
                DrawSpot(cursorPos_curr);
            }
        );
        AddInputCmd(
            DeviceType.Mouse, (uint)MouseButton.Button1,
            InputState.Hold,
            (self) =>
            {
                Vector2 cursorPos_delta = (cursorPos_curr - cursorPos_prev);

                int steps = Mathf.RoundToInt(cursorPos_delta.magnitude / 4.0f);
                float dt = 1.0f / steps;

                for (float t = 0; t <= 1.0f; t += dt)
                    DrawSpot(cursorPos_delta * t + cursorPos_prev);
            }
        );

        AddInputCmd(
            DeviceType.Mouse, (uint)MouseButton.Button2,
            InputState.Pressed,
            (self) =>
            {
                Vector2 cursorPos_delta = (cursorPos_curr - cursorPos_prev);

                int steps = Mathf.RoundToInt(cursorPos_delta.magnitude / 4.0f);
                float dt = 1.0f / steps;

                for (float t = 0; t <= 1.0f; t += dt)
                    EraseSpot(cursorPos_delta * t + cursorPos_prev);
            }
        );
        AddInputCmd(
            DeviceType.Mouse, (uint)MouseButton.Button2,
            InputState.Hold,
            (self) =>
            {
                EraseSpot(cursorPos_curr);
            }
        );

        AddInputCmd(
            DeviceType.Mouse, (uint)MouseButton.Button3,
            InputState.Pressed,
            (self) =>
            {
                Clear();
            }
        );

        AddInputCmd(
            DeviceType.Keyboard, (uint)KeyCode.VcTab,
            InputState.Pressed,
            (self) =>
            {
                IngameAppController.Instance.ToggleApp_ScreenCanvas();
                IngameAppController.Instance.ToggleApp_MinimapCanvas();
            }
        );

        AddInputCmd(
            DeviceType.Keyboard, (uint)KeyCode.VcF7,
            InputState.Pressed,
            (self) =>
            {
                ToggleHelper();
            }
        );

        AddInputCmd(
            DeviceType.Keyboard, (uint)KeyCode.Vc1,
            InputState.Pressed,
            (self) =>
            {
                CurrentPenColor = PenColors["white"];
            }
        );
        AddInputCmd(
            DeviceType.Keyboard, (uint)KeyCode.Vc2,
            InputState.Pressed,
            (self) =>
            {
                CurrentPenColor = PenColors["red"];
            }
        );
        AddInputCmd(
            DeviceType.Keyboard, (uint)KeyCode.Vc3,
            InputState.Pressed,
            (self) =>
            {
                CurrentPenColor = PenColors["green"];
            }
        );
        AddInputCmd(
            DeviceType.Keyboard, (uint)KeyCode.Vc4,
            InputState.Pressed,
            (self) =>
            {
                CurrentPenColor = PenColors["blue"];
            }
        );
        AddInputCmd(
            DeviceType.Keyboard, (uint)KeyCode.Vc5,
            InputState.Pressed,
            (self) =>
            {
                CurrentPenColor = PenColors["yellow"];
            }
        );
    }

    protected void ToggleHelper()
    {
        Helper.gameObject.SetActive(!Helper.gameObject.activeSelf);
    }
}
