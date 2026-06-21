using UnityEngine;

// Reproduce una secuencia de sprites una sola vez (slash 2D) y se autodestruye al terminar.
// La usa el prefab del slash que instancia PlayerCombat.
[RequireComponent(typeof(SpriteRenderer))]
public class SlashSpriteAnim : MonoBehaviour
{
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float fps = 24f;
    [SerializeField] private bool destroyOnFinish = true;

    private SpriteRenderer sr;
    private float t;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (frames != null && frames.Length > 0)
            sr.sprite = frames[0];
    }

    void Update()
    {
        if (frames == null || frames.Length == 0)
            return;

        t += Time.deltaTime * fps;
        int index = Mathf.FloorToInt(t);

        if (index >= frames.Length)
        {
            if (destroyOnFinish)
                Destroy(gameObject);
            else
                sr.sprite = frames[frames.Length - 1];
            return;
        }

        sr.sprite = frames[index];
    }

    // Permite setear los frames desde código (al armar el prefab).
    public void SetFrames(Sprite[] f) => frames = f;
}
