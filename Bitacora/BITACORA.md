# Bitácora de desarrollo — *Freaky Super Pixel 3D*

Juego de acción 3D: **Emilio**, un viejo/científico atrapado en un supermercado, sobrevive
oleadas de enemigos (esqueletos, mapaches, arañas, esqueleto con carrito) usando un sistema de
**parry** que carga su transformación en **Freaky Emilio**, hasta enfrentar al jefe final, el
**Oso del Súper**.

- **Motor:** Unity 6000.4.1f1 (3D) · **Plataforma:** PC (teclado/mouse + joystick)
- **Equipo:** Equipo Dinamita · **Repo:** https://github.com/Naciohh/freaky-super-pixel
- **Período documentado:** 09/04/2026 → 24/06/2026 (pre-producción en abril; ~20 sesiones de trabajo)

> Capturas en [`capturas/`](capturas/), numeradas en orden de aparición. Cada tarea incluye un
> **Cómo** (cómo se hizo o cómo se resolvió el problema). Los 🖊️ son detalles para completar a mano.

---

## Resumen por fases

| Fase | Período | Foco | Tareas |
|------|---------|------|:--:|
| **0** | 09/04–15/05 | Pre-producción: proyecto, assets, modelos (Meshy AI), escenarios y menú v1 | 4 |
| **1** | 22–23/05 | Combate base, oleadas, jefe, transformación, enemigos | 6 |
| **2** | 05/06 | Físicas y feel: gravedad, control, carrito, audio, MCP | 4 |
| **3** | 10–12/06 | El Oso (jefe), cinemática, stun, paredes del mapa | 4 |
| **4** | 16–17/06 | Animaciones y arañas | 2 |
| **5** | 18–19/06 | Menú jugable, guardado, portal, HUD, ataque, victoria, carga de UI | 9 |
| **6** | 21/06 | Dificultad, balance, aura, minions, joystick | 5 |
| **🔧** | — | Pendientes / a resolver | 4 |

**Total: 34 tareas resueltas + 4 pendientes.**

---

## Tabla maestra (prioridad y fecha)

> **Prioridad:** ⭐⭐⭐ Alta (núcleo del juego) · ⭐⭐ Media (importante) · ⭐ Baja (pulido/extra).
> Fechas aproximadas según los archivos del proyecto, los commits de git y las sesiones (año 2026).

