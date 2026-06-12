using UnityEngine;
using System.Collections;
using System;

public class BossBearHealth : MonoBehaviour
{
    public EnemyData data;

    public static event Action<BossBearHealth> OnBossDied;

    public int currentHP;

    // Si es >= 0, al iniciar usa este HP en vez de maxHP (para restaurar guardados).
    [HideInInspector] public int restoreHP = -1;

    [Header("Boss")]
    public bool isVulnerable = true;
    public float vulnerableDuration = 5f;

    [Header("Feedback vulnerable")]
    public Color vulnerableTint = new Color(1f, 0.35f, 0.35f, 1f);

    private EnemyHealthBar _healthBar;
    private EnemyAI _enemyAI;
    private BossBearAI _bearAI;
    private Animator _anim;

    private Renderer[] _renderers;
    private MaterialPropertyBlock _mpb;
    private Coroutine _vulnerableCo;

    private bool isDead = false;

    void Start()
    {
        if (data == null)
        {
            Debug.LogError($"[BossBearHealth] EnemyData not assigned on {gameObject.name}.", this);
            Destroy(gameObject);
            return;
        }

        currentHP = restoreHP >= 0 ? restoreHP : data.maxHP;

        _healthBar = GetComponentInChildren<EnemyHealthBar>(true);

        _enemyAI = GetComponent<EnemyAI>();
        _bearAI = GetComponent<BossBearAI>();

        _anim = GetComponent<Animator>();

        _renderers = GetComponentsInChildren<Renderer>(true);
        _mpb = new MaterialPropertyBlock();

        // El boss arranca invulnerable: solo se le pega tras pararle un golpe (parry).
        isVulnerable = false;
        SetVulnerableVisual(false);
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || isDead)
            return;

        // EL BOSS SOLO RECIBE DAÑO CUANDO ES VULNERABLE
        if (!isVulnerable)
            return;

        currentHP -= amount;

        currentHP = Mathf.Max(currentHP, 0);

        _healthBar?.UpdateBar(currentHP, data.maxHP);

        if (currentHP <= 0)
        {
            Die();
        }
    }

    public void BecomeVulnerable()
    {
        if (isDead) return;

        if (_vulnerableCo != null)
            StopCoroutine(_vulnerableCo);

        _vulnerableCo = StartCoroutine(VulnerableRoutine());
    }

    private IEnumerator VulnerableRoutine()
    {
        isVulnerable = true;
        SetVulnerableVisual(true);

        // Aturdir al oso: animación de stun + IA congelada durante la ventana.
        if (_bearAI != null) _bearAI.isAIActive = false;
        if (_anim != null)
        {
            // Limpiar un ataque pendiente evita el "salto" instantáneo a la animación de stun.
            _anim.ResetTrigger("Attack");
            _anim.SetBool("isWalking", false);
            _anim.SetBool("isStunned", true);
        }

        Debug.Log("BOSS VULNERABLE");

        yield return new WaitForSeconds(vulnerableDuration);

        // Recupera: deja de estar aturdido y vuelve a atacar.
        if (_anim != null) _anim.SetBool("isStunned", false);
        if (_bearAI != null) _bearAI.isAIActive = true;

        isVulnerable = false;
        SetVulnerableVisual(false);
        _vulnerableCo = null;
    }

    // Tiñe al boss mientras está vulnerable, para que se vea cuándo se le puede pegar.
    private void SetVulnerableVisual(bool on)
    {
        if (_renderers == null || _mpb == null)
            return;

        Color c = on ? vulnerableTint : Color.white;

        foreach (var r in _renderers)
        {
            if (r == null) continue;
            r.GetPropertyBlock(_mpb);
            _mpb.SetColor("_BaseColor", c);
            _mpb.SetColor("_Color", c);
            r.SetPropertyBlock(_mpb);
        }
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        if (_enemyAI != null)
        {
            _enemyAI.isAIActive = false;
        }

        if (_anim != null)
        {
            _anim.SetBool("isWalking", false);
            _anim.SetBool("isAttacking", false);
            _anim.SetTrigger("Die");
        }

        OnBossDied?.Invoke(this);

        Destroy(gameObject, 3f);
    }
}