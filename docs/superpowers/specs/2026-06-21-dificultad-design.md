# Selección de dificultad — Diseño

**Fecha:** 2026-06-21
**Estado:** Aprobado el enfoque; pendiente revisión del spec.

## Objetivo

Permitir elegir la dificultad al empezar una partida nueva. Al tocar **Nuevo
Juego**, antes del video del portal, aparece un panel con cuatro opciones:
**Fácil**, **Normal**, **Difícil** y **Pesadilla**. La elección modifica el
balance del nivel (daño recibido, vida, parry, transformación y spawn de
enemigos) y se guarda en la partida.

## Principios

- **Normal = línea base.** Multiplicador ×1.0 en todo; el juego se comporta
  exactamente como hoy.
- **No mutar assets ni valores serializados de forma permanente.** Cada sistema
  lee su multiplicador desde un holder estático y lo aplica sobre su valor base
  del Inspector, en su `Start`. Los ScriptableObject (`EnemyData`) no se tocan.
- **Spread "Marcado".** Las diferencias entre niveles son notorias.
- **Pesadilla = reto real.** Pensado para ser genuinamente brutal.

## Arquitectura

### `GameDifficulty` (nuevo, estático)

`Assets/Scripts/Systems/GameDifficulty.cs`

```csharp
public enum Difficulty { Facil = 0, Normal = 1, Dificil = 2, Pesadilla = 3 }

public static class GameDifficulty
{
    public static Difficulty Current = Difficulty.Normal;

    // Multiplicadores derivados de Current (tabla interna por dificultad):
    public static float EnemyDamageMult     { get; }   // daño que los enemigos le hacen al jugador
    public static float EnemyHpMult         { get; }
    public static float PlayerMaxHpMult     { get; }
    public static float ParryCooldownMult   { get; }
    public static float ParriesToTransformMult { get; }
    public static float SpawnCountMult      { get; }   // multiplica máx enemigos simultáneos
    public static float SpawnIntervalMult   { get; }   // multiplica intervalo (menor = más frecuente)

    // --- Boss ---
    public static float BossHpMult                { get; }   // multiplica maxHP del oso
    public static float BossVulnerableDurationMult{ get; }   // multiplica la ventana vulnerable (menor = más difícil)
    public static float BossVulnerableDamageMult  { get; }   // multiplica el vulnerableDamageMultiplier (menor = más difícil)
}
```

Implementación: una tabla `static readonly` indexada por `(int)Current` con un
struct de multiplicadores. Las propiedades devuelven el campo correspondiente.

### Tabla de balance

| Aspecto | Fácil | Normal | Difícil | Pesadilla |
|---|---|---|---|---|
| Daño de enemigos (`EnemyDamageMult`) | 0.55 | 1.0 | 1.8 | 3.0 |
| Vida de enemigos (`EnemyHpMult`) | 0.7 | 1.0 | 1.5 | 2.2 |
| Vida máx de Emilio (`PlayerMaxHpMult`) | 1.4 | 1.0 | 0.7 | 0.5 |
| Cooldown de parry (`ParryCooldownMult`) | 0.7 | 1.0 | 1.4 | 1.8 |
| Parries para transformar (`ParriesToTransformMult`) | 0.6 | 1.0 | 1.6 | 2.2 |
| Máx enemigos simultáneos (`SpawnCountMult`) | 0.6 | 1.0 | 1.8 | 2.8 |
| Intervalo de spawn (`SpawnIntervalMult`) | 1.4 | 1.0 | 0.6 | 0.4 |

Con `parriesToTransform` base = 5, el multiplicador da: **3 / 5 / 8 / 11**
(redondeado, mínimo 1).

### Tabla de balance — Boss (oso)

| Knob del boss | Fácil | Normal | Difícil | Pesadilla |
|---|---|---|---|---|
| HP del oso (`BossHpMult`) | 0.7 | 1.0 | 1.4 | 1.9 |
| Ventana vulnerable (`BossVulnerableDurationMult`) | 1.3 | 1.0 | 0.75 | 0.55 |
| Daño en ventana vulnerable (`BossVulnerableDamageMult`) | 1.2 | 1.0 | 0.8 | 0.65 |

