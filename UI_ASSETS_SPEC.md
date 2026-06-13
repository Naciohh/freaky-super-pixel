# UI Assets — Guía para dibujar a mano (Freaky Super Pixel 3D)

> Resolución de referencia del juego: **1920×1080**.
> Regla general: **dibujá todo a 2x** (el doble del tamaño en pantalla) → queda nítido y Unity lo escala.
> Todo se exporta en **PNG con transparencia (alpha)**. Nada de fondo blanco.

---

## 1. Reglas generales de exportación

- **Formato:** PNG-24 con canal alpha.
- **1 archivo por elemento** (no pegar varios juntos).
- **Nombres claros y sin espacios/acentos**, ej: `barra_vida_jugador.png`.
- Potencias de 2 no son obligatorias en UI, pero ayudan (256, 512, 1024).
- Dibujar a **2x** el tamaño de pantalla (ver tablas).

---

## 2. Barras de vida / habilidad (contenedores)

Sistema: **opción B** → vos dibujás SOLO el **contenedor/marco** con el **interior transparente**.
El relleno lo pone el código (rectángulo de color plano detrás del hueco). El color cambia
con la vida (verde→amarillo→rojo) y el ancho baja al perder vida.

| Barra | Tamaño en pantalla (1080p) | **Dibujar contenedor a 2x** | Proporción del hueco interior |
|-------|----------------------------|------------------------------|-------------------------------|
| Vida de Emilio (jugador) | 250 × 25 px | **500 × 50 px** | ~10:1 |
| Barra de transformación / habilidad | 250 × 20 px | **500 × 40 px** | ~12:1 |
| Vida del Boss (arriba, centrada) | ~600 × 30 px* | **~1200 × 60 px** | ~20:1 |
| Vida de enemigos (flota sobre el bicho) | ratio 10:1 (world-space) | **300 × 30 px** | ~10:1 |

\* El ancho exacto del boss se ajusta al dibujo cuando lo armemos (la barra está inactiva en escena).

**Importante sobre el marco:**
- El número de la tabla es el **área de relleno**. Si dibujás un marco decorado/grueso, el PNG
  final va a ser un poco **más grande** que eso (el marco rodea el hueco). No hay problema:
  yo meto el relleno adentro del hueco.
- Lo único que tenés que respetar es la **proporción del hueco interior** (columna de la derecha).
- **Hueco interior = transparente** (ahí se ve el color).
- El enemigo es muy chiquito en pantalla → dibujalo simple, sin detalles finos.

---

## 3. Botones

- Dibujar **estados**: `normal`, `apretado/hover`, y opcional `deshabilitado`.
- Si el botón **cambia de tamaño** (texto largo, varias resoluciones) → diseñar pensando en
  **9-slice**: esquinas y bordes que NO se deformen, centro liso/repetible.
- Si es **tamaño fijo**, no hace falta 9-slice; se usa tal cual.
- Dibujar a 2x del tamaño en pantalla (ej: botón que se ve ~300×100 → dibujar ~600×200).
- Texto del botón = lo pone el código con la tipografía (no dibujarlo encima, salvo que sea logo).

---

## 4. Menús / paneles / contenedores

- PNG con alpha.
- Si el panel escala → 9-slice (bordes fijos, centro estirable).
- Dibujar a 2x.
- Separar fondo del panel de los íconos/botones (archivos distintos).

---

## 5. Paredes del horizonte (borde del mapa)

- Una **foto plana por lado** del cuadrado (4 quads). No se tilea.
- El **aspect ratio de la foto = ancho del lado / alto del muro** (si no, se estira).
- Tamaño recomendado: **2048 × 1024** (2:1). Si el lado es muy ancho: **4096 × 1024** (4:1).
- Import en Unity: **Wrap Mode = Clamp**, material **Unlit**.

---

## 6. Tipografía (custom)

> Unity NO usa las fuentes instaladas en Windows. El HUD usa **TextMeshPro**, así que la fuente
> tiene que estar **dentro del proyecto** y convertida a **TMP Font Asset**.

**Qué hacer:**
1. Elegir la fuente y descargar el archivo **`.ttf` o `.otf`**.
2. Instalarla en Windows (para verla en el programa de dibujo). ← opcional, solo para diseñar.
3. **Copiar el archivo `.ttf`/`.otf` a `Assets/Fonts/`** del proyecto.
4. (Yo) genero el **TMP Font Asset** y lo asigno a todos los textos del HUD.

**Recomendaciones:**
- Conviene **2 fuentes**: una *display* con personalidad para títulos/botones, y una más legible
  para números (vida, timer, daño).
- Revisar que tenga **números** claros y, si va a haber texto en español, que incluya
  **acentos y ñ** (á é í ó ú ñ ¿ ¡).
- **Licencia:** usar fuentes con licencia para uso comercial/juegos (Google Fonts es seguro;
  en dafont.com revisar que diga "free for commercial use"; itch.io también).
- Formatos que sirven: `.ttf` y `.otf`. (Evitar `.woff` web.)

**Fuentes para pasarme:** dejá el/los archivos en `Assets/Fonts/` o pasámelos y los coloco yo.

---

## 7. Checklist de lo que me pasás de cada asset

- [ ] PNG con alpha (sin fondo).
- [ ] Barras: el contenedor con el hueco interior transparente.
- [ ] Botones/paneles: indicar si son tamaño fijo o se estiran (9-slice).
- [ ] Botones: estados normal / apretado (/ deshabilitado).
- [ ] Fuente(s): archivo `.ttf`/`.otf` en `Assets/Fonts/`.
