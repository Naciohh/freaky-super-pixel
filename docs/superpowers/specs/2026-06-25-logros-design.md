# Sistema de Logros — Diseño

Fecha: 2026-06-25
Estado: aprobado para planificar

## Objetivo

Sistema de logros que:
1. Muestra un **banner animado** (video) cuando desbloqueás un logro en partida.
2. Tiene un **popup en Extras** (lobby) con la grilla de todos los logros, los desbloqueados y los que faltan.
3. Persiste los desbloqueos a nivel **perfil** (no por slot de guardado): una vez que lo sacaste, queda para siempre.

## Logros (6)

| ID (enum) | Nombre | Condición | PNG | Video |
|-----------|--------|-----------|-----|-------|
| `DeCompras` | De compras | Bienvenida al entrar al Nivel 01 (Mercadito) | `LOGRO_mercadito.png` | `logro_nivel1.mov` |
| `ControlDePlagas` | Control de plagas | Matar 10 arañas en Mercadito | `LOGRO_arañas.png` | `logro_arañas.mov` |
| `SepultureroEnRacha` | Sepulturero en racha | Matar 15 esqueletos en Mercadito | `LOGRO_esqueleto.png` | `logro_esqueletos.mov` |
| `UnOsoWacho` | Un oso wacho | Llegar al jefe del stage (cinemática del boss) | `LOGRO_oso.png` | `logro_oso.mov` |
| `ElFamosoEasterEgg` | El famoso easter egg | Descubrir el objeto secreto en Mercadito | `LOGRO_easteregg.png` | `logro_easteregg.mov` |
| `AxelElCapo` | Axel el capo | Terminar Mercadito sin recibir daño (no-hit) | `LOGRO_axel.png` | `logro_axel.mov` |

> Nota clave: cada `LOGRO_*.png` ya trae avatar + nombre + descripción horneados en la imagen.
> No hay que componer texto ni avatar por separado: la celda de la grilla **es** el PNG.

## Assets

Origen (fuera del proyecto): `C:\Users\juani\Desktop\UI juego\últ ui\Logros`
Destino: `Assets/Achievements/`
- `Assets/Achievements/Sprites/` — los 6 `LOGRO_*.png` (import como Sprite, UI) + `LOGROS_contenedor.png` (fondo del popup).
- `Assets/Achievements/Videos/` — los 6 `logro_*.mov`.

### Caveat de video (.mov)
Unity suele renegar con `.mov` según el codec (ya pasó con el portal CineForm→H.264).
Antes de importar, transcodificar cada `.mov` a `.mp4` H.264 + AAC con ffmpeg
(mismo flujo que `PortalTransition`). Si algún `.mov` ya es H.264, se importa tal cual.

## Arquitectura

### `AchievementId` (enum)
Los 6 ids de la tabla.

### `AchievementDefinition` (ScriptableObject)
Un asset por logro. Campos:
- `AchievementId id`
- `string displayName`
- `Sprite image` (el `LOGRO_*.png`)
- `VideoClip video` (el banner animado, ya transcodificado)

Se crea un `AchievementDatabase` (ScriptableObject con `List<AchievementDefinition>`)
para que el manager y la UI tengan una sola fuente de verdad y orden de grilla.

### `AchievementManager` (singleton, persistente entre escenas)
- Carga/guarda el estado desde `Application.persistentDataPath/achievements.json`.
  Formato: lista de ids desbloqueados + contadores (`spidersKilled`, `skeletonsKilled`).
- API:
  - `bool IsUnlocked(AchievementId id)`
  - `void Unlock(AchievementId id)` — si ya estaba, no hace nada; si es nuevo, marca,
    persiste y dispara `OnUnlocked(AchievementDefinition)`.
  - `void AddSpiderKill()` / `void AddSkeletonKill()` — incrementan contador, persisten,
    y disparan el `Unlock` correspondiente al llegar al umbral (10 / 15).
- `event Action<AchievementDefinition> OnUnlocked` — lo escucha el banner.
- Contadores de arañas/esqueletos son **acumulativos del perfil** salvo que se decida
  reiniciarlos por partida (ver "Decisión abierta" abajo). Por defecto: acumulativos.

### `AchievementTriggers` (MonoBehaviour, en cada escena de nivel)
Traduce eventos del juego a llamadas al manager. Engancha en `OnEnable`, desengancha en `OnDisable`:
- **De compras**: al cargar/entrar a Mercadito (Nivel01) → `Unlock(DeCompras)`.
  Engancha donde dispara `LevelIntroDirector`, o en el `Start` de la escena Nivel01.
- **Control de plagas / Sepulturero**: `EnemyHealth.OnAnyEnemyDied` → según el tipo de
  enemigo, `AddSpiderKill()` o `AddSkeletonKill()`.
  - Distinción de tipo: por `EnemyData.displayName` (p.ej. contiene "Araña"/"Spider" vs
    "Esqueleto"/"Skeleton"). **A verificar en implementación**: confirmar que las arañas
    mueren vía `EnemyHealth.OnAnyEnemyDied` con un `EnemyData` identificable; si las arañas
    usan `SpiderAI` sin `EnemyHealth/EnemyData`, se engancha la muerte de `SpiderAI` aparte.