El **daño que el oso le hace al jugador NO tiene knob propio**: pega vía
`PlayerHealth.TakeDamage`, así que ya escala con `EnemyDamageMult` (×3 en
Pesadilla).

### Tabla de balance — Buffs a Emilio (compensan la horda en dificultades altas)

| Buff | Fácil | Normal | Difícil | Pesadilla |
|---|---|---|---|---|
| Daño de ataque (`PlayerDamageMult`) | 1.0 | 1.0 | 1.2 | 1.4 |
| Rango de ataque (`PlayerAttackRangeMult`) | 1.0 | 1.0 | 1.15 | 1.3 |
| Daño del aura (`AuraDamageMult`) | 1.0 | 1.0 | 1.3 | 1.6 |
| Radio del aura (`AuraRadiusMult`) | 1.0 | 1.0 | 1.15 | 1.3 |

Se aplican en `PlayerCombat.Start` (`playerDamage`, `attackRange`) y
`TransformationMode.Awake` (`auraDamagePerTick`, `auraRadius`). En Fácil/Normal son
×1 (no cambian nada); solo Difícil/Pesadilla le dan a Emilio más pegada y alcance
para lidiar con más enemigos y más tanques.

**Fuera de alcance (no escalan):** duración del modo Freaky (fija), velocidad de
movimiento y cadencia de ataque del oso (fijas, para mantener legible la
coreografía de la pelea y el ritmo de parry).

## Puntos de aplicación (chokepoints)

Cada sistema aplica su multiplicador en su `Start` (o al inicializar), sobre el
valor base del Inspector. Salvo donde se indique, se usa `Mathf.RoundToInt` con
mínimo 1.

1. **Daño de enemigos → `PlayerHealth.TakeDamage(int damage)`**
   Escalar el daño entrante: `damage = Mathf.RoundToInt(damage * GameDifficulty.EnemyDamageMult)`.
   Único chokepoint: cubre `EnemyAI`, `SpiderAI` y `BossBearAI` (todos llaman a
   `PlayerHealth.TakeDamage`).

2. **Vida de enemigos → `EnemyHealth`**
   Hoy `currentHP = restoreHP >= 0 ? restoreHP : data.maxHP` y la barra usa
   `data.maxHP` como denominador. Se introduce un `maxHPEffective`:
   - Si `data.isBoss` → `maxHPEffective = data.maxHP` (sin escalar).
   - Si no → `maxHPEffective = RoundToInt(data.maxHP * EnemyHpMult)` (mín 1).
   - `currentHP` (cuando no es restore) parte de `maxHPEffective`.
   - `EnemyHealth.TakeDamage` usa `maxHPEffective` como denominador de la barra
     en vez de `data.maxHP`.
   Restaurar desde guardado (`restoreHP`) usa el HP guardado tal cual; el
   denominador de la barra usa `maxHPEffective` recalculado con la dificultad de
   la partida (que ya está restaurada antes de cargar la escena).

3. **Vida máx de Emilio → `PlayerHealth.Start`**
   `maxHealth = Mathf.Max(1, RoundToInt(maxHealth * PlayerMaxHpMult))` antes de
   setear `currentHealth = maxHealth` y el slider.

4. **Cooldown de parry → `PlayerCombat`**
   En `Start`, `parryCooldown *= GameDifficulty.ParryCooldownMult`.

5. **Parries para transformar → `TransformationMode`**
   En `Awake`, `parriesToTransform = Mathf.Max(1, RoundToInt(parriesToTransform * ParriesToTransformMult))`.
   `RestoreState` ya hace `Clamp(savedParryCount, 0, parriesToTransform)`, que
   queda consistente con el nuevo valor.

6. **Cantidad y frecuencia de spawn → `EnemySpawner` y `SpiderSpawner`**
   En `Start` (o al primer `StartSpawning`):
   - `maxEnemiesSimultaneous = Mathf.Max(1, RoundToInt(maxEnemiesSimultaneous * SpawnCountMult))`
   - `spawnInterval *= SpawnIntervalMult` (mínimo razonable, p.ej. 0.2s)
   En `SpiderSpawner`, lo mismo con `maxSpidersSimultaneous` y `spawnInterval`.

