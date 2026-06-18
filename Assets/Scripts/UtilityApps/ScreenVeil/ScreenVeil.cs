using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GlobalInputSystem;
using DeviceType = GlobalInputSystem.DeviceType;
using KeyCode = SharpHook.Native.KeyCode;

public class ScreenVeil: UtilityAppBase
{
    public UnityEngine.UI.RawImage rawImage;

    public Material mat_multiplyOpacity;

    private RenderTexture renderTex;
    private RenderTexture renderTex_prev;

    private Coroutine coroutine_imageSwapFading = null;

    public List<Texture2D> images = new List<Texture2D>();

    private int currentImageIndex = -1;

    public void Select(int imgNo)
    {
        if (imgNo < 0)
            currentImageIndex = images.Count + imgNo;
        else
            currentImageIndex = imgNo;
    }

    // Start is called before the first frame update
    protected void Awake()
    {
        // Ser Black and White textures
        images.Add(new Texture2D(1, 1, TextureFormat.RGBA32, false));
        images.Add(new Texture2D(1, 1, TextureFormat.RGBA32, false));

        images[images.Count - 1].SetPixel(0, 0, new Color(0, 0, 0, 1));
        images[images.Count - 1].Apply();
        images[images.Count - 2].SetPixel(0, 0, new Color(1, 1, 1, 1));
        images[images.Count - 2].Apply();

        // Set the texture to display
        renderTex = new RenderTexture(1, 1, 1, RenderTextureFormat.ARGB32, 0);
        renderTex_prev = new RenderTexture(1, 1, 1, RenderTextureFormat.ARGB32, 0);

        mat_multiplyOpacity.SetTexture("_DestTex", renderTex_prev);

        rawImage = transform.Find("Raw Image").GetComponent<UnityEngine.UI.RawImage>();
        rawImage.texture = renderTex;
        rawImage.color = new Color(1, 1, 1, 0);
    }

    public void ShowBlackScreen()
    {
        gameObject.SetActive(true);

        Select(-1);

        Graphics.CopyTexture(renderTex, renderTex_prev);

        coroutine_imageSwapFading = StartCoroutine(Coroutine_FadeIn());
    }

    public void ShowWhiteScreen()
    {
        if (coroutine_imageSwapFading != null) 
            StopCoroutine(coroutine_imageSwapFading);

        Select(-2);

        coroutine_imageSwapFading = StartCoroutine(Coroutine_FadeIn());
    }
    public void Disappear()
    {
        coroutine_imageSwapFading = StartCoroutine(Coroutine_FadeOut());
    }

    IEnumerator Coroutine_FadeIn()
    {
        if (!rawImage.gameObject.activeSelf)
        {
            rawImage.gameObject.SetActive(true);
            Graphics.CopyTexture(images[currentImageIndex], renderTex_prev);
        }
        else
        {
            Graphics.CopyTexture(renderTex, renderTex_prev);
        }

        for (float alpha = 0; alpha <= 1.0f; alpha += Time.deltaTime)
        {
            mat_multiplyOpacity.SetFloat("_Opacity", alpha);

            if (rawImage.color.a < 1.0f) 
                rawImage.color = new Color(1, 1, 1, alpha);

            Graphics.Blit(images[currentImageIndex], renderTex, mat_multiplyOpacity);

            yield return new WaitForEndOfFrame();
        }

        mat_multiplyOpacity.SetFloat("_Opacity", 1.0f);
        rawImage.color = new Color(1, 1, 1, 1);

        Graphics.Blit(images[currentImageIndex], renderTex, mat_multiplyOpacity);

        yield return null;
    }
    IEnumerator Coroutine_FadeOut()
    {
        if (coroutine_imageSwapFading != null) 
            StopCoroutine(Coroutine_FadeIn());

        for (float alpha = 1.0f; alpha >= 0.0f; alpha -= Time.deltaTime)
        {
            rawImage.color = new Color(1, 1, 1, alpha);
            yield return new WaitForEndOfFrame();
        }
        rawImage.color = new Color(1, 1, 1, 0);

        rawImage.gameObject.SetActive(false);
        yield return null;
    }

    public override void InitializeInputs()
    {
        AddInputCmd(
            DeviceType.Keyboard, (uint)KeyCode.VcInsert,
            InputState.Pressed,
            (self) =>
            {
                ShowWhiteScreen();
            }
        );
        AddInputCmd(
            DeviceType.Keyboard, (uint)KeyCode.VcHome,
            InputState.Pressed,
            (self) =>
            {
                ShowBlackScreen();
            }
        );

        AddInputCmd(
            DeviceType.Keyboard, (uint)KeyCode.VcDelete,
            InputState.Pressed,
            (self) =>
            {
                Disappear();
            }
        );
    }
}