| # | Tarea | Fase | Prioridad | Fecha | Estado |
|---|-------|:----:|:---------:|:-----:|:------:|
| 0.1 | Setup del proyecto, repo y assets 3D | 0 | ⭐⭐⭐ | 09–10/04 | ✅ |
| 0.2 | Modelos de Emilio y Freaky (Meshy AI + animaciones Mixamo) | 0 | ⭐⭐⭐ | 09–10/04 | ✅ |
| 0.3 | Escenarios (laboratorio + súper) y cámara isométrica | 0 | ⭐⭐⭐ | abr–may | ✅ |
| 0.4 | Menú principal v1 | 0 | ⭐⭐ | 15/05 | ✅ |
| 1.1 | Enemigos por fichas de datos (variantes del esqueleto) | 1 | ⭐⭐⭐ | 22/05 | ✅ |
| 1.2 | Comportamiento de enemigos y colisiones | 1 | ⭐⭐⭐ | 22/05 | ✅ |
| 1.3 | Vida y muerte de enemigos | 1 | ⭐⭐⭐ | 23/05 | ✅ |
| 1.4 | Combate del jugador: ataque, parry y transformación | 1 | ⭐⭐⭐ | 22/05 | ✅ |
| 1.5 | Oleadas y generador de enemigos | 1 | ⭐⭐⭐ | 23/05 | ✅ |
| 1.6 | Primeros enemigos, props y música | 1 | ⭐⭐ | 23/05 | ✅ |
| 2.1 | Físicas y control (gravedad, piso, apuntado) | 2 | ⭐⭐⭐ | 05/06 | ✅ |
| 2.2 | Esqueleto que empuja el carrito | 2 | ⭐⭐ | 05/06 | ✅ |
| 2.3 | Audio entre estados (transición + sonidos) | 2 | ⭐⭐ | 05/06 | ✅ |
| 2.4 | Vinculación de Unity con Claude (MCP) | 2 | ⭐⭐ | 05/06 | ✅ |
| 3.1 | El Oso: modelo, comportamiento y vida | 3 | ⭐⭐⭐ | 10/06 | ✅ |
| 3.2 | Cinemática de intro del jefe + aturdimiento | 3 | ⭐⭐ | 12/06 | ✅ |
| 3.3 | Paredes-fondo del borde del mapa | 3 | ⭐ | 12/06 | ✅ |
| 3.4 | Tanda de arreglos varios | 3 | ⭐⭐ | 12/06 | ✅ |
| 4.1 | Animaciones de golpe y muerte | 4 | ⭐⭐ | 16/06 | ✅ |
| 4.2 | Arañas con generador propio | 4 | ⭐⭐ | 17/06 | ✅ |
| 5.1 | Menú principal jugable | 5 | ⭐⭐ | 19/06 | ✅ |
| 5.2 | Guardado y pausa | 5 | ⭐⭐⭐ | 19/06 | ✅ |
| 5.3 | Portal de transición al entrar a partida | 5 | ⭐ | 19/06 | ✅ |
| 5.4 | HUD con arte: barras y retrato | 5 | ⭐⭐ | 18/06 | ✅ |
| 5.5 | Rework del ataque (automático y apuntado) | 5 | ⭐⭐⭐ | 19/06 | ✅ |
| 5.6 | Derrota y victoria (Game Over / Victoria) | 5 | ⭐⭐⭐ | 19/06 | ✅ |
| 5.7 | Cinemática de entrada al nivel | 5 | ⭐ | 19/06 | ✅ |
| 5.8 | Carga e integración de toda la UI diseñada (menú + nivel) | 5 | ⭐⭐ | 18/06 | ✅ |
| 5.9 | Animación del cartel de parry | 5 | ⭐⭐ | 18/06 | ✅ |
| 6.1 | Selector de dificultad | 6 | ⭐⭐ | 21/06 | ✅ |
| 6.2 | Rework del aura de Freaky | 6 | ⭐⭐ | 21/06 | ✅ |
| 6.3 | Spawn global, escalado y pulido del jefe | 6 | ⭐⭐ | 21/06 | ✅ |
| 6.4 | Minions del jefe (modo Pesadilla) | 6 | ⭐ | 21/06 | ✅ |
| 6.5 | Soporte de joystick / gamepad | 6 | ⭐⭐ | 21/06 | ✅ |
| 🔧 1 | Animación de muerte del jugador | — | ⭐⭐⭐ | — | 🔧 Pendiente |
| 🔧 2 | Contenido faltante (Nivel 2, Opciones, Extras, portal rojo) | — | ⭐⭐ | — | 🔧 Pendiente |
| 🔧 3 | Verificación en Play (control físico) | — | ⭐⭐ | — | 🔧 Pendiente |
| 🔧 4 | Balance general (playtest) | — | ⭐⭐ | — | 🔧 Pendiente |

---

# FASE 0 — Pre-producción y arranque *(09/04 – 15/05)*

> El proyecto arrancó el **~09–10/04/2026** (pre-producción), unas 6 semanas antes del primer commit
> de git (22/05). En abril ya había **80 assets** importados.

### 0.1 · Setup del proyecto, repo y assets 3D *(≈ 09–10/04)*
- Proyecto de **Unity 3D** creado (~09–10/04), subido a GitHub y ordenado en carpetas. Se buscaron e
  importaron todos los modelos 3D (personajes, góndolas y props del súper, escenario, plantas).
- **Cómo:** los modelos se cargaron al proyecto y los que se repiten se guardaron como **piezas
  reutilizables**. Los **personajes** se generaron con Meshy AI (ver 0.2). 🖊️ *Anotar de dónde salieron
  los packs de props/escenario.*

### 0.2 · Modelos de Emilio y Freaky Emilio *(09–10/04)*
- Modelos de **Emilio** (09/04) y **Freaky** (10/04) generados con **Meshy AI** (una IA que crea modelos
  3D a partir de una imagen o texto). Las animaciones de quieto, correr y atacar se bajaron de **Mixamo**.