7. **Pelea con el boss → `BossBearHealth` (+ `BossController`)**
   En `BossBearHealth.Start`:
   - `maxHPEffective = Mathf.Max(1, RoundToInt(data.maxHP * BossHpMult))`.
     `currentHP` (cuando no es restore) parte de `maxHPEffective`. La barra de
     vida usa `maxHPEffective` como denominador en `TakeDamage` (en vez de
     `data.maxHP`).
   - `vulnerableDuration *= BossVulnerableDurationMult`.
   - `vulnerableDamageMultiplier *= BossVulnerableDamageMult` (se mantiene el
     `Min(1)` para que nunca baje de 1).
   `BossBearHealth` expone `MaxHP => maxHPEffective`. `BossController.MaxHP` (que
   hoy devuelve `bossData.maxHP` y lo consume la HUD para la barra del boss) pasa
   a leer el max efectivo del componente de vida (`bearHealth.MaxHP` /
   `enemyHealth.MaxHPEffective`), para que la barra no quede inconsistente al
   escalar el HP.
   El daño del oso al jugador (`BossBearAI.DealDamage` → `PlayerHealth.TakeDamage`)
   ya queda cubierto por el chokepoint 1, sin cambios en `BossBearAI`.
   Restaurar (`restoreHP`/`ActivateRestored`) usa el HP guardado tal cual; el
   denominador de la barra usa `maxHPEffective` recalculado con la dificultad de
   la partida (ya restaurada antes de cargar la escena).

## Flujo de UI

`MainMenuController`:

- **`NuevoJuego()`** deja de entrar directo al portal. Ahora abre el
  `difficultyPanel` (nuevo GameObject en la escena MainMenu).
- El panel tiene 4 botones (Fácil/Normal/Difícil/Pesadilla) + **Volver**.
- Cada botón llama a un método nuevo `ElegirDificultad(int nivel)`:
  1. `GameDifficulty.Current = (Difficulty)nivel;`
  2. Borra `LastSaveSlot` (como hoy hace `NuevoJuego`).
  3. Llama a `EnterGame(gameSceneName)` (portal → nivel).
- **Volver** cierra el panel y vuelve al menú (resetea `_isLoading` si hiciera
  falta).
- **`Continuar()` y `CargarSlot()` NO muestran el panel.** Antes de cargar la
  escena setean `GameDifficulty.Current = (Difficulty)save.difficulty;`.

El panel se construye en la escena MainMenu reutilizando el estilo de los
botones existentes (mismo prefab/tipografía/hover). Visualmente: título "ELEGÍ
LA DIFICULTAD" + los 4 botones en columna + Volver. Pesadilla puede destacarse
con color (rojo) para señalar que es el reto extremo.

## Persistencia

- `SaveData`: nuevo campo `public int difficulty = 1;` (default 1 = Normal, para
  que saves viejos sin el campo caigan en Normal vía el inicializador de campo
  que `JsonUtility.FromJson` respeta cuando la clave no está presente).
- `GameSnapshot.Capture()`: `data.difficulty = (int)GameDifficulty.Current;`.
- Carga: `GameDifficulty.Current` se setea **en el menú** antes de cargar la
  escena (en `Continuar()`/`CargarSlot()`), nunca en `GameLoader`. Así está
  disponible cuando los `Start` de spawners/combate/jugador corren en el nivel.
  Para Nuevo Juego, se setea en `ElegirDificultad`. Regla única: **la dificultad
  siempre queda fijada antes de que cargue la escena de gameplay.**

## Orden de inicialización (riesgo y mitigación)

Los sistemas leen `GameDifficulty.Current` en sus `Start/Awake` dentro del
nivel. Como `Current` se fija en el menú (antes del `LoadScene`), está
garantizado disponible. El default estático (`Normal`) cubre el caso de entrar
al nivel directo desde el editor sin pasar por el menú.

## Archivos

**Nuevo:**
- `Assets/Scripts/Systems/GameDifficulty.cs`

