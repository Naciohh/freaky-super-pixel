# -*- coding: utf-8 -*-
"""Genera Freaky_Super_Mesh_Sobrio.pdf - versión editorial sobria, 13 slides 16:9."""
from reportlab.pdfgen import canvas
from reportlab.lib.colors import HexColor
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
import os

W, H = 13.33 * 72, 7.5 * 72

# ---- palette sobria (editorial / academic) ----
C = {
    "bg":        HexColor("#1C2128"),   # slate oscuro
    "bgLight":   HexColor("#F5F2EC"),   # papel cálido
    "card":      HexColor("#272D38"),   # slate card
    "cardLight": HexColor("#FFFFFF"),
    "ink":       HexColor("#1C2128"),   # tinta
    "inkSoft":   HexColor("#3A404D"),
    "accent":    HexColor("#8B6F47"),   # bronce apagado
    "accent2":   HexColor("#6B4423"),   # bronce oscuro
    "rule":      HexColor("#C9BFA8"),   # línea crema
    "muted":     HexColor("#6B7280"),
    "mutedSoft": HexColor("#9CA3AF"),
    "paper":     HexColor("#EFEAE0"),
    "text":      HexColor("#F5F2EC"),
    "border":    HexColor("#D8D2C4"),
}

OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "Freaky_Super_Mesh_Sobrio.pdf")
c = canvas.Canvas(OUT, pagesize=(W, H))

FONTS = {"serif": "Times-Roman", "serifB": "Times-Bold", "serifI": "Times-Italic",
         "sans": "Helvetica", "sansB": "Helvetica-Bold", "sansI": "Helvetica-Oblique",
         "mono": "Courier"}
try:
    pdfmetrics.registerFont(TTFont("Georgia",  r"C:\Windows\Fonts\georgia.ttf"))
    pdfmetrics.registerFont(TTFont("GeorgiaB", r"C:\Windows\Fonts\georgiab.ttf"))
    pdfmetrics.registerFont(TTFont("GeorgiaI", r"C:\Windows\Fonts\georgiai.ttf"))
    FONTS["serif"]  = "Georgia"
    FONTS["serifB"] = "GeorgiaB"
    FONTS["serifI"] = "GeorgiaI"
except Exception:
    pass
try:
    pdfmetrics.registerFont(TTFont("Calibri",  r"C:\Windows\Fonts\calibri.ttf"))
    pdfmetrics.registerFont(TTFont("CalibriB", r"C:\Windows\Fonts\calibrib.ttf"))
    pdfmetrics.registerFont(TTFont("CalibriI", r"C:\Windows\Fonts\calibrii.ttf"))
    FONTS["sans"]  = "Calibri"
    FONTS["sansB"] = "CalibriB"
    FONTS["sansI"] = "CalibriI"
except Exception:
    pass
try:
    pdfmetrics.registerFont(TTFont("Consolas",  r"C:\Windows\Fonts\consola.ttf"))
    pdfmetrics.registerFont(TTFont("ConsolasB", r"C:\Windows\Fonts\consolab.ttf"))
    FONTS["mono"] = "Consolas"
except Exception:
    pass

SERIF = FONTS["serif"]; SERIFB = FONTS["serifB"]; SERIFI = FONTS["serifI"]
SANS = FONTS["sans"]; SANSB = FONTS["sansB"]; SANSI = FONTS["sansI"]
MONO = FONTS["mono"]

def IN(v): return v * 72
def Y(y_top_inches): return H - y_top_inches * 72

def rect(x, y, w, h, fill=None, stroke=None, stroke_w=0):
    c.saveState()
    if fill is not None: c.setFillColor(fill)
    if stroke is not None:
        c.setStrokeColor(stroke)
        c.setLineWidth(stroke_w)
    c.rect(IN(x), H - IN(y) - IN(h), IN(w), IN(h),
           stroke=1 if stroke else 0, fill=1 if fill else 0)
    c.restoreState()

def circle(cx, cy, r, fill=None, stroke=None, stroke_w=0, alpha=1.0):
    c.saveState()
    if alpha < 1.0: c.setFillAlpha(alpha)
    if fill is not None: c.setFillColor(fill)
    if stroke is not None:
        c.setStrokeColor(stroke); c.setLineWidth(stroke_w)
    c.circle(IN(cx), H - IN(cy), IN(r),
             stroke=1 if stroke else 0, fill=1 if fill else 0)
    c.restoreState()

def line(x1, y1, x2, y2, color, width=0.75, dash=None):
    c.saveState()
    c.setStrokeColor(color); c.setLineWidth(width)
    if dash: c.setDash(dash)
    c.line(IN(x1), H - IN(y1), IN(x2), H - IN(y2))
    c.restoreState()

def text(s, x, y_top, size, color, font=SANS, align="left", valign="top",
         box_w=None, box_h=None, char_spacing=0):
    c.saveState()
    c.setFillColor(color); c.setFont(font, size)
    base_w = c.stringWidth(s, font, size)
    extra = char_spacing * max(0, len(s) - 1)
    tw = base_w + extra
    tx = IN(x)
    if box_w is not None and align == "center":
        tx = IN(x) + (IN(box_w) - tw) / 2
    elif box_w is not None and align == "right":
        tx = IN(x) + IN(box_w) - tw
    if box_h is not None and valign == "middle":
        ty = H - IN(y_top) - IN(box_h)/2 - size*0.35
    elif box_h is not None and valign == "bottom":
        ty = H - IN(y_top) - IN(box_h) + size*0.2
    else:
        ty = H - IN(y_top) - size*0.85
    if char_spacing:
        to = c.beginText(tx, ty)
        to.setFont(font, size); to.setFillColor(color)
        to.setCharSpace(char_spacing); to.textOut(s); c.drawText(to)
    else:
        c.drawString(tx, ty, s)
    c.restoreState()