- **Bug resuelto — texturas que no se veían:** se corrigió la configuración con la que se importaba la
  textura de cada modelo y se volvió a cargar.

### 0.3 · Escenarios y cámara isométrica
- **Escenario del menú (laboratorio)** y **escenario del Nivel 1 (supermercado)**, más una **cámara
  isométrica** que sigue al jugador y se reposiciona si algo lo tapa (**anti-oclusión**).
- **Cómo:** cada escenario se **armó** ubicando los props importados dentro de su escena. La cámara queda
  en ángulo isométrico y acompaña a Emilio.

### 0.4 · Menú principal v1 *(spec `2026-05-15-main-menu-design.md`)*
- Pantalla de menú con el científico mostrado en **3D dentro de la interfaz**, el logo, las opciones y
  música de laboratorio. **Nuevo Juego** baja la música de a poco y carga el nivel; **Salir** cierra.
- **Cómo:** una cámara aparte "filma" al modelo y esa imagen se muestra dentro de la interfaz, para que el
  personaje 3D aparezca integrado al menú 2D. Se renombró `SampleScene` → **`Nivel01`** y se ordenó la
  lista de escenas (menú primero, nivel después).

---

# FASE 1 — Combate, oleadas y jefe *(22–23/05)*
*(spec `2026-05-22-combat-waves-boss-design.md`)*

### 1.1 · Enemigos por fichas de datos
- Los datos de cada enemigo (vida, daño, velocidad, tamaño) se guardan como **fichas aparte**, así crear
  o cambiar un enemigo no toca el código. El esqueleto tiene **3 variantes**:

| Variante | Vida | Daño | Velocidad | Tamaño | Aparece |
| --- | --- | --- | --- | --- | --- |
| Chico | Baja | Bajo | Rápido | Petiso | Seguido |
| Mediano | Media | Medio | Normal | Normal | Normal |
| Grande | Alta | Alto | Lento | Grandote | De vez en cuando |

### 1.2 · Comportamiento de enemigos y colisiones
- Los enemigos **persiguen siempre** a Emilio, **intentan rodearlo** en vez de amontonarse de frente, y
  tienen **cajas de colisión** para no encimarse. También se les pusieron cajas de colisión a las paredes,
  pisos y objetos del escenario (Nivel 1 y lobby) para que nadie atraviese nada.
- **Cómo:** se les sacó el "radio de detección" (antes solo atacaban si te acercabas) para que persigan
  desde que aparecen; el rodeo los manda a un punto alrededor del jugador, no a su centro.

### 1.3 · Vida y muerte de enemigos
- Cada enemigo tiene su vida: recibe daño y, al llegar a 0, **muere con su animación**. Arriba suyo flota
  una **barrita de vida** que siempre mira a la cámara.

### 1.4 · Combate del jugador: ataque, parry y transformación
- Emilio golpea a los enemigos que tiene cerca adelante y, con el **click derecho**, hace **parry**
  (cancela el golpe del enemigo si llega justo en ese momento). Juntando **5 parries** se transforma en
  Freaky por unos 8 segundos y mata con solo tocar. Tenía barras provisorias de vida y de parry.
- **Cómo:** el parry abre una ventana corta; si en ese instante hay un golpe entrante, lo anula y suma a
  la barra de transformación. Las barras provisorias se hicieron para después cambiarlas por el arte final.

### 1.5 · Oleadas y generador de enemigos
- Un reloj de oleada (90 s); al llegar a 0 deja de generar enemigos, limpia los que quedan y aparece el
  jefe. El generador crea enemigos en puntos al azar del mapa, con un **máximo a la vez** y una **distancia
  mínima al jugador**.
- **Cómo:** cada tipo de enemigo aparece con distinta frecuencia, y el punto de aparición se descarta si
  cae demasiado cerca de Emilio.

### 1.6 · Primeros enemigos, props y música
- Enemigo **mapache**, esqueleto que empuja un **carrito de súper**, y un sistema de música con **3 temas**
  según el momento:

