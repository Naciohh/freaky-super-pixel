# -*- coding: utf-8 -*-
"""Genera Freaky_Super_Mesh_Medio.pdf - tono studio pitch, dark mode, 13 slides 16:9."""
from reportlab.pdfgen import canvas
from reportlab.lib.colors import HexColor
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
import os, math

W, H = 13.33 * 72, 7.5 * 72

# ---- palette: dark studio, one amber accent, one coral alert ----
C = {
    "bg":        HexColor("#121722"),   # deep ink
    "bgAlt":     HexColor("#0E1219"),   # darker bands
    "card":      HexColor("#1C2230"),   # slate card
    "cardHi":    HexColor("#242B3B"),   # lighter card
    "ink":       HexColor("#EAE4D4"),   # warm paper text
    "inkSoft":   HexColor("#B8B3A2"),
    "amber":     HexColor("#D4A03C"),   # primary warm accent (lab glow)
    "amberDim":  HexColor("#8A6A28"),
    "coral":     HexColor("#E85A4F"),   # alert / NO
    "teal":      HexColor("#4FB3A9"),   # subtle support
    "grid":      HexColor("#2A3142"),
    "rule":      HexColor("#3A4256"),
    "muted":     HexColor("#6D7688"),
    "mutedSoft": HexColor("#8892A5"),
}

OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "Freaky_Super_Mesh_Medio.pdf")
c = canvas.Canvas(OUT, pagesize=(W, H))

FONTS = {"sans": "Helvetica", "sansB": "Helvetica-Bold", "sansI": "Helvetica-Oblique",
         "mono": "Courier", "monoB": "Courier-Bold", "disp": "Helvetica-Bold"}
try:
    pdfmetrics.registerFont(TTFont("Calibri",   r"C:\Windows\Fonts\calibri.ttf"))
    pdfmetrics.registerFont(TTFont("CalibriB",  r"C:\Windows\Fonts\calibrib.ttf"))
    pdfmetrics.registerFont(TTFont("CalibriI",  r"C:\Windows\Fonts\calibrii.ttf"))
    FONTS["sans"]  = "Calibri"
    FONTS["sansB"] = "CalibriB"
    FONTS["sansI"] = "CalibriI"
except Exception: pass
try:
    pdfmetrics.registerFont(TTFont("SegoeB",    r"C:\Windows\Fonts\segoeuib.ttf"))
    pdfmetrics.registerFont(TTFont("SegoeSB",   r"C:\Windows\Fonts\seguisb.ttf"))
    FONTS["disp"]  = "SegoeB"
    FONTS["dispSB"] = "SegoeSB"
except Exception:
    FONTS["dispSB"] = FONTS["sansB"]
try:
    pdfmetrics.registerFont(TTFont("Consolas",  r"C:\Windows\Fonts\consola.ttf"))
    pdfmetrics.registerFont(TTFont("ConsolasB", r"C:\Windows\Fonts\consolab.ttf"))
    FONTS["mono"]  = "Consolas"
    FONTS["monoB"] = "ConsolasB"
except Exception: pass

SANS = FONTS["sans"]; SANSB = FONTS["sansB"]; SANSI = FONTS["sansI"]
DISP = FONTS["disp"]; DISPSB = FONTS["dispSB"]
MONO = FONTS["mono"]; MONOB = FONTS["monoB"]

def IN(v): return v * 72

def rect(x, y, w, h, fill=None, stroke=None, stroke_w=0):
    c.saveState()
    if fill is not None: c.setFillColor(fill)
    if stroke is not None:
        c.setStrokeColor(stroke); c.setLineWidth(stroke_w)
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
        to = c.beginText(tx, ty); to.setFont(font, size)
        to.setFillColor(color); to.setCharSpace(char_spacing)
        to.textOut(s); c.drawText(to)
    else:
        c.drawString(tx, ty, s)
    c.restoreState()

def wrap_text(s, font, size, max_w_pts):
    words = s.split(); lines, cur = [], ""
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
    rad = math.radians(angle_deg)
    tx, ty = IN(xt), H - IN(yt)
    bl_x = tx - IN(size)*math.cos(rad) + IN(size*0.5)*math.cos(rad + math.pi/2)
    bl_y = ty + IN(size)*math.sin(rad) - IN(size*0.5)*math.sin(rad + math.pi/2)
    br_x = tx - IN(size)*math.cos(rad) + IN(size*0.5)*math.cos(rad - math.pi/2)
    br_y = ty + IN(size)*math.sin(rad) - IN(size*0.5)*math.sin(rad - math.pi/2)
    c.saveState()
    c.setFillColor(color); c.setStrokeColor(color)
    p = c.beginPath(); p.moveTo(tx, ty); p.lineTo(bl_x, bl_y); p.lineTo(br_x, br_y); p.close()
    c.drawPath(p, stroke=0, fill=1)
    c.restoreState()

def bidir_arrow(x1, y1, x2, y2, color, width=1.0, inset=0.0):
    dx, dy = x2 - x1, y2 - y1
    dist = math.hypot(dx, dy)
    if dist == 0: return
    ux, uy = dx/dist, dy/dist
    sx, sy = x1 + ux*inset, y1 + uy*inset
    ex, ey = x2 - ux*inset, y2 - uy*inset
    line(sx, sy, ex, ey, color, width=width)
    ang_e = math.degrees(math.atan2(-(ey - sy), (ex - sx)))
    ang_s = math.degrees(math.atan2(-(sy - ey), (sx - ex)))
    arrow_head(ex, ey, ang_e, color); arrow_head(sx, sy, ang_s, color)

