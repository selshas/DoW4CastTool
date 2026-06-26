using UnityEngine;
using UnityEngine.UI;
using static GlobalInputSystem;
using DeviceType = GlobalInputSystem.DeviceType;
using KeyCode = SharpHook.Data.KeyCode;

public class MinimapOverlay : UtilityAppBase
{
    public MinimapCanvas MinimapCanvas;
    public RawImage RawImg_MinimapOverlay;

    private bool mirrored = false;

    public override void InitializeInputs()
    {
    }

    protected override void Update()
    {
        base.Update();

        if (!mirrored && MinimapCanvas.Canvas != null)
        {
            MinimapCanvas.Canvas.CreateMirror(RawImg_MinimapOverlay.rectTransform);
            RawImg_MinimapOverlay.enabled = false;
            mirrored = true;
        }
    }
}
