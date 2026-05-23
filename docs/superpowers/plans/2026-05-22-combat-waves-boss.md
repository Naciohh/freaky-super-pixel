# Combat, Waves & Boss System — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Agregar sistema de combate para Emilio (ataque + parry), oleadas de enemigos con timer, modo transformación, boss con cinemática, y música por fase.

**Architecture:** Sistemas independientes que se comunican via eventos C# (`System.Action`). `WaveManager` orquesta el flujo general. Los stats de enemigos viven en `EnemyData` ScriptableObjects para que el mismo código funcione en múltiples niveles.

**Tech Stack:** Unity (C#), TextMeshPro, Unity UI (Image.fillAmount), CharacterController, Physics.OverlapSphere, AudioSource crossfade manual.

---

## Estructura de archivos

```
Assets/Scripts/Data/
  EnemyData.cs                      ← ScriptableObject (NUEVO)

Assets/Scripts/Enemy/
  EnemyHealth.cs                    ← HP + muerte del enemigo (NUEVO)
  EnemyAI.cs                        ← REFACTOR (leer EnemyData, siempre perseguir)
  EnemySpawner.cs                   ← Spawn random con pesos (NUEVO)
  BossController.cs                 ← Extiende lógica de EnemyAI (NUEVO)

Assets/Scripts/Player/
  PlayerCombat.cs                   ← Ataque click izq + parry click der (NUEVO)
  TransformationMode.cs             ← Barra parry + modo transformación (NUEVO)
  PlayerMovement.cs                 ← REFACTOR (quitar tecla E, agregar SetTransformed)

Assets/Scripts/Systems/
  WaveManager.cs                    ← Timer oleada + eventos (NUEVO)
  MusicManager.cs                   ← Crossfade música por fase (NUEVO)

Assets/Scripts/UI/
  GameHUD.cs                        ← Todas las barras y timer en pantalla (NUEVO)
  BossIntroDirector.cs              ← Cinemática intro boss (NUEVO)
  VictoryScreen.cs                  ← Panel de victoria con resumen + botón siguiente nivel (NUEVO)

Assets/Scripts/Systems/
  GameStats.cs                      ← Acumula estadísticas de partida (kills, parries, tiempo) (NUEVO)

Assets/Data/Enemies/
  Enemy_Small.asset                 ← ScriptableObject asset
  Enemy_Medium.asset
  Enemy_Large.asset
  Boss_Supermarket.asset
```

---

## Task 1: EnemyData ScriptableObject

**Files:**
- Create: `Assets/Scripts/Data/EnemyData.cs`

- [ ] **Paso 1: Crear la clase EnemyData**

Crear `Assets/Scripts/Data/EnemyData.cs`:

```csharp
using UnityEngine;

public enum BossAttackType { Melee }

[CreateAssetMenu(fileName = "EnemyData", menuName = "Game/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Identity")]
    public string displayName = "Enemy";
    public bool isBoss = false;

    [Header("Stats")]
    public int maxHP = 60;
    public int damage = 25;
    public float moveSpeed = 2.5f;
    public float modelScale = 1f;

    [Header("Ataque")]
    public float attackCooldown = 1.2f;

    [Header("Boss Only")]
    public BossAttackType bossAttackType = BossAttackType.Melee;
}
```

- [ ] **Paso 2: Crear los 4 assets en el Editor**

En Unity: click derecho en `Assets/Data/Enemies/` → Create → Game → Enemy Data. Crear 4 assets con estos valores:

| Asset | displayName | isBoss | maxHP | damage | moveSpeed | modelScale |
|---|---|---|---|---|---|---|
| Enemy_Small | "Esqueleto" | false | 30 | 10 | 3.5 | 0.7 |
| Enemy_Medium | "Esqueleto" | false | 60 | 25 | 2.5 | 1.0 |
| Enemy_Large | "Esqueleto" | false | 100 | 40 | 1.8 | 1.4 |
| Boss_Supermarket | "Jefe del Súper" | true | 500 | 60 | 2.0 | 2.0 |

Además, en cada asset setear `attackCooldown`: Small=1.2, Medium=1.2, Large=1.5, Boss=0.8

- [ ] **Paso 3: Verificar**

Entrar en Play Mode, no debe haber errores en consola. Los assets deben verse en el Inspector con sus valores.

---

## Task 2: EnemyHealth

**Files:**
- Create: `Assets/Scripts/Enemy/EnemyHealth.cs`

- [ ] **Paso 1: Crear EnemyHealth**

```csharp
using UnityEngine;
using System;

public class EnemyHealth : MonoBehaviour
{
    public EnemyData data;

    public static event Action<EnemyHealth> OnAnyEnemyDied;

    public int currentHP;

    void Start()
    {
        currentHP = data.maxHP;
    }

    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        if (currentHP <= 0)
        {
            currentHP = 0;
            Die();
        }
    }

    private void Die()
    {
        OnAnyEnemyDied?.Invoke(this);
        Destroy(gameObject);
    }
}
```

- [ ] **Paso 2: Agregar EnemyHealth al prefab del esqueleto**

En Unity: seleccionar el prefab del esqueleto → Add Component → EnemyHealth → asignar `Enemy_Medium.asset` en el campo `data` (usaremos uno solo por ahora).

- [ ] **Paso 3: Verificar**

Entrar en Play Mode. En la consola ejecutar (o hacer un test manual): agregar temporalmente `GetComponent<EnemyHealth>().TakeDamage(999)` en `EnemyAI.Start()` y verificar que el enemigo se destruye. Quitar esa línea después.

---

## Task 3: Refactor EnemyAI

**Files:**
- Modify: `Assets/Scripts/EnemyAI.cs`

El script actual tiene `detectionDistance` (el enemigo solo persigue si está cerca). Lo eliminamos — el enemigo siempre persigue. También lee stats de `EnemyData` y expone `isAttacking` para que `PlayerCombat` pueda detectarlo.

- [ ] **Paso 1: Reemplazar EnemyAI.cs completo**

```csharp
using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;
    public EnemyData data;

    [Header("Config")]
    public float attackDistance = 2f;

    public bool isAttacking { get; private set; }
    public bool isAIActive = true;

    private Animator anim;
    private PlayerHealth playerHealth;
    private float nextAttackTime = 0f;

    void Start()
    {
        anim = GetComponent<Animator>();
        playerHealth = player.GetComponent<PlayerHealth>();
        transform.localScale = Vector3.one * data.modelScale;
    }

    void Update()
    {
        if (!isAIActive) return;

        float distance = Vector3.Distance(transform.position, player.position);
        transform.LookAt(player);

        if (distance < attackDistance)
        {
            anim.SetBool("isWalking", false);
            anim.SetBool("isAttacking", true);
            isAttacking = true;

            if (Time.time >= nextAttackTime)
            {
                StartCoroutine(DealDamage());
                nextAttackTime = Time.time + data.attackCooldown;
            }
        }
        else
        {
            anim.SetBool("isAttacking", false);
            isAttacking = false;
            anim.SetBool("isWalking", true);

            transform.position = Vector3.MoveTowards(
                transform.position,
                player.position,
                data.moveSpeed * Time.deltaTime
            );
        }
    }

    private IEnumerator DealDamage()
    {
        yield return new WaitForSeconds(0.5f);
        if (playerHealth != null)
            playerHealth.TakeDamage(data.damage);
    }
}
```

- [ ] **Paso 2: Actualizar el prefab del esqueleto**

Seleccionar el prefab del esqueleto → en el componente EnemyAI → asignar `Enemy_Medium.asset` en el campo `data`.

- [ ] **Paso 3: Verificar**

Entrar en Play Mode. El esqueleto debe perseguir al jugador desde cualquier distancia y atacar al llegar cerca.

---

## Task 4: Refactor PlayerMovement

**Files:**
- Modify: `Assets/Emilio/PlayerMovement.cs`

Quitar el toggle con tecla `E` (ahora la transformación la controla `TransformationMode`). Exponer `SetTransformed(bool)` público.

- [ ] **Paso 1: Modificar PlayerMovement.cs**

Reemplazar el método `HandleTransformation()` y agregar el método público:

```csharp
// ELIMINAR este método:
// void HandleTransformation() { ... }

// ELIMINAR esta línea en Update():
// HandleTransformation();

// AGREGAR este método público:
public void SetTransformed(bool value)
{
    isTransformed = value;
    normalForm.SetActive(!isTransformed);
    transformedForm.SetActive(isTransformed);
}
```

El `Update()` queda así:
```csharp
void Update()
{
    HandleMovement();
}
```

- [ ] **Paso 2: Verificar**

Entrar en Play Mode. La tecla `E` ya no debe cambiar de forma. El personaje se mueve normalmente.

---

## Task 5: PlayerCombat

**Files:**
- Create: `Assets/Scripts/Player/PlayerCombat.cs`

- [ ] **Paso 1: Crear PlayerCombat.cs**

```csharp
using UnityEngine;
using System;
using System.Collections;

public class PlayerCombat : MonoBehaviour
{
    [Header("Ataque")]
    public float attackRange = 1.5f;
    public int playerDamage = 34;
    public float attackCooldown = 0.6f;
    public LayerMask enemyLayer;

    [Header("Parry")]
    public float parryWindow = 0.4f;
    public float parryRange = 2.5f;

    public static event Action OnParrySuccess;

    private float nextAttackTime = 0f;
    private bool isParrying = false;

    public bool IsTransforming { get; set; } = false;

    void Update()
    {
        if (IsTransforming) return;

        if (Input.GetMouseButtonDown(0) && Time.time >= nextAttackTime)
        {
            Attack();
            nextAttackTime = Time.time + attackCooldown;
        }

        if (Input.GetMouseButtonDown(1))
        {
            StartCoroutine(ParryWindow());
        }
    }

    private void Attack()
    {
        Vector3 origin = transform.position + transform.forward * (attackRange * 0.5f);
        Collider[] hits = Physics.OverlapSphere(origin, attackRange, enemyLayer);
        foreach (var hit in hits)
        {
            EnemyHealth eh = hit.GetComponent<EnemyHealth>();
            if (eh != null)
                eh.TakeDamage(playerDamage);
        }
    }

    private IEnumerator ParryWindow()
    {
        isParrying = true;
        float elapsed = 0f;

        while (elapsed < parryWindow)
        {
            elapsed += Time.deltaTime;

            Collider[] nearby = Physics.OverlapSphere(transform.position, parryRange, enemyLayer);
            foreach (var col in nearby)
            {
                EnemyAI ai = col.GetComponent<EnemyAI>();
                if (ai != null && ai.isAttacking)
                {
                    isParrying = false;
                    OnParrySuccess?.Invoke();
                    yield break;
                }
            }
            yield return null;
        }

        isParrying = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward * (attackRange * 0.5f), attackRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, parryRange);
    }
}
```

- [ ] **Paso 2: Agregar PlayerCombat al GameObject de Emilio**

Seleccionar Emilio en la escena → Add Component → PlayerCombat. Configurar `enemyLayer`: en Unity crear un Layer llamado "Enemy" y asignárselo al prefab del esqueleto. Arrastrar ese layer al campo `enemyLayer` de PlayerCombat.

- [ ] **Paso 3: Verificar**

Entrar en Play Mode. Click izquierdo cerca de un esqueleto → debe recibir daño y morir si recibe suficiente. Click derecho cuando el esqueleto está atacando → debe imprimirse el evento (verificar en siguiente tarea).

---

## Task 6: TransformationMode

**Files:**
- Create: `Assets/Scripts/Player/TransformationMode.cs`

- [ ] **Paso 1: Crear TransformationMode.cs**

```csharp
using UnityEngine;
using System.Collections;

public class TransformationMode : MonoBehaviour
{
    [Header("Config")]
    public int parriesToTransform = 5;
    public float transformDuration = 8f;
    public LayerMask enemyLayer;

    private int parryCount = 0;
    private bool isTransformed = false;
    private PlayerMovement playerMovement;
    private PlayerCombat playerCombat;

    public int ParryCount => parryCount;
    public bool IsTransformed => isTransformed;
    public float TransformProgress => (float)parryCount / parriesToTransform;

    void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerCombat = GetComponent<PlayerCombat>();
    }

    void OnEnable()
    {
        PlayerCombat.OnParrySuccess += HandleParrySuccess;
    }

    void OnDisable()
    {
        PlayerCombat.OnParrySuccess -= HandleParrySuccess;
    }

    private void HandleParrySuccess()
    {
        if (isTransformed) return;

        parryCount++;
        if (parryCount >= parriesToTransform)
        {
            parryCount = parriesToTransform;
            StartCoroutine(TransformRoutine());
        }
    }

    private IEnumerator TransformRoutine()
    {
        isTransformed = true;
        playerMovement.SetTransformed(true);
        playerCombat.IsTransforming = true;

        float elapsed = 0f;
        while (elapsed < transformDuration)
        {
            elapsed += Time.deltaTime;
            parryCount = Mathf.RoundToInt(Mathf.Lerp(parriesToTransform, 0, elapsed / transformDuration));
            yield return null;
        }

        parryCount = 0;
        isTransformed = false;
        playerMovement.SetTransformed(false);
        playerCombat.IsTransforming = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isTransformed) return;

        EnemyHealth eh = other.GetComponent<EnemyHealth>();
        if (eh != null && !eh.data.isBoss)
            eh.TakeDamage(99999);
    }
}
```

- [ ] **Paso 2: Agregar TransformationMode al GameObject de Emilio**

Seleccionar Emilio → Add Component → TransformationMode. Asignar el mismo `enemyLayer` que en PlayerCombat. Asegurarse que el `Collider` de la forma transformada de Emilio tiene `Is Trigger = true`.

- [ ] **Paso 3: Verificar**

Entrar en Play Mode. Hacer 5 parries exitosos (con el enemigo atacando) → Emilio debe cambiar a forma transformada automáticamente y volver después de 8 segundos.

---

## Task 7: EnemySpawner

**Files:**
- Create: `Assets/Scripts/Enemy/EnemySpawner.cs`

- [ ] **Paso 1: Crear EnemySpawner.cs**

```csharp
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class EnemySpawnEntry
{
    public GameObject prefab;
    public EnemyData data;
    [Range(1, 100)] public int weight = 33;
}

public class EnemySpawner : MonoBehaviour
{
    [Header("Config")]
    public List<EnemySpawnEntry> enemyTypes;
    public Transform player;
    public int maxEnemiesSimultaneous = 15;
    public float spawnInterval = 3f;
    public float minDistanceFromPlayer = 5f;

    [Header("Área de spawn (World Space)")]
    public Vector3 spawnAreaCenter;
    public Vector3 spawnAreaSize = new Vector3(20f, 0f, 20f);

    private int activeEnemies = 0;
    private bool spawning = false;
    private Coroutine spawnCoroutine;

    void OnEnable()
    {
        EnemyHealth.OnAnyEnemyDied += OnEnemyDied;
    }

    void OnDisable()
    {
        EnemyHealth.OnAnyEnemyDied -= OnEnemyDied;
    }

    public void StartSpawning()
    {
        spawning = true;
        spawnCoroutine = StartCoroutine(SpawnLoop());
    }

    public void StopSpawning()
    {
        spawning = false;
        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);
    }

    public void DestroyAllNormalEnemies()
    {
        EnemyHealth[] all = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
        foreach (var e in all)
        {
            if (!e.data.isBoss)
                Destroy(e.gameObject);
        }
        activeEnemies = 0;
    }

    private IEnumerator SpawnLoop()
    {
        while (spawning)
        {
            if (activeEnemies < maxEnemiesSimultaneous)
            {
                TrySpawn();
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void TrySpawn()
    {
        Vector3 pos = GetRandomSpawnPosition();
        EnemySpawnEntry entry = PickWeightedRandom();
        if (entry == null) return;

        GameObject go = Instantiate(entry.prefab, pos, Quaternion.identity);

        EnemyHealth eh = go.GetComponent<EnemyHealth>();
        if (eh != null) eh.data = entry.data;

        EnemyAI ai = go.GetComponent<EnemyAI>();
        if (ai != null)
        {
            ai.data = entry.data;
            ai.player = player;
        }

        activeEnemies++;
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
        while (Vector3.Distance(pos, player.position) < minDistanceFromPlayer && attempts < 10);

        return pos;
    }

    private EnemySpawnEntry PickWeightedRandom()
    {
        if (enemyTypes == null || enemyTypes.Count == 0) return null;

        int total = 0;
        foreach (var e in enemyTypes) total += e.weight;

        int roll = Random.Range(0, total);
        int cumulative = 0;
        foreach (var e in enemyTypes)
        {
            cumulative += e.weight;
            if (roll < cumulative) return e;
        }
        return enemyTypes[enemyTypes.Count - 1];
    }

    private void OnEnemyDied(EnemyHealth _)
    {
        activeEnemies = Mathf.Max(0, activeEnemies - 1);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawCube(spawnAreaCenter, spawnAreaSize + Vector3.up * 0.1f);
    }
}
```

- [ ] **Paso 2: Configurar en la escena**

Crear un GameObject vacío llamado "EnemySpawner" → Add Component → EnemySpawner. En `enemyTypes`: agregar 3 entradas (Small/Medium/Large prefab + data + peso 60/30/10). Asignar `player` con el Transform de Emilio. Ajustar `spawnAreaCenter` y `spawnAreaSize` para cubrir el mapa del supermercado (ver desde Scene View con Gizmos activos).

- [ ] **Paso 3: Verificar gizmo**

En Scene View con el GameObject seleccionado debe verse el área verde de spawn sobre el mapa.

---

## Task 8: WaveManager

**Files:**
- Create: `Assets/Scripts/Systems/WaveManager.cs`

- [ ] **Paso 1: Crear WaveManager.cs**

```csharp
using UnityEngine;
using System;
using System.Collections;

public class WaveManager : MonoBehaviour
{
    [Header("Config")]
    public float waveDuration = 90f;

    [Header("Referencias")]
    public EnemySpawner spawner;
    public BossController bossController;

    public static event Action OnWaveStart;
    public static event Action OnWaveEnd;

    public float TimeRemaining { get; private set; }
    public bool WaveActive { get; private set; }

    void Start()
    {
        StartWave();
    }

    public void StartWave()
    {
        TimeRemaining = waveDuration;
        WaveActive = true;
        spawner.StartSpawning();
        OnWaveStart?.Invoke();
        StartCoroutine(WaveCountdown());
    }

    private IEnumerator WaveCountdown()
    {
        while (TimeRemaining > 0f)
        {
            TimeRemaining -= Time.deltaTime;
            yield return null;
        }

        TimeRemaining = 0f;
        WaveActive = false;
        EndWave();
    }

    private void EndWave()
    {
        spawner.StopSpawning();
        spawner.DestroyAllNormalEnemies();
        OnWaveEnd?.Invoke();
        bossController.Activate();
    }
}
```

- [ ] **Paso 2: Configurar en la escena**

Crear GameObject "WaveManager" → Add Component → WaveManager. Asignar referencias a `EnemySpawner` y `BossController` (este último se crea en Task 9).

- [ ] **Paso 3: Verificar (con waveDuration temporal en 10s)**

Cambiar `waveDuration = 10` en el Inspector temporalmente. Entrar en Play Mode → enemigos deben spawnar → a los 10 segundos todos se destruyen. Restaurar a 90s.

---

## Task 9: BossController

**Files:**
- Create: `Assets/Scripts/Enemy/BossController.cs`

- [ ] **Paso 1: Crear BossController.cs**

```csharp
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

        enemyAI.data = bossData;
        enemyAI.player = player;
        enemyHealth.data = bossData;

        enemyAI.isAIActive = false;
        gameObject.SetActive(false);
    }

    public void Activate()
    {
        gameObject.SetActive(true);
        introDirector.PlayIntro(this);
    }

    public void OnIntroComplete()
    {
        enemyAI.isAIActive = true;
    }
}
```

- [ ] **Paso 2: Crear el prefab del boss**

Duplicar el prefab del esqueleto → renombrarlo "Boss_Supermarket". Agregar los componentes `BossController` y `EnemyHealth`. Asignar `Boss_Supermarket.asset` en los campos correspondientes. Posicionarlo en la escena en el punto donde debe aparecer (desactivado por defecto — `BossController.Awake()` lo desactiva).

- [ ] **Paso 3: Verificar**

Cuando el WaveManager termine (con el timer reducido a 10s), el boss debe aparecer en su posición. La IA se activa después de la cinemática (Task 10).

---

## Task 10: BossIntroDirector

**Files:**
- Create: `Assets/Scripts/UI/BossIntroDirector.cs`

- [ ] **Paso 1: Crear BossIntroDirector.cs**

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BossIntroDirector : MonoBehaviour
{
    [Header("Cámaras")]
    public Camera mainCamera;
    public Camera cinematicCamera;

    [Header("UI Badge")]
    public CanvasGroup badgeCanvasGroup;
    public TMP_Text bossNameText;

    [Header("Config")]
    public float introDuration = 3f;
    public float fadeDuration = 0.5f;

    private BossController currentBoss;

    public void PlayIntro(BossController boss)
    {
        currentBoss = boss;
        StartCoroutine(IntroSequence());
    }

    private IEnumerator IntroSequence()
    {
        // Pausar juego (excepto coroutines no-scaled — usamos unscaled time)
        Time.timeScale = 0f;

        // Cambiar cámaras
        mainCamera.gameObject.SetActive(false);
        cinematicCamera.gameObject.SetActive(true);

        // Apuntar cámara cinemática al boss
        cinematicCamera.transform.LookAt(currentBoss.transform.position + Vector3.up * 1.5f);

        // Fade-in del badge
        bossNameText.text = currentBoss.bossData.displayName;
        badgeCanvasGroup.alpha = 0f;
        badgeCanvasGroup.gameObject.SetActive(true);

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            badgeCanvasGroup.alpha = t / fadeDuration;
            yield return null;
        }
        badgeCanvasGroup.alpha = 1f;

        // Esperar durante la cinemática
        yield return new WaitForSecondsRealtime(introDuration);

        // Fade-out del badge
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            badgeCanvasGroup.alpha = 1f - (t / fadeDuration);
            yield return null;
        }
        badgeCanvasGroup.gameObject.SetActive(false);

        // Restaurar
        cinematicCamera.gameObject.SetActive(false);
        mainCamera.gameObject.SetActive(true);
        Time.timeScale = 1f;

        // Activar IA del boss
        currentBoss.OnIntroComplete();
    }
}
```

- [ ] **Paso 2: Configurar en la escena**

1. Crear un segundo GameObject `Camera` en la escena llamado "CinematicCamera". Posicionarlo a ras del suelo apuntando hacia el spawn del boss (ángulo bajo, dramático). Desactivarlo por defecto.
2. Crear en el Canvas un GameObject "BossIntoBadge" con un `CanvasGroup` + `Image` (fondo blanco placeholder) + `TMP_Text` para el nombre. Desactivarlo por defecto.
3. Crear GameObject "BossIntroDirector" → Add Component → BossIntroDirector. Asignar todas las referencias.

- [ ] **Paso 3: Verificar**

Al finalizar la oleada (timer reducido a 10s para prueba): debe verse la cámara cinemática mostrando el boss, el badge con el nombre aparece y desaparece, luego el boss comienza a moverse.

---

## Task 11: GameHUD

**Files:**
- Create: `Assets/Scripts/UI/GameHUD.cs`

- [ ] **Paso 1: Preparar el Canvas**

En la escena, en el Canvas de juego (o crear uno nuevo "GameHUD"), agregar estos elementos UI:

```
Canvas (GameHUD)
  ├── PlayerHealthBar       → Image (fillAmount horizontal), abajo izquierda
  ├── TransformBar          → Image (fillAmount horizontal), abajo izquierda bajo la vida
  ├── WaveTimerText         → TMP_Text, arriba centro
  ├── BossHealthBarGroup    → GameObject padre (se activa/desactiva)
  │     └── BossHealthBar   → Image (fillAmount horizontal), arriba centro
  └── BossIntoBadge         → (ya creado en Task 10)