| Momento | Música |
| --- | --- |
| Explorando (antes de la oleada) | Tema tranquilo |
| Oleada / pelea | Tema de combate |
| Jefe | Tema del jefe |

Además, cada tipo de enemigo aparece con **distinta frecuencia**: los chicos seguido, los grandes de vez en cuando.

---

# FASE 2 — Físicas, control y feel *(05/06)*

### 2.1 · Físicas y control (gravedad, piso, apuntado al mouse)
- **Gravedad** y detección de piso en **Emilio, Freaky y enemigos** (para que no floten) y **rotación
  hacia el mouse**.
- **Cómo:** el apuntado lleva la mira a donde está el mouse en el piso y el personaje gira hacia ahí. Es
  la base del ataque automático (ver 5.5).

### 2.2 · Esqueleto que empuja el carrito
- Esqueleto + carrito como mecánica de **embestida**.
- **Bug resuelto:** la forma estaba mal y el esqueleto quedaba "adentro" del carrito → se corrigió la
  posición y el orden (el carrito adelante del esqueleto) y la gravedad para que empuje desde atrás.

### 2.3 · Audio entre estados
- **Transición suave** de música entre explorar / pelea / jefe + **sonidos distintos** según el golpe
  conecte o pegue al aire.

### 2.4 · Vinculación de Unity con Claude (MCP)
- **Integración de Unity con Claude** para crear/editar scripts, modificar escenas, leer la consola y
  **sacar capturas** (las de esta bitácora salieron así).
- **Bug resuelto:** la conexión se cortaba por errores de credencial → se reconfiguró la vinculación.
  🖊️ *Anotar el fix puntual.*

---

# FASE 3 — El Oso (jefe) y el mapa *(10–12/06)*

![El Oso del Súper](capturas/03_boss_oso.png)

### 3.1 · El Oso: modelo, comportamiento y vida
- Modelo + texturas del oso y su lógica de jefe (vida y daño muy altos).
- **Cómo:** usa su propia ficha de datos y un comportamiento aparte del de los esqueletos; tiene su barra
  de vida propia.

### 3.2 · Cinemática de intro del jefe + aturdimiento
- Al aparecer el jefe se **congela el juego**, la cámara hace una toma dramática y aparece el cartel con
  su nombre. Cuando queda vulnerable tras un parry, reproduce una **animación de aturdimiento**.

![Cartel del jefe — EL OSO](capturas/10_cinematica_boss.png)

### 3.3 · Paredes-fondo del borde del mapa
- Las 4 paredes muestran una **foto aérea de estacionamiento**.
- **Bug resuelto — pared Norte de cabeza:** una de las paredes mostraba la foto dada vuelta → se le aplicó
  un material espejado solo a esa pared; las otras tres quedaron con el normal.

### 3.4 · Tanda de arreglos varios *(sesión "Game bugs and features")*
- Sesión dedicada a bugs y detalles sueltos de gameplay.
- 🖊️ *Detallar los arreglos puntuales de esa sesión.*

---

# FASE 4 — Animaciones y arañas *(16–17/06)*

### 4.1 · Animaciones de golpe y muerte
- Animaciones de **golpe** y **muerte** integradas con sus condiciones y transiciones.

### 4.2 · Arañas con generador propio
- Arañas con su propio generador, aparte del de los esqueletos: solo salen **durante la oleada** y se
  limpian cuando empieza el jefe. Tienen su propia lógica para **no encimarse entre ellas**.

---

# FASE 5 — Menú jugable, UI y flujo *(18–19/06)*

### 5.1 · Menú principal **jugable**

![Menú jugable — el cuarto de Emilio](capturas/01_menu_lobby.png)

- El menú pasó a un **cuarto 3D jugable**: Emilio camina y se acerca a **6 estaciones** que muestran
  **"PRESIONA E"** (Nuevo Juego, Continuar, Cargar, Opciones, Extras, Salir).
- **Cómo:** cada opción es una estación con un cartel que crece al acercarse; al apretar E se ejecuta su
  acción. El cuarto está a ~5× escala, así que se escaló a Emilio ×5.

![Prompt de interacción](capturas/12_prompt_interactuar.png)

