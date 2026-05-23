# -*- coding: utf-8 -*-
"""Genera Freaky_Super_Mesh.pdf - slides horizontales 16:9."""
from reportlab.pdfgen import canvas
from reportlab.lib.colors import HexColor
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
import os

# 13.33 x 7.5 inches in points
W, H = 13.33 * 72, 7.5 * 72  # 959.76 x 540

# ---- palette ----
C = {
    "bg":        HexColor("#0A0E27"),
    "bgLight":   HexColor("#F5F3FF"),
    "card":      HexColor("#1A1F3A"),
    "cardLight": HexColor("#FFFFFF"),
    "primary":   HexColor("#7C3AED"),
    "accent":    HexColor("#00F5D4"),
    "hot":       HexColor("#FF3D7F"),
    "gold":      HexColor("#FFD166"),
    "text":      HexColor("#F5F3FF"),
    "textDark":  HexColor("#1A1F3A"),
    "muted":     HexColor("#A78BFA"),
    "mutedDark": HexColor("#64748B"),
    "border":    HexColor("#E5E7EB"),
}

OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "Freaky_Super_Mesh.pdf")
c = canvas.Canvas(OUT, pagesize=(W, H))

# Try to register nicer fonts from Windows
FONTS = {"impact": "Helvetica-Bold", "sans": "Helvetica", "sansB": "Helvetica-Bold",
         "sansI": "Helvetica-Oblique", "mono": "Courier-Bold"}
try:
    pdfmetrics.registerFont(TTFont("Impact", r"C:\Windows\Fonts\impact.ttf"))
    FONTS["impact"] = "Impact"
except Exception:
    pass
try:
    pdfmetrics.registerFont(TTFont("Calibri",   r"C:\Windows\Fonts\calibri.ttf"))
    pdfmetrics.registerFont(TTFont("CalibriB",  r"C:\Windows\Fonts\calibrib.ttf"))
    pdfmetrics.registerFont(TTFont("CalibriI",  r"C:\Windows\Fonts\calibrii.ttf"))
    FONTS["sans"]  = "Calibri"
    FONTS["sansB"] = "CalibriB"
    FONTS["sansI"] = "CalibriI"
except Exception:
    pass
try:
    pdfmetrics.registerFont(TTFont("Consolas",  r"C:\Windows\Fonts\consola.ttf"))
    pdfmetrics.registerFont(TTFont("ConsolasB", r"C:\Windows\Fonts\consolab.ttf"))
    FONTS["mono"] = "ConsolasB"
except Exception:
    pass

IMP = FONTS["impact"]; SANS = FONTS["sans"]; SANSB = FONTS["sansB"]
SANSI = FONTS["sansI"]; MONO = FONTS["mono"]

# Helpers: reportlab has y origin at bottom. We'll keep x from left, y from TOP (like pptx)
def Y(y_top_inches):
    return H - y_top_inches * 72

def IN(v):  # inches to points
    return v * 72

def rect(x, y, w, h, fill=None, stroke=None, stroke_w=0):
    c.saveState()
    if fill is not None:
        c.setFillColor(fill)
    if stroke is not None:
        c.setStrokeColor(stroke)
        c.setLineWidth(stroke_w)
    # y here is top-of-box in inches; convert to reportlab (bottom origin)
    rx, ry = IN(x), H - IN(y) - IN(h)
    c.rect(rx, ry, IN(w), IN(h), stroke=1 if stroke else 0, fill=1 if fill else 0)
    c.restoreState()

def circle(cx, cy, r, fill=None, alpha=1.0):
    c.saveState()
    if alpha < 1.0:
        c.setFillAlpha(alpha)
    if fill is not None:
        c.setFillColor(fill)
    c.circle(IN(cx), H - IN(cy), IN(r), stroke=0, fill=1)
    c.restoreState()

def text(s, x, y_top, size, color, font=SANS, align="left", valign="top",
         box_w=None, box_h=None, char_spacing=0):
    """Draw single-line text. x,y_top in inches. If box_w set and align center/right, anchor in that width."""
    c.saveState()
    c.setFillColor(color)
    c.setFont(font, size)
    # compute string width with char spacing applied
    base_w = c.stringWidth(s, font, size)
    extra = char_spacing * max(0, len(s) - 1)
    tw = base_w + extra
    tx = IN(x)
    if box_w is not None and align == "center":
        tx = IN(x) + (IN(box_w) - tw) / 2
    elif box_w is not None and align == "right":
        tx = IN(x) + IN(box_w) - tw
    # vertical anchor
    if box_h is not None and valign == "middle":
        ty = H - IN(y_top) - IN(box_h)/2 - size*0.35
    elif box_h is not None and valign == "bottom":
        ty = H - IN(y_top) - IN(box_h) + size*0.2
    else:
        ty = H - IN(y_top) - size*0.85
    if char_spacing:
        to = c.beginText(tx, ty)
        to.setFont(font, size)
        to.setFillColor(color)
        to.setCharSpace(char_spacing)
        to.textOut(s)
        c.drawText(to)
    else:
        c.drawString(tx, ty, s)
    c.restoreState()

