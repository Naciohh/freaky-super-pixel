using UnityEngine;
using System.Collections;

public class SpiderSpawner : MonoBehaviour
{
    [Header("Configuración de la Araña")]
    public GameObject spiderPrefab;     // Acá va el prefab de la araña
    public EnemyData spiderData;        // El archivo Enemy_Spider que creamos
    public Transform player;            // El Transform del Player

    [Header("Configuración del Spawn")]
    [SerializeField] private int maxSpidersSimultaneous = 5; // Máximo de arañas de este spawner
    [SerializeField] private float spawnInterval = 4f;        // Cada cuántos segundos sale una
    [SerializeField] private float minDistanceFromPlayer = 4f; // Distancia mínima para que no te aparezca encima

    [Header("Área de Spawn")]
    [SerializeField] private Vector3 spawnAreaCenter;
    [SerializeField] private Vector3 spawnAreaSize = new Vector3(10f, 0f, 10f);

    private int activeSpiders = 0;
    private bool spawning = true;

    void Start()
    {
        // Vinculamos el evento de muerte para saber cuándo muere una araña y poder spawnear otra
        EnemyHealth.OnAnyEnemyDied += OnSpiderDied;
        StartCoroutine(SpawnLoop());
    }

    void OnDestroy()
    {
        EnemyHealth.OnAnyEnemyDied -= OnSpiderDied;
    }

    private IEnumerator SpawnLoop()
    {
        while (spawning)
        {
            if (activeSpiders < maxSpidersSimultaneous)
            {
                TrySpawn();
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void TrySpawn()
    {
        if (spiderPrefab == null) return;

        Vector3 pos = GetRandomSpawnPosition();

        // Instanciamos la araña
        GameObject go = Instantiate(spiderPrefab, pos, Quaternion.identity);

        // Le pasamos los datos de vida (EnemyHealth)
        EnemyHealth eh = go.GetComponent<EnemyHealth>();
        if (eh != null) eh.data = spiderData;

        // Le pasamos el player a la IA de la araña (SpiderAI)
        SpiderAI spider = go.GetComponent<SpiderAI>();
        if (spider != null)
        {
            spider.player = player;
        }

        activeSpiders++;
    }

    private Vector3 GetRandomSpawnPosition()
    {
        Vector3 pos;
        int attempts = 0;
        do
        {
            pos = spawnAreaCenter + new Vector3(
                Random.Range(-spawnAreaSize.x / 2f, spawnAreaSize.x / 2f),
                0f,
                Random.Range(-spawnAreaSize.z / 2f, spawnAreaSize.z / 2f)
            );
            attempts++;
        }
        while (player != null && Vector3.Distance(pos, player.position) < minDistanceFromPlayer && attempts < 10);

        return pos;
    }

    private void OnSpiderDied(EnemyHealth eh)
    {
        // Si muere un enemigo y era una araña, restamos una al contador para que puedan salir más
        if (eh != null && eh.GetComponent<SpiderAI>() != null)
        {
            activeSpiders = Mathf.Max(0, activeSpiders - 1);
        }
    }

    // Dibuja una caja verde en la pestaña Scene para saber dónde van a spawnear
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.3f);
        Gizmos.DrawCube(spawnAreaCenter, spawnAreaSize + Vector3.up * 0.1f);
    }
}