### 5.2 · Guardado y pausa
- **Guardado por partidas** (slots) y **menú de pausa**.
- **Cómo:** se guarda el estado completo (posiciones, enemigos generados, etapa, vida de los personajes,
  del jefe y de Emilio, y la dificultad) y se restaura al continuar. La pausa congela el juego, **pausa
  también la música** y muestra continuar/salir.

### 5.3 · Portal de transición al entrar a partida
- Al entrar a la partida se reproduce un **video a pantalla completa con audio** (el portal) y recién
  cuando termina carga el nivel.
- **Bugs resueltos:** el video venía en un formato que Unity no reconocía → se convirtió a uno compatible
  (y de paso bajó de 115 MB a 2,4 MB). Y se sacó el truco de ir cargando el nivel por detrás del video,
  porque hacía que te atacaran durante el portal.

### 5.4 · HUD con arte: barras y retrato
- Barras de **vida** y **transformación** + **retrato** que cambia de Emilio a Freaky.
- **Cómo:** el relleno de las barras va por detrás del dibujo y recortado a la forma del marco. El retrato
  muestra dos caras que se **generaron solas por código, sacándole una "foto" de frente a cada modelo 3D**.

![HUD — barras](capturas/04_hud_barras.png) ![Retrato Emilio](capturas/05_orbe_emilio.png) ![Retrato Freaky](capturas/06_orbe_freaky.png) ![Barra del jefe](capturas/09_barra_jefe.png)

### 5.5 · Rework del ataque (automático y apuntado)
- El ataque dejó de hacerse con un botón y pasó a ser **automático**: Emilio golpea solo cuando hay un
  enemigo cerca, **apuntando hacia donde está el mouse**, en un **arco casi completo** alrededor suyo (todo
  menos la espalda). El golpe muestra un **efecto de tajo** y, en modo Freaky, aparece el **aura**. El
  parry, además de frenar el golpe, **aturde a todos los enemigos cercanos** de una.

### 5.6 · Derrota y victoria
- **Muerte de Emilio** a 0 de vida → **Game Over** (Reintentar / Salir), **pantalla de Victoria** y
  **portal de derrota del jefe**.
- *(La animación de death de Emilio quedó pendiente — ver 🔧.)*

![Game Over](capturas/08_game_over.png)

### 5.7 · Cinemática de entrada al nivel
- La cámara hace un **picado desde arriba** hasta encuadrar a Emilio y aparece el cartel del "mercadito"
  con **un rebotecito y un desvanecido**, igual que el cartel del jefe.

![Cartel del nivel — el mercadito](capturas/11_cinematica_nivel.png)

### 5.8 · Carga e integración de toda la UI diseñada (menú + nivel)
- Se metieron al juego **todas las imágenes ya diseñadas** y se ubicaron en su lugar correcto: los
  **botones del menú** y el cartel de "interactuar", las **barras de vida** de Emilio y del jefe, el
  **retrato** del HUD, los **carteles** (jefe y mercadito), los **popups** de Game Over y Victoria, y los
  **botones de pausa**. Algunas van en el **menú** y otras en el **Nivel 1**.

### 5.9 · Animación del cartel de parry
- Cuando hacés un **parry exitoso**, aparece la **imagen de "parry"** en el mundo, justo sobre el enemigo
  que frenaste, con un **golpe de aparición** (crece de golpe y se desvanece) y un **sonido**. En el jefe,
  como es enorme, la imagen se ancla **sobre su cabeza** para que no quede tapada. También hay un indicador
  del **tiempo de recarga** del parry.

![Feedback de parry](capturas/07_parry_feedback.png)

---

# FASE 6 — Dificultad, balance y controles *(21/06)*
*(spec `2026-06-21-dificultad-design.md`)*

### 6.1 · Selector de dificultad
- Al empezar una partida nueva se elige entre **4 niveles**. La elección queda guardada. **Normal** es la
  base; las demás suben o bajan los números.

**Cómo cambia el juego en general:**

