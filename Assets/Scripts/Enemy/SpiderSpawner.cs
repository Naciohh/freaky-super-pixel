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

    [Header("Área de spawn (todo el mapa, entre las 4 paredes)")]
    [Tooltip("Las arañas aparecen en cualquier punto dentro de esta caja, igual que el resto de los enemigos.")]
    [SerializeField] private Vector3 mapBoundsCenter = new Vector3(-56.5f, 0f, -0.25f);
    [SerializeField] private Vector3 mapBoundsSize   = new Vector3(280f, 0f, 235f);

    private int activeSpiders = 0;
    private bool _bossPhase = false;   // true cuando empieza la pelea del boss (fin de oleada)

    void OnEnable()
    {
        EnemyHealth.OnAnyEnemyDied += OnSpiderDied;
        // La pelea del boss empieza al terminar la oleada: cortar spawn + limpiar.
        // (OnEnable corre antes que cualquier Start, así no nos perdemos el evento que
        //  dispara GameLoader al restaurar un guardado ya en fase boss.)
        WaveManager.OnWaveEnd += OnBossPhaseBegin;
        WaveManager.OnWaveStart += OnWaveBegin;
    }

    void OnDisable()
    {
        EnemyHealth.OnAnyEnemyDied -= OnSpiderDied;
        WaveManager.OnWaveEnd -= OnBossPhaseBegin;
        WaveManager.OnWaveStart -= OnWaveBegin;
    }

    void Start()
    {
        // La dificultad escala cuántas arañas hay a la vez y cada cuánto salen.
        maxSpidersSimultaneous = Mathf.Max(1, Mathf.RoundToInt(maxSpidersSimultaneous * GameDifficulty.SpawnCountMult));
        spawnInterval = Mathf.Max(0.2f, spawnInterval * GameDifficulty.SpawnIntervalMult);

        StartCoroutine(SpawnLoop());
    }

    // Empieza la fase del boss: dejar de spawnear y eliminar las arañas vivas.
    private void OnBossPhaseBegin()
    {
        _bossPhase = true;
        ClearSpiders();
    }

    // (Re)arrancó una oleada: volver a permitir arañas.
    private void OnWaveBegin()
    {
        _bossPhase = false;
    }

    // Las arañas salen durante la oleada (comportamiento normal) y SOLO dejan de salir
    // cuando empieza la pelea del boss. No dependemos de WaveActive (que puede no estar
    // seteado según cómo se entró al nivel) para no romper el spawn.
    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            if (!_bossPhase && activeSpiders < maxSpidersSimultaneous)
                TrySpawn();

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

    // Elimina todas las arañas vivas (en la pelea del boss solo debe estar el boss).
    private void ClearSpiders()
    {
        var spiders = FindObjectsByType<SpiderAI>();
        foreach (var s in spiders)
            if (s != null) Destroy(s.gameObject);
        activeSpiders = 0;
    }

    private Vector3 GetRandomSpawnPosition()
    {
        // Cualquier punto dentro del mapa (entre las 4 paredes), evitando caer
        // demasiado cerca de Emilio.
        Vector3 pos = RandomPointInMap();
        for (int i = 0; i < 12 && player != null &&
             Vector3.Distance(pos, player.position) < minDistanceFromPlayer; i++)
            pos = RandomPointInMap();
        return pos;
    }

    private Vector3 RandomPointInMap()
    {
        Vector3 half = mapBoundsSize * 0.5f;
        return new Vector3(
            mapBoundsCenter.x + Random.Range(-half.x, half.x),
            mapBoundsCenter.y,
            mapBoundsCenter.z + Random.Range(-half.z, half.z));
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
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.12f);
        Gizmos.DrawCube(mapBoundsCenter, mapBoundsSize + Vector3.up * 0.1f);
    }
}
