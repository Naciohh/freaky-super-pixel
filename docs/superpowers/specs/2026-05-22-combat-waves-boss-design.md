# Combat, Waves & Boss System — Design Spec
**Fecha:** 2026-05-22  
**Proyecto:** Freaky Super Pixel 3D Universal  
**Estado:** Aprobado

---

## Resumen

Se agrega al juego un sistema completo de combate para Emilio, oleadas de enemigos con timer, modo transformación por parries, y una fase de boss con cinemática de introducción. El sistema está diseñado para funcionar en múltiples niveles (supermercado + futuro nivel 2) cambiando únicamente los ScriptableObjects de configuración, sin modificar código.

---

## 1. Arquitectura General

Comunicación entre sistemas vía eventos C# (`System.Action`). Sin referencias cruzadas directas.

| Script | Tipo | Responsabilidad |
|---|---|---|
| `EnemyData` | ScriptableObject | Stats del enemigo: HP, daño, velocidad, escala del modelo |
| `EnemyAI` | MonoBehaviour (refactor) | Siempre persigue al jugador. Lee stats desde `EnemyData`. Sin detection radius. |
| `EnemyHealth` | MonoBehaviour (nuevo) | HP del enemigo, recibe daño, dispara `OnDeath` |
| `PlayerCombat` | MonoBehaviour (nuevo) | Click izq → ataque. Click der → parry. Comunica parry exitoso a `TransformationMode` |
| `TransformationMode` | MonoBehaviour (nuevo) | Barra de carga por parries. Activa modo transformación (colisión mata). Timer. |
| `WaveManager` | MonoBehaviour (nuevo) | Timer de oleada. Eventos: `OnWaveEnd`. Coordina transición al boss. |
| `EnemySpawner` | MonoBehaviour (nuevo) | Spawn random dentro del mapa. Respeta máximo simultáneo y distancia mínima al jugador. |
| `BossController` | MonoBehaviour (nuevo) | Extiende lógica de EnemyAI. Usa `EnemyData` del boss. Activa cinemática al aparecer. |
| `BossIntroDirector` | MonoBehaviour (nuevo) | Maneja la cinemática de introducción del boss (cámara, badge, timer). |
| `GameHUD` | MonoBehaviour (nuevo) | Controla toda la UI en juego: barras, timer, badge del boss. |

---

## 2. ScriptableObjects — EnemyData

```
Assets/Data/Enemies/
  Enemy_Small.asset     → daño bajo,   HP bajo,   velocidad alta,  escala 0.7
  Enemy_Medium.asset    → daño medio,  HP medio,  velocidad media, escala 1.0
  Enemy_Large.asset     → daño alto,   HP alto,   velocidad baja,  escala 1.4
  Boss_Supermarket.asset → daño muy alto, HP muy alto, velocidad media, escala 2.0
```

**Campos de EnemyData:**
- `int maxHP`
- `int damage`
- `float moveSpeed`
- `float modelScale`
- `string displayName` (usado por el badge del boss)
- `bool isBoss`

Cuando llegue el Nivel 2: solo se crean nuevos assets con diferentes valores y prefabs. Sin tocar código.

---

## 3. Sistema de Oleadas

**WaveManager** (configurable desde Inspector):
- `waveDuration = 90f` — duración de la oleada en segundos
- Timer cuenta regresiva mostrada en HUD
- Al llegar a 0: dispara `OnWaveEnd` → `EnemySpawner` deja de spawnear → todos los enemigos normales activos se destruyen → `BossController` instancia el boss

**EnemySpawner** (configurable desde Inspector):
- `maxEnemiesSimultaneous = 15`
- `spawnInterval = 3f` — segundos entre cada spawn
- `minDistanceFromPlayer = 5f` — el spawn point random se reintenta si está muy cerca del jugador
- `spawnAreaCenter` + `spawnAreaSize` — define el área de spawn como bounds en el mapa
- Distribución de tipos: `spawnWeights[]` — array de pesos por tipo (ej: Small=60, Medium=30, Large=10)
- Al alcanzar el máximo simultáneo, el spawner pausa hasta que muera algún enemigo

---

## 4. EnemyAI — Refactor

Cambios respecto al script actual:
- **Eliminar** `detectionDistance` — el enemigo siempre persigue al jugador
- **Leer** `moveSpeed` y `damage` desde `EnemyData` en lugar de valores hardcodeados
- **Aplicar** `modelScale` al `transform.localScale` en `Start()`
- **Delegar** el HP a `EnemyHealth` — EnemyAI no maneja muerte

---

## 5. Sistema de Combate del Jugador

### PlayerCombat

**Click izquierdo — Ataque:**
- `Physics.OverlapSphere` en frente del jugador, radio `attackRange = 1.5f`
- Aplica `playerDamage = 34` a todos los `EnemyHealth` en rango
- Cooldown `attackCooldown = 0.6f`
- Dispara animación de ataque