def wrap_text(s, font, size, max_w_pts):
    """Simple word wrap → list of lines."""
    words = s.split()
    lines, cur = [], ""
    for w in words:
        test = (cur + " " + w).strip()
        if pdfmetrics.stringWidth(test, font, size) <= max_w_pts:
            cur = test
        else:
            if cur:
                lines.append(cur)
            cur = w
    if cur:
        lines.append(cur)
    return lines

def paragraph(s, x, y_top, box_w, size, color, font=SANS, leading=None, max_lines=None):
    lines = wrap_text(s, font, size, IN(box_w))
    if max_lines:
        lines = lines[:max_lines]
    lh = leading or size * 1.25
    c.saveState()
    c.setFillColor(color)
    c.setFont(font, size)
    for i, ln in enumerate(lines):
        ty = H - IN(y_top) - size*0.85 - i*lh
        c.drawString(IN(x), ty, ln)
    c.restoreState()
    return len(lines) * lh

def line(x1, y1, x2, y2, color, width=1.5, dash=None):
    c.saveState()
    c.setStrokeColor(color)
    c.setLineWidth(width)
    if dash:
        c.setDash(dash)
    c.line(IN(x1), H - IN(y1), IN(x2), H - IN(y2))
    c.restoreState()

def arrow_head(xt, yt, angle_deg, color, size=0.12):
    """Draw a filled triangular arrowhead at (xt,yt) pointing in angle_deg direction."""
    import math
    rad = math.radians(angle_deg)
    # tip
    tx, ty = IN(xt), H - IN(yt)
    # base points
    bl_x = tx - IN(size) * math.cos(rad) + IN(size*0.55) * math.cos(rad + math.pi/2)
    bl_y = ty + IN(size) * math.sin(rad) - IN(size*0.55) * math.sin(rad + math.pi/2)
    br_x = tx - IN(size) * math.cos(rad) + IN(size*0.55) * math.cos(rad - math.pi/2)
    br_y = ty + IN(size) * math.sin(rad) - IN(size*0.55) * math.sin(rad - math.pi/2)
    c.saveState()
    c.setFillColor(color)
    c.setStrokeColor(color)
    p = c.beginPath()
    p.moveTo(tx, ty)
    p.lineTo(bl_x, bl_y)
    p.lineTo(br_x, br_y)
    p.close()
    c.drawPath(p, stroke=0, fill=1)
    c.restoreState()

def bidir_arrow(x1, y1, x2, y2, color, width=1.5, inset=0.0):
    """Draw a line with arrowheads at both ends. inset shortens both ends."""
    import math
    dx, dy = x2 - x1, y2 - y1
    dist = math.hypot(dx, dy)
    if dist == 0:
        return
    ux, uy = dx / dist, dy / dist
    sx, sy = x1 + ux * inset, y1 + uy * inset
    ex, ey = x2 - ux * inset, y2 - uy * inset
    line(sx, sy, ex, ey, color, width=width)
    # angle in pptx-style coords: y grows downward, so invert dy for atan2
    ang_end  = math.degrees(math.atan2(-(ey - sy), (ex - sx)))
    ang_start = math.degrees(math.atan2(-(sy - ey), (sx - ex)))
    arrow_head(ex, ey, ang_end, color)
    arrow_head(sx, sy, ang_start, color)

def draw_chrome(dark=False):
    bar = C["accent"] if dark else C["primary"]
    rect(0, 0, 13.33, 0.12, fill=bar)
    rect(0, 0.12, 0.12, 1.2, fill=C["hot"])
    rect(13.33-0.12, 7.5-1.2, 0.12, 1.08, fill=C["accent"])
    rect(0, 7.5-0.12, 13.33, 0.12, fill=bar)

def footer(n, total, dark=False):
    tc = C["muted"] if dark else C["mutedDark"]
    text("FREAKY SUPER MESH", 0.4, 7.5-0.55, 9, tc, font=MONO,
         box_w=4, box_h=0.35, valign="middle", char_spacing=1.5)
    text(f"{n:02d} / {total:02d}", 13.33-2.4, 7.5-0.55, 9, tc, font=MONO,
         box_w=2, box_h=0.35, valign="middle", align="right")

def section_tag(tag, color):
    rect(0.6, 0.55, 0.25, 0.25, fill=color)
    text(tag, 0.95, 0.5, 11, color, font=MONO,
         box_h=0.35, valign="middle", char_spacing=2)

def big_title(s, size=40, color=None):
    color = color or C["textDark"]
    text(s, 0.6, 0.9, size, color, font=IMP,
         box_w=13.33-1.2, box_h=1.1, valign="middle")

TOTAL = 12

