# UI: barras de vida, feedback de parry y Game Over

Fecha: 2026-06-18
Estado: aprobado por el usuario (implementación en curso)

## Contexto

Los sistemas de juego ya existen y funcionan; falta "vestirlos" con el arte
nuevo provisto por el usuario. Escena de juego: `Nivel01` (también `Nivel 2`).
HUD en `GameHUD_Canvas` con `GameHUD.cs` ya cableado:
`PlayerHealthBar`, `TransformBar`, `WaveTimerText`, `BossHealthBarGroup>BossHealthBar`,
`BossBadge`, `VictoryPanel`.

Assets de origen (escritorio del usuario):
- `Barras de vida/emilio_barras.png` — marco HUD jugador (retrato circular + 2 ranuras)
- `Barras de vida/barra_jefe.png` — marco barra del jefe ("UN OSO WACHO", fuego)
- `efectos pelea/parry_hit.png` — cartel "PARRY"
- `efectos pelea/ruido parry.mpeg` — sonido del parry (convertir a .wav con ffmpeg)
- `UI game over/game_over.png` — cartel "CADUCASTE PA"
- `UI game over/reintentar_boton.png` — botón REINTENTAR
- `UI game over/salir_menu.png` — botón SALIR

## Diseño

### 1. Marco del jugador (`emilio_barras.png`)
Image de fondo anclada arriba-izquierda en `GameHUD_Canvas`. Se reposicionan los
fills existentes (`PlayerHealthBar`, `TransformBar`) para alinear dentro de las
ranuras del marco. No se toca la lógica de `GameHUD.cs`.

### 2. Marco del jefe (`barra_jefe.png`)
Marco sobre `BossHealthBarGroup`, fill rojo adentro. Se desactiva
`BossBadge`/`BossNameText` porque el nombre ya está pintado en la imagen.

### 3. Parry (`parry_hit.png` + audio)
Nuevo `ParryFeedback.cs` que escucha `PlayerCombat.OnParrySuccess`:
muestra Image `ParryPopup` (~0.4s, pop de escala) + reproduce AudioClip vía
AudioSource. El .mpeg se convierte a `parry_hit.wav` con ffmpeg.

### 4. Game Over (3 sprites)
`GameOverScreen.cs` se sigue construyendo en runtime pero carga los sprites desde
`Assets/Resources/UI/` (cartel + 2 botones imagen). Sin cableado en escena.

## Importación
- `Assets/UI/HUD/` → PNG de barras y parry (Sprite, 2D and UI).
- `Assets/Resources/UI/` → sprites de game over + `parry_hit.wav`.

## Alcance / YAGNI
Sin sistemas nuevos. Solo importar arte + reposicionar/skinear UI existente y un
componente chico de feedback de parry.