**Click derecho — Parry:**
- Abre ventana de parry durante `parryWindow = 0.4f` segundos
- Si un enemigo está en estado "atacando" (`EnemyAI.isAttacking == true`) dentro de `parryRange = 2.5f`:
  - El daño entrante se cancela
  - Se notifica a `TransformationMode.OnParrySuccess()`
- Si no hay ataque entrante: no pasa nada (sin penalización)

### TransformationMode

- `parriesToTransform = 5` — parries exitosos para llenar la barra
- Al llenarse: activa modo transformación automáticamente
- **Modo transformación:**
  - Duración: `transformDuration = 8f` segundos
  - Emilio mata enemigos normales por `OnTriggerEnter` (colisión directa)
  - No puede parry ni atacar durante la transformación
  - La barra se vacía gradualmente con el tiempo (no por eventos)
  - Al terminar: vuelve al estado normal, barra en 0

---

## 6. Boss

### BossController

- Usa `EnemyData` del boss (HP muy alto, daño muy alto)
- Hereda el comportamiento de persecución de EnemyAI
- El tipo de ataque del boss es configurable por nivel (campo `BossAttackType` enum en `EnemyData`)
- Por ahora: `BossAttackType.Melee` — mismo sistema de ataque cuerpo a cuerpo pero con más daño y cooldown más corto

### Cinemática de Introducción (BossIntroDirector)

**Secuencia al spawnar el boss:**
1. Juego se pausa (`Time.timeScale = 0`, excepto la cinemática)
2. Cámara principal se desactiva
3. Cámara cinemática se activa — posición fija apuntando al boss de frente, ángulo bajo y dramático
4. Boss reproduce animación idle (IA desactivada durante la cinemática)
5. Badge con nombre del boss aparece con fade-in (0.5s) — texto `TMP_Text` + fondo PNG (placeholder blanco hasta tener el asset)
6. Duración total de la cinemática: `introDuration = 3f` segundos
7. Badge hace fade-out, cámara principal se reactiva, juego se reanuda, IA del boss se activa

**Posición de la cámara cinemática:** configurable desde el Inspector, se coloca manualmente en la escena para cada nivel.

---

## 7. UI / HUD

Todas las barras usan `Image.fillAmount` — fácil de swapear por assets PNG cuando estén listos.

| Elemento | Posición | Visible cuando |
|---|---|---|
| Barra de vida jugador | Abajo izquierda | Siempre |
| Barra de transformación | Abajo izquierda, bajo vida | Siempre |
| Timer de oleada | Arriba centro | Durante oleada (se oculta al aparecer boss) |
| Barra de vida boss | Arriba centro | Solo durante fase boss |
| Badge nombre boss | Centro pantalla | Solo durante cinemática intro |

**Nota sobre assets PNG futuros:** cada barra es un `Image` component independiente. Para swapear: reemplazar el `Sprite` del componente. No requiere cambios de código.

---

## 8. Flujo General de la Partida

```
Inicio de nivel
    ↓
WaveManager inicia timer (90s)
    ↓
EnemySpawner spawna enemigos continuamente (hasta max 15)
    ↓
Jugador combate: ataca y hace parry → llena barra de transformación
    ↓
[Opcional] Transformación activa → mata por colisión por 8s
    ↓
Timer llega a 0
    ↓
Enemigos normales destruidos → BossIntroDirector inicia cinemática
    ↓
Cinemática: cámara boss + badge con nombre (3s)
    ↓
Boss activo → combate final
    ↓
[TBD] Condición de victoria/derrota post-boss
```

---

## 9. Sistema de Música

3 tracks por nivel, manejadas por un `MusicManager` (singleton):

| Track | Cuándo suena |
|---|---|
| `explorationClip` | Al iniciar el nivel, antes de que arranque la oleada |
| `fightClip` | Cuando `WaveManager` inicia el timer (oleada activa) |
| `bossClip` | Cuando termina la cinemática de intro del boss y su IA se activa |

- Transiciones con crossfade suave (`fadeDuration = 1.5f`)
- Cada nivel tiene su propio set de 3 clips configurado en el `MusicManager` o en un ScriptableObject de nivel
- Los clips de audio se asignan desde el Inspector — fácil de swapear por nivel

---

## 10. Lo que queda fuera de este spec (para más adelante)

- Animaciones de ataque/muerte del jugador y enemigos
- Efectos de partículas (muerte enemigo, transformación)
- Sonidos (SFX de golpes, parry, muerte)
- Condición de victoria post-boss y pantalla de fin de nivel
- Assets finales de UI (PNG de barras, badge del boss)
- Niveles 2+ (solo requieren nuevos EnemyData assets y posición de cámara cinemática)
- 2-3 tipos de ataque distintos del boss según nivel