```

- [ ] **Paso 2: Crear GameHUD.cs**

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameHUD : MonoBehaviour
{
    [Header("Vida jugador")]
    public Image playerHealthBar;
    public PlayerHealth playerHealth;

    [Header("Barra transformación")]
    public Image transformBar;
    public TransformationMode transformationMode;

    [Header("Timer oleada")]
    public TMP_Text waveTimerText;
    public WaveManager waveManager;

    [Header("Boss")]
    public GameObject bossHealthBarGroup;
    public Image bossHealthBar;
    private EnemyHealth bossEnemyHealth;

    void OnEnable()
    {
        WaveManager.OnWaveEnd += OnWaveEnd;
    }

    void OnDisable()
    {
        WaveManager.OnWaveEnd -= OnWaveEnd;
    }

    private void OnWaveEnd()
    {
        waveTimerText.gameObject.SetActive(false);

        // Buscar el boss recién spawneado
        BossController boss = FindAnyObjectByType<BossController>();
        if (boss != null)
        {
            bossEnemyHealth = boss.GetComponent<EnemyHealth>();
            bossHealthBarGroup.SetActive(true);
        }
    }

    void Update()
    {
        // Vida jugador
        playerHealthBar.fillAmount = (float)playerHealth.currentHealth / playerHealth.maxHealth;

        // Barra transformación
        transformBar.fillAmount = transformationMode.TransformProgress;

        // Timer
        if (waveManager.WaveActive)
            waveTimerText.text = Mathf.CeilToInt(waveManager.TimeRemaining).ToString();

        // Vida boss
        if (bossEnemyHealth != null)
            bossHealthBar.fillAmount = (float)bossEnemyHealth.currentHP / bossEnemyHealth.data.maxHP;
    }
}
```

