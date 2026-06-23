using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;

public class ChunkedCanvas
{
    public const int CHUNK_SIZE = 512;

    private RenderTexture[,] chunks;
    private RawImage[,] chunkImages;

    public int Cols { get; private set; }
    public int Rows { get; private set; }
    public int CanvasWidth { get; private set; }
    public int CanvasHeight { get; private set; }

    public ChunkedCanvas(int width, int height, RectTransform parent)
    {
        CanvasWidth = width;
        CanvasHeight = height;

        Cols = Mathf.CeilToInt((float)width / CHUNK_SIZE);
        Rows = Mathf.CeilToInt((float)height / CHUNK_SIZE);

        chunks = new RenderTexture[Cols, Rows];

        for (int c = 0; c < Cols; c++)
        {
            for (int r = 0; r < Rows; r++)
                chunks[c, r] = new RenderTexture(CHUNK_SIZE, CHUNK_SIZE, 0, GraphicsFormat.R8G8B8A8_UNorm, 0);
        }

        CreateChunkImages(parent);
        Clear();
    }

    private RawImage[,] CreateChunkImagesOnParent(RectTransform parent, bool raycastTarget)
    {
        var images = new RawImage[Cols, Rows];

        for (int c = 0; c < Cols; c++)
        {
            for (int r = 0; r < Rows; r++)
            {
                float anchorMinX = (float)(c * CHUNK_SIZE) / CanvasWidth;
                float anchorMinY = (float)(r * CHUNK_SIZE) / CanvasHeight;
                float anchorMaxX = (float)Mathf.Min((c + 1) * CHUNK_SIZE, CanvasWidth) / CanvasWidth;
                float anchorMaxY = (float)Mathf.Min((r + 1) * CHUNK_SIZE, CanvasHeight) / CanvasHeight;

                int actualWidth = Mathf.Min(CHUNK_SIZE, CanvasWidth - c * CHUNK_SIZE);
                int actualHeight = Mathf.Min(CHUNK_SIZE, CanvasHeight - r * CHUNK_SIZE);

                var go = new GameObject($"Chunk_{c}_{r}", typeof(RectTransform));
                go.transform.SetParent(parent, false);
                go.layer = parent.gameObject.layer;

                var rt = go.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(anchorMinX, anchorMinY);
                rt.anchorMax = new Vector2(anchorMaxX, anchorMaxY);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;

                var img = go.AddComponent<RawImage>();
                img.texture = chunks[c, r];
                img.uvRect = new Rect(0, 0, (float)actualWidth / CHUNK_SIZE, (float)actualHeight / CHUNK_SIZE);
                img.raycastTarget = raycastTarget;

                images[c, r] = img;
            }
        }

        return images;
    }

    private void CreateChunkImages(RectTransform parent)
    {
        chunkImages = CreateChunkImagesOnParent(parent, false);
    }

    public void CreateMirror(RectTransform parent)
    {
        CreateChunkImagesOnParent(parent, false);
    }

    public void BlitSpot(Vector2 canvasPos, float radius, Material mat)
    {
        int minCol = Mathf.Max(0, Mathf.FloorToInt((canvasPos.x - radius) / CHUNK_SIZE));
        int maxCol = Mathf.Min(Cols - 1, Mathf.FloorToInt((canvasPos.x + radius) / CHUNK_SIZE));
        int minRow = Mathf.Max(0, Mathf.FloorToInt((canvasPos.y - radius) / CHUNK_SIZE));
        int maxRow = Mathf.Min(Rows - 1, Mathf.FloorToInt((canvasPos.y + radius) / CHUNK_SIZE));

        for (int c = minCol; c <= maxCol; c++)
        {
            for (int r = minRow; r <= maxRow; r++)
            {
                Vector2 localPos = canvasPos - new Vector2(c * CHUNK_SIZE, r * CHUNK_SIZE);
                mat.SetVector("_CursorPos", localPos);
                Graphics.Blit(null, chunks[c, r], mat);
            }
        }
    }

    public void Clear()
    {
        for (int c = 0; c < Cols; c++)
        {
            for (int r = 0; r < Rows; r++)
            {
                RenderTexture.active = chunks[c, r];
                GL.Clear(true, true, new Color(0, 0, 0, 0));
            }
        }

        RenderTexture.active = null;
    }

    public void Release()
    {
        for (int c = 0; c < Cols; c++)
        {
            for (int r = 0; r < Rows; r++)
            {
                if (chunks[c, r] != null)
                    chunks[c, r].Release();

                if (chunkImages != null && chunkImages[c, r] != null)
                    Object.Destroy(chunkImages[c, r].gameObject);
            }
        }
    }
}