def wrap_text(s, font, size, max_w_pts):
    words = s.split()
    lines, cur = [], ""
    for w in words:
        test = (cur + " " + w).strip()
        if pdfmetrics.stringWidth(test, font, size) <= max_w_pts:
            cur = test
        else:
            if cur: lines.append(cur)
            cur = w
    if cur: lines.append(cur)
    return lines

def paragraph(s, x, y_top, box_w, size, color, font=SANS, leading=None, max_lines=None):
    lines = wrap_text(s, font, size, IN(box_w))
    if max_lines: lines = lines[:max_lines]
    lh = leading or size * 1.35
    c.saveState(); c.setFillColor(color); c.setFont(font, size)
    for i, ln in enumerate(lines):
        ty = H - IN(y_top) - size*0.85 - i*lh
        c.drawString(IN(x), ty, ln)
    c.restoreState()
    return len(lines) * lh

def arrow_head(xt, yt, angle_deg, color, size=0.1):
    import math
    rad = math.radians(angle_deg)
    tx, ty = IN(xt), H - IN(yt)
    bl_x = tx - IN(size) * math.cos(rad) + IN(size*0.5) * math.cos(rad + math.pi/2)
    bl_y = ty + IN(size) * math.sin(rad) - IN(size*0.5) * math.sin(rad + math.pi/2)
    br_x = tx - IN(size) * math.cos(rad) + IN(size*0.5) * math.cos(rad - math.pi/2)
    br_y = ty + IN(size) * math.sin(rad) - IN(size*0.5) * math.sin(rad - math.pi/2)
    c.saveState()
    c.setFillColor(color); c.setStrokeColor(color)
    p = c.beginPath(); p.moveTo(tx, ty); p.lineTo(bl_x, bl_y); p.lineTo(br_x, br_y); p.close()
    c.drawPath(p, stroke=0, fill=1)
    c.restoreState()

def bidir_arrow(x1, y1, x2, y2, color, width=1.0, inset=0.0):
    import math
    dx, dy = x2 - x1, y2 - y1
    dist = math.hypot(dx, dy)
    if dist == 0: return
    ux, uy = dx / dist, dy / dist
    sx, sy = x1 + ux * inset, y1 + uy * inset
    ex, ey = x2 - ux * inset, y2 - uy * inset
    line(sx, sy, ex, ey, color, width=width)
    ang_end  = __import__("math").degrees(__import__("math").atan2(-(ey - sy), (ex - sx)))
    ang_start = __import__("math").degrees(__import__("math").atan2(-(sy - ey), (sx - ex)))
    arrow_head(ex, ey, ang_end, color)
    arrow_head(sx, sy, ang_start, color)

# ---- chrome sobrio: solo finas líneas horizontales ----
def draw_chrome(dark=False):
    rule = C["mutedSoft"] if dark else C["rule"]
    acc  = C["accent"]
    # Thin top hairline
    line(0.6, 0.35, 13.33-0.6, 0.35, rule, width=0.5)
    # Bottom hairline
    line(0.6, 7.15, 13.33-0.6, 7.15, rule, width=0.5)
    # Tiny accent square top-left
    rect(0.6, 0.3, 0.08, 0.1, fill=acc)

def footer(n, total, label=None, dark=False):
    tc = C["mutedSoft"] if dark else C["muted"]
    text("FREAKY SUPER MESH  ·  UADE 2026", 0.6, 7.22, 8, tc, font=MONO,
         box_h=0.22, valign="middle", char_spacing=1.2)
    if label:
        text(label, 0.6, 7.22, 8, tc, font=MONO,
             box_w=13.33-1.2, box_h=0.22, align="center", valign="middle",
             char_spacing=1.2)
    text(f"{n:02d} / {total:02d}", 13.33-2.4, 7.22, 8, tc, font=MONO,
         box_w=2, box_h=0.22, valign="middle", align="right", char_spacing=1.2)

def eyebrow(s, color=None, y=0.75):
    color = color or C["accent"]
    text(s, 0.6, y, 9, color, font=MONO, char_spacing=2.5)

def big_title(s, size=36, color=None, y=1.05):
    color = color or C["ink"]
    text(s, 0.6, y, size, color, font=SERIFB,
         box_w=13.33-1.2, box_h=1.0, valign="middle")

def title_rule(y=2.15, color=None):
    color = color or C["accent"]
    rect(0.6, y, 0.8, 0.04, fill=color)

TOTAL = 13