# ---- HUD-style chrome: corner crosshairs + thin rules ----
def corner_mark(x, y, size=0.18, color=None, corner="tl"):
    color = color or C["amber"]
    if corner == "tl":
        line(x, y, x + size, y, color, 1.2); line(x, y, x, y + size, color, 1.2)
    elif corner == "tr":
        line(x, y, x - size, y, color, 1.2); line(x, y, x, y + size, color, 1.2)
    elif corner == "bl":
        line(x, y, x + size, y, color, 1.2); line(x, y, x, y - size, color, 1.2)
    elif corner == "br":
        line(x, y, x - size, y, color, 1.2); line(x, y, x, y - size, color, 1.2)

def draw_chrome():
    # four amber corner crosshairs (HUD feel)
    m = 0.4
    corner_mark(m, m, 0.18, C["amber"], "tl")
    corner_mark(13.33-m, m, 0.18, C["amber"], "tr")
    corner_mark(m, 7.5-m, 0.18, C["amber"], "bl")
    corner_mark(13.33-m, 7.5-m, 0.18, C["amber"], "br")
    # subtle rule band top / bottom (out of content area)
    line(0.7, 0.42, 13.33-0.7, 0.42, C["rule"], 0.4)
    line(0.7, 7.08, 13.33-0.7, 7.08, C["rule"], 0.4)

def footer(n, total, label=None):
    text("FSM · FREAKY SUPER MESH", 0.6, 7.15, 8, C["mutedSoft"], font=MONO,
         box_h=0.22, valign="middle", char_spacing=1.5)
    if label:
        text(label, 0.6, 7.15, 8, C["inkSoft"], font=MONO,
             box_w=13.33-1.2, box_h=0.22, align="center", valign="middle",
             char_spacing=1.5)
    text(f"{n:02d} / {total:02d}  ·  UADE 2026",
         13.33-3.6, 7.15, 8, C["mutedSoft"], font=MONO,
         box_w=3, box_h=0.22, valign="middle", align="right", char_spacing=1.5)

def tech_label(s, x, y, size=9, color=None):
    color = color or C["amber"]
    text(s, x, y, size, color, font=MONO, char_spacing=2.5)

def section_id(num_text):
    # e.g. "[ 03 / MDA · DYNAMICS ]" small tag top-left under chrome
    text(num_text, 0.6, 0.7, 9, C["amber"], font=MONO, char_spacing=2.5)
    # small bracket decor
    rect(0.6, 0.93, 0.35, 0.03, fill=C["amber"])

def big_title(s, size=38, color=None, y=1.15):
    color = color or C["ink"]
    text(s, 0.6, y, size, color, font=DISP,
         box_w=13.33-1.2, box_h=1.0, valign="middle")

TOTAL = 13

# ============== 1 · PORTADA ==============
def slide1():
    c.setFillColor(C["bg"]); c.rect(0, 0, W, H, stroke=0, fill=1)
    draw_chrome()
    # faint grid blocks (dimensional background)
    for i in range(8):
        line(0, 0.6 + i*0.9, 13.33, 0.6 + i*0.9, C["grid"], 0.25)

    # top strip
    text("PROYECTO  /  DISEÑO DE VIDEOJUEGOS", 0.8, 0.75, 10, C["amber"],
         font=MONO, char_spacing=3)
    text("DIM · 03", 13.33-2.4, 0.75, 10, C["mutedSoft"], font=MONO,
         box_w=2, align="right", char_spacing=3)

    # title
    text("FREAKY", 0.75, 1.5, 90, C["ink"], font=DISP, char_spacing=3)
    text("SUPER MESH", 0.75, 2.85, 90, C["amber"], font=DISP, char_spacing=3)

    # tagline
    text("Action Rogue-lite dimensional  ·  análisis teórico de diseño",
         0.8, 4.3, 16, C["inkSoft"], font=SANSI, box_w=12)

    # triple divider
    rect(0.8, 4.85, 1.4, 0.04, fill=C["amber"])
    rect(2.3, 4.85, 0.4, 0.04, fill=C["coral"])
    rect(2.8, 4.85, 0.2, 0.04, fill=C["teal"])

    # info grid
    def block(x, w, label, lines, col=None):
        col = col or C["amber"]
        text(label, x, 5.15, 9, col, font=MONO, char_spacing=2.5, box_w=w)
        for i, ln in enumerate(lines):
            text(ln, x, 5.45 + i*0.3, 13, C["ink"], font=SANS, box_w=w)

    block(0.8, 4.0, "INTEGRANTES",
          ["Fidmay, Agustín", "Nuñez Di Meo, Dante", "Hug, Juan Ignacio"])
    block(5.1, 4.0, "CÁTEDRA",
          ["Prof. Aparicio, Lucas", "Prof. Ulrich, Gonzalo Ezequiel"])
    block(9.4, 3.5, "UADE · 2026",
          ["Diseño de Videojuegos", "Primer Cuatrimestre · Tarde",
           "Entrega: 24 / 04 / 2026"])

    # team stamp
    line(0.8, 6.6, 13.33-0.8, 6.6, C["rule"], 0.5)
    text("EQUIPO DINAMITA", 0.8, 6.75, 11, C["amber"], font=MONOB, char_spacing=3)
    text("BUILD  00.1", 13.33-2.4, 6.75, 10, C["mutedSoft"],
         font=MONO, box_w=2, align="right", char_spacing=2.5)

    footer(1, TOTAL)

