# Menú Principal Jugable — Diseño

**Fecha:** 2026-06-18
**Escena:** `Assets/Scenes/MainMenu.unity`

## Objetivo

Convertir el Main Menu (hoy: UI 2D con botones + Emilio mostrado en un panel) en un
**menú jugable**: el jugador controla a Emilio caminando por el escenario 3D (la sala
`LobbyBackground`), y al acercarse a distintos muebles aparece un **cartel-botón flotante**
que crece tipo holograma. Al apretar **E** estando cerca, se ejecuta la acción de ese botón.

## Estado actual (lo que ya existe)

- `MainMenu.unity` tiene una sala 3D real y caminable: `LobbyBackground` (taberna de piedra,
  piso de baldosas, muebles en las 4 esquinas), renderizada como **fondo estático** por
  `BackgroundCamera`.
- `EmilioDisplay` (tag `Player`) ya tiene `CharacterController` (desactivado) y `PlayerMovement`
  (referencias en `null`); hoy se muestra en un `CharacterStage` con cámara propia, volcado a un
  panel del Canvas.
- `MenuPanel` (Canvas) tiene 6 botones 2D que llaman a `MainMenuController`:
  `NuevoJuego`, `Continuar`, `AbrirCargar`, `MostrarProximamente` (Opciones), `MostrarProximamente`
  (Extras), `Salir`.
- `MainMenuController.cs` conserva toda la lógica de acciones, audio (sfx hover/click), pantalla
  de carga y panel de Cargar. **Se reutiliza sin cambios de lógica.**
- `AudioManager` con `BGMSource` y `SFXSource`.
- Sprites de botones en `Desktop/UI juego/Botones menúi/` (6 PNGs) + `botones_interactuar.png`
  en `Desktop/UI juego/Efecto de sonido/`. **Aún no importados al proyecto.** Sonido
  `popup botones de menú.mpeg` en `Desktop/UI juego/Efecto de sonido/`.

## Arquitectura propuesta

Una **sola cámara de juego** que ve a Emilio dentro del cuarto (se retira el setup de doble
cámara / stage). Emilio pasa a ser un personaje jugable real en la sala. Las estaciones de
interacción son objetos en el mundo con un cartel sprite que mira a la cámara.

### Cambios en la escena

1. **Piso caminable:** agregar al cuarto un collider de piso (plano/box) para que el
   `CharacterController` de Emilio no se caiga. Paredes ya tienen colisión visual; si no,
   se agregan box colliders simples al perímetro.
2. **Emilio jugable:** mover `EmilioDisplay` dentro del cuarto (centro del piso), activar su
   `CharacterController` y `PlayerMovement`, y conectar referencias: `normalForm`,
   `transformedForm`, `normalAnimator`, `transformedAnimator`, `aimCamera` (la cámara de juego).
3. **Cámara seguidora:** una cámara única con `MenuFollowCamera` (variante de `CameraFollow`)
   apuntando a Emilio, con offset isométrico **más cercano** que Nivel01 (el menú usa un zoom
   más cerrado). Reemplaza a `BackgroundCamera` y `CharacterCamera` (que se retiran/desactivan).
   **`UICamera` se conserva** si el Canvas la necesita para renderizar el overlay (logo, créditos,
   pantalla de carga); si el Canvas está en Screen Space - Overlay, `UICamera` también se puede
   retirar. Se verifica en implementación.
4. **Ocultar UI 2D vieja:** `MenuPanel` (los 6 botones) se desactiva. `LogoImage`, créditos y
   `LoadingPanel`/`LoadMenuPanel` se conservan (la pantalla de carga y el panel de cargar
   slots siguen funcionando vía `MainMenuController`).
5. **6 estaciones** bajo un contenedor `Stations`, una por mueble: Nuevo Juego, Continuar,
   Cargar, Opciones, Extras, Salir.

### Componentes nuevos (scripts)

**`InteractionStation.cs`** — el cerebro de cada estación.
- Detecta proximidad del jugador (trigger collider `isTrigger`, busca tag `Player`, o por
  distancia en `Update` para robustez).
- Estado "cerca": el cartel (sprite) interpola su escala de pequeña → grande (efecto holograma),
  y se muestra el sprite `botones_interactuar` ("E para interactuar").