**Nota:** `EnemyHealth.currentHP` ya es `public` desde Task 2, no requiere cambios.

- [ ] **Paso 3: Conectar en Inspector**

Asignar todas las referencias en el componente GameHUD desde el Inspector. Verificar en Play Mode que las barras se mueven correctamente.

- [ ] **Paso 4: Verificar**

Entrar en Play Mode: la barra de vida del jugador baja cuando recibe daño (tecla H del test en PlayerHealth). La barra de transformación sube con parries exitosos. El timer cuenta hacia atrás. Al finalizar la oleada, aparece la barra del boss.

---

## Task 12: MusicManager

**Files:**
- Create: `Assets/Scripts/Systems/MusicManager.cs`

- [ ] **Paso 1: Crear MusicManager.cs**

```csharp
using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Tracks del nivel actual")]
    public AudioClip explorationClip;
    public AudioClip fightClip;
    public AudioClip bossClip;

    [Header("Config")]
    public float fadeDuration = 1.5f;

    private AudioSource sourceA;
    private AudioSource sourceB;
    private AudioSource activeSource;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        sourceA = gameObject.AddComponent<AudioSource>();
        sourceB = gameObject.AddComponent<AudioSource>();
        sourceA.loop = true;
        sourceB.loop = true;
        activeSource = sourceA;
    }

    void OnEnable()
    {
        WaveManager.OnWaveStart += PlayFight;
        WaveManager.OnWaveEnd += OnWaveEnd;
    }

    void OnDisable()
    {
        WaveManager.OnWaveStart -= PlayFight;
        WaveManager.OnWaveEnd -= OnWaveEnd;
    }

    void Start()
    {
        PlayClip(explorationClip);
    }

    private void OnWaveEnd()
    {
        // La música del boss se activa al terminar la cinemática (llamada desde BossController.OnIntroComplete)
    }

    public void PlayFight() => CrossfadeTo(fightClip);
    public void PlayBoss()  => CrossfadeTo(bossClip);

    private void PlayClip(AudioClip clip)
    {
        activeSource.clip = clip;
        activeSource.volume = 1f;
        activeSource.Play();
    }

    private void CrossfadeTo(AudioClip clip)
    {
        AudioSource incoming = (activeSource == sourceA) ? sourceB : sourceA;
        incoming.clip = clip;
        incoming.volume = 0f;
        incoming.Play();
        StartCoroutine(Crossfade(activeSource, incoming));
        activeSource = incoming;
    }

    private IEnumerator Crossfade(AudioSource outgoing, AudioSource incoming)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            outgoing.volume = 1f - t;
            incoming.volume = t;
            yield return null;
        }
        outgoing.Stop();
        outgoing.volume = 1f;
    }
}
```