# ============== SLIDE 1 - PORTADA ==============
def slide1():
    c.setFillColor(C["bg"]); c.rect(0, 0, W, H, stroke=0, fill=1)
    draw_chrome(dark=True)
    # portal (top right only, kept away from text)
    circle(13.33-1.2, 1.1, 2.4, fill=C["primary"], alpha=0.65)
    circle(13.33-1.2, 1.1, 1.75, fill=C["hot"], alpha=0.55)
    circle(13.33-1.2, 1.1, 1.0, fill=C["accent"], alpha=0.65)
    # small gold dot bottom-right corner
    circle(13.33-0.6, 7.5-0.8, 0.25, fill=C["gold"], alpha=0.8)

    text("ANÁLISIS TEÓRICO · DISEÑO DE VIDEOJUEGO", 0.8, 1.4, 13, C["accent"],
         font=MONO, char_spacing=2.5)
    text("FREAKY",     0.7, 1.9, 100, C["text"], font=IMP, char_spacing=2)
    text("SUPER MESH", 0.7, 3.4, 100, C["hot"],  font=IMP, char_spacing=2)

    rect(0.8, 5.2, 1.5, 0.06, fill=C["accent"])

    # Integrantes
    text("INTEGRANTES", 0.8, 5.45, 10, C["muted"], font=MONO, char_spacing=2)
    text("Fidmay Agustín",        0.8, 5.7,  13, C["text"])
    text("Nuñez Di Meo Dante",    0.8, 5.95, 13, C["text"])
    text("Hug Juan Ignacio",      0.8, 6.2,  13, C["text"])

    # Profesores
    text("PROFESORES", 5.4, 5.45, 10, C["muted"], font=MONO, char_spacing=2)
    text("Aparicio Lucas",           5.4, 5.7,  13, C["text"])
    text("Ulrich Gonzalo Ezequiel",  5.4, 5.95, 13, C["text"])

    # Meta
    text("UADE", 9.3, 5.45, 11, C["accent"], font=MONO,
         box_w=3.5, align="right", char_spacing=2)
    text("Primer Cuatrimestre · Turno Tarde", 9.3, 5.7, 12, C["text"],
         box_w=3.5, align="right")
    text("2026  ·  24 / 04 / 26", 9.3, 5.95, 12, C["gold"], font=SANSB,
         box_w=3.5, align="right")

    footer(1, TOTAL, dark=True)

# ============== SLIDE 2 - RESUMEN ==============
def slide2():
    c.setFillColor(C["bgLight"]); c.rect(0, 0, W, H, stroke=0, fill=1)
    draw_chrome()
    section_tag("RESUMEN DEL JUEGO", C["primary"])
    big_title("Action Rogue-lite dimensional")

    # main card
    rect(0.6, 2.1, 7.5, 4.5, fill=C["cardLight"],
         stroke=C["primary"], stroke_w=0.75)
    rect(0.6, 2.1, 0.12, 4.5, fill=C["hot"])

    # rows
    y = 2.4
    def field(label, value, is_title=False):
        nonlocal y
        text(label, 0.95, y, 10, C["primary"], font=MONO, char_spacing=1.5)
        if is_title:
            text(value, 0.95, y+0.3, 13, C["textDark"], font=SANSB)
        else:
            text(value, 0.95, y+0.3, 12, C["textDark"])

    field("PLATAFORMA", "PC · 1 jugador · Offline")
    y += 0.8
    field("PROTAGONISTA", "Dr. Emilio", is_title=True)
    paragraph("Científico atrapado entre dimensiones tras un experimento de portales fallido.",
              0.95, y+0.58, 7.0, 11, C["mutedDark"], font=SANSI, max_lines=2)
    y += 1.25
    field("CÁMARA", "Cenital inclinada · estilo isométrico")
    y += 0.75
    field("LOOP", "Atravesar mundos absurdos y vencer jefes finales.")

    # Pilares
    text("PILARES DE DISEÑO", 8.5, 2.15, 11, C["hot"], font=MONO, char_spacing=2.5)
    pillars = [
        ("Combate frenético",  C["hot"]),
        ("Mecánicas simples",  C["primary"]),
        ("Alta rejugabilidad", C["accent"]),
        ("Humor visual",       C["gold"]),
        ("Progreso por habilidad", C["hot"]),
    ]
    for i, (t, col) in enumerate(pillars):
        yy = 2.65 + i*0.78
        rect(8.5, yy, 4.3, 0.65, fill=C["cardLight"], stroke=col, stroke_w=0.75)
        rect(8.5, yy, 0.12, 0.65, fill=col)
        text(t, 8.75, yy, 13, C["textDark"], font=SANSB, box_h=0.65, valign="middle")

    footer(2, TOTAL)