- Al **entrar** en rango (transición de lejos→cerca) reproduce una vez el sfx `popup`.
- Si el jugador está en rango y aprieta **E** (`Input.GetKeyDown(KeyCode.E)`), invoca un
  `UnityEvent` configurable en el Inspector → se mapea al método de `MainMenuController`
  (ej. `NuevoJuego`).
- Campos serializados: `SpriteRenderer botón`, `SpriteRenderer promptE`, escalas
  min/max, velocidad de lerp, radio de interacción, `AudioSource` + `AudioClip popup`,
  `UnityEvent onActivate`, y un flag/propiedad `interactable`.
- **Estado deshabilitado:** si `interactable == false`, el cartel se muestra apagado
  (gris/tinte oscuro o alpha bajo), no crece a full, no suena el popup y la E no hace nada.

**`Billboard.cs`** — hace que el transform (el cartel) siempre mire a la cámara de juego.
Componente reutilizable, se pone en cada cartel sprite y en el prompt E.

### Estaciones especiales (Continuar / Cargar)

- En `Start`, una pieza (puede ser `MainMenuController` o un pequeño `MenuStationsSetup`) consulta
  `SaveManager.HasAnySave()`:
  - Si **no hay** save → estaciones de **Continuar** y **Cargar** quedan `interactable = false`
    (carteles apagados/grises, sin respuesta a la E).
  - Si **hay** save → activas normalmente.

### Audio

- Al crecer el cartel por acercarse → suena `popup botones de menú` (una vez por entrada en
  rango), vía el `SFXSource` existente.
- El sfx de "click" al activar lo sigue manejando `MainMenuController` dentro de cada acción
  (ya existe). El sfx de hover viejo de UI queda sin uso.

## Flujo de interacción

```
Jugador camina (WASD, mira al mouse)
        │
        ▼
Entra en rango de una estación ──► cartel crece (holograma) + aparece "E" + suena popup
        │
        ├── aprieta E ──► UnityEvent ──► MainMenuController.<Accion>()  (NuevoJuego / Continuar / …)
        │
        ▼
Sale de rango ──► cartel se achica + se oculta "E"
```

## Mapeo estación → acción

| Estación      | Método de MainMenuController | Disponibilidad           |
|---------------|------------------------------|--------------------------|
| Nuevo Juego   | `NuevoJuego()`               | Siempre                  |
| Continuar     | `Continuar()`                | Solo si hay save         |
| Cargar        | `AbrirCargar()`              | Solo si hay save         |
| Opciones      | `MostrarProximamente()`      | Siempre                  |
| Extras        | `MostrarProximamente()`      | Siempre                  |
| Salir         | `Salir()`                    | Siempre                  |

## Assets a importar

Copiar al proyecto (ej. `Assets/UI/MenuButtons/`) e importar como Sprite (2D):
- `botones_menu_nuevo.png`, `botones_menu_continuar.png`, `botones_menu_cargar.png`,
  `botones_menu_opciones.png`, `botones_menu_extras.png`, `botones_menu_salir.png`
- `botones_interactuar.png`
- `popup botones de menú.mpeg` → importar como AudioClip (si Unity no lee mpeg directo, se
  convierte a `.wav`/`.ogg`).

## Fuera de alcance (YAGNI)

- No se rediseña la lógica de guardado/carga ni la pantalla de carga.
- No se tocan los niveles del juego (`Nivel01`, `Nivel 2`).
- No se agrega un menú de Opciones/Extras real (siguen como "Próximamente").
- No se cambia el sistema de movimiento de Emilio (se reutiliza `PlayerMovement`).

## Riesgos / cosas a verificar en implementación

- Que la sala tenga colisión de piso real para el `CharacterController` (hoy `LobbyBackground`
  solo tiene un BoxCollider tamaño 1 en el origen → insuficiente).
- Unificar a una sola cámara sin romper el render del cuarto ni de Emilio (layers/culling).
- Que `PlayerMovement` apunte a la cámara de juego correcta (`aimCamera`) para el giro al mouse.
- Conversión del audio `.mpeg` a un formato que Unity importe como AudioClip.