# ============== 2 · RESUMEN ==============
def slide2():
    c.setFillColor(C["bg"]); c.rect(0, 0, W, H, stroke=0, fill=1)
    draw_chrome()
    section_id("[ 01 ]  ·  RESUMEN DEL JUEGO")
    big_title("Action Rogue-lite dimensional")

    text("El Dr. Emilio queda atrapado entre mundos tras un experimento fallido. "
         "Debe sobrevivir oleadas, vencer jefes y volver a casa.",
         0.6, 2.2, 15, C["inkSoft"], font=SANSI, box_w=7.5)

    # narrative block
    tech_label("PROPUESTA", 0.6, 2.95)
    paragraph(
        "Freaky Super Mesh es un Action Rogue-lite single-player para PC. "
        "Desde una cámara cenital inclinada, el jugador atraviesa dimensiones "
        "absurdas derrotando enemigos temáticos y a un jefe final por mundo. "
        "El combate se construye sobre tres pilares: movimiento, bloqueo "
        "y un Estado Eufórico que recompensa la ofensiva continua.",
        0.6, 3.2, 7.5, 13, C["ink"], font=SANS, leading=20, max_lines=6)

    tech_label("LOOP DE JUEGO", 0.6, 5.25)
    # visual loop chips
    loop = ["Explorar", "Oleadas", "Euforia", "Boss", "Nueva dim."]
    x = 0.6
    for i, ch in enumerate(loop):
        w = pdfmetrics.stringWidth(ch, SANSB, 11) / 72 + 0.35
        rect(x, 5.5, w, 0.38, stroke=C["amber"], stroke_w=0.6)
        text(ch, x, 5.5, 11, C["ink"], font=SANSB,
             box_w=w, box_h=0.38, align="center", valign="middle")
        x += w + 0.12
        if i < len(loop)-1:
            line(x-0.1, 5.69, x-0.02, 5.69, C["amber"], 0.8)
            arrow_head(x-0.02, 5.69, 0, C["amber"], size=0.06)
            x += 0.08

    # right panel: data sheet
    rect(8.5, 2.2, 4.25, 4.7, fill=C["card"], stroke=C["rule"], stroke_w=0.5)
    rect(8.5, 2.2, 4.25, 0.35, fill=C["cardHi"])
    tech_label("FICHA TÉCNICA", 8.7, 2.27, size=9, color=C["amber"])

    fields = [
        ("GÉNERO",       "Action Rogue-lite"),
        ("PLATAFORMA",   "PC"),
        ("JUGADORES",    "1 · Offline"),
        ("PERSONAJE",    "Dr. Emilio"),
        ("CÁMARA",       "Cenital 53° (iso)"),
        ("INPUT",        "WASD + Mouse + Shift"),
        ("ENGINE",       "Unity"),
    ]
    for i, (k, v) in enumerate(fields):
        yy = 2.75 + i * 0.56
        text(k, 8.75, yy, 9, C["amber"], font=MONO, char_spacing=2)
        text(v, 8.75, yy + 0.22, 13, C["ink"], font=SANSB)
        if i < len(fields)-1:
            line(8.75, yy + 0.5, 12.5, yy + 0.5, C["rule"], 0.4)

    footer(2, TOTAL, label="RESUMEN")

# ============== 3 · MDA FLOW ==============
def slide3():
    c.setFillColor(C["bg"]); c.rect(0, 0, W, H, stroke=0, fill=1)
    draw_chrome()
    section_id("[ 02 ]  ·  FRAMEWORK MDA")
    big_title("Del diseñador al jugador", size=36)
    text("Hunicke, LeBlanc & Zubek  ·  la dirección del flujo es la clave del análisis.",
         0.6, 2.25, 14, C["inkSoft"], font=SANSI)

    nodes = [
        ("01", "MECHANICS",  "Reglas, sistemas, componentes del juego.",
         "Lo que diseñamos"),
        ("02", "DYNAMICS",   "Comportamiento emergente en tiempo real.",
         "Lo que emerge"),
        ("03", "AESTHETICS", "Respuestas emocionales del jugador.",
         "Lo que se siente"),
    ]
    startX, startY = 0.6, 2.95
    nodeW, nodeH, gap = 3.85, 3.2, 0.43
    for i, (num, t, body, sub) in enumerate(nodes):
        cx = startX + i * (nodeW + gap)
        rect(cx, startY, nodeW, nodeH, fill=C["card"],
             stroke=C["rule"], stroke_w=0.6)
        # top tag
        rect(cx, startY, nodeW, 0.4, fill=C["cardHi"])
        text(num, cx + 0.25, startY, 10, C["amber"], font=MONO,
             box_h=0.4, valign="middle", char_spacing=2)
        text(t, cx + 0.8, startY, 12, C["ink"], font=SANSB,
             box_h=0.4, valign="middle", char_spacing=3)
        # accent bar
        rect(cx + 0.25, startY + 0.6, 0.4, 0.03, fill=C["amber"])
        text(sub, cx + 0.25, startY + 0.8, 10, C["amber"],
             font=MONO, char_spacing=2)
        paragraph(body, cx + 0.25, startY + 1.2, nodeW - 0.5, 14, C["ink"],
                  font=SANSB, leading=20, max_lines=3)
        # footer tag
        line(cx + 0.25, startY + nodeH - 0.4, cx + nodeW - 0.25,
             startY + nodeH - 0.4, C["rule"], 0.4)
        tag_map = {"MECHANICS": "INPUT", "DYNAMICS": "PROCESS", "AESTHETICS": "OUTPUT"}
        text(tag_map[t], cx + 0.25, startY + nodeH - 0.25, 9, C["mutedSoft"],
             font=MONO, char_spacing=2)

    # arrows between nodes
    for i in range(2):
        ax = startX + (i + 1) * nodeW + i * gap
        line(ax + 0.05, startY + nodeH/2, ax + gap - 0.1, startY + nodeH/2,
             C["amber"], 1.5)
        arrow_head(ax + gap - 0.1, startY + nodeH/2, 0, C["amber"], size=0.1)

    # perspective strip
    rect(0.6, 6.3, 13.33-1.2, 0.55, fill=C["cardHi"])
    text("→  DISEÑADOR  parte de Mechanics", 0.6, 6.3, 11, C["amber"], font=MONO,
         box_w=6.25, box_h=0.55, align="center", valign="middle", char_spacing=1.5)
    text("JUGADOR  experimenta desde Aesthetics  ←", 6.85, 6.3, 11, C["teal"],
         font=MONO, box_w=6.25, box_h=0.55, align="center", valign="middle",
         char_spacing=1.5)

    footer(3, TOTAL, label="MDA · FLUJO")

