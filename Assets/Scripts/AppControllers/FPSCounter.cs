using UnityEngine;
using TMPro;

public class FPSCounter : MonoBehaviour
{
    public TextMeshProUGUI fpsText;

    void Update()
    {
        if (!gameObject.activeSelf)
            return;

        var fps = Mathf.RoundToInt(1.0f / Time.unscaledDeltaTime);
        fpsText.text = $"FPS: {fps}";
    }
}