# ============== 1 · PORTADA ==============
def slide1():
    c.setFillColor(C["bgLight"]); c.rect(0, 0, W, H, stroke=0, fill=1)
    draw_chrome()
    # top label
    text("ANÁLISIS TEÓRICO · DISEÑO DE VIDEOJUEGOS", 0.6, 1.0, 10, C["accent"],
         font=MONO, char_spacing=3)

    # Title — serif editorial
    text("Freaky Super Mesh", 0.6, 1.7, 64, C["ink"], font=SERIFB,
         box_w=13.33-1.2)
    text("Un estudio de Mechanics, Dynamics & Aesthetics, y de la Tétrada de Schell",
         0.6, 3.05, 16, C["inkSoft"], font=SERIFI, box_w=13.33-1.2)

    # divider
    rect(0.6, 3.9, 2.0, 0.04, fill=C["accent"])

    # two columns: integrantes / cátedra
    text("INTEGRANTES", 0.6, 4.15, 9, C["accent"], font=MONO, char_spacing=2.5)
    text("Fidmay, Agustín",        0.6, 4.5,  13, C["ink"], font=SERIF)
    text("Nuñez Di Meo, Dante",    0.6, 4.8,  13, C["ink"], font=SERIF)
    text("Hug, Juan Ignacio",      0.6, 5.1,  13, C["ink"], font=SERIF)

    text("CÁTEDRA", 4.8, 4.15, 9, C["accent"], font=MONO, char_spacing=2.5)
    text("Prof. Aparicio, Lucas",          4.8, 4.5,  13, C["ink"], font=SERIF)
    text("Prof. Ulrich, Gonzalo Ezequiel", 4.8, 4.8,  13, C["ink"], font=SERIF)

    text("INSTITUCIÓN", 9.2, 4.15, 9, C["accent"], font=MONO, char_spacing=2.5,
         align="left", box_w=3.5)
    text("Universidad Argentina de la Empresa", 9.2, 4.5, 12, C["ink"], font=SERIF)
    text("Diseño de Videojuegos", 9.2, 4.78, 12, C["ink"], font=SERIFI)
    text("Primer Cuatrimestre · Turno Tarde", 9.2, 5.05, 11, C["inkSoft"], font=SERIF)

    # bottom stamp
    line(0.6, 6.3, 13.33-0.6, 6.3, C["rule"], width=0.5)
    text("EQUIPO DINAMITA", 0.6, 6.45, 10, C["accent2"], font=MONO, char_spacing=3)
    text("24 / 04 / 2026", 13.33-2.4, 6.45, 10, C["muted"], font=MONO,
         box_w=2, align="right", char_spacing=1.5)

    footer(1, TOTAL)

# ============== 2 · RESUMEN ==============
def slide2():
    c.setFillColor(C["bgLight"]); c.rect(0, 0, W, H, stroke=0, fill=1)
    draw_chrome()
    eyebrow("I.  RESUMEN DEL JUEGO")
    big_title("Action Rogue-lite dimensional")
    title_rule()

    # left: narrative block
    text("PROPUESTA", 0.6, 2.45, 9, C["accent"], font=MONO, char_spacing=2.5)
    paragraph(
        "Freaky Super Mesh es un Action Rogue-lite para PC en el que el Dr. Emilio, "
        "tras el fallo de un experimento dimensional, queda atrapado entre mundos "
        "absurdos. Desde una cámara cenital inclinada el jugador debe sobrevivir "
        "oleadas, vencer jefes y regresar a casa.",
        0.6, 2.8, 7.2, 14, C["ink"], font=SERIF, leading=20, max_lines=6)

    text("LOOP DE JUEGO", 0.6, 4.7, 9, C["accent"], font=MONO, char_spacing=2.5)
    paragraph(
        "Explorar dimensión → enfrentar oleadas → activar estado eufórico → "
        "derrotar al jefe → avanzar a una nueva dimensión.",
        0.6, 5.05, 7.2, 13, C["inkSoft"], font=SERIFI, leading=18, max_lines=3)

    # right: data card
    rect(8.2, 2.45, 4.55, 4.2, fill=C["cardLight"], stroke=C["border"], stroke_w=0.6)
    rect(8.2, 2.45, 0.04, 4.2, fill=C["accent"])

    fields = [
        ("GÉNERO",       "Action Rogue-lite"),
        ("PLATAFORMA",   "PC"),
        ("MODALIDAD",    "1 jugador · Offline"),
        ("PROTAGONISTA", "Dr. Emilio"),
        ("CÁMARA",       "Cenital inclinada (isométrica)"),
        ("CONTROLES",    "WASD + mouse + Shift"),
    ]
    for i, (k, v) in enumerate(fields):
        yy = 2.7 + i * 0.65
        text(k, 8.5, yy, 9, C["accent"], font=MONO, char_spacing=2)
        text(v, 8.5, yy + 0.25, 13, C["ink"], font=SERIFB)

    # pillars strip
    text("PILARES", 0.6, 6.2, 9, C["accent"], font=MONO, char_spacing=2.5)
    pillars = ["Combate frenético", "Mecánicas simples", "Alta rejugabilidad",
               "Humor absurdo", "Progreso por habilidad"]
    pw = 7.2 / len(pillars)
    for i, p in enumerate(pillars):
        text(p, 0.6 + i * pw, 6.5, 10, C["inkSoft"], font=SERIFI,
             box_w=pw, align="left")

    footer(2, TOTAL, label="Resumen")

