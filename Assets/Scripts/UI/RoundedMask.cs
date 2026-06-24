using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Recorta sus hijos a un rectángulo de esquinas redondeadas, generando el
/// sprite-máscara en runtime según su propio tamaño. Se usa para que el relleno
/// de una barra siga la silueta redondeada del marco y no se asome por las
/// esquinas. cornerRadiusFrac = 1 da forma de pastilla (extremos semicirculares);
/// valores menores dan esquinas más suaves (para cajas tipo la del jefe).
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class RoundedMask : MonoBehaviour
{
    [Range(0f, 1f)] public float cornerRadiusFrac = 1f;

    void Start()
    {
        var img = GetComponent<Image>();
        if (img == null) img = gameObject.AddComponent<Image>();

        var mask = GetComponent<Mask>();
        if (mask == null) mask = gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        Canvas.ForceUpdateCanvases();
        Vector2 size = ((RectTransform)transform).rect.size;

        img.sprite = Build(size, cornerRadiusFrac);
        img.type = Image.Type.Simple;
        img.color = Color.white;
    }

    private static Sprite Build(Vector2 size, float frac)
    {
        float aspect = size.y > 0.001f ? size.x / size.y : 4f;
        int h = 64;
        int w = Mathf.Clamp(Mathf.RoundToInt(h * aspect), 8, 2048);
        float radius = Mathf.Clamp(frac * (h / 2f), 0f, Mathf.Min(w, h) / 2f);

        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
        var cols = new Color[w * h];
        Color on = Color.white;
        Color off = new Color(1f, 1f, 1f, 0f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float cx = Mathf.Clamp(x + 0.5f, radius, w - radius);
                float cy = Mathf.Clamp(y + 0.5f, radius, h - radius);
                float dx = (x + 0.5f) - cx;
                float dy = (y + 0.5f) - cy;
                cols[y * w + x] = (dx * dx + dy * dy <= radius * radius) ? on : off;
            }
        }

        tex.SetPixels(cols);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
    }
}
