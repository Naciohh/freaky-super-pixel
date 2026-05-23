# Diseño: Pantalla de Menú Principal — Freaky Super Mesh

**Fecha:** 2026-05-15
**Proyecto:** Freaky Super Pixel 3d universal (Unity 6.4, URP)
**Equipo:** Equipo Dinamita

## Objetivo

Crear la pantalla de menú principal del juego siguiendo la referencia visual aprobada: logo "FREAKY SUPER MESH" a la izquierda, render del científico animado a la derecha sobre fondo de laboratorio, lista vertical de opciones de menú con resaltado púrpura en la opción seleccionada, y música de fondo.

## Decisiones tomadas

| Decisión | Elección |
|---|---|
| Arquitectura | Escena separada `MainMenu.unity` |
| Visual del personaje | Modelo 3D real renderizado vía RenderTexture |
| Botones funcionales | `Nuevo Juego` (carga shopping) + `Salir` (cierra app) |
| Botones visuales (placeholder) | `Cargar Partida`, `Opciones`, `Extras` |
| Música | `Laboratorio_Principal_Incidente_KLICKAUD.mp3`, loop, volumen ~0.6 |
| Transición al jugar | Fade-out de música ~0.5s antes de cargar `SampleScene` |
| SFX UI | Hooks preparados pero clips vacíos para llenar después |

## Estructura de la escena

```
MainMenu (Scene)
├── UICamera                       # Solid Color negro/gris, Culling Mask = UI
├── EventSystem                    # Auto-creado al agregar el primer Canvas
├── AudioManager                   # Contenedor de audio
│   ├── BGMSource (AudioSource)    # clip = Laboratorio_Principal_Incidente, loop, vol 0.6
│   └── SFXSource (AudioSource)    # vacío por ahora, hook para SFX UI
├── CharacterStage                 # Contenedor del setup 3D
│   ├── CharacterCamera            # Target Texture = MenuCharacter.renderTexture
│   ├── KeyLight (Directional)     # Luz dramática frontal-derecha
│   ├── FillLight (Point, opc.)    # Suaviza sombras desde la izquierda
│   └── Emilio                     # Modelo del científico con Animator (idle)
└── Canvas (Screen Space - Overlay)
    ├── BackgroundImage            # Imagen del laboratorio
    ├── CharacterDisplay           # RawImage con MenuCharacter.renderTexture
    ├── LogoImage                  # PNG del logo "FREAKY SUPER MESH"
    ├── CreditsLabel ("DESARROLLADO POR")
    ├── CreditsTeam ("EQUIPO DINAMITA")
    └── MenuPanel (VerticalLayoutGroup)
        ├── BtnNuevoJuego          # → MainMenuController.NuevoJuego()
        ├── BtnCargarPartida       # → MostrarProximamente()
        ├── BtnOpciones            # → MostrarProximamente()
        ├── BtnExtras              # → MostrarProximamente()
        └── BtnSalir               # → Salir()
```

## Assets a organizar

Crear las siguientes carpetas dentro de `Assets/`:

```
Assets/
├── Audio/
│   └── Music/
│       └── Laboratorio_Principal_Incidente.mp3   # MOVER desde la raíz del proyecto
├── UI/
│   ├── Logo.png
│   ├── BackgroundLab.png
│   └── RenderTextures/
│       └── MenuCharacter.renderTexture           # Crear en Unity
└── Scenes/
    ├── MainMenu.unity                             # NUEVO
    └── Nivel01.unity                              # RENOMBRAR desde SampleScene.unity
```

**Acciones requeridas:**
- Mover el MP3 desde la raíz del proyecto a `Assets/Audio/Music/`.
- Renombrar `SampleScene.unity` → `Nivel01.unity` (desde el Project window de Unity, para que actualice los `.meta` y referencias).

## Setup del RenderTexture

1. Crear asset: `Assets/UI/RenderTextures/MenuCharacter.renderTexture`
   - Size: `1024 x 1024` (cuadrada, después se escala con el `RawImage`)
   - Anti-aliasing: `4x`
   - Color Format: `R8G8B8A8_UNORM` (con alpha para fondo transparente)
   - Depth Buffer: `At least 24 bits with stencil`