**Modificados:**
- `Assets/Scripts/UI/MainMenuController.cs` — panel + `ElegirDificultad`, set en Continuar/CargarSlot
- `Assets/Scripts/PlayerHealth.cs` — escala maxHealth y daño entrante
- `Assets/Scripts/Player/PlayerCombat.cs` — escala parryCooldown
- `Assets/Scripts/Player/TransformationMode.cs` — escala parriesToTransform
- `Assets/Scripts/Enemy/enemySpawner.cs` — escala count/interval
- `Assets/Scripts/Enemy/SpiderSpawner.cs` — escala count/interval
- `Assets/Scripts/Enemy/EnemyHealth.cs` — maxHPEffective con EnemyHpMult (expone MaxHPEffective)
- `Assets/Scripts/Enemy/BossBearHealth.cs` — HP, ventana vulnerable y daño vulnerable escalados (expone MaxHP)
- `Assets/Scripts/Enemy/BossController.cs` — MaxHP lee el max efectivo del componente de vida
- `Assets/Scripts/SaveSystem/SaveData.cs` — campo difficulty
- `Assets/Scripts/SaveSystem/GameSnapshot.cs` — captura difficulty

**Escena:**
- `Assets/Scenes/MainMenu` (o equivalente) — panel de dificultad con 4 botones + Volver

## Pesadilla: horda de ositos minion (IMPLEMENTADO)

En vez de "dos osos" (que era un refactor transversal de cinemática/HUD/guardado),
en Pesadilla el boss tiene **minions**: ositos chiquitos que salen en horda durante
la pelea para agobiar y entorpecer (se meten en el cono de ataque y los parries).

- `BossMinionSpawner` (`Assets/Scripts/Enemy/BossMinionSpawner.cs`): arranca con
  `BossController.OnBossReady` **solo si** `GameDifficulty.Current == Pesadilla`;
  para + limpia con `BossBearHealth.OnBossDied`. Spawnea en cualquier punto del
  mapa (mismas `mapBounds` que los otros spawners). Horda media: `maxAlive=8`,
  `spawnInterval=2.5`.
- Minion = prefab `Assets/Prefabs/BearMinion.prefab`: clon del oso del boss
  (`Bear_4`, animator `BearBossAnimator`, CapsuleCollider, layer Enemy) pero con
  `EnemyAI`/`EnemyHealth` (enemigo normal, NO los scripts de boss). Data
  `Assets/Data/Enemies/Enemy_BearMinion.asset` ("Osito": HP 28, dmg 12, spd 3.6,
  modelScale 0.9). Caveat: el animator no tiene el bool `isAttacking` de EnemyAI,
  así que camina y daña pero no reproduce zarpazo (suficiente para la jam).
- Caveat futuro: la transformación está bloqueada durante la pelea del boss, así
  que parrear ositos no acumula Freaky (sí los stunea, da aire). Los ositos
  reciben los multiplicadores de Pesadilla (vida/daño) como cualquier EnemyAI.

## Futuro (fuera de alcance de esta iteración)

- **Animator propio del osito** con animación de ataque (zarpazo).
- **Video de portal por dificultad.** Más adelante, asignar a cada dificultad su
  propia versión del video del portal (mismo video con el portal de otro color;
  p.ej. Pesadilla todo rojo). `PortalTransition` recibiría el clip según
  `GameDifficulty.Current` antes de reproducir. No se implementa ahora.

## Criterios de éxito

- Nuevo Juego muestra el panel antes del portal; elegir un nivel entra al juego
  con ese balance.
- Normal se comporta idéntico al juego actual.
- Pesadilla es marcadamente más difícil (más enemigos, más rápido, más daño,
  menos vida, transformación más lejana).
- La pelea con el boss escala: en dificultades altas el oso aguanta más, da
  menos ventana para castigarlo y recibe menos daño por golpe; la barra de vida
  del boss en la HUD muestra el porcentaje correcto con el HP escalado.
- Continuar/cargar slot retoman con la dificultad con la que se guardó.
- Saves viejos (sin campo difficulty) cargan como Normal.
