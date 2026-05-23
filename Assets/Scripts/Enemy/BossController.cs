using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("Referencias")]
    public EnemyData bossData;
    public Transform player;
    public BossIntroDirector introDirector;

    private EnemyAI enemyAI;
    private EnemyHealth enemyHealth;

    void Awake()
    {
        enemyAI = GetComponent<EnemyAI>();
        enemyHealth = GetComponent<EnemyHealth>();

        if (bossData != null)
        {
            if (enemyAI != null) enemyAI.data = bossData;
            if (enemyHealth != null) enemyHealth.data = bossData;
        }
        else
            Debug.LogError("[BossController] bossData not assigned on " + gameObject.name, this);

        if (enemyAI != null) enemyAI.isAIActive = false;
        gameObject.SetActive(false);
    }

    public void Activate()
    {
        gameObject.SetActive(true);

        if (player != null && enemyAI != null)
            enemyAI.player = player;

        if (introDirector != null)
            introDirector.PlayIntro(this);
        else
        {
            Debug.LogWarning("[BossController] No BossIntroDirector assigned — activating AI directly.", this);
            OnIntroComplete();
        }
    }

    public void OnIntroComplete()
    {
        if (enemyAI != null) enemyAI.isAIActive = true;
        MusicManager.Instance?.PlayBoss();
    }
}