# ============== 3 · MDA FLOW ==============
def slide3():
    c.setFillColor(C["bgLight"]); c.rect(0, 0, W, H, stroke=0, fill=1)
    draw_chrome()
    eyebrow("II.  MARCO TEÓRICO  ·  HUNICKE, LEBLANC & ZUBEK")
    big_title("El flujo del diseño")
    title_rule()

    text("El framework MDA distingue tres capas y un sentido de flujo: "
         "el diseñador actúa sobre las Mechanics; el jugador experimenta desde las Aesthetics.",
         0.6, 2.4, 13, C["inkSoft"], font=SERIFI, box_w=13.33-1.2)

    nodes = [
        ("01", "MECHANICS",  "Reglas, sistemas y componentes del juego."),
        ("02", "DYNAMICS",   "Comportamiento emergente en tiempo real."),
        ("03", "AESTHETICS", "Respuestas emocionales del jugador."),
    ]
    startX, startY = 0.6, 3.1
    nodeW, nodeH, gap = 3.85, 2.85, 0.45
    for i, (num, t, body) in enumerate(nodes):
        cx = startX + i * (nodeW + gap)
        rect(cx, startY, nodeW, nodeH, fill=C["cardLight"],
             stroke=C["border"], stroke_w=0.6)
        text(num, cx + 0.3, startY + 0.25, 30, C["accent"], font=SERIFB)
        text(t, cx + 0.3, startY + 0.95, 11, C["accent2"], font=MONO, char_spacing=3)
        rect(cx + 0.3, startY + 1.3, 0.5, 0.03, fill=C["accent"])
        paragraph(body, cx + 0.3, startY + 1.5, nodeW - 0.6, 13, C["ink"],
                  font=SERIF, leading=18, max_lines=3)

    # arrows between nodes
    for i in range(2):
        ax = startX + (i + 1) * nodeW + i * gap
        line(ax + 0.05, startY + nodeH/2, ax + gap - 0.05, startY + nodeH/2,
             C["accent"], width=1.0)
        arrow_head(ax + gap - 0.05, startY + nodeH/2, 0, C["accent"], size=0.09)

    # perspective strip
    text("DISEÑADOR  →  parte de Mechanics", 0.6, 6.4, 11, C["accent2"], font=MONO,
         char_spacing=1.5)
    text("JUGADOR  ←  experimenta desde Aesthetics", 13.33-0.6, 6.4, 11, C["accent2"],
         font=MONO, box_w=6, align="right", char_spacing=1.5)

    footer(3, TOTAL, label="MDA · Flujo")