- [ ] **Paso 2: Actualizar BossController.OnIntroComplete para llamar PlayBoss**

En `BossController.cs`, agregar al método `OnIntroComplete()`:

```csharp
public void OnIntroComplete()
{
    enemyAI.isAIActive = true;
    MusicManager.Instance?.PlayBoss();
}
```

- [ ] **Paso 3: Configurar en la escena**

Crear GameObject "MusicManager" → Add Component → MusicManager. Asignar los 3 AudioClips del nivel supermercado en los campos `explorationClip`, `fightClip`, `bossClip`.

- [ ] **Paso 4: Verificar**

Entrar en Play Mode con volumen activo:
- Al inicio: suena la canción de exploración
- Cuando arranca el timer de oleada: crossfade a canción de pelea
- Al aparecer el boss (después de la cinemática): crossfade a canción del boss

---

## Task 13: Wiring final y ajuste de escena

- [ ] **Paso 1: Verificar todas las referencias en Inspector**

Checklist:
- [ ] `WaveManager` tiene asignado `EnemySpawner` y `BossController`
- [ ] `EnemySpawner` tiene el área de spawn correctamente centrada sobre el mapa (usar Gizmo)
- [ ] `EnemySpawner.enemyTypes` tiene las 3 entradas (Small/Medium/Large) con prefab + data + peso
- [ ] `BossController` tiene asignado `bossData`, `player` y `BossIntroDirector`
- [ ] `BossIntroDirector` tiene asignado `mainCamera`, `cinematicCamera`, `badgeCanvasGroup`, `bossNameText`
- [ ] `GameHUD` tiene todas sus referencias asignadas
- [ ] `MusicManager` tiene los 3 AudioClips asignados
- [ ] El prefab del esqueleto tiene Layer = "Enemy"
- [ ] El `enemyLayer` de `PlayerCombat` y `TransformationMode` apunta al Layer "Enemy"