# ============== SLIDE 3 - MDA FLOW ==============
def slide_mda_flow():
    c.setFillColor(C["bgLight"]); c.rect(0, 0, W, H, stroke=0, fill=1)
    draw_chrome()
    section_tag("FRAMEWORK MDA · HUNICKE, LEBLANC & ZUBEK", C["primary"])
    big_title("Del diseñador al jugador", size=36)
    text("La dirección del flujo es la clave del análisis MDA",
         0.6, 1.85, 13, C["mutedDark"], font=SANSI)

    # Three big nodes: MECHANICS → DYNAMICS → AESTHETICS
    nodes = [
        ("MECHANICS",  "Reglas, sistemas,\ncomponentes del juego.",     "Lo que diseñamos",         C["hot"]),
        ("DYNAMICS",   "Comportamiento\nen tiempo real al jugar.",      "Lo que emerge",            C["primary"]),
        ("AESTHETICS", "Respuestas emocionales\ndel jugador.",          "Lo que se siente",         C["accent"]),
    ]
    startX, startY = 0.7, 2.6
    nodeW, nodeH, gap = 3.6, 3.3, 0.55
    for i, (t, body, sub, col) in enumerate(nodes):
        cx = startX + i * (nodeW + gap)
        rect(cx, startY, nodeW, nodeH, fill=C["cardLight"],
             stroke=col, stroke_w=1)
        rect(cx, startY, nodeW, 0.45, fill=col)
        text(f"0{i+1}", cx + 0.2, startY, 11, C["cardLight"],
             font=MONO, box_h=0.45, valign="middle", char_spacing=1)
        text(t, cx, startY, 13, C["cardLight"], font=MONO,
             box_w=nodeW, box_h=0.45, align="center", valign="middle",
             char_spacing=2)
        text(sub, cx+0.3, startY+0.7, 11, col, font=MONO, char_spacing=1)
        # body (multiline)
        for j, ln in enumerate(body.split("\n")):
            text(ln, cx+0.3, startY+1.1+j*0.35, 14, C["textDark"],
                 font=SANSB, box_w=nodeW-0.6)

    # Arrows (designer direction)
    for i in range(2):
        ax = startX + (i+1) * nodeW + i * gap
        bidir_arrow(ax + 0.05, startY + nodeH/2,
                    ax + gap - 0.05, startY + nodeH/2,
                    C["primary"], width=2, inset=0)

    # bottom: dual perspective strip
    rect(0.6, 6.2, 12.1, 0.7, fill=C["bg"])
    text("DISEÑADOR →  parte de Mechanics  ·  JUGADOR ←  experimenta desde Aesthetics",
         0.6, 6.2, 13, C["accent"], font=MONO,
         box_w=12.1, box_h=0.7, align="center", valign="middle",
         char_spacing=1.5)

    footer(3, TOTAL)