2. En `CharacterCamera`:
   - `Target Texture` = `MenuCharacter`
   - `Clear Flags` = `Solid Color` con alpha `0` (para que el fondo del laboratorio se vea detrás del personaje)
   - `Culling Mask` = solo la layer del personaje (crear layer `MenuCharacter`)
   - Posicionar para encuadrar al científico de pecho hacia arriba
3. En `CharacterDisplay` (RawImage del Canvas):
   - `Texture` = `MenuCharacter.renderTexture`
   - Anclado al borde derecho del Canvas

## Configuración de audio

**Import settings del MP3:**
- `Load Type`: Streaming
- `Compression Format`: Vorbis
- `Quality`: 70%
- `Force To Mono`: false

**AudioSource del AudioManager:**
- `Clip`: Laboratorio_Principal_Incidente
- `Play On Awake`: true
- `Loop`: true
- `Volume`: 0.6
- `Spatial Blend`: 0 (2D, no 3D positional)

## Script: `MainMenuController.cs`

Ubicación: `Assets/Scripts/UI/MainMenuController.cs`

**Responsabilidades:**
- Manejar los click events de los 5 botones del menú.
- Hacer fade-out del AudioSource antes de cargar la escena del shopping.
- Hooks vacíos para SFX de hover/click que se conectan después.

**Métodos públicos (asignables desde el Inspector):**
- `NuevoJuego()` → corutina que baja el volumen de `bgmSource` a 0 en `fadeDuration` segundos, luego `SceneManager.LoadScene(gameSceneName)`.
- `Salir()` → `Application.Quit()` en build, `EditorApplication.isPlaying = false` en editor (con `#if UNITY_EDITOR`).
- `MostrarProximamente()` → por ahora un `Debug.Log("Próximamente...")`. Más adelante puede mostrar un panel modal.

**Campos serializados:**
- `[SerializeField] AudioSource bgmSource;`
- `[SerializeField] float fadeDuration = 0.5f;`
- `[SerializeField] string gameSceneName = "Nivel01";`
- `[SerializeField] AudioClip hoverSfx;` (vacío por ahora)
- `[SerializeField] AudioClip clickSfx;` (vacío por ahora)
- `[SerializeField] AudioSource sfxSource;` (vacío por ahora)

## Estilo de los botones

Componente `Button` de cada botón con `ColorBlock` configurado:
- `Normal Color`: blanco transparente (texto visible, sin fondo)
- `Highlighted Color`: púrpura (`#7B2DBE` aprox., a ajustar al estilo del logo)
- `Pressed Color`: púrpura más oscuro
- `Selected Color`: púrpura (mismo que highlighted, para que el botón con foco se vea seleccionado)
- `Fade Duration`: 0.1s

El texto va con TextMeshPro. Fuente bold, mayúsculas, color blanco.

`EventSystem` con `First Selected = BtnNuevoJuego` para que aparezca resaltado al abrir la escena (como en la referencia).

## Build Settings

`File → Build Profiles → Scene List`:
1. Index 0: `Assets/Scenes/MainMenu.unity` ← **debe ser la primera**
2. Index 1: `Assets/Scenes/Nivel01.unity`

## Criterios de éxito

- [ ] Al iniciar el juego (Play en editor o build), aparece el menú principal.
- [ ] Se ve el científico 3D con animación idle en el lado derecho.
- [ ] Suena la música de laboratorio en loop a volumen razonable.
- [ ] El botón "Nuevo Juego" está resaltado al abrir la escena.
- [ ] Apretar "Nuevo Juego" hace fade-out de la música y carga `Nivel01`.
- [ ] Apretar "Salir" cierra la aplicación (en build) o detiene el play (en editor).
- [ ] Los otros 3 botones loguean "Próximamente" en consola sin romper nada.
- [ ] El menú es navegable con teclado (flechas + Enter), no solo mouse.

## Fuera de alcance (para futuras iteraciones)

- Sistema de save/load real para "Cargar Partida".
- Pantalla de Opciones (volumen, gráficos, controles).
- Pantalla de Extras (créditos, galería).
- SFX de UI (hover/click).
- Música del shopping durante gameplay.
- Animaciones de entrada del logo o del personaje.
- Localización a otros idiomas.
