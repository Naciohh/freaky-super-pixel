using UnityEngine;
using System.Collections;

public class TransformationMode : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private int parriesToTransform = 5;
    [SerializeField] private float transformDuration = 8f;

    private int parryCount = 0;
    private bool isTransformed = false;
    private Coroutine transformCoroutine;
    private PlayerMovement playerMovement;
    private PlayerCombat playerCombat;

    public int ParryCount => parryCount;
    public bool IsTransformed => isTransformed;
    public float TransformProgress => Mathf.Clamp01((float)parryCount / parriesToTransform);

    void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerCombat = GetComponent<PlayerCombat>();

        if (playerMovement == null)
            Debug.LogError("[TransformationMode] PlayerMovement not found on this GameObject.", this);
        if (playerCombat == null)
            Debug.LogError("[TransformationMode] PlayerCombat not found on this GameObject.", this);
    }

    void OnEnable()
    {
        PlayerCombat.OnParrySuccess += HandleParrySuccess;
    }

    void OnDisable()
    {
        PlayerCombat.OnParrySuccess -= HandleParrySuccess;
        if (transformCoroutine != null)
        {
            StopCoroutine(transformCoroutine);
            transformCoroutine = null;
        }
    }

    // Restaura el estado de transformación desde un guardado.
    public void RestoreState(int savedParryCount, bool wasTransformed)
    {
        parryCount = Mathf.Clamp(savedParryCount, 0, parriesToTransform);

        if (wasTransformed && transformCoroutine == null)
            transformCoroutine = StartCoroutine(TransformRoutine());
    }

    private void HandleParrySuccess()
    {
        if (isTransformed) return;

        parryCount++;
        if (parryCount >= parriesToTransform)
        {
            parryCount = parriesToTransform;
            if (transformCoroutine == null)
                transformCoroutine = StartCoroutine(TransformRoutine());
        }
    }

    private IEnumerator TransformRoutine()
    {
        isTransformed = true;
        if (playerMovement != null) playerMovement.SetTransformed(true);
        if (playerCombat != null) playerCombat.IsTransforming = true;

        float elapsed = 0f;
        while (elapsed < transformDuration)
        {
            elapsed += Time.deltaTime;
            parryCount = Mathf.RoundToInt(Mathf.Lerp(parriesToTransform, 0, elapsed / transformDuration));
            yield return null;
        }

        parryCount = 0;
        isTransformed = false;
        if (playerMovement != null) playerMovement.SetTransformed(false);
        if (playerCombat != null) playerCombat.IsTransforming = false;
        transformCoroutine = null;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isTransformed) return;

        EnemyHealth eh = other.GetComponent<EnemyHealth>();
        if (eh != null && !eh.data.isBoss)
            eh.TakeDamage(99999);
    }
}