- [ ] **Paso 2: Test de flujo completo**

Con `waveDuration = 15` en WaveManager:
1. Entrar en Play Mode → suena música de exploración, spawnan esqueletos
2. Timer empieza → música cambia a pelea
3. Atacar un esqueleto (click izq) → muere al recibir suficiente daño
4. Hacer 5 parries → Emilio se transforma, mata por colisión por 8s
5. Timer llega a 0 → esqueletos desaparecen
6. Cinemática del boss → cámara cinemática, badge con nombre
7. Boss aparece y persigue a Emilio → música de boss
8. Atacar al boss → barra de vida del boss baja en el HUD
9. Boss muere → aparece VictoryScreen con stats y botón "Siguiente Nivel"
10. Click en botón → carga la escena Level2

- [ ] **Paso 3: Restaurar waveDuration = 90**

- [ ] **Paso 4: Limpiar código de test**

Remover la línea de test `TakeDamage(999)` de EnemyAI.Start() si quedó del Task 2. Remover el `Input.GetKeyDown(KeyCode.H)` de `PlayerHealth.Update()` si ya no hace falta.

---

## Task 14: GameStats + VictoryScreen

**Files:**
- Create: `Assets/Scripts/Systems/GameStats.cs`
- Create: `Assets/Scripts/UI/VictoryScreen.cs`