| Qué cambia | Fácil | Normal | Difícil | Pesadilla |
| --- | --- | --- | --- | --- |
| Daño / Vida de enemigos | Menor | Base | Mayor | Mucho mayor |
| Cantidad de enemigos | Menos | Base | Más | Muchos más |
| Vida del jefe | Menor | Base | Mayor | Mucho mayor |
| Horda de ositos del jefe | No | No | No | Sí |

**Cómo cambian las stats de Emilio:**

| Stat de Emilio | Fácil | Normal | Difícil | Pesadilla |
| --- | --- | --- | --- | --- |
| Vida máxima | Mayor | Base | Menor | Menor |
| Daño de golpe | Base | Base | +20% | +40% |
| Alcance de ataque | Base | Base | +15% | +30% |
| Daño del aura (Freaky) | Base | Base | +30% | +60% |
| Radio del aura (Freaky) | Base | Base | +15% | +30% |
| Parry (tiempo de recarga) | Más corto | Base | Más largo | Más largo |

Los refuerzos a Emilio en Difícil y Pesadilla compensan que haya más enemigos y más duros.

### 6.2 · Rework del aura de Freaky
- El aura **ya no empuja** a los enemigos: ahora les hace **más daño cuanto más cerca están**.
- **Cómo se resolvió:** se sacó el efecto viejo que los mataba de un solo toque.

### 6.3 · Spawn global, escalado y pulido del jefe
- Enemigos en **todo el mapa**, un único generador de enemigos + uno de arañas, **parry con tiempo de
  recarga**, daño al jefe solo en su **ventana de vulnerabilidad** y **vida del jefe = 3000**.
- **Cómo se resolvió:** se pasó a una **zona de aparición que cubre todo el mapa** (con distancia mínima
  al jugador) y se **borró un generador duplicado que estaba dormido**. El jefe solo recibe daño en la
  ventana que abre cada parry; se le subió la vida de 1000 a 3000 y se reforzó a Emilio en las
  dificultades altas.

### 6.4 · Minions del jefe (modo Pesadilla)
- Solo en Pesadilla, el jefe invoca una **horda de ositos** que se limpia cuando el jefe muere.
- **Cómo:** el osito es un **clon del oso** pero con comportamiento de enemigo normal.

### 6.5 · Soporte de joystick / gamepad
- El juego se puede jugar con **joystick** además de teclado y mouse, con esquema de dos sticks:

| Control | Acción |
| --- | --- |
| Stick izquierdo | Mover a Emilio |
| Stick derecho | Apuntar |
| R1 | Parry |
| Options / Start | Pausa |
| Círculo | Volver / cancelar en menús |
| X | Confirmar / interactuar |

Los menús también se navegan con el pad. El botón de confirmar se eligió por su **posición** (el de
abajo), así funciona igual en mandos de PlayStation y de Xbox.

---

# 🔧 Tareas pendientes / a resolver

- **Animación de muerte del jugador:** la *lógica* de morir anda (5.6) pero la **animación** de death
  nunca se disparó bien (varios intentos fallidos). Revisar el disparador/transición en el Animator de Emilio.
- **Contenido faltante:** desarrollar el **Nivel 2** (la escena base ya existe), las pantallas de
  **Opciones** y **Extras/créditos** (siguen como placeholder) y el **portal rojo** de Pesadilla.
- **Verificación en Play:** dificultad, twin-stick, R1 y spawns funcionan en las pruebas básicas, pero
  falta una pasada jugada completa con control físico.
- **Balance general:** afinar vida / daño / oleadas con playtest.

---

# Vista general del nivel

![Gameplay en el supermercado](capturas/02_nivel_gameplay.png)

---

## Notas técnicas (aprendizajes del proceso)

- **Todo configurable por fichas de datos:** los números de enemigos, jefe y dificultad viven en datos,
  no en el código, así se ajustan sin reprogramar.
- **Capturas en modo edición:** las imágenes se sacaron desde las cámaras reales de cada escena, porque
  el modo de prueba por herramientas no avanzaba el tiempo del juego.
- **Carpetas de código:** `Assets/Scripts/` → `Player/`, `Enemy/`, `UI/`, `Systems/`, `SaveSystem/`,
  `Data/`, `VFX/`.