# ============== SLIDE 4 - MECHANICS ==============
def slide3():
    c.setFillColor(C["bgLight"]); c.rect(0, 0, W, H, stroke=0, fill=1)
    draw_chrome()
    section_tag("MDA · 01 MECHANICS", C["hot"])
    big_title("Mecánicas", size=48)
    text("Las reglas del sistema", 0.6, 1.85, 13, C["mutedDark"], font=SANSI)

    items = [
        "Movimiento WASD", "Apuntado con mouse",
        "Lanzamiento de probetas", "Bloqueo con Shift",
        "Enemigos por oleadas", "Jefes de dimensión",
        "Sistema de salud", "Estado Eufórico",
        "Mejora temporal de stats", "Recuperación en euforia",
    ]
    cols = 2
    cardW, cardH, gapX, gapY = 6.0, 0.6, 0.25, 0.18
    startX, startY = 0.6, 2.6
    for i, t in enumerate(items):
        cx = startX + (i % cols) * (cardW + gapX)
        cy = startY + (i // cols) * (cardH + gapY)
        rect(cx, cy, cardW, cardH, fill=C["cardLight"], stroke=C["border"], stroke_w=0.5)
        # number circle
        circle(cx+0.3, cy+0.3, 0.18, fill=C["hot"])
        text(f"{i+1:02d}", cx+0.12, cy+0.12, 9, C["cardLight"], font=MONO,
             box_w=0.36, box_h=0.36, align="center", valign="middle", char_spacing=0.5)
        text(t, cx+0.6, cy, 13, C["textDark"], font=SANSB, box_h=cardH, valign="middle")

    # Objective
    rect(0.6, 6.15, 12.1, 0.7, fill=C["bg"])
    text("OBJETIVO", 0.9, 6.15, 11, C["accent"], font=MONO,
         box_h=0.7, valign="middle", char_spacing=2.5)
    text("Superar dimensiones y sobrevivir.", 2.4, 6.15, 15, C["text"],
         font=SANSB, box_h=0.7, valign="middle")

    footer(4, TOTAL)

# ============== SLIDE 4 - DYNAMICS ==============
def slide4():
    c.setFillColor(C["bgLight"]); c.rect(0, 0, W, H, stroke=0, fill=1)
    draw_chrome()
    section_tag("MDA · 02 DYNAMICS", C["primary"])
    big_title("Dinámicas emergentes", size=44)
    text("Lo que ocurre cuando el jugador se encuentra con las reglas",
         0.6, 1.85, 13, C["mutedDark"], font=SANSI)

    items = [
        ("01", "Movimiento constante",  "Esquivar y reposicionarse es obligatorio"),
        ("02", "Posicionamiento",       "Lectura del espacio frente a hordas"),
        ("03", "Bloqueo preciso",       "Defensa activa como recurso estratégico"),
        ("04", "Incentivo ofensivo",    "Matar rápido = activar modo eufórico"),
        ("05", "Aprendizaje",           "Leer patrones enemigos y jefes"),
        ("06", "Presión creciente",     "El sistema sube el ritmo progresivamente"),
    ]
    for i, (n, t, d) in enumerate(items):
        col = i % 3
        row = i // 3
        cx = 0.6 + col * 4.15
        cy = 2.55 + row * 1.8
        rect(cx, cy, 3.95, 1.55, fill=C["cardLight"],
             stroke=C["primary"], stroke_w=0.5)
        text(n, cx+0.25, cy+0.12, 26, C["primary"], font=IMP)
        text(t, cx+0.25, cy+0.55, 14, C["textDark"], font=SANSB)
        paragraph(d, cx+0.25, cy+0.9, 3.5, 10, C["mutedDark"], font=SANSI, max_lines=2)

    # Tempo strip
    steps = ["Inicio simple", "Más enemigos", "Presión creciente", "Jefe final", "Nueva dimensión"]
    stepW = 12.1 / len(steps)
    for i, st in enumerate(steps):
        sx = 0.6 + i * stepW
        fill = C["hot"] if i == len(steps)-1 else C["bg"]
        rect(sx, 6.2, stepW-0.08, 0.7, fill=fill)
        text(st, sx, 6.2, 10, C["text"], font=MONO,
             box_w=stepW-0.08, box_h=0.7, align="center", valign="middle",
             char_spacing=1)

    footer(5, TOTAL)

# ============== SLIDE 6 - AESTHETICS (8 categorías Hunicke) ==============
def slide_aesthetics():
    c.setFillColor(C["bg"]); c.rect(0, 0, W, H, stroke=0, fill=1)
    draw_chrome(dark=True)
    section_tag("MDA · 03 AESTHETICS", C["accent"])
    big_title("Las 8 categorías (Hunicke et al.)", size=36, color=C["text"])
    text("Cuáles buscamos generar en el jugador y cuáles no aplican",
         0.6, 1.85, 13, C["muted"], font=SANSI)

    # 4 x 2 grid - compact cards
    items = [
        ("SENSATION",  "Combate frenético, feedback audiovisual del lanzamiento de probetas y del estado eufórico.", True),
        ("FANTASY",    "Universo bizarro: laboratorio que se rebela contra su dueño; dimensiones absurdas.",        True),
        ("NARRATIVE",  "Dr. Emilio intenta volver a casa tras el fallo de su portal dimensional.",                  True),
        ("CHALLENGE",  "Supervivencia frente a oleadas y jefes; dificultad creciente partida a partida.",           True),
        ("FELLOWSHIP", "No aplica: el juego es single-player offline, sin interacción social.",                     False),
        ("DISCOVERY",  "Cada dimensión (desierto, supermercado, océano) trae nuevos enemigos y jefes.",             True),
        ("EXPRESSION", "No se contempla explícitamente en el GDD (sin customización declarada).",                   False),
        ("SUBMISSION", "Loop arcade + rejugabilidad: \"una partida más\" es la propuesta central.",                  True),
    ]
    cols, rows = 4, 2
    startX, startY = 0.6, 2.55
    cardW, cardH, gapX, gapY = 3.0, 2.1, 0.13, 0.2
    for i, (t, d, applies) in enumerate(items):
        c_i = i % cols
        r_i = i // cols
        cx = startX + c_i * (cardW + gapX)
        cy = startY + r_i * (cardH + gapY)
        col = C["accent"] if applies else C["mutedDark"]
        rect(cx, cy, cardW, cardH, fill=C["card"], stroke=col, stroke_w=1)
        # status pill
        pillW = 0.75
        rect(cx+cardW-pillW-0.15, cy+0.15, pillW, 0.28,
             fill=col if applies else C["card"],
             stroke=col, stroke_w=0.75)
        text("SÍ" if applies else "NO", cx+cardW-pillW-0.15, cy+0.15,
             9, C["bg"] if applies else C["mutedDark"], font=MONO,
             box_w=pillW, box_h=0.28, align="center", valign="middle",
             char_spacing=1.5)
        # title
        text(t, cx+0.2, cy+0.55, 17, col if applies else C["muted"], font=IMP,
             box_h=0.4, valign="middle", char_spacing=0.8)
        # description
        paragraph(d, cx+0.2, cy+1.05, cardW-0.4, 9.5,
                  C["text"] if applies else C["muted"], max_lines=5,
                  leading=12)

    footer(6, TOTAL, dark=True)

# ============== SLIDE 6 - CADENA MDA ==============
def slide6():
    c.setFillColor(C["bgLight"]); c.rect(0, 0, W, H, stroke=0, fill=1)
    draw_chrome()
    section_tag("CADENA COMPLETA MDA", C["gold"])
    big_title("Ejemplo: Estado Eufórico", size=40)
    text("Cómo una mecánica se convierte en emoción",
         0.6, 1.85, 13, C["mutedDark"], font=SANSI)

    chain = [
        ("MECÁNICA", "Eliminar enemigos y bloquear ataques activa el estado eufórico.", C["hot"], "M"),
        ("DINÁMICA", "El jugador combina ofensiva y defensa para activarlo rápidamente.", C["primary"], "D"),
        ("ESTÉTICA", "Challenge · Sensation · Expression.", C["accent"], "A"),
    ]
    for i, (lbl, body, col, icon) in enumerate(chain):
        cx = 0.6 + i * 4.23
        cy = 2.55
        rect(cx, cy, 3.95, 3.5, fill=C["cardLight"], stroke=col, stroke_w=1)
        rect(cx, cy, 3.95, 0.4, fill=col)
        text(lbl, cx, cy, 11, C["cardLight"], font=MONO,
             box_w=3.95, box_h=0.4, align="center", valign="middle",
             char_spacing=3)
        text(icon, cx+0.1, cy+0.5, 96, col, font=IMP,
             box_w=3.75, box_h=1.9, align="center", valign="middle")
        paragraph(body, cx+0.3, cy+2.45, 3.35, 12, C["textDark"], max_lines=3)
        if i < 2:
            text(">", cx+3.85, cy+1.4, 36, C["gold"], font=IMP,
                 box_w=0.55, box_h=0.7, align="center", valign="middle")

    rect(0.6, 6.3, 12.1, 0.55, fill=C["bg"])
    text("Una misma acción alimenta defensa, ofensiva y recompensa emocional.",
         0.6, 6.3, 12, C["accent"], font=SANSI,
         box_w=12.1, box_h=0.55, align="center", valign="middle")

    footer(7, TOTAL)

# ============== SLIDE - TÉTRADA (DIAGRAMA) ==============
def slide_tetrad():
    c.setFillColor(C["bgLight"]); c.rect(0, 0, W, H, stroke=0, fill=1)
    draw_chrome()
    section_tag("TÉTRADA DE SCHELL", C["primary"])
    big_title("Los cuatro elementos y sus relaciones", size=32)
    text("Diagrama de influencias recíprocas entre los elementos del juego",
         0.6, 1.85, 13, C["mutedDark"], font=SANSI)

    # ----- Diagram (left side) -----
    # Geometry in inches. Slide is 13.33 x 7.5.
    # Nodes positioned as a diamond fitted left of right panel (x=8.9).
    nodes = {
        "MECÁNICA":   {"cx": 4.5, "cy": 2.85, "col": C["hot"]},
        "ESTÉTICA":   {"cx": 7.0, "cy": 4.65, "col": C["gold"]},
        "TECNOLOGÍA": {"cx": 4.5, "cy": 6.45, "col": C["accent"]},
        "HISTORIA":   {"cx": 2.0, "cy": 4.65, "col": C["primary"]},
    }
    nodeW, nodeH = 2.6, 1.05

    # Arrows first so nodes overlap the endpoints cleanly
    pairs = [("MECÁNICA","ESTÉTICA"), ("MECÁNICA","TECNOLOGÍA"),
             ("MECÁNICA","HISTORIA"), ("ESTÉTICA","TECNOLOGÍA"),
             ("ESTÉTICA","HISTORIA"), ("TECNOLOGÍA","HISTORIA")]
    for a, b in pairs:
        na, nb = nodes[a], nodes[b]
        bidir_arrow(na["cx"], na["cy"], nb["cx"], nb["cy"],
                    C["primary"], width=1.8, inset=0.72)

    # Central hub (small, does not block lines)
    circle(4.5, 4.65, 0.2, fill=C["bgLight"])
    circle(4.5, 4.65, 0.15, fill=C["primary"])

    # Nodes on top
    for name, n in nodes.items():
        cx, cy = n["cx"] - nodeW/2, n["cy"] - nodeH/2
        rect(cx, cy, nodeW, nodeH, fill=C["cardLight"],
             stroke=n["col"], stroke_w=1.5)
        rect(cx, cy, nodeW, 0.3, fill=n["col"])
        text(name, cx, cy, 12, C["cardLight"], font=MONO,
             box_w=nodeW, box_h=0.3, align="center", valign="middle",
             char_spacing=2)
        # content bullets per element
        content = {
            "MECÁNICA":   ["Combate en tiempo real", "Bosses por dimensión", "Estado eufórico"],
            "ESTÉTICA":   ["Humor absurdo", "Mundos bizarros", "Feedback frenético"],
            "TECNOLOGÍA": ["Unity · Modelos 3D", "Cámara cenital 53°", "PC · Offline"],
            "HISTORIA":   ["Dr. Emilio", "Portal fallido", "Volver a casa"],
        }
        for j, it in enumerate(content[name]):
            text("· " + it, cx + 0.15, cy + 0.35 + j*0.23,
                 9, C["textDark"], box_w=nodeW-0.3)

    # ----- Right side: explicativo -----
    panelX, panelY, panelW, panelH = 8.9, 2.55, 3.9, 4.35
    rect(panelX, panelY, panelW, panelH, fill=C["bg"])
    rect(panelX, panelY, panelW, 0.35, fill=C["primary"])
    text("INFLUENCIAS RECÍPROCAS", panelX, panelY, 10, C["cardLight"],
         font=MONO, box_w=panelW, box_h=0.35,
         align="center", valign="middle", char_spacing=2)

    infl = [
        ("HISTORIA → MECÁNICA",
         "El portal fallido justifica las oleadas y los bosses por dimensión.",
         C["primary"]),
        ("MECÁNICA → ESTÉTICA",
         "El combate frenético y la euforia generan sensation y submission.",
         C["hot"]),
        ("TECNOLOGÍA → MECÁNICA",
         "La cámara isométrica en Unity habilita el combate con apuntado.",
         C["accent"]),
        ("ESTÉTICA → HISTORIA",
         "El tono absurdo construye el universo bizarro del laboratorio.",
         C["gold"]),
    ]
    y = panelY + 0.55
    for lbl, body, col in infl:
        rect(panelX + 0.2, y + 0.05, 0.08, 0.75, fill=col)
        text(lbl, panelX + 0.38, y, 9, col, font=MONO, char_spacing=1)
        paragraph(body, panelX + 0.38, y + 0.25, panelW - 0.6,
                  9, C["text"], max_lines=3, leading=11)
        y += 0.95

    footer(8, TOTAL)

# ============== SLIDE 8 - RELACIÓN ==============
def slide8():
    c.setFillColor(C["bgLight"]); c.rect(0, 0, W, H, stroke=0, fill=1)
    draw_chrome()
    section_tag("RELACIÓN ENTRE LOS ELEMENTOS", C["accent"])
    big_title("Todo conecta", size=48)
    text("La coherencia como columna vertebral del diseño",
         0.6, 1.85, 13, C["mutedDark"], font=SANSI)

    rels = [
        ("HISTORIA",   "→", "DIMENSIONES", "La narrativa justifica cada mundo.",   C["primary"]),
        ("MECÁNICA",   "→", "DESAFÍO",     "Cada dimensión es un puzzle jugable.", C["hot"]),
        ("ESTÉTICA",   "→", "MEMORIA",     "El universo se vuelve inolvidable.",   C["gold"]),
        ("TECNOLOGÍA", "→", "FLUIDEZ",     "Unity garantiza jugabilidad sólida.",  C["accent"]),
    ]
    for i, (a, ar, b, d, col) in enumerate(rels):
        cy = 2.55 + i*0.85
        rect(0.6, cy, 8.1, 0.72, fill=C["cardLight"], stroke=C["border"], stroke_w=0.5)
        rect(0.6, cy, 0.12, 0.72, fill=col)
        text(a, 0.9, cy, 12, col, font=MONO,
             box_h=0.72, valign="middle", char_spacing=1.5)
        text(ar, 2.45, cy, 12, C["mutedDark"], box_h=0.72, valign="middle")
        text(b, 2.8, cy, 12, C["textDark"], font=MONO,
             box_h=0.72, valign="middle", char_spacing=1.5)
        text(d, 5.15, cy, 11, C["mutedDark"], font=SANSI,
             box_h=0.72, valign="middle")

    # conclusion card
    rect(9.0, 2.55, 3.75, 3.85, fill=C["bg"])
    text("CONCLUSIÓN", 9.25, 2.75, 11, C["accent"], font=MONO, char_spacing=2.5)
    text("Coherencia", 9.25, 3.2, 32, C["text"], font=IMP)
    paragraph("entre narrativa, sistemas y presentación visual.",
              9.25, 4.05, 3.35, 15, C["text"], max_lines=4)
    rect(9.25, 5.9, 0.8, 0.08, fill=C["hot"])

    footer(9, TOTAL)

# ============== SLIDE - REPARTO GDD ==============
def slide_gdd():
    c.setFillColor(C["bgLight"]); c.rect(0, 0, W, H, stroke=0, fill=1)
    draw_chrome()
    section_tag("REPARTO DE TAREAS · EQUIPO DINAMITA", C["primary"])
    big_title("Quién desarrolló cada sección del GDD", size=30)
    text("Secciones del Game Design Document · completar con el integrante responsable",
         0.6, 1.85, 13, C["mutedDark"], font=SANSI)

    # Table: two columns of rows
    sections = [
        ("01", "Concepto General",                    "___________________________"),
        ("02", "Narrativa y contexto",                "___________________________"),
        ("03", "Mecánicas principales",               "___________________________"),
        ("04", "Progresión y estructura de niveles",  "___________________________"),
        ("05", "Listado de assets",                   "___________________________"),
        ("06", "Desarrollo",                          "___________________________"),
        ("07", "Referencias",                         "___________________________"),
        ("08", "Uso de IA",                           "___________________________"),
    ]

    cols = 2
    rowsPerCol = 4
    startX, startY = 0.6, 2.55
    colW = 6.1
    gapX = 0.3
    rowH = 0.9
    for i, (n, sect, holder) in enumerate(sections):
        col = i // rowsPerCol
        row = i % rowsPerCol
        cx = startX + col * (colW + gapX)
        cy = startY + row * rowH
        # card
        rect(cx, cy, colW, rowH - 0.15, fill=C["cardLight"],
             stroke=C["border"], stroke_w=0.5)
        # number chip
        rect(cx, cy, 0.75, rowH - 0.15, fill=C["primary"])
        text(n, cx, cy, 22, C["cardLight"], font=IMP,
             box_w=0.75, box_h=rowH-0.15, align="center", valign="middle")
        # section title
        text(sect, cx + 0.95, cy + 0.1, 14, C["textDark"], font=SANSB,
             box_h=0.4, valign="middle")
        # integrante line
        text("Integrante:", cx + 0.95, cy + 0.42, 10, C["primary"],
             font=MONO, char_spacing=1)
        text(holder, cx + 2.05, cy + 0.42, 12, C["mutedDark"],
             font=MONO, char_spacing=0.5)

    # Footer note
    rect(0.6, 6.45, 12.1, 0.4, fill=C["bg"])
    text("Equipo Dinamita · Fidmay · Nuñez Di Meo · Hug",
         0.6, 6.45, 11, C["accent"], font=MONO,
         box_w=12.1, box_h=0.4, align="center", valign="middle",
         char_spacing=2)

    footer(10, TOTAL)

# ============== SLIDE 11 - REFERENCIAS ==============
def slide9():
    c.setFillColor(C["bgLight"]); c.rect(0, 0, W, H, stroke=0, fill=1)
    draw_chrome()
    section_tag("REFERENCIAS E INFLUENCIAS", C["hot"])
    big_title("El ADN del juego", size=42)

    refs = [
        ("MegaBonk",           "Diversión arcade inmediata y acción directa.",        C["hot"]),
        ("Vampire Survivors",  "Supervivencia frente a hordas y rejugabilidad.",      C["primary"]),
        ("Hotline Miami",      "Ritmo frenético, presión constante, combate intenso.", C["gold"]),
        ("Rick and Morty",     "Humor absurdo y dimensiones alternativas.",            C["accent"]),
        ("Dr. Mundo — LoL",    "Científico descontrolado, poder bruto.",               C["hot"]),
        ("Heimerdinger — LoL", "Inventor científico, estética tecnológica.",           C["primary"]),
    ]
    for i, (t, d, col) in enumerate(refs):
        c_i = i % 3
        r_i = i // 3
        cx = 0.6 + c_i * 4.15
        cy = 2.3 + r_i * 2.2
        rect(cx, cy, 3.95, 2.0, fill=C["cardLight"], stroke=C["border"], stroke_w=0.5)
        rect(cx, cy, 3.95, 0.35, fill=col)
        text(f"REF · 0{i+1}", cx+0.2, cy, 9, C["cardLight"], font=MONO,
             box_h=0.35, valign="middle", char_spacing=2.5)
        text(t, cx+0.2, cy+0.55, 20, C["textDark"], font=IMP,
             box_h=0.6, valign="middle", char_spacing=0.5)
        paragraph(d, cx+0.2, cy+1.25, 3.55, 11, C["mutedDark"], font=SANSI, max_lines=3)

    footer(11, TOTAL)

# ============== SLIDE 10 - CIERRE ==============
def slide10():
    c.setFillColor(C["bg"]); c.rect(0, 0, W, H, stroke=0, fill=1)
    draw_chrome(dark=True)
    # corner decorations — kept clear of the centered text column (~x 2.5 to 10.8)
    circle(1.0, 7.5-1.0, 1.6, fill=C["primary"], alpha=0.45)
    circle(13.33-1.0, 0.9, 1.6, fill=C["hot"], alpha=0.45)
    circle(1.1, 1.2, 0.4, fill=C["accent"], alpha=0.6)
    circle(13.33-1.1, 7.5-1.2, 0.4, fill=C["gold"], alpha=0.6)

    text("FIN DE LA PRESENTACIÓN", 0.8, 1.6, 12, C["accent"], font=MONO,
         box_w=13.33-1.6, align="center", char_spacing=3)

    text("Sistemas claros.",  0.6, 2.3, 60, C["text"],   font=IMP,
         box_w=13.33-1.2, box_h=1.1, align="center", valign="middle")
    text("Acción intensa.",   0.6, 3.4, 60, C["accent"], font=IMP,
         box_w=13.33-1.2, box_h=1.1, align="center", valign="middle")
    text("Identidad propia.", 0.6, 4.5, 60, C["hot"],    font=IMP,
         box_w=13.33-1.2, box_h=1.1, align="center", valign="middle")

    rect(13.33/2-0.75, 5.85, 1.5, 0.06, fill=C["gold"])
    text("GRACIAS", 0.6, 6.0, 22, C["gold"], font=IMP,
         box_w=13.33-1.2, align="center", char_spacing=5)
    text("Freaky Super Mesh · UADE · 2026", 0.6, 6.55, 11, C["muted"],
         font=MONO, box_w=13.33-1.2, align="center", char_spacing=1.5)

    footer(12, TOTAL, dark=True)

for fn in [slide1, slide2, slide_mda_flow, slide3, slide4, slide_aesthetics,
           slide6, slide_tetrad, slide8, slide_gdd, slide9, slide10]:
    fn()
    c.showPage()

c.save()
print("WROTE:", OUT)
