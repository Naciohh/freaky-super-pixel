# Pulido de la cinemática de aparición del boss

Fecha: 2026-06-19

## Objetivo

Mejorar la cinemática de aparición del boss (el oso) para que:

1. La barra de vida del boss **no** aparezca durante la cinemática; debe aparecer
   recién cuando la cinemática termina.
2. El nombre "jefe de super" (texto) se reemplace por el logo **EL OSO**
   (`Assets/UI/HUD/jefe_titulo.png`), apareciendo con una animación de "pop"
   (de chiquito a grande) + fade-in.
3. La cámara se quede enfocando al boss unos segundos (~6s total).
4. El boss reproduzca su animación idle y pegue **un** golpe durante la cinemática
   (solo visual, sin dañar al player).

## Estado actual

- `GameHUD.OnWaveEnd()` ([Assets/Scripts/UI/GameHUD.cs](../../../Assets/Scripts/UI/GameHUD.cs))
  prende `bossHealthBarGroup` apenas termina la oleada — justo cuando arranca la
  cinemática. Esa es la causa de que la barra se vea durante la intro.
- `BossIntroDirector.IntroSequence()`
  ([Assets/Scripts/UI/BossIntroDirector.cs](../../../Assets/Scripts/UI/BossIntroDirector.cs))
  hace un travelling de cámara de `introDuration` (3s), muestra un badge con
  `bossNameText` (= "jefe de super") con fade in/out, y al final llama a
  `currentBoss.OnIntroComplete()`.
- `BossController.OnIntroComplete()`
  ([Assets/Scripts/Enemy/BossController.cs](../../../Assets/Scripts/Enemy/BossController.cs))
  activa la IA (`bearAI.isAIActive = true`) y arranca la música del boss.
- El boss está activo durante la intro pero con la IA apagada, así que se queda
  en idle. El Animator está en el mismo GameObject que `BossBearAI` y tiene el
  trigger `"Attack"`.

## Diseño

### 1. Barra de vida al terminar la cinemática

- En `GameHUD`: quitar `bossHealthBarGroup.SetActive(true)` de `OnWaveEnd()`.
  En `OnWaveEnd()` solo se busca/cachea el `BossController` (para tener la
  referencia), pero la barra queda oculta.
- `BossController` expone un evento estático `public static event Action OnBossReady`.
  Se dispara dentro de `OnIntroComplete()` (cubre tanto el camino de cinemática
  como el de guardado restaurado `ActivateRestored`).
- `GameHUD` se suscribe a `BossController.OnBossReady` en `OnEnable` y, al
  recibirlo, hace `bossHealthBarGroup.SetActive(true)`. Desuscribe en `OnDisable`.

### 2. Logo EL OSO con pop en vez del texto

- En `BossIntroDirector` agregar campo `public Image bossBadgeImage`.
- El sprite se asigna en escena (logo `jefe_titulo.png`). `bossNameText` queda
  opcional (si está, se oculta/ignora).
- Animación del badge durante la intro:
  - Fade-in vía `badgeCanvasGroup.alpha` (como hoy).
  - "Pop": `bossBadgeImage.transform.localScale` interpola de `0.3` a `1.0`
    con un ease-out-back (overshoot suave) en ~0.45s al inicio.

### 3. Cámara con dwell + idle + un golpe (~6s)

Reestructurar `IntroSequence()` en fases con tiempos configurables
(`[SerializeField]`), usando `Time.unscaledDeltaTime` (la intro corre con
`Time.timeScale = 0`):

| Fase | Duración aprox | Qué pasa |
|------|----------------|----------|
| Travelling | 2.0s | Cámara viaja de la principal al encuadre del boss; el logo hace pop + fade-in en los primeros ~0.45s |
| Golpe | (instante a ~2.2s) | `BossController.TriggerIntroAttack()` → `anim.SetTrigger("Attack")` una sola vez (sin daño) |
| Dwell | ~2.8s | La cámara se queda enfocando al boss en idle; el logo en escala completa |
| Salida | ~1.0s | Fade-out del logo |
| Fin | — | Corta a cámara principal → `OnIntroComplete()` → `OnBossReady` (barra) + IA + música |

- `BossController.TriggerIntroAttack()`: obtiene el `Animator` (mismo GameObject)
  y hace `anim.SetTrigger("Attack")`. No invoca daño (no usa el `DealDamage` de
  `BossBearAI`), es puramente visual.

## Cableado (Unity, vía MCP)

- Importar `Assets/UI/HUD/jefe_titulo.png` como **Sprite (2D and UI)**.
- En la escena `Nivel01`, dentro del badge del `BossIntroDirector`, crear/usar un
  `Image` con el sprite del logo y asignarlo al campo `bossBadgeImage`.
- Verificar que `GameHUD.bossHealthBarGroup` siga referenciado (no se toca su
  asignación, solo el momento en que se prende).

## Fuera de alcance

- No se cambia el sistema de parry/vulnerabilidad del boss.
- No se cambia la lógica de daño ni el balance.
- No se agregan golpes múltiples ni VFX nuevos (solo el golpe único existente).

## Criterios de aceptación

- Durante la cinemática la barra de vida del boss **no** se ve.
- Al terminar la cinemática, la barra aparece.
- Se ve el logo EL OSO creciendo de chiquito a grande con fade-in.
- La cámara queda enfocando al boss varios segundos.
- El boss pega un golpe (animación) durante la cinemática, sin dañar al player.