- **Un oso wacho**: al arrancar `BossIntroDirector` → `Unlock(UnOsoWacho)`.
- **Axel el capo (no-hit)**: marcar una bandera `tookDamage` cuando `PlayerHealth.TakeDamage`
  se llama durante el nivel; al completar el stage (victoria), si `!tookDamage` → `Unlock(AxelElCapo)`.
  Engancha en el evento/flujo de victoria existente (`VictoryScreen` / `BossDefeatPortal`).
- **El famoso easter egg**: trigger nuevo en Mercadito. Reusar `InteractionStation` (o un
  collider `OnTriggerEnter` del jugador) en el objeto secreto → `Unlock(ElFamosoEasterEgg)`.
  **A definir con el usuario**: qué objeto y dónde (ver "Decisión abierta").

### `AchievementBanner` (prefab + script, in-game)
- Canvas overlay propio (o se suma al HUD), oculto por defecto.
- Suscrito a `AchievementManager.OnUnlocked`.
- Al desbloquear: encola el logro y reproduce su `VideoClip` (el `.mov` transcodificado)
  con un `VideoPlayer` renderizando a una `RawImage`. Aparece, se reproduce, se va.
- **Cola**: si caen dos juntos, se muestran de a uno (no se pisan).
- No usa el PNG ni texto extra: la animación del video ya es el banner completo.

### `AchievementsPanelUI` (popup de Extras, lobby)
- Se abre desde la estación/botón de Extras del lobby. Usa `LobbyModal.Open()`/`Close()`
  para congelar a Emilio mientras está abierto (igual que los otros popups).
- Layout exacto según `LOGROS_contenedor.png`:
  - Fondo = `LOGROS_contenedor.png` (la hoja rasgada con el título "LOGROS").
  - **Grilla 2 columnas × 3 filas** con los 6 PNGs, posicionada sobre los slots del fondo.
  - Como todos los `LOGRO_*.png` son del mismo alto/ancho, se usa un `GridLayoutGroup`
    (2 columnas, cell size + spacing + padding calibrados para caer sobre los slots del arte).
  - **Scrolleable** (ScrollRect vertical): el content crece con los logros futuros; los que
    no entran en el viewport quedan ocultos hasta que el usuario baja. El fondo contenedor
    actúa como marco/viewport.
- **Estado bloqueado vs desbloqueado** por celda:
  - Desbloqueado: se muestra el `LOGRO_*.png` normal (con su descripción ya horneada).
  - Bloqueado: el mismo PNG oscurecido/desaturado (tint gris + alpha bajo) para que se note
    que hay algo pero no se lea el detalle. (Sin panel de detalle aparte: no hace falta,
    el PNG ya trae todo.)
- Sin panel de detalle inferior. La especificación original de "detalle debajo de la grilla"
  queda **descartada** a pedido del usuario.

## Flujo de datos

```
Evento de juego ──► AchievementTriggers ──► AchievementManager.Unlock(id)
                                                   │
                                                   ├─► persiste achievements.json
                                                   └─► OnUnlocked(def) ──► AchievementBanner (video)

Extras (lobby) ──► AchievementsPanelUI ──► AchievementManager.IsUnlocked(id) por celda
```

## Persistencia

- Archivo: `Application.persistentDataPath/achievements.json`.
- Independiente de los slots de `SaveManager` (los logros son del perfil, no de la partida).
- Contenido: `{ unlocked: [ids...], spidersKilled: int, skeletonsKilled: int }`.

## Orden de implementación

1. **Importar assets**: transcodificar `.mov`→`.mp4` H.264, copiar a `Assets/Achievements/`,
   importar PNGs como Sprite.
2. **Núcleo de datos**: `AchievementId`, `AchievementDefinition`, `AchievementDatabase`,
   `AchievementManager` (con persistencia). Probar con logs.
3. **Triggers**: conectar uno por uno y verificar cada desbloqueo con logs.
4. **Banner in-game**: prefab con `VideoPlayer` + cola, suscrito a `OnUnlocked`.
5. **Popup de Extras**: grilla 2×3 scrolleable sobre el contenedor + estado bloqueado/gris.

## Decisiones abiertas (a confirmar con el usuario antes/durante implementación)

1. **Easter egg**: qué objeto secreto es y dónde se coloca en Mercadito (¿ya existe en escena
   o hay que crearlo?). Se reusa `InteractionStation` o un collider trigger.
2. **Contadores de kills**: ¿acumulativos del perfil (default) o por partida? Default elegido:
   acumulativos, porque el logro es "derrota a 10 arañas en Mercadito" sin exigir que sea en una
   sola corrida. Si se quiere "en una sola partida", se reinicia el contador al empezar nivel.
```