# ============== 4 · MECHANICS ==============
def slide4():
    c.setFillColor(C["bgLight"]); c.rect(0, 0, W, H, stroke=0, fill=1)
    draw_chrome()
    eyebrow("II.  MDA  ·  01  MECHANICS", color=C["accent"])
    big_title("Mecánicas")
    title_rule()

    text("Las reglas y sistemas del juego, tal como fueron diseñados.",
         0.6, 2.4, 13, C["inkSoft"], font=SERIFI)

    items = [
        ("Movimiento",        "WASD en cuatro direcciones."),
        ("Apuntado",          "Dirección con el mouse."),
        ("Ataque",            "Lanzamiento de probetas."),
        ("Defensa",           "Bloqueo con Shift."),
        ("Enemigos",          "Oleadas temáticas por dimensión."),
        ("Bosses",            "Jefe final al cierre de cada dimensión."),
        ("Salud",             "Sistema numérico con daño recibido."),
        ("Estado Eufórico",   "Se activa al combinar bloqueo y kills."),
        ("Buff temporal",     "Stats mejoradas durante la euforia."),
        ("Regeneración",      "Recuperación de vida en estado eufórico."),
    ]
    cols = 2
    cardW, cardH, gapX, gapY = 6.0, 0.65, 0.25, 0.1
    startX, startY = 0.6, 2.95
    for i, (t, d) in enumerate(items):
        cx = startX + (i % cols) * (cardW + gapX)
        cy = startY + (i // cols) * (cardH + gapY)
        # hairline separator
        line(cx, cy + cardH, cx + cardW, cy + cardH, C["border"], width=0.5)
        text(f"{i+1:02d}", cx, cy + 0.1, 10, C["accent"], font=MONO,
             char_spacing=1, box_w=0.45, box_h=cardH, valign="middle")
        text(t, cx + 0.55, cy + 0.08, 13, C["ink"], font=SERIFB,
             box_h=0.3, valign="middle")
        text(d, cx + 0.55, cy + 0.38, 11, C["inkSoft"], font=SERIFI)

    footer(4, TOTAL, label="MDA · Mechanics")

# ============== 5 · DYNAMICS ==============
def slide5():
    c.setFillColor(C["bgLight"]); c.rect(0, 0, W, H, stroke=0, fill=1)
    draw_chrome()
    eyebrow("II.  MDA  ·  02  DYNAMICS")
    big_title("Dinámicas emergentes")
    title_rule()

    text("Lo que ocurre cuando el jugador entra en contacto con las reglas.",
         0.6, 2.4, 13, C["inkSoft"], font=SERIFI)

    items = [
        ("01", "Movimiento constante",  "Esquivar y reposicionarse es parte del combate."),
        ("02", "Posicionamiento",       "Lectura del espacio frente a hordas."),
        ("03", "Bloqueo estratégico",   "Defensa activa como recurso de supervivencia."),
        ("04", "Incentivo ofensivo",    "Eliminar rápido acelera el estado eufórico."),
        ("05", "Aprendizaje",           "Lectura de patrones de enemigos y jefes."),
        ("06", "Presión creciente",     "El ritmo aumenta partida a partida."),
    ]
    for i, (n, t, d) in enumerate(items):
        col = i % 3
        row = i // 3
        cx = 0.6 + col * 4.15
        cy = 2.95 + row * 1.65
        rect(cx, cy, 3.95, 1.45, fill=C["cardLight"],
             stroke=C["border"], stroke_w=0.5)
        text(n, cx + 0.3, cy + 0.2, 22, C["accent"], font=SERIFB)
        text(t, cx + 0.3, cy + 0.7, 13, C["ink"], font=SERIFB,
             box_w=3.4)
        paragraph(d, cx + 0.3, cy + 1.0, 3.4, 11, C["inkSoft"],
                  font=SERIFI, max_lines=2, leading=14)

    # tempo
    text("RITMO DE PARTIDA", 0.6, 6.25, 9, C["accent"], font=MONO, char_spacing=2.5)
    steps = ["Inicio", "Más enemigos", "Presión creciente", "Jefe final", "Nueva dimensión"]
    stepW = (13.33 - 1.2) / len(steps)
    for i, st in enumerate(steps):
        sx = 0.6 + i * stepW
        text(st, sx, 6.55, 11, C["inkSoft"], font=SERIF, box_w=stepW - 0.2)
        if i < len(steps) - 1:
            line(sx + stepW - 0.25, 6.62, sx + stepW - 0.05, 6.62,
                 C["accent"], width=0.8)
            arrow_head(sx + stepW - 0.05, 6.62, 0, C["accent"], size=0.07)

    footer(5, TOTAL, label="MDA · Dynamics")

# ============== 6 · AESTHETICS (8 HUNICKE) ==============
def slide6():
    c.setFillColor(C["bgLight"]); c.rect(0, 0, W, H, stroke=0, fill=1)
    draw_chrome()
    eyebrow("II.  MDA  ·  03  AESTHETICS")
    big_title("Las ocho categorías de Hunicke et al.")
    title_rule()

    text("Identificamos cuáles aplican al juego y cuáles quedan fuera de alcance.",
         0.6, 2.4, 13, C["inkSoft"], font=SERIFI)

    items = [
        ("SENSATION",  "Combate frenético, feedback del lanzamiento y del estado eufórico.", True),
        ("FANTASY",    "Universo bizarro: un laboratorio que se rebela contra su dueño.",     True),
        ("NARRATIVE",  "Dr. Emilio intenta volver a casa tras un experimento fallido.",       True),
        ("CHALLENGE",  "Supervivencia frente a oleadas y jefes con dificultad creciente.",    True),
        ("FELLOWSHIP", "No aplica: propuesta single-player offline sin interacción social.",  False),
        ("DISCOVERY",  "Cada dimensión introduce nuevos enemigos y escenarios.",              True),
        ("EXPRESSION", "No contemplada explícitamente en el GDD (sin customización).",        False),
        ("SUBMISSION", "Loop arcade y rejugabilidad: la partida corta como propuesta.",       True),
    ]
    cols, rows = 4, 2
    startX, startY = 0.6, 2.9
    cardW, cardH, gapX, gapY = 3.0, 1.95, 0.13, 0.2
    for i, (t, d, ok) in enumerate(items):
        ci = i % cols; ri = i // cols
        cx = startX + ci * (cardW + gapX)
        cy = startY + ri * (cardH + gapY)
        rect(cx, cy, cardW, cardH, fill=C["cardLight"],
             stroke=C["border"], stroke_w=0.5)
        # left accent bar only when applies
        if ok:
            rect(cx, cy, 0.04, cardH, fill=C["accent"])
        # status pill
        pill_col = C["accent"] if ok else C["mutedSoft"]
        pillW = 0.6
        rect(cx + cardW - pillW - 0.15, cy + 0.18, pillW, 0.26,
             stroke=pill_col, stroke_w=0.7)
        text("SÍ" if ok else "NO", cx + cardW - pillW - 0.15, cy + 0.18,
             9, pill_col, font=MONO, char_spacing=2,
             box_w=pillW, box_h=0.26, align="center", valign="middle")
        text(t, cx + 0.2, cy + 0.55, 15, C["ink"] if ok else C["muted"],
             font=SERIFB, char_spacing=1.2)
        paragraph(d, cx + 0.2, cy + 0.95, cardW - 0.4, 10,
                  C["inkSoft"] if ok else C["muted"], font=SERIF,
                  max_lines=5, leading=13)

    footer(6, TOTAL, label="MDA · Aesthetics")

# ============== 7 · CADENA MDA ==============
def slide7():
    c.setFillColor(C["bgLight"]); c.rect(0, 0, W, H, stroke=0, fill=1)
    draw_chrome()
    eyebrow("II.  MDA  ·  CADENA COMPLETA")
    big_title("Ejemplo: Estado Eufórico")
    title_rule()
    text("Cómo una mecánica se convierte en una experiencia estética.",
         0.6, 2.4, 13, C["inkSoft"], font=SERIFI)

    chain = [
        ("MECÁNICA", "Eliminar enemigos y bloquear ataques activa el estado eufórico."),
        ("DINÁMICA", "El jugador combina ofensiva y defensa para acelerar su activación."),
        ("ESTÉTICA", "Challenge · Sensation · Submission."),
    ]
    startY = 3.0
    for i, (lbl, body) in enumerate(chain):
        cx = 0.6 + i * 4.23
        rect(cx, startY, 3.95, 3.3, fill=C["cardLight"],
             stroke=C["border"], stroke_w=0.6)
        text(f"0{i+1}", cx + 0.3, startY + 0.3, 26, C["accent"], font=SERIFB)
        text(lbl, cx + 0.3, startY + 0.95, 11, C["accent2"], font=MONO, char_spacing=3)
        rect(cx + 0.3, startY + 1.3, 0.5, 0.03, fill=C["accent"])
        paragraph(body, cx + 0.3, startY + 1.55, 3.35, 13, C["ink"],
                  font=SERIF, leading=19, max_lines=4)
        if i < 2:
            ax = cx + 3.95
            line(ax + 0.05, startY + 3.3/2, ax + 0.4, startY + 3.3/2,
                 C["accent"], width=1.0)
            arrow_head(ax + 0.4, startY + 3.3/2, 0, C["accent"], size=0.09)

    line(0.6, 6.55, 13.33-0.6, 6.55, C["rule"], width=0.5)
    text("Una misma acción alimenta defensa, ofensiva y recompensa emocional.",
         0.6, 6.7, 12, C["accent2"], font=SERIFI,
         box_w=13.33-1.2, align="center")

    footer(7, TOTAL, label="MDA · Cadena")

# ============== 8 · TÉTRADA DE SCHELL ==============
def slide8():
    c.setFillColor(C["bgLight"]); c.rect(0, 0, W, H, stroke=0, fill=1)
    draw_chrome()
    eyebrow("III.  TÉTRADA DE SCHELL")
    big_title("Los cuatro elementos y sus relaciones")
    title_rule()
    text("Diagrama de influencias recíprocas entre Mecánica, Estética, Tecnología e Historia.",
         0.6, 2.4, 13, C["inkSoft"], font=SERIFI)

    nodes = {
        "MECÁNICA":   {"cx": 4.3, "cy": 3.4},
        "ESTÉTICA":   {"cx": 6.7, "cy": 5.0},
        "TECNOLOGÍA": {"cx": 4.3, "cy": 6.6},
        "HISTORIA":   {"cx": 1.9, "cy": 5.0},
    }
    nodeW, nodeH = 2.5, 1.0

    pairs = [("MECÁNICA","ESTÉTICA"), ("MECÁNICA","TECNOLOGÍA"),
             ("MECÁNICA","HISTORIA"), ("ESTÉTICA","TECNOLOGÍA"),
             ("ESTÉTICA","HISTORIA"), ("TECNOLOGÍA","HISTORIA")]
    for a, b in pairs:
        na, nb = nodes[a], nodes[b]
        bidir_arrow(na["cx"], na["cy"], nb["cx"], nb["cy"],
                    C["accent"], width=1.0, inset=0.68)

    # tiny central marker
    circle(4.3, 5.0, 0.08, fill=C["accent2"])

    content = {
        "MECÁNICA":   ["Combate en tiempo real", "Bosses por dimensión", "Estado eufórico"],
        "ESTÉTICA":   ["Humor absurdo", "Mundos bizarros", "Feedback frenético"],
        "TECNOLOGÍA": ["Unity · Modelos 3D", "Cámara cenital 53°", "PC · Offline"],
        "HISTORIA":   ["Dr. Emilio", "Portal fallido", "Volver a casa"],
    }
    for name, n in nodes.items():
        cx, cy = n["cx"] - nodeW/2, n["cy"] - nodeH/2
        rect(cx, cy, nodeW, nodeH, fill=C["cardLight"],
             stroke=C["border"], stroke_w=0.7)
        rect(cx, cy, 0.04, nodeH, fill=C["accent"])
        text(name, cx + 0.2, cy + 0.13, 11, C["accent2"], font=MONO,
             char_spacing=3)
        for j, it in enumerate(content[name]):
            text("· " + it, cx + 0.2, cy + 0.38 + j*0.2,
                 9, C["ink"], font=SERIF)

    # Right panel — influencias
    px, py, pw, ph = 8.7, 3.0, 4.1, 3.9
    rect(px, py, pw, ph, fill=C["cardLight"], stroke=C["border"], stroke_w=0.6)
    text("INFLUENCIAS RECÍPROCAS", px + 0.25, py + 0.2, 9, C["accent"],
         font=MONO, char_spacing=2.5)
    rect(px + 0.25, py + 0.55, 0.4, 0.03, fill=C["accent"])

    infl = [
        ("HISTORIA  →  MECÁNICA",   "El portal fallido justifica oleadas y bosses por dimensión."),
        ("MECÁNICA  →  ESTÉTICA",   "El combate frenético y la euforia generan sensation y submission."),
        ("TECNOLOGÍA  →  MECÁNICA", "La cámara isométrica en Unity habilita el combate con apuntado."),
        ("ESTÉTICA  →  HISTORIA",   "El tono absurdo construye el universo bizarro del laboratorio."),
    ]
    yy = py + 0.8
    for lbl, body in infl:
        text(lbl, px + 0.25, yy, 9, C["accent2"], font=MONO, char_spacing=1.5)
        paragraph(body, px + 0.25, yy + 0.22, pw - 0.5, 10, C["ink"],
                  font=SERIF, max_lines=3, leading=13)
        yy += 0.82

    footer(8, TOTAL, label="Tétrada de Schell")

# ============== 9 · RELACIÓN ==============
def slide9():
    c.setFillColor(C["bgLight"]); c.rect(0, 0, W, H, stroke=0, fill=1)
    draw_chrome()
    eyebrow("IV.  RELACIÓN ENTRE LOS ELEMENTOS")
    big_title("Coherencia del sistema")
    title_rule()
    text("La articulación entre narrativa, sistemas y presentación como columna vertebral del diseño.",
         0.6, 2.4, 13, C["inkSoft"], font=SERIFI)

    rels = [
        ("HISTORIA",   "DIMENSIONES", "La narrativa justifica cada mundo y sus enemigos."),
        ("MECÁNICA",   "DESAFÍO",     "Cada dimensión se traduce en un puzzle jugable."),
        ("ESTÉTICA",   "MEMORIA",     "El universo absurdo vuelve al juego inolvidable."),
        ("TECNOLOGÍA", "FLUIDEZ",     "Unity garantiza una jugabilidad sólida y estable."),
    ]
    y = 3.0
    for a, b, d in rels:
        rect(0.6, y, 12.1, 0.78, stroke=C["border"], stroke_w=0.5)
        rect(0.6, y, 0.04, 0.78, fill=C["accent"])
        text(a, 0.9, y, 12, C["accent2"], font=MONO,
             char_spacing=3, box_h=0.78, valign="middle")
        line(3.0, y + 0.39, 3.5, y + 0.39, C["accent"], width=0.8)
        arrow_head(3.5, y + 0.39, 0, C["accent"], size=0.08)
        text(b, 3.8, y, 12, C["ink"], font=SERIFB,
             char_spacing=2, box_h=0.78, valign="middle")
        text(d, 6.3, y, 12, C["inkSoft"], font=SERIFI,
             box_h=0.78, valign="middle")
        y += 0.88

    # conclusion
    line(0.6, 6.55, 13.33-0.6, 6.55, C["rule"], width=0.5)
    text("CONCLUSIÓN", 0.6, 6.7, 9, C["accent"], font=MONO, char_spacing=2.5)
    text("El diseño de Freaky Super Mesh mantiene coherencia entre narrativa, sistemas y presentación visual.",
         2.2, 6.7, 12, C["ink"], font=SERIFI)

    footer(9, TOTAL, label="Relación")

# ============== 10 · REPARTO GDD ==============
def slide10():
    c.setFillColor(C["bgLight"]); c.rect(0, 0, W, H, stroke=0, fill=1)
    draw_chrome()
    eyebrow("V.  REPARTO DE TAREAS  ·  EQUIPO DINAMITA")
    big_title("Autoría por sección del GDD")
    title_rule()
    text("Secciones del Game Design Document · completar con el integrante responsable.",
         0.6, 2.4, 13, C["inkSoft"], font=SERIFI)

    sections = [
        ("01", "Concepto General"),
        ("02", "Narrativa y contexto"),
        ("03", "Mecánicas principales"),
        ("04", "Progresión y estructura de niveles"),
        ("05", "Listado de assets"),
        ("06", "Desarrollo"),
        ("07", "Referencias"),
        ("08", "Uso de IA"),
    ]
    cols = 2; rowsPerCol = 4
    startX, startY = 0.6, 2.95
    colW = 6.1; gapX = 0.3; rowH = 0.9
    for i, (n, sect) in enumerate(sections):
        col = i // rowsPerCol
        row = i % rowsPerCol
        cx = startX + col * (colW + gapX)
        cy = startY + row * rowH
        rect(cx, cy, colW, rowH - 0.15, stroke=C["border"], stroke_w=0.5)
        rect(cx, cy, 0.04, rowH - 0.15, fill=C["accent"])
        text(n, cx + 0.25, cy, 18, C["accent2"], font=SERIFB,
             box_h=rowH-0.15, valign="middle")
        text(sect, cx + 0.95, cy + 0.1, 13, C["ink"], font=SERIFB,
             box_h=0.35, valign="middle")
        text("Integrante:", cx + 0.95, cy + 0.42, 9, C["accent"],
             font=MONO, char_spacing=2)
        text("_____________________________", cx + 2.0, cy + 0.42, 11,
             C["muted"], font=MONO)

    line(0.6, 6.55, 13.33-0.6, 6.55, C["rule"], width=0.5)
    text("EQUIPO DINAMITA  ·  FIDMAY  ·  NUÑEZ DI MEO  ·  HUG",
         0.6, 6.7, 10, C["accent2"], font=MONO,
         box_w=13.33-1.2, align="center", char_spacing=3)

    footer(10, TOTAL, label="Reparto GDD")

# ============== 11 · REFERENCIAS ==============
def slide11():
    c.setFillColor(C["bgLight"]); c.rect(0, 0, W, H, stroke=0, fill=1)
    draw_chrome()
    eyebrow("VI.  REFERENCIAS E INFLUENCIAS")
    big_title("Marcos de inspiración")
    title_rule()

    refs = [
        ("MegaBonk",            "Diversión arcade inmediata y acción directa."),
        ("Vampire Survivors",   "Supervivencia frente a hordas, progresión y rejugabilidad."),
        ("Hotline Miami",       "Ritmo frenético, presión constante y combate intenso."),
        ("Rick and Morty",      "Humor absurdo y dimensiones alternativas."),
        ("Dr. Mundo — LoL",     "Científico descontrolado, poder bruto."),
        ("Heimerdinger — LoL",  "Inventor científico, estética tecnológica."),
    ]
    for i, (t, d) in enumerate(refs):
        col = i % 3
        row = i // 3
        cx = 0.6 + col * 4.15
        cy = 2.9 + row * 1.9
        rect(cx, cy, 3.95, 1.7, fill=C["cardLight"],
             stroke=C["border"], stroke_w=0.5)
        text(f"REF · 0{i+1}", cx + 0.3, cy + 0.25, 9, C["accent"], font=MONO,
             char_spacing=2.5)
        text(t, cx + 0.3, cy + 0.55, 17, C["ink"], font=SERIFB,
             box_w=3.35)
        rect(cx + 0.3, cy + 1.0, 0.5, 0.03, fill=C["accent"])
        paragraph(d, cx + 0.3, cy + 1.15, 3.35, 11, C["inkSoft"],
                  font=SERIFI, max_lines=3, leading=14)

    footer(11, TOTAL, label="Referencias")

# ============== 12 · EVIDENCIAS DE DESARROLLO (NEW) ==============
def slide12():
    c.setFillColor(C["bgLight"]); c.rect(0, 0, W, H, stroke=0, fill=1)
    draw_chrome()
    eyebrow("VII.  DESARROLLO DEL PROYECTO")
    big_title("Evidencias de producción")
    title_rule()
    text("Material visual del trabajo: concept art de personajes, escenas en Unity y registros de gameplay.",
         0.6, 2.4, 13, C["inkSoft"], font=SERIFI)

    # 4 image placeholders in 2x2 grid
    placeholders = [
        ("PERSONAJES",          "Dr. Emilio · enemigos por dimensión"),
        ("ESCENAS EN UNITY",    "Capturas de editor y escenarios"),
        ("GAMEPLAY",            "Videos de combate y bosses"),
        ("ITERACIÓN DE DISEÑO", "Pruebas, bocetos, prototipos"),
    ]
    cols = 2
    startX, startY = 0.6, 2.95
    cardW, cardH, gapX, gapY = 6.05, 1.85, 0.2, 0.2
    for i, (t, d) in enumerate(placeholders):
        ci = i % cols; ri = i // cols
        cx = startX + ci * (cardW + gapX)
        cy = startY + ri * (cardH + gapY)
        # dashed placeholder box
        c.saveState()
        c.setStrokeColor(C["rule"]); c.setLineWidth(0.8)
        c.setDash(6, 4)
        c.rect(IN(cx), H - IN(cy) - IN(cardH), IN(cardW), IN(cardH),
               stroke=1, fill=0)
        c.restoreState()
        # paper wash
        rect(cx + 0.04, cy + 0.04, cardW - 0.08, cardH - 0.08,
             fill=C["paper"])
        # label block
        text(t, cx + 0.3, cy + 0.35, 13, C["accent2"], font=MONO,
             char_spacing=3)
        rect(cx + 0.3, cy + 0.72, 0.5, 0.03, fill=C["accent"])
        text(d, cx + 0.3, cy + 0.9, 12, C["ink"], font=SERIFI)
        # corner marker
        text("[  INSERTAR  ]", cx + cardW - 1.5, cy + cardH - 0.35,
             9, C["muted"], font=MONO, char_spacing=2,
             box_w=1.3, align="right")

    footer(12, TOTAL, label="Evidencias")

# ============== 13 · CIERRE ==============
def slide13():
    c.setFillColor(C["bg"]); c.rect(0, 0, W, H, stroke=0, fill=1)
    draw_chrome(dark=True)

    # Editorial closer
    text("GRACIAS", 0.6, 2.4, 10, C["accent"], font=MONO,
         box_w=13.33-1.2, align="center", char_spacing=8)
    rect(13.33/2 - 0.75, 2.85, 1.5, 0.04, fill=C["accent"])

    text("Freaky Super Mesh", 0.6, 3.2, 52, C["text"], font=SERIFB,
         box_w=13.33-1.2, align="center")
    text("Del concepto a la jugabilidad — un ejercicio de diseño integral.",
         0.6, 4.4, 16, C["rule"], font=SERIFI,
         box_w=13.33-1.2, align="center")

    # signature block
    line(13.33/2 - 2, 5.5, 13.33/2 + 2, 5.5, C["mutedSoft"], width=0.5)
    text("EQUIPO DINAMITA", 0.6, 5.7, 10, C["accent"], font=MONO,
         box_w=13.33-1.2, align="center", char_spacing=3)
    text("Fidmay  ·  Nuñez Di Meo  ·  Hug",
         0.6, 6.0, 13, C["rule"], font=SERIF,
         box_w=13.33-1.2, align="center")
    text("UADE  ·  Diseño de Videojuegos  ·  2026",
         0.6, 6.3, 10, C["mutedSoft"], font=MONO,
         box_w=13.33-1.2, align="center", char_spacing=2)

    footer(13, TOTAL, dark=True)

for fn in [slide1, slide2, slide3, slide4, slide5, slide6,
           slide7, slide8, slide9, slide10, slide11, slide12, slide13]:
    fn()
    c.showPage()

c.save()
print("WROTE:", OUT)