- [ ] **Paso 1: Crear GameStats.cs**

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStats : MonoBehaviour
{
    public static GameStats Instance { get; private set; }

    public int EnemiesKilled { get; private set; }
    public int ParriesPerformed { get; private set; }
    public float TimeElapsed { get; private set; }

    private bool tracking = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnEnable()
    {
        EnemyHealth.OnAnyEnemyDied += OnEnemyDied;
        PlayerCombat.OnParrySuccess += OnParry;
        WaveManager.OnWaveStart += StartTracking;
    }

    void OnDisable()
    {
        EnemyHealth.OnAnyEnemyDied -= OnEnemyDied;
        PlayerCombat.OnParrySuccess -= OnParry;
        WaveManager.OnWaveStart -= StartTracking;
    }

    void Update()
    {
        if (tracking) TimeElapsed += Time.deltaTime;
    }

    private void StartTracking() => tracking = true;

    private void OnEnemyDied(EnemyHealth eh)
    {
        if (!eh.data.isBoss) EnemiesKilled++;
    }

    private void OnParry() => ParriesPerformed++;

    public void StopTracking() => tracking = false;
}
```

- [ ] **Paso 2: Crear VictoryScreen.cs**

```csharp
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class VictoryScreen : MonoBehaviour
{
    [Header("Panel")]
    public GameObject panel;

    [Header("Texto estadísticas")]
    public TMP_Text enemiesKilledText;
    public TMP_Text parriesText;
    public TMP_Text timeText;

    [Header("Botón")]
    public Button nextLevelButton;
    public string nextLevelSceneName = "Level2";

    void OnEnable()
    {
        EnemyHealth.OnAnyEnemyDied += OnEnemyDied;
    }

    void OnDisable()
    {
        EnemyHealth.OnAnyEnemyDied -= OnEnemyDied;
    }

    private void OnEnemyDied(EnemyHealth eh)
    {
        if (eh.data.isBoss) ShowVictory();
    }

    private void ShowVictory()
    {
        GameStats stats = GameStats.Instance;
        if (stats != null)
        {
            stats.StopTracking();
            enemiesKilledText.text = "Enemigos eliminados: " + stats.EnemiesKilled;
            parriesText.text       = "Parries realizados: "  + stats.ParriesPerformed;

            int minutes = Mathf.FloorToInt(stats.TimeElapsed / 60f);
            int seconds = Mathf.FloorToInt(stats.TimeElapsed % 60f);
            timeText.text = string.Format("Tiempo: {0:00}:{1:00}", minutes, seconds);
        }

        Time.timeScale = 0f;
        panel.SetActive(true);

        nextLevelButton.onClick.RemoveAllListeners();
        nextLevelButton.onClick.AddListener(() =>
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(nextLevelSceneName);
        });
    }
}
```

- [ ] **Paso 3: Armar el panel en el Canvas**

En el Canvas de juego agregar:
```
Canvas
  └── VictoryPanel (desactivado por defecto)
        ├── TitleText         → TMP_Text: "¡NIVEL COMPLETADO!"
        ├── EnemiesKilledText → TMP_Text (placeholder)
        ├── ParriesText       → TMP_Text (placeholder)
        ├── TimeText          → TMP_Text (placeholder)
        └── NextLevelButton   → Button + TMP_Text "Siguiente Nivel"