# ============== 4 · MECHANICS ==============
def slide4():
    c.setFillColor(C["bg"]); c.rect(0, 0, W, H, stroke=0, fill=1)
    draw_chrome()
    section_id("[ 03 ]  ·  MDA · 01 MECHANICS")
    big_title("Mecánicas del sistema", size=40)
    text("Las reglas con las que se construye el juego.",
         0.6, 2.25, 14, C["inkSoft"], font=SANSI)

    items = [
        ("Movimiento",        "WASD en cuatro direcciones."),
        ("Apuntado",          "Dirección con el mouse."),
        ("Ataque",            "Lanzamiento de probetas."),
        ("Defensa",           "Bloqueo con Shift."),
        ("Enemigos",          "Oleadas temáticas por dimensión."),
        ("Bosses",            "Jefe al cierre de cada dimensión."),
        ("Salud",             "Sistema numérico con daño recibido."),
        ("Estado Eufórico",   "Se activa al combinar bloqueo y kills."),
        ("Buff temporal",     "Stats mejoradas durante la euforia."),
        ("Regeneración",      "Recuperación de vida en euforia."),
    ]
    cols = 2; cardW, cardH, gapX, gapY = 6.05, 0.7, 0.25, 0.1
    startX, startY = 0.6, 2.85
    for i, (t, d) in enumerate(items):
        cx = startX + (i % cols) * (cardW + gapX)
        cy = startY + (i // cols) * (cardH + gapY)
        rect(cx, cy, cardW, cardH, fill=C["card"], stroke=C["rule"], stroke_w=0.4)
        rect(cx, cy, 0.04, cardH, fill=C["amber"])
        text(f"M.{i+1:02d}", cx + 0.2, cy, 9, C["amber"], font=MONO,
             char_spacing=1.5, box_h=cardH, valign="middle")
        text(t, cx + 0.95, cy + 0.1, 13, C["ink"], font=SANSB,
             box_h=0.3, valign="middle")
        text(d, cx + 0.95, cy + 0.42, 11, C["inkSoft"], font=SANS)

    footer(4, TOTAL, label="MDA · MECHANICS")

# ============== 5 · DYNAMICS ==============
def slide5():
    c.setFillColor(C["bg"]); c.rect(0, 0, W, H, stroke=0, fill=1)
    draw_chrome()
    section_id("[ 04 ]  ·  MDA · 02 DYNAMICS")
    big_title("Dinámicas emergentes", size=40)
    text("Lo que ocurre cuando el jugador entra en contacto con las reglas.",
         0.6, 2.25, 14, C["inkSoft"], font=SANSI)

    items = [
        ("01", "Movimiento constante",  "Esquivar y reposicionarse es parte del combate."),
        ("02", "Posicionamiento",       "Lectura del espacio frente a hordas."),
        ("03", "Bloqueo estratégico",   "Defensa activa como recurso de supervivencia."),
        ("04", "Incentivo ofensivo",    "Eliminar rápido acelera el estado eufórico."),
        ("05", "Aprendizaje",           "Lectura de patrones de enemigos y jefes."),
        ("06", "Presión creciente",     "El ritmo aumenta partida a partida."),
    ]
    for i, (n, t, d) in enumerate(items):
        col = i % 3; row = i // 3
        cx = 0.6 + col * 4.15; cy = 2.85 + row * 1.7
        rect(cx, cy, 3.95, 1.5, fill=C["card"], stroke=C["rule"], stroke_w=0.5)
        # top meta bar
        rect(cx, cy, 3.95, 0.3, fill=C["cardHi"])
        text(f"D.{n}", cx + 0.2, cy, 9, C["amber"], font=MONO,
             box_h=0.3, valign="middle", char_spacing=1.5)
        text(t, cx + 0.2, cy + 0.4, 14, C["ink"], font=SANSB,
             box_w=3.6, box_h=0.35, valign="top")
        paragraph(d, cx + 0.2, cy + 0.8, 3.6, 11, C["inkSoft"],
                  font=SANSI, max_lines=2, leading=14)

    # tempo strip (HUD-style)
    tech_label("RITMO DE PARTIDA", 0.6, 6.3)
    steps = ["Inicio", "Más enemigos", "Presión creciente", "Jefe final", "Nueva dimensión"]
    stepW = (13.33 - 1.2) / len(steps)
    for i, st in enumerate(steps):
        sx = 0.6 + i * stepW
        last = i == len(steps)-1
        col = C["coral"] if last else C["amber"]
        circle(sx + 0.1, 6.68, 0.06, fill=col)
        text(st, sx + 0.25, 6.58, 11, C["ink"] if last else C["inkSoft"],
             font=SANSB if last else SANS)
        if i < len(steps)-1:
            line(sx + 0.2, 6.68, sx + stepW - 0.1, 6.68, C["rule"], 0.5)

    footer(5, TOTAL, label="MDA · DYNAMICS")

# ============== 6 · AESTHETICS (8 HUNICKE) ==============
def slide6():
    c.setFillColor(C["bg"]); c.rect(0, 0, W, H, stroke=0, fill=1)
    draw_chrome()
    section_id("[ 05 ]  ·  MDA · 03 AESTHETICS")
    big_title("Las 8 categorías de Hunicke", size=38)
    text("Identificamos cuáles aplican al juego y cuáles quedan fuera de alcance.",
         0.6, 2.25, 14, C["inkSoft"], font=SANSI)

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
    startX, startY = 0.6, 2.85
    cardW, cardH, gapX, gapY = 3.0, 1.95, 0.13, 0.18
    for i, (t, d, ok) in enumerate(items):
        ci = i % cols; ri = i // cols
        cx = startX + ci * (cardW + gapX)
        cy = startY + ri * (cardH + gapY)
        col = C["amber"] if ok else C["coral"]
        rect(cx, cy, cardW, cardH, fill=C["card"], stroke=C["rule"], stroke_w=0.5)
        rect(cx, cy, 0.04, cardH, fill=col)
        # status pill
        pillW, pillH = 0.6, 0.3
        rect(cx + cardW - pillW - 0.15, cy + 0.15, pillW, pillH,
             fill=col if ok else C["card"], stroke=col, stroke_w=0.7)
        text("SÍ" if ok else "NO", cx + cardW - pillW - 0.15, cy + 0.15,
             10, C["bg"] if ok else col, font=MONOB,
             box_w=pillW, box_h=pillH, align="center", valign="middle",
             char_spacing=2)
        text(f"A.{i+1:02d}", cx + 0.2, cy + 0.2, 9, col, font=MONO, char_spacing=2)
        text(t, cx + 0.2, cy + 0.55, 16, C["ink"] if ok else C["inkSoft"],
             font=SANSB, char_spacing=1)
        paragraph(d, cx + 0.2, cy + 1.0, cardW - 0.4, 10,
                  C["inkSoft"] if ok else C["muted"], font=SANS,
                  max_lines=5, leading=13)

    footer(6, TOTAL, label="MDA · AESTHETICS")

# ============== 7 · CADENA MDA ==============
def slide7():
    c.setFillColor(C["bg"]); c.rect(0, 0, W, H, stroke=0, fill=1)
    draw_chrome()
    section_id("[ 06 ]  ·  CADENA COMPLETA MDA")
    big_title("Caso: Estado Eufórico", size=40)
    text("Cómo una mecánica concreta se transforma en experiencia estética.",
         0.6, 2.25, 14, C["inkSoft"], font=SANSI)

    chain = [
        ("M", "MECÁNICA", "Eliminar enemigos y bloquear ataques activa el estado eufórico."),
        ("D", "DINÁMICA", "El jugador combina ofensiva y defensa para acelerar su activación."),
        ("A", "ESTÉTICA", "Challenge · Sensation · Submission."),
    ]
    startY = 2.85
    for i, (icon, lbl, body) in enumerate(chain):
        cx = 0.6 + i * 4.23
        rect(cx, startY, 3.95, 3.35, fill=C["card"], stroke=C["rule"], stroke_w=0.6)
        rect(cx, startY, 3.95, 0.4, fill=C["cardHi"])
        text(f"STEP 0{i+1}", cx + 0.2, startY, 9, C["amber"],
             font=MONO, box_h=0.4, valign="middle", char_spacing=2.5)
        text(lbl, cx, startY, 11, C["ink"], font=SANSB,
             box_w=3.75, box_h=0.4, align="right", valign="middle", char_spacing=3)
        # big letter circle
        circle(cx + 3.95/2, startY + 1.3, 0.5, fill=C["cardHi"])
        circle(cx + 3.95/2, startY + 1.3, 0.47, stroke=C["amber"], stroke_w=1.2)
        text(icon, cx, startY + 0.78, 46, C["amber"], font=DISP,
             box_w=3.95, box_h=1.05, align="center", valign="middle")
        line(cx + 0.4, startY + 2.2, cx + 3.55, startY + 2.2, C["rule"], 0.5)
        paragraph(body, cx + 0.3, startY + 2.4, 3.35, 12, C["ink"],
                  font=SANS, leading=18, max_lines=3)
        if i < 2:
            ax = cx + 3.95
            line(ax + 0.08, startY + 3.35/2, ax + 0.37, startY + 3.35/2,
                 C["amber"], 1.5)
            arrow_head(ax + 0.37, startY + 3.35/2, 0, C["amber"], size=0.09)

    # footnote
    rect(0.6, 6.4, 13.33-1.2, 0.5, fill=C["cardHi"])
    text("Una misma acción alimenta defensa, ofensiva y recompensa emocional.",
         0.6, 6.4, 12, C["amber"], font=SANSB,
         box_w=13.33-1.2, box_h=0.5, align="center", valign="middle")

    footer(7, TOTAL, label="MDA · CADENA")

# ============== 8 · TÉTRADA DE SCHELL ==============
def slide8():
    c.setFillColor(C["bg"]); c.rect(0, 0, W, H, stroke=0, fill=1)
    draw_chrome()
    section_id("[ 07 ]  ·  TÉTRADA DE SCHELL")
    big_title("Los cuatro elementos y sus relaciones", size=32)
    text("Diagrama de influencias recíprocas entre los elementos del juego.",
         0.6, 2.25, 14, C["inkSoft"], font=SANSI)

    nodes = {
        "MECÁNICA":   {"cx": 4.5, "cy": 3.1},
        "ESTÉTICA":   {"cx": 7.0, "cy": 4.85},
        "TECNOLOGÍA": {"cx": 4.5, "cy": 6.6},
        "HISTORIA":   {"cx": 2.0, "cy": 4.85},
    }
    nodeW, nodeH = 2.6, 1.05

    pairs = [("MECÁNICA","ESTÉTICA"), ("MECÁNICA","TECNOLOGÍA"),
             ("MECÁNICA","HISTORIA"), ("ESTÉTICA","TECNOLOGÍA"),
             ("ESTÉTICA","HISTORIA"), ("TECNOLOGÍA","HISTORIA")]
    for a, b in pairs:
        na, nb = nodes[a], nodes[b]
        bidir_arrow(na["cx"], na["cy"], nb["cx"], nb["cy"],
                    C["amber"], width=1.2, inset=0.72)

    circle(4.5, 4.85, 0.15, fill=C["amber"])
    circle(4.5, 4.85, 0.08, fill=C["bg"])

    content = {
        "MECÁNICA":   ["Combate en tiempo real", "Bosses por dimensión", "Estado eufórico"],
        "ESTÉTICA":   ["Humor absurdo", "Mundos bizarros", "Feedback frenético"],
        "TECNOLOGÍA": ["Unity · Modelos 3D", "Cámara cenital 53°", "PC · Offline"],
        "HISTORIA":   ["Dr. Emilio", "Portal fallido", "Volver a casa"],
    }
    for name, n in nodes.items():
        cx, cy = n["cx"] - nodeW/2, n["cy"] - nodeH/2
        rect(cx, cy, nodeW, nodeH, fill=C["card"], stroke=C["amber"], stroke_w=1.1)
        rect(cx, cy, nodeW, 0.3, fill=C["cardHi"])
        text(name, cx, cy, 11, C["amber"], font=MONOB,
             box_w=nodeW, box_h=0.3, align="center", valign="middle",
             char_spacing=3)
        for j, it in enumerate(content[name]):
            text("· " + it, cx + 0.18, cy + 0.4 + j*0.21,
                 9, C["ink"], font=SANS, box_w=nodeW-0.3)

    # right panel
    px, py, pw, ph = 8.8, 2.85, 4.0, 4.1
    rect(px, py, pw, ph, fill=C["card"], stroke=C["rule"], stroke_w=0.6)
    rect(px, py, pw, 0.35, fill=C["cardHi"])
    text("INFLUENCIAS RECÍPROCAS", px, py, 10, C["amber"],
         font=MONOB, box_w=pw, box_h=0.35,
         align="center", valign="middle", char_spacing=3)

    infl = [
        ("HISTORIA  →  MECÁNICA",   "El portal fallido justifica oleadas y bosses por dimensión."),
        ("MECÁNICA  →  ESTÉTICA",   "El combate frenético y la euforia generan sensation y submission."),
        ("TECNOLOGÍA  →  MECÁNICA", "La cámara isométrica en Unity habilita el combate con apuntado."),
        ("ESTÉTICA  →  HISTORIA",   "El tono absurdo construye el universo bizarro del laboratorio."),
    ]
    yy = py + 0.55
    for lbl, body in infl:
        rect(px + 0.25, yy + 0.05, 0.06, 0.7, fill=C["amber"])
        text(lbl, px + 0.45, yy, 9, C["amber"], font=MONO, char_spacing=1.5)
        paragraph(body, px + 0.45, yy + 0.22, pw - 0.65, 10, C["ink"],
                  font=SANS, max_lines=3, leading=13)
        yy += 0.88

    footer(8, TOTAL, label="TÉTRADA DE SCHELL")

# ============== 9 · RELACIÓN ==============
def slide9():
    c.setFillColor(C["bg"]); c.rect(0, 0, W, H, stroke=0, fill=1)
    draw_chrome()
    section_id("[ 08 ]  ·  RELACIÓN ENTRE ELEMENTOS")
    big_title("Coherencia del sistema", size=42)
    text("La articulación entre narrativa, sistemas y presentación.",
         0.6, 2.25, 14, C["inkSoft"], font=SANSI)

    rels = [
        ("HISTORIA",   "DIMENSIONES", "La narrativa justifica cada mundo y sus enemigos."),
        ("MECÁNICA",   "DESAFÍO",     "Cada dimensión se traduce en un puzzle jugable."),
        ("ESTÉTICA",   "MEMORIA",     "El universo absurdo vuelve al juego inolvidable."),
        ("TECNOLOGÍA", "FLUIDEZ",     "Unity garantiza una jugabilidad sólida y estable."),
    ]
    y = 2.85
    for a, b, d in rels:
        rect(0.6, y, 8.2, 0.8, fill=C["card"], stroke=C["rule"], stroke_w=0.5)
        rect(0.6, y, 0.05, 0.8, fill=C["amber"])
        text(a, 0.85, y, 12, C["amber"], font=MONOB,
             char_spacing=3, box_h=0.8, valign="middle")
        line(2.95, y + 0.4, 3.45, y + 0.4, C["amber"], 0.8)
        arrow_head(3.45, y + 0.4, 0, C["amber"], size=0.08)
        text(b, 3.75, y, 12, C["ink"], font=SANSB,
             char_spacing=2, box_h=0.8, valign="middle")
        text(d, 6.1, y, 12, C["inkSoft"], font=SANSI,
             box_h=0.8, valign="middle")
        y += 0.92

    # conclusion card
    rect(9.1, 2.85, 3.65, 3.55, fill=C["cardHi"], stroke=C["amber"], stroke_w=0.8)
    rect(9.1, 2.85, 3.65, 0.35, fill=C["amber"])
    text("CONCLUSIÓN", 9.1, 2.85, 10, C["bg"], font=MONOB,
         box_w=3.65, box_h=0.35, align="center", valign="middle", char_spacing=3)
    text("Coherencia", 9.3, 3.5, 32, C["amber"], font=DISP)
    paragraph("entre narrativa, sistemas y presentación visual.",
              9.3, 4.4, 3.3, 14, C["ink"], font=SANS, leading=18, max_lines=3)
    rect(9.3, 5.75, 0.8, 0.04, fill=C["coral"])
    text("— FSM TEAM", 9.3, 5.9, 10, C["mutedSoft"], font=MONO, char_spacing=2)

    footer(9, TOTAL, label="RELACIÓN")

# ============== 10 · REPARTO GDD ==============
def slide10():
    c.setFillColor(C["bg"]); c.rect(0, 0, W, H, stroke=0, fill=1)
    draw_chrome()
    section_id("[ 09 ]  ·  REPARTO DE TAREAS")
    big_title("Autoría por sección del GDD", size=34)
    text("Secciones del Game Design Document  ·  completar con el integrante responsable.",
         0.6, 2.25, 14, C["inkSoft"], font=SANSI)

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
    startX, startY = 0.6, 2.85
    colW = 6.1; gapX = 0.3; rowH = 0.9
    for i, (n, sect) in enumerate(sections):
        col = i // rowsPerCol
        row = i % rowsPerCol
        cx = startX + col * (colW + gapX)
        cy = startY + row * rowH
        rect(cx, cy, colW, rowH - 0.15, fill=C["card"],
             stroke=C["rule"], stroke_w=0.5)
        rect(cx, cy, 0.7, rowH - 0.15, fill=C["cardHi"])
        text(n, cx, cy, 22, C["amber"], font=DISP,
             box_w=0.7, box_h=rowH-0.15, align="center", valign="middle")
        text(sect, cx + 0.9, cy + 0.1, 13, C["ink"], font=SANSB,
             box_h=0.35, valign="middle")
        text("INTEGRANTE", cx + 0.9, cy + 0.44, 8, C["amber"],
             font=MONO, char_spacing=2)
        text("___________________________", cx + 2.0, cy + 0.44, 11,
             C["inkSoft"], font=MONO)

    line(0.6, 6.5, 13.33-0.6, 6.5, C["rule"], 0.5)
    text("EQUIPO DINAMITA  ·  FIDMAY  ·  NUÑEZ DI MEO  ·  HUG",
         0.6, 6.65, 10, C["amber"], font=MONOB,
         box_w=13.33-1.2, align="center", char_spacing=3)

    footer(10, TOTAL, label="REPARTO GDD")

# ============== 11 · REFERENCIAS ==============
def slide11():
    c.setFillColor(C["bg"]); c.rect(0, 0, W, H, stroke=0, fill=1)
    draw_chrome()
    section_id("[ 10 ]  ·  REFERENCIAS E INFLUENCIAS")
    big_title("El ADN del juego", size=42)

    refs = [
        ("MegaBonk",            "Diversión arcade inmediata y acción directa."),
        ("Vampire Survivors",   "Supervivencia frente a hordas, progresión y rejugabilidad."),
        ("Hotline Miami",       "Ritmo frenético, presión constante y combate intenso."),
        ("Rick and Morty",      "Humor absurdo y dimensiones alternativas."),
        ("Dr. Mundo — LoL",     "Científico descontrolado, poder bruto."),
        ("Heimerdinger — LoL",  "Inventor científico, estética tecnológica."),
    ]
    for i, (t, d) in enumerate(refs):
        col = i % 3; row = i // 3
        cx = 0.6 + col * 4.15; cy = 2.4 + row * 2.1
        rect(cx, cy, 3.95, 1.85, fill=C["card"],
             stroke=C["rule"], stroke_w=0.5)
        rect(cx, cy, 3.95, 0.35, fill=C["cardHi"])
        text(f"REF.{i+1:02d}", cx + 0.2, cy, 9, C["amber"], font=MONO,
             box_h=0.35, valign="middle", char_spacing=2.5)
        text(t, cx + 0.25, cy + 0.55, 18, C["ink"], font=SANSB,
             box_w=3.45, char_spacing=0.5)
        rect(cx + 0.25, cy + 1.05, 0.4, 0.03, fill=C["amber"])
        paragraph(d, cx + 0.25, cy + 1.2, 3.45, 11, C["inkSoft"],
                  font=SANSI, max_lines=3, leading=14)

    footer(11, TOTAL, label="REFERENCIAS")

# ============== 12 · EVIDENCIAS DE DESARROLLO ==============
def slide12():
    c.setFillColor(C["bg"]); c.rect(0, 0, W, H, stroke=0, fill=1)
    draw_chrome()
    section_id("[ 11 ]  ·  DESARROLLO DEL PROYECTO")
    big_title("Evidencias de producción", size=40)
    text("Material visual del trabajo: concept art, escenas en Unity y gameplay.",
         0.6, 2.25, 14, C["inkSoft"], font=SANSI)

    placeholders = [
        ("PERSONAJES",          "Dr. Emilio · enemigos por dimensión"),
        ("ESCENAS EN UNITY",    "Capturas de editor y escenarios"),
        ("GAMEPLAY",            "Videos de combate y bosses"),
        ("ITERACIÓN DE DISEÑO", "Pruebas, bocetos, prototipos"),
    ]
    cols = 2
    startX, startY = 0.6, 2.85
    cardW, cardH, gapX, gapY = 6.05, 1.95, 0.2, 0.2
    for i, (t, d) in enumerate(placeholders):
        ci = i % cols; ri = i // cols
        cx = startX + ci * (cardW + gapX)
        cy = startY + ri * (cardH + gapY)
        # dashed placeholder frame
        c.saveState()
        c.setStrokeColor(C["amber"]); c.setLineWidth(0.9); c.setDash(5, 3)
        c.rect(IN(cx), H - IN(cy) - IN(cardH), IN(cardW), IN(cardH),
               stroke=1, fill=0)
        c.restoreState()
        # inner fill
        rect(cx + 0.08, cy + 0.08, cardW - 0.16, cardH - 0.16,
             fill=C["card"])
        # corner marks on placeholder
        corner_mark(cx + 0.18, cy + 0.22, 0.12, C["amber"], "tl")
        corner_mark(cx + cardW - 0.18, cy + 0.22, 0.12, C["amber"], "tr")
        corner_mark(cx + 0.18, cy + cardH - 0.22, 0.12, C["amber"], "bl")
        corner_mark(cx + cardW - 0.18, cy + cardH - 0.22, 0.12, C["amber"], "br")
        # label
        text(f"SLOT.{i+1:02d}", cx + 0.45, cy + 0.35, 9, C["amber"],
             font=MONO, char_spacing=2.5)
        text(t, cx + 0.45, cy + 0.65, 16, C["ink"], font=SANSB, char_spacing=1)
        rect(cx + 0.45, cy + 1.05, 0.4, 0.03, fill=C["amber"])
        text(d, cx + 0.45, cy + 1.2, 12, C["inkSoft"], font=SANSI)
        # insert tag bottom-right
        text("[  INSERTAR CAPTURA / VIDEO  ]",
             cx + cardW - 2.3, cy + cardH - 0.45, 9, C["mutedSoft"],
             font=MONO, char_spacing=2,
             box_w=2.0, align="right")

    footer(12, TOTAL, label="EVIDENCIAS")

# ============== 13 · CIERRE ==============
def slide13():
    c.setFillColor(C["bg"]); c.rect(0, 0, W, H, stroke=0, fill=1)
    draw_chrome()

    # subtle grid
    for i in range(8):
        line(0, 0.6 + i*0.9, 13.33, 0.6 + i*0.9, C["grid"], 0.25)

    text("END  /  FIN DE LA PRESENTACIÓN", 0.6, 2.0, 10, C["amber"], font=MONO,
         box_w=13.33-1.2, align="center", char_spacing=6)
    rect(13.33/2 - 0.75, 2.45, 1.5, 0.04, fill=C["amber"])

    text("Freaky Super Mesh", 0.6, 2.85, 56, C["ink"], font=DISP,
         box_w=13.33-1.2, box_h=1.2, align="center", valign="middle")

    # tagline tri-color
    text("Sistemas claros.   Acción intensa.   Identidad propia.",
         0.6, 4.25, 18, C["amber"], font=SANSI,
         box_w=13.33-1.2, align="center")

    # divider
    line(13.33/2 - 3, 5.05, 13.33/2 + 3, 5.05, C["rule"], 0.6)

    text("GRACIAS", 0.6, 5.3, 28, C["amber"], font=DISP,
         box_w=13.33-1.2, align="center", char_spacing=6)

    text("EQUIPO DINAMITA", 0.6, 6.1, 10, C["amber"], font=MONOB,
         box_w=13.33-1.2, align="center", char_spacing=3)
    text("Fidmay  ·  Nuñez Di Meo  ·  Hug",
         0.6, 6.35, 13, C["ink"], font=SANS,
         box_w=13.33-1.2, align="center")
    text("UADE  ·  Diseño de Videojuegos  ·  2026",
         0.6, 6.65, 10, C["mutedSoft"], font=MONO,
         box_w=13.33-1.2, align="center", char_spacing=2)

    footer(13, TOTAL)

for fn in [slide1, slide2, slide3, slide4, slide5, slide6,
           slide7, slide8, slide9, slide10, slide11, slide12, slide13]:
    fn(); c.showPage()

c.save()
print("WROTE:", OUT)