```

Crear GameObject "VictoryScreen" → Add Component → VictoryScreen. Asignar todas las referencias. Asegurarse que `nextLevelSceneName = "Level2"` coincide con el nombre exacto de la escena del nivel 2 (Task 15).

- [ ] **Paso 4: Agregar GameStats al GameObject de la escena**

Crear GameObject "GameStats" → Add Component → GameStats.

- [ ] **Paso 5: Verificar**

Con boss en HP bajo (reducir `maxHP` a 10 en `Boss_Supermarket.asset` temporalmente): matar al boss → el panel de victoria debe aparecer con las estadísticas correctas. Click en "Siguiente Nivel" → debe cargar Level2 (puede dar error si la escena no existe todavía — normal, se arregla en Task 15). Restaurar HP del boss a 500.

---

## Task 15: Nivel 2 — Configurar escena "Nivel 2" (Lejano Oeste)

La escena del nivel 2 ya existe en `Assets/Escenario/Lejano oeste/Nivel 2.unity`. No hay que crearla — solo configurarla con los mismos sistemas del nivel 1.

- [ ] **Paso 1: Registrar ambas escenas en Build Settings**

File → Build Settings → arrastrar ambas escenas:
- La escena del supermercado (nivel 1)
- `Assets/Escenario/Lejano oeste/Nivel 2.unity`

Verificar que el nombre exacto que aparece en Build Settings para el nivel 2 es **"Nivel 2"**. Actualizar el campo `nextLevelSceneName` del `VictoryScreen` en el nivel 1 con ese nombre exacto.

- [ ] **Paso 2: Crear EnemyData assets para Nivel 2**

En `Assets/Data/Enemies/NivelDos/` crear 4 nuevos assets (Create → Game → Enemy Data). Valores placeholder hasta tener los assets finales:

| Asset | displayName | maxHP | damage | moveSpeed | attackCooldown | modelScale |
|---|---|---|---|---|---|---|
| L2_Enemy_Small | "Vaquero" | 40 | 12 | 3.8 | 1.2 | 0.7 |
| L2_Enemy_Medium | "Vaquero" | 75 | 30 | 2.8 | 1.2 | 1.0 |
| L2_Enemy_Large | "Vaquero" | 120 | 50 | 1.6 | 1.5 | 1.4 |
| L2_Boss | "Sheriff" | 700 | 80 | 2.2 | 0.8 | 2.0 |

- [ ] **Paso 3: Agregar todos los GameObjects de sistemas a la escena Nivel 2**

Abrir `Assets/Escenario/Lejano oeste/Nivel 2.unity`. Crear y configurar los mismos GameObjects que en el nivel 1:

| GameObject | Componente | Notas |
|---|---|---|
| WaveManager | WaveManager | waveDuration = 90, asignar EnemySpawner y BossController |
| EnemySpawner | EnemySpawner | Usar L2_Enemy_Small/Medium/Large. Ajustar spawnAreaCenter/Size al mapa del lejano oeste (usar Gizmo) |
| GameStats | GameStats | Sin configuración extra |
| MusicManager | MusicManager | Asignar los 3 AudioClips del nivel 2 |
| BossController + prefab | BossController | Usar L2_Boss.asset. Posicionar en el mapa |
| BossIntroDirector | BossIntroDirector | Nueva CinematicCamera posicionada para este mapa |
| GameHUD (Canvas) | GameHUD | Misma UI — copiar prefab del Canvas del nivel 1 |
| VictoryScreen | VictoryScreen | nextLevelSceneName = "" (fin del juego por ahora) |

- [ ] **Paso 4: Posicionar Emilio en la escena Nivel 2**

Arrastrar el prefab de Emilio a la escena. Asignar las referencias de `player` en `EnemySpawner`, `WaveManager`, `EnemyAI` del boss, etc.

- [ ] **Paso 5: Verificar flujo Level1 → Nivel 2**

Play Mode en nivel 1 → matar boss → click "Siguiente Nivel" → carga "Nivel 2" correctamente → spawnan los nuevos enemigos → boss del lejano oeste aparece con cinemática → las mecánicas funcionan igual.

---

## Notas para assets futuros

- **Barras UI**: Para swapear a PNG → seleccionar el componente `Image` de cada barra en el Inspector → cambiar el `Sprite`. Sin tocar código.
- **Badge del boss**: Reemplazar el `Image` background del BossIntoBadge con el PNG del badge cuando esté listo.
- **Nivel 2**: Assets de enemigos en `Assets/Data/Enemies/Level2/`. Swapear prefabs en `EnemySpawner`. Reasignar `bossData` en `BossController`. 3 nuevos AudioClips en `MusicManager`. Reposicionar `CinematicCamera`.
- **VictoryScreen PNG**: Reemplazar el panel placeholder con el PNG del frame cuando esté listo. Sin cambios de código.
- **Modelos de enemigos Level2**: Cambiar el prefab en cada `EnemySpawnEntry.prefab` — el código no sabe ni le importa qué modelo usa.
