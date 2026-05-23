# -*- coding: utf-8 -*-
"""
Freaky Super Mesh - GDD
Informe A4 vertical con estetica dark studio.
El texto es EXACTAMENTE el mismo del documento original.
"""
import os
from reportlab.pdfgen import canvas
from reportlab.lib.pagesizes import A4
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from reportlab.lib.utils import ImageReader
from reportlab.lib.colors import HexColor

# ---------- FONTS ----------
FONTS_DIR = r"C:\Windows\Fonts"
def _reg(name, fname):
    try:
        pdfmetrics.registerFont(TTFont(name, os.path.join(FONTS_DIR, fname)))
        return True
    except Exception:
        return False

_reg("Calibri",       "calibri.ttf")
_reg("Calibri-Bold",  "calibrib.ttf")
_reg("Calibri-It",    "calibrii.ttf")
_reg("SegoeSB",       "seguisb.ttf")
_reg("SegoeBold",     "segoeuib.ttf")
_reg("SegoeReg",      "segoeui.ttf")
_reg("Consolas",      "consola.ttf")
_reg("ConsolasB",     "consolab.ttf")
_reg("Georgia",       "georgia.ttf")

F_BODY   = "Calibri"
F_BODY_B = "Calibri-Bold"
F_BODY_I = "Calibri-It"
F_DISP   = "SegoeSB"
F_DISP_B = "SegoeBold"
F_DISP_R = "SegoeReg"
F_MONO   = "Consolas"
F_MONO_B = "ConsolasB"

# ---------- PALETTE ----------
C = {
    "paper":    HexColor("#F4F0E4"),  # cream paper
    "paperAlt": HexColor("#EAE4D4"),
    "ink":      HexColor("#1C2128"),  # main text
    "inkSoft":  HexColor("#3A4256"),
    "muted":    HexColor("#6D7688"),
    "mutedSoft":HexColor("#8892A5"),
    "rule":     HexColor("#B8B3A2"),
    "ruleSoft": HexColor("#D3CEBE"),
    "amber":    HexColor("#D4A03C"),
    "amberDim": HexColor("#8A6A28"),
    "dark":     HexColor("#121722"),
    "darkCard": HexColor("#1C2230"),
    "coral":    HexColor("#C44536"),
    "teal":     HexColor("#4FB3A9"),
    "white":    HexColor("#FFFFFF"),
    "inkOnDark":HexColor("#EAE4D4"),
}

# ---------- PAGE ----------
PW, PH = A4          # 595 x 842
MARGIN_X = 54
MARGIN_TOP = 80
MARGIN_BOT = 72
CONTENT_W = PW - MARGIN_X*2

IMG_DIR  = os.path.join(os.path.dirname(__file__), "gdd_images_opt")
OUT_PATH = os.path.join(os.path.dirname(__file__), "Freaky_Super_Mesh_GDD.pdf")

# ---------- TEXT WRAP ----------
def wrap_text(c, text, font, size, max_w):
    c.setFont(font, size)
    words = text.split()
    if not words: return []
    lines, cur = [], words[0]
    for w in words[1:]:
        t = cur + " " + w
        if pdfmetrics.stringWidth(t, font, size) <= max_w:
            cur = t
        else:
            lines.append(cur); cur = w
    lines.append(cur)
    return lines

def draw_paragraph(c, text, x, y, w, font=F_BODY, size=10.5, leading=14.5, color=None, align="left"):
    if color is not None:
        c.setFillColor(color)
    else:
        c.setFillColor(C["ink"])
    lines = wrap_text(c, text, font, size, w)
    cy = y
    for line in lines:
        if align == "center":
            tw = pdfmetrics.stringWidth(line, font, size)
            c.drawString(x + (w - tw)/2, cy, line)
        elif align == "right":
            tw = pdfmetrics.stringWidth(line, font, size)
            c.drawString(x + w - tw, cy, line)
        else:
            c.drawString(x, cy, line)
        cy -= leading
    return y - leading*len(lines)

# ---------- CHROME ----------
def corner_mark(c, x, y, size, color, corner):
    c.setStrokeColor(color); c.setLineWidth(1.1); c.setLineCap(0)
    if corner == "tl":
        c.line(x, y, x+size, y); c.line(x, y, x, y-size)
    elif corner == "tr":
        c.line(x, y, x-size, y); c.line(x, y, x, y-size)
    elif corner == "bl":
        c.line(x, y, x+size, y); c.line(x, y, x, y+size)
    elif corner == "br":
        c.line(x, y, x-size, y); c.line(x, y, x, y+size)

def page_chrome(c, page_num, total, section_id, section_name, dark=False):
    bg = C["dark"] if dark else C["paper"]
    ink = C["inkOnDark"] if dark else C["ink"]
    muted = C["mutedSoft"] if dark else C["muted"]
    rule = HexColor("#3A4256") if dark else C["rule"]

    c.setFillColor(bg); c.rect(0, 0, PW, PH, fill=1, stroke=0)

    # corner marks
    cm_size = 10
    m = 22
    col = C["amber"]
    corner_mark(c, m,      PH-m, cm_size, col, "tl")
    corner_mark(c, PW-m,   PH-m, cm_size, col, "tr")
    corner_mark(c, m,      m,    cm_size, col, "bl")
    corner_mark(c, PW-m,   m,    cm_size, col, "br")

    # top rule
    c.setStrokeColor(rule); c.setLineWidth(0.4)
    c.line(MARGIN_X, PH-54, PW-MARGIN_X, PH-54)

    # top header text
    c.setFillColor(muted); c.setFont(F_MONO, 7.5)
    c.drawString(MARGIN_X, PH-46, section_id)
    c.setFillColor(ink); c.setFont(F_DISP, 8.5)
    c.drawString(MARGIN_X + 42, PH-46, section_name)

    # top right
    c.setFillColor(muted); c.setFont(F_MONO, 7.5)
    right_txt = f"FSM · GDD · {page_num:02d}/{total:02d}"
    tw = pdfmetrics.stringWidth(right_txt, F_MONO, 7.5)
    c.drawString(PW-MARGIN_X-tw, PH-46, right_txt)

    # bottom rule
    c.setStrokeColor(rule); c.setLineWidth(0.4)
    c.line(MARGIN_X, 48, PW-MARGIN_X, 48)

    # footer
    c.setFillColor(muted); c.setFont(F_MONO, 7.5)
    c.drawString(MARGIN_X, 36, "EQUIPO DINAMITA")
    c.setFillColor(ink); c.setFont(F_DISP, 8)
    c.drawString(MARGIN_X + 78, 36, "Freaky Super Mesh")
    right_txt2 = "BUILD 00.1 · DIM · 03"
    tw2 = pdfmetrics.stringWidth(right_txt2, F_MONO, 7.5)
    c.setFillColor(muted); c.setFont(F_MONO, 7.5)
    c.drawString(PW-MARGIN_X-tw2, 36, right_txt2)

# ---------- SECTION HEADER ----------
def section_header(c, number, title, subtitle, y):
    # big number
    c.setFillColor(C["amber"]); c.setFont(F_DISP_B, 44)
    num_txt = f"{number:02d}"
    c.drawString(MARGIN_X, y-44, num_txt)
    num_w = pdfmetrics.stringWidth(num_txt, F_DISP_B, 44)

    # vertical bar
    c.setStrokeColor(C["amber"]); c.setLineWidth(1.2)
    bx = MARGIN_X + num_w + 14
    c.line(bx, y-46, bx, y-2)

    # title + subtitle
    c.setFillColor(C["ink"]); c.setFont(F_DISP_B, 22)
    c.drawString(bx + 14, y-22, title)
    c.setFillColor(C["muted"]); c.setFont(F_MONO, 8)
    c.drawString(bx + 14, y-38, subtitle)

    # under-rule
    c.setStrokeColor(C["ruleSoft"]); c.setLineWidth(0.6)
    c.line(MARGIN_X, y-60, PW-MARGIN_X, y-60)
    return y - 78

def sub_header(c, text, y, tag=""):
    c.setFillColor(C["ink"]); c.setFont(F_DISP_B, 13)
    c.drawString(MARGIN_X, y, text)
    if tag:
        c.setFillColor(C["amberDim"]); c.setFont(F_MONO, 7.5)
        tw = pdfmetrics.stringWidth(text, F_DISP_B, 13)
        c.drawString(MARGIN_X + tw + 10, y+2, tag)
    # short accent under
    c.setStrokeColor(C["amber"]); c.setLineWidth(1.2)
    c.line(MARGIN_X, y-6, MARGIN_X + 28, y-6)
    return y - 22

def tech_label(c, text, x, y, color=None, size=8):
    c.setFillColor(color or C["amberDim"])
    c.setFont(F_MONO, size)
    c.drawString(x, y, text)

def place_image(c, path, x, y, w, caption=None, tag=None, bordered=True):
    if not os.path.exists(path):
        return y
    try:
        img = ImageReader(path)
        iw, ih = img.getSize()
        ratio = ih / iw
        h = w * ratio
        if bordered:
            # light frame
            c.setStrokeColor(C["rule"]); c.setLineWidth(0.5)
            c.rect(x-1, y-h-1, w+2, h+2, stroke=1, fill=0)
            # inner corner marks
            s = 6
            corner_mark(c, x+4,       y-4,   s, C["amber"], "tl")
            corner_mark(c, x+w-4,     y-4,   s, C["amber"], "tr")
            corner_mark(c, x+4,       y-h+4, s, C["amber"], "bl")
            corner_mark(c, x+w-4,     y-h+4, s, C["amber"], "br")
        c.drawImage(img, x, y-h, width=w, height=h, preserveAspectRatio=True, mask='auto')
        # caption
        cy = y - h - 4
        if tag:
            tech_label(c, tag, x, cy-8)
        if caption:
            c.setFillColor(C["muted"]); c.setFont(F_BODY_I, 9)
            c.drawString(x + (pdfmetrics.stringWidth(tag, F_MONO, 8) + 8 if tag else 0), cy-8, caption)
        return cy - 18
    except Exception as e:
        return y

# ---------- DOCUMENT ----------
TOTAL_PAGES = 23
c = canvas.Canvas(OUT_PATH, pagesize=A4)
c.setTitle("Freaky Super Mesh - GDD - Equipo Dinamita")
c.setAuthor("Equipo Dinamita")

page_num = 1
def new_page(section_id="§00", section_name="", dark=False):
    global page_num
    if page_num > 1:
        c.showPage()
    page_chrome(c, page_num, TOTAL_PAGES, section_id, section_name, dark=dark)
    page_num += 1

# =============== PAGE 1 — COVER (dark) ===============
page_chrome(c, 1, TOTAL_PAGES, "§00", "DOCUMENTO INTERNO", dark=True)
page_num = 2

# large decorative amber rect background motif
c.setFillColor(HexColor("#1C2230")); c.rect(MARGIN_X, 180, CONTENT_W, 380, fill=1, stroke=0)
# thin amber line
c.setStrokeColor(C["amber"]); c.setLineWidth(1.5)
c.line(MARGIN_X, 555, MARGIN_X+90, 555)

# Cover text block
c.setFillColor(C["amber"]); c.setFont(F_MONO, 9)
c.drawString(MARGIN_X + 16, 520, "EQUIPO DINAMITA / DOC.GDD.01")
c.setFillColor(C["inkOnDark"]); c.setFont(F_DISP_B, 46)
c.drawString(MARGIN_X + 16, 460, "FREAKY")
c.drawString(MARGIN_X + 16, 410, "SUPER MESH")
c.setFillColor(HexColor("#8892A5")); c.setFont(F_DISP_R, 14)
c.drawString(MARGIN_X + 16, 380, "Action Rogue-lite · offline · PC")

# meta grid
meta = [
    ("TIPO",        "Game Design Document"),
    ("GÉNERO",      "Action Rogue-lite"),
    ("PLATAFORMA",  "PC (alta y baja gama)"),
    ("VISTA",       "Cenital isométrica 53°"),
    ("MODO",        "Single-player"),
    ("EQUIPO",      "Dinamita (3 integrantes)"),
]
gy = 330
for i, (k, v) in enumerate(meta):
    col = i % 2
    row = i // 2
    gx = MARGIN_X + 16 + col*230
    gyy = gy - row*36
    c.setFillColor(C["amberDim"]); c.setFont(F_MONO, 7.5)
    c.drawString(gx, gyy, k)
    c.setFillColor(C["inkOnDark"]); c.setFont(F_DISP, 12)
    c.drawString(gx, gyy-14, v)

# bottom band
c.setFillColor(C["amber"]); c.rect(MARGIN_X, 205, CONTENT_W, 2, fill=1, stroke=0)
c.setFillColor(HexColor("#6D7688")); c.setFont(F_MONO, 8)
c.drawString(MARGIN_X + 16, 186, "RELEASE / PRESENTACIÓN: 24.04.2026")
c.drawString(MARGIN_X + 16, 172, "DOC. CONFIDENCIAL · INTERNO")

# Bottom title small
c.setFillColor(C["inkOnDark"]); c.setFont(F_DISP_B, 11)
c.drawString(MARGIN_X, 130, "GAME DESIGN DOCUMENT")
c.setFillColor(HexColor("#8892A5")); c.setFont(F_MONO, 8)
c.drawString(MARGIN_X, 115, "v.01  ·  ES-AR  ·  A4  ·  Equipo Dinamita")

# corner accent squares
for (xx, yy) in [(MARGIN_X, 560), (PW-MARGIN_X-8, 560)]:
    c.setFillColor(C["amber"]); c.rect(xx, yy, 8, 8, fill=1, stroke=0)

# =============== PAGE 2 — ÍNDICE ===============
new_page("§00.2", "TABLA DE CONTENIDOS")
y = section_header(c, 0, "Índice", "tabla de contenidos · sección 00", PH-MARGIN_TOP)

toc = [
    ("01", "Concepto General",                      "03"),
    ("02", "Narrativa y contexto",                  "04"),
    ("03", "Mecánicas principales",                 "04"),
    ("04", "Progresión y estructura de Niveles",    "05"),
    ("05", "Assets",                                "07"),
    ("06", "Desarrollo",                            "11"),
    ("07", "Referencias",                           "13"),
    ("08", "Uso de IA",                             "16"),
]
row_h = 36
for i, (n, t, p) in enumerate(toc):
    ry = y - i*row_h
    # dashed separator
    c.setStrokeColor(C["ruleSoft"]); c.setLineWidth(0.3)
    c.setDash(1,2); c.line(MARGIN_X, ry-8, PW-MARGIN_X, ry-8); c.setDash()
    # number
    c.setFillColor(C["amber"]); c.setFont(F_DISP_B, 18)
    c.drawString(MARGIN_X, ry-2, n)
    # title
    c.setFillColor(C["ink"]); c.setFont(F_DISP, 14)
    c.drawString(MARGIN_X + 40, ry-2, t)
    # page
    c.setFillColor(C["muted"]); c.setFont(F_MONO, 10)
    tw = pdfmetrics.stringWidth(f"p.{p}", F_MONO, 10)
    c.drawString(PW-MARGIN_X-tw, ry-2, f"p.{p}")

# footer note
c.setFillColor(C["mutedSoft"]); c.setFont(F_BODY_I, 9)
c.drawString(MARGIN_X, 80, "Equipo Dinamita  ·  Game Design Document  ·  v.01")

# =============== PAGE 3 — CONCEPTO GENERAL ===============
new_page("§01", "CONCEPTO GENERAL")
y = section_header(c, 1, "Concepto general de juego", "equipo dinamita · sección 01", PH-MARGIN_TOP)

t1 = ("El siguiente documento presentado por el Equipo Dinamita propone su próximo juego "
      "Action Rogue-lite offline destinado a ser jugable en computadoras. Freaky Super Mesh "
      "rescata elementos de la acción, el humor y la supervivencia. El jugador controla un "
      "personaje que ataca automáticamente mientras lucha contra oleadas continuas de monstruos, "
      "con el objetivo de sobrevivir al ataque el mayor tiempo posible y desbloquear armas y "
      "reliquias adicionales para las sesiones posteriores.")
y = draw_paragraph(c, t1, MARGIN_X, y, CONTENT_W)
y -= 8

t2 = ("Se busca la reminiscencia jugable y visual hacia el gameplay de juego que hoy en día "
      "triunfan en el mercado con ideas simples pero efectivas. Generar en el jugador un feedback "
      "de comodidad el cual no venga cargado de mucho esfuerzo por entender el juego pero si con "
      "una alta recompensa estimulante.")
y = draw_paragraph(c, t2, MARGIN_X, y, CONTENT_W)
y -= 8

t3 = ("El énfasis se encuentra en la rejugabilidad del juego en solitario donde cada partida el "
      "usuario por mérito propio aprenda y que la experiencia conseguida sea la recompensa más "
      "valiosa.")
y = draw_paragraph(c, t3, MARGIN_X, y, CONTENT_W)
y -= 20

# Pull-quote
quote_h = 60
c.setFillColor(C["paperAlt"]); c.rect(MARGIN_X, y-quote_h, CONTENT_W, quote_h, fill=1, stroke=0)
c.setStrokeColor(C["amber"]); c.setLineWidth(3); c.line(MARGIN_X, y-quote_h, MARGIN_X, y)
c.setFillColor(C["amberDim"]); c.setFont(F_MONO, 7.5)
c.drawString(MARGIN_X + 16, y-16, "CITA · DR. EMILIO")
c.setFillColor(C["ink"]); c.setFont(F_DISP_B, 16)
c.drawString(MARGIN_X + 16, y-36, "\u201CLos errores te hacen más fuerte\u2026\u201D")
c.setFillColor(C["muted"]); c.setFont(F_BODY_I, 10)
c.drawString(MARGIN_X + 16, y-52, "el Doctor Emilio:")
y -= quote_h + 18

# decorative cover art if exists
img1 = os.path.join(IMG_DIR, "p03_img2.jpg")
if os.path.exists(img1):
    y = place_image(c, img1, MARGIN_X, y, 200, caption="render conceptual del protagonista", tag="FIG.01")

# =============== PAGE 4 — NARRATIVA Y MECANICAS ===============
new_page("§02-03", "NARRATIVA · MECÁNICAS")
y = section_header(c, 2, "Narrativa y contexto", "historia del dr. emilio · sección 02", PH-MARGIN_TOP)

t1 = ("En Freaky Super Mesh el jugador debe guiar al Dr. Emilio de vuelta a su casa luego de que "
      "un experimento fallido lo lleve a otras dimensiones.")
y = draw_paragraph(c, t1, MARGIN_X, y, CONTENT_W)
y -= 6

t2 = ("Las acciones toman lugar en una perspectiva cenital, con una inclinación de la cámara en 53 "
      "grados hacia abajo, estilo isométrica (ver imagen de referencia)")
y = draw_paragraph(c, t2, MARGIN_X, y, CONTENT_W)
y -= 14

# reference image
img_ref = os.path.join(IMG_DIR, "p04_img1.jpg")
if os.path.exists(img_ref):
    y = place_image(c, img_ref, MARGIN_X, y, 280, caption="vista isométrica 53° de referencia", tag="REF.01")
y -= 8

y = section_header(c, 3, "Mecánicas principales", "sistema de combate · sección 03", y+12)

m1 = ("El juego tendrá un sistema de combate en el que se moverá el Dr Emilio con la teclas WASD "
      "(w= arriba) (A= izquierda) (S= Abajo) (D= Derecha), podrá apuntar con el mouse a donde quiere "
      "lanzar sus probetas para así derrotar a los enemigos de cada dimensión y también podrá "
      "bloquear todo tipo de ataques con la tecla \u201CShift\u201D.")
y = draw_paragraph(c, m1, MARGIN_X, y, CONTENT_W)
y -= 6

# Controls mini-card
card_h = 54
c.setFillColor(C["paperAlt"]); c.rect(MARGIN_X, y-card_h, CONTENT_W, card_h, fill=1, stroke=0)
c.setFillColor(C["amberDim"]); c.setFont(F_MONO, 7.5)
c.drawString(MARGIN_X + 12, y-14, "CONTROLES · INPUT MAP")
controls = [("W","ARRIBA"), ("A","IZQUIERDA"), ("S","ABAJO"), ("D","DERECHA"), ("MOUSE","APUNTAR"), ("SHIFT","BLOQUEAR")]
cx = MARGIN_X + 12
for k, v in controls:
    c.setFillColor(C["dark"]); c.roundRect(cx, y-42, 38, 18, 3, fill=1, stroke=0)
    c.setFillColor(C["amber"]); c.setFont(F_MONO_B, 9)
    tw = pdfmetrics.stringWidth(k, F_MONO_B, 9)
    c.drawString(cx + (38-tw)/2, y-36, k)
    c.setFillColor(C["ink"]); c.setFont(F_MONO, 7.5)
    c.drawString(cx + 42, y-32, v)
    cx += 80
y -= card_h + 10

m2 = ("Cada dimensión tendrá su propio estilo de enemigo y al final de cada una, habrá un jefe final "
      "que eliminarlo te va a dar acceso a la próxima.")
y = draw_paragraph(c, m2, MARGIN_X, y, CONTENT_W)
y -= 4

m3 = ("La racha de eliminaciones y el bloqueo de ataques van a hacer que Emilio entre en un estado "
      "eufórico. En este estado todas sus estadísticas se potencian.")
y = draw_paragraph(c, m3, MARGIN_X, y, CONTENT_W)
y -= 4

m4 = ("La salud de Emilio va a bajar dependiendo del enemigo que lo ataque. La única forma que "
      "recupere su salud es entrando en el modo eufórico.")
y = draw_paragraph(c, m4, MARGIN_X, y, CONTENT_W)
y -= 4

m5 = ("El corazón de Freaky Super Mesh es su combate frenético en el que el jugador deberá tomar "
      "decisiones en poco tiempo. Mientras el jugador progrese en el videojuego, este se volverá más "
      "difícil, este sistema facilita el entendimiento de las mecánicas y que los jugadores no lo "
      "abandonen en el primer nivel.")
y = draw_paragraph(c, m5, MARGIN_X, y, CONTENT_W)

# =============== PAGE 5 — TRAMA Y ESCENARIOS ===============
new_page("§04", "TRAMA Y ESCENARIOS")
y = section_header(c, 4, "Trama y escenarios", "universo del juego · sección 04", PH-MARGIN_TOP)

t1 = ("Freaky Super Mesh está situado en un universo bizarro y absurdo en el que los elementos de "
      "un laboratorio se rebelan contra su dueño.")
y = draw_paragraph(c, t1, MARGIN_X, y, CONTENT_W); y -= 6

t2 = ("El Dr Emilio, un científico frustrado, que desde joven fue mediocre, se pasó toda su vida "
      "profesional intentando crear un invento que lo hiciera entrar al salón de la fama de la "
      "ciencia. Cuando por fin logra crear un portal que une distintas dimensiones, este falla "
      "tragándose su laboratorio entero. En estas realidades opuestas a su vida regular, Emilio se da "
      "cuenta de un odio recíproco a la ciencia, lo que lo lleva a batallar contra los elementos de "
      "su laboratorio e intenta volver a su dimensión original.")
y = draw_paragraph(c, t2, MARGIN_X, y, CONTENT_W); y -= 6

t3 = ("Por suerte, en este camino consigue un aliado: sus probetas que guarda desde que es niño, "
      "las cuales usará de arma principal.")
y = draw_paragraph(c, t3, MARGIN_X, y, CONTENT_W)
y -= 14

# two-up images
imgA = os.path.join(IMG_DIR, "p05_img1.jpg")
imgB = os.path.join(IMG_DIR, "p05_img3.jpg")
col_w = (CONTENT_W - 16) / 2
yA = place_image(c, imgA, MARGIN_X, y, col_w, caption="concept · dr. emilio", tag="CH.01") if os.path.exists(imgA) else y
yB = place_image(c, imgB, MARGIN_X + col_w + 16, y, col_w, caption="concept · freaky emilio", tag="CH.02") if os.path.exists(imgB) else y
y = min(yA, yB)

# =============== PAGE 6 — NIVELES (intro + N0 + N1) ===============
new_page("§05", "PROGRESION Y NIVELES")
y = section_header(c, 5, "Progresión y estructura de niveles", "escenarios · sección 05", PH-MARGIN_TOP)

# N0
y = sub_header(c, "Nivel / escenario 0", y, "N.00 · LAB")
y = draw_paragraph(c, "Laboratorio, el jugador puede moverse libremente pero no atacar", MARGIN_X, y, CONTENT_W)
y -= 14

# N1
y = sub_header(c, "Nivel / escenario 1 — El desierto", y, "N.01 · WESTERN")
n1 = ("Dr Emilio es teletransportado a un pueblo del lejano oeste. Los Mecheros Bunsen unos "
      "forajidos vestidos con ponchos, sombreros vaqueros y un palillo en la boca, tomaron el "
      "pueblo. Se mueven muy lento y escupen bolas de fuego. Es el primer enemigo en masa que ataca "
      "a Emilio.")
y = draw_paragraph(c, n1, MARGIN_X, y, CONTENT_W); y -= 4
n1b = ("Entre ellos, está el nuevo sheriff del pueblo, el microscopio, Un enorme y oxidado "
       "microscopio óptico con una gran estrella de sheriff. Es la ley en esta dimensión y no tolera "
       "forasteros, confunde a Emilio con uno de ellos. El sheriff proyecta una luz roja en forma de "
       "cono frente a él (su \"campo de visión\"). Gira lentamente buscando a Emilio. Si el jugador "
       "entra en el cono de luz, el jefe dispara una ráfaga rápida de lentes rotos. Obliga al jugador "
       "a usar las probetas de Emilio apuntando desde lejos y usando las rocas del mapa como cobertura.")
y = draw_paragraph(c, n1b, MARGIN_X, y, CONTENT_W); y -= 4
n1m = "Esta dimensión tiene sonidos típicos del mundo western, y una música frenética acompañada por un banjo."
y = draw_paragraph(c, n1m, MARGIN_X, y, CONTENT_W, font=F_BODY_I, color=C["inkSoft"])
y -= 14

# N2
y = sub_header(c, "Nivel / dimension 2 — El supermercado", y, "N.02 · RETAIL")
n2 = ("Dr. Emilio aparece en medio de un supermercado dominado por Abuelas Lupas. Las Lupas "
      "Jubilada, son lupas de laboratorio con pelucas grises y carritos de compra. tomaron los "
      "pasillos y odian que Emilio corra por la tienda. Se mueven muy lento empujando sus carritos. "
      "El ataque de las abuelas es concentrar la luz, disparando un rayo de calor lineal. Si te toca, "
      "Emilio se quema y corre más rápido descontroladamente por un segundo.")
y = draw_paragraph(c, n2, MARGIN_X, y, CONTENT_W); y -= 4
n2b = ("El boss final de esta dimensión es una caja registradora centrífuga, una mezcla bizarra "
       "entre una caja registradora antigua y una centrífuga de laboratorio. Está ubicada en la zona "
       "de cajas rápidas y cree que Emilio quiere irse sin pagar.")
y = draw_paragraph(c, n2b, MARGIN_X, y, CONTENT_W); y -= 4
n2c = ("Las mecánicas de este jefe son: Se queda fija en un lugar pero gira sobre sí misma lanzando "
       "monedas y tickets de laboratorio en todas direcciones.")
y = draw_paragraph(c, n2c, MARGIN_X, y, CONTENT_W); y -= 4
n2m = ("Música: Sonidos de \"Cha-ching!\" de cajas registradoras, murmullos de gente y una música de "
       "ascensor pero acelerada con un ritmo de French House.")
y = draw_paragraph(c, n2m, MARGIN_X, y, CONTENT_W, font=F_BODY_I, color=C["inkSoft"])

# =============== PAGE 7 — N3 + intro assets ===============
new_page("§05", "PROGRESION Y NIVELES")
y = PH - MARGIN_TOP

# N3
y = sub_header(c, "Nivel / dimension 3 — El oceano", y, "N.03 · OCEAN")
n3 = ("Emilio aparece en una balsa y es atacado por matraces Erlenmeyer piratas con parches en el "
      "ojo y bandanas. Son caóticos y siempre parecen estar mareados por el movimiento del barco.")
y = draw_paragraph(c, n3, MARGIN_X, y, CONTENT_W); y -= 4
n3b = "Su ataque es tirarse de cabeza contra Emilio, intentando hundir el barco."
y = draw_paragraph(c, n3b, MARGIN_X, y, CONTENT_W); y -= 4
n3c = ("El jefe de esta dimensión es el termómetro torpedo, un termómetro que se cree tiburón y "
       "solo se ve su punta roja sobresaliendo del mercurio")
y = draw_paragraph(c, n3c, MARGIN_X, y, CONTENT_W); y -= 4
n3d = ("La mecánica de este combate se basa en el jefe saliendo del agua y atacando el barco "
       "saltando de un lado a otro. El jugador debe usar las probetas para disparar en el aire. De "
       "vez en cuando, el jefe golpea el costado del barco, haciendo que todo se sacuda y Emilio "
       "pierda el equilibrio si no bloquea con Shift.")
y = draw_paragraph(c, n3d, MARGIN_X, y, CONTENT_W)
y -= 20

# section 6 header
y = section_header(c, 6, "Listado de assets", "inventario de desarrollo · sección 06", y+10)

intro_assets = ("Dado que el juego contendrá una gran cantidad de assets, hicimos un listado de ellos.")
y = draw_paragraph(c, intro_assets, MARGIN_X, y, CONTENT_W, color=C["muted"], font=F_BODY_I)
y -= 12

# Personajes (start listing)
y = sub_header(c, "Personajes", y, "A.01 · CHAR")
personajes = [
    "Dr. Emilio (modelo 3D normal)",
    "Freaky Emilio (modelo 3D)",
    "Emilio — anim. Idle",
    "Emilio — anim. Caminar (8 dir.)",
    "Emilio — anim. Atacar",
    "Emilio — anim. Bloquear/Parry",
    "Emilio — anim. Transformación",
    "Emilio — anim. Usar ítem",
    "Emilio — anim. Morir",
    "Freaky Emilio — anim. Idle",
    "Freaky Emilio — anim. Caminar",
    "Freaky Emilio — anim. Atacar",
    "Freaky Emilio — anim. Morir",
]
# two-column list
col_w2 = (CONTENT_W - 20) / 2
for i, it in enumerate(personajes):
    col = i % 2
    row = i // 2
    x = MARGIN_X + col*(col_w2+20)
    yy = y - row*14
    c.setFillColor(C["amber"]); c.circle(x+3, yy+3, 1.2, fill=1, stroke=0)
    c.setFillColor(C["ink"]); c.setFont(F_BODY, 10)
    c.drawString(x+10, yy, it)
y = y - ((len(personajes)+1)//2)*14 - 8

# =============== PAGE 8 — ASSETS: Laboratorio, Desierto ===============
def draw_asset_group(c, y, title, tag, items):
    y = sub_header(c, title, y, tag)
    col_w2 = (CONTENT_W - 20) / 2
    rows = (len(items) + 1) // 2
    for i, it in enumerate(items):
        col = i % 2
        row = i // 2
        x = MARGIN_X + col*(col_w2+20)
        yy = y - row*14
        c.setFillColor(C["amber"]); c.circle(x+3, yy+3, 1.2, fill=1, stroke=0)
        c.setFillColor(C["ink"]); c.setFont(F_BODY, 10)
        c.drawString(x+10, yy, it)
    return y - rows*14 - 14

new_page("§06", "LISTADO DE ASSETS")
y = PH - MARGIN_TOP

lab = [
    "Escenario: laboratorio",
    "Props: mesadas con aparatos",
    "Props: estantes y probetas",
    "Portal dimensional",
    "Interacción con experimento",
    "Música: laboratorio / ambient",
]
y = draw_asset_group(c, y, "Laboratorio", "A.02 · LAB", lab)

des_en = [
    "Mechero Bunsen (modelo 3D)",
    "Mechero — variantes de poncho",
    "Mechero — anim. Idle",
    "Mechero — anim. Caminar",
    "Mechero — anim. Atacar",
    "Mechero — anim. Morir",
    "Microscopio Sheriff (modelo 3D)",
    "Sheriff — anim. Idle / Girar",
    "Sheriff — anim. Disparar",
    "Sheriff — anim. Recibir daño",
    "Sheriff — anim. Morir",
]
y = draw_asset_group(c, y, "Desierto · Enemigos", "A.03 · ENEM", des_en)

des_fx = [
    "Escenario: pueblo del desierto",
    "Props: rocas de cobertura",
    "Cielo naranja / atardecer",
    "Efecto: bola de fuego",
    "Efecto: cono de luz roja",
    "Efecto: ráfaga de lentes rotos",
    "Efecto: nube de polvo",
    "Música: banjo frenético",
    "SFX: clic de encendedor",
    "SFX: silbido western",
    "SFX: acorde de tensión",
    "SFX: disparo de lentes",
]
y = draw_asset_group(c, y, "Desierto · Escenarios y efectos", "A.04 · FX", des_fx)

super_en = [
    "Lupa Jubilada (modelo 3D)",
    "Lupa — anim. Idle",
    "Lupa — anim. Caminar",
    "Lupa — anim. Atacar",
    "Lupa — anim. Morir",
    "Caja Registradora Centrífuga",
    "Caja — anim. Giro lento (f1)",
    "Caja — anim. Giro rápido (f2)",
    "Caja — anim. Morir",
]
y = draw_asset_group(c, y, "Supermercado · Enemigos", "A.05 · ENEM", super_en)

# =============== PAGE 9 — ASSETS: super fx + oceano + globales ===============
new_page("§06", "LISTADO DE ASSETS")
y = PH - MARGIN_TOP

super_fx = [
    "Escenario: supermercado",
    "Props: estanterías con productos",
    "Props: carrito de compra",
    "Iluminación: neón fluorescente",
    "Efecto: rayo de calor lineal",
    "Efecto: quemado + sprint loco",
    "Efecto: monedas girando",
    "Efecto: tickets de laboratorio",
    "Música: ascensor / French House",
    "SFX: cha-ching caja registradora",
    "SFX: pitido código de barras",
    "SFX: carrito chirriando",
    "SFX: murmullo de abuela",
]
y = draw_asset_group(c, y, "Supermercado · Escenarios y efectos", "A.06 · FX", super_fx)

oc_en = [
    "Matraz Erlenmeyer Pirata (3D)",
    "Matraz — variantes de bandana",
    "Matraz — anim. Idle (tambaleo)",
    "Matraz — anim. Caminar",
    "Matraz — anim. Embestida",
    "Matraz — anim. Morir",
    "Termómetro Torpedo (modelo 3D)",
    "Termómetro — anim. Nadar",
    "Termómetro — anim. Saltar",
    "Termómetro — anim. Atacar",
    "Termómetro — anim. Morir",
]
y = draw_asset_group(c, y, "Océano · Enemigos", "A.07 · ENEM", oc_en)

oc_fx = [
    "Escenario: balsa jugable",
    "Escenario: agua animada",
    "Escenario: horizonte / cielo",
    "Efecto: sacudida de balsa",
    "Efecto: salpicadura de mercurio",
    "Efecto: lluvia termómetros rotos",
    "Música: acordeón caribeño",
    "Música: tensión del boss",
    "SFX: sonidos de agua",
    "SFX: gaviotas",
    "SFX: canto pirata desafinado",
    "SFX: golpe en casco de balsa",
    "SFX: explosión de mercurio",
]
y = draw_asset_group(c, y, "Océano · Escenarios y efectos", "A.08 · FX", oc_fx)

# =============== PAGE 10 — ASSETS globales + HUD/UI ===============
new_page("§06", "LISTADO DE ASSETS")
y = PH - MARGIN_TOP

glob = [
    "Proyectil: probeta",
    "Efecto: splash al impactar",
    "Efecto: parry / bloqueo",
    "Efecto: transformación Freaky",
    "Medidor de Absurdidad (UI)",
    "Barra de vida (UI)",
    "HUD general",
    "SFX: lanzar probeta",
    "SFX: impacto y splash",
    "SFX: parry exitoso",
    "SFX: activación Suero X",
    "Música: modo Freaky (Suero X)",
]
y = draw_asset_group(c, y, "Globales · Sistema de combate", "A.09 · SYS", glob)

hud = [
    "Pantalla de título",
    "Menú principal",
    "Pantalla de game over",
    "Pantalla de victoria / portal",
    "Pantalla de inventario/stats",
    "Cinemáticas de intro",
    "Tipografía: display title",
    "Tipografía: nombre de dimensión",
    "Tipografía: UI en juego",
    "Tipografía: diálogos de boss",
    "Tipografía: game over",
]
y = draw_asset_group(c, y, "HUD / UI", "A.10 · UI", hud)

# state card
y -= 6
c.setFillColor(C["paperAlt"]); c.rect(MARGIN_X, y-50, CONTENT_W, 50, fill=1, stroke=0)
c.setStrokeColor(C["amber"]); c.setLineWidth(2); c.line(MARGIN_X, y-50, MARGIN_X, y)
c.setFillColor(C["amberDim"]); c.setFont(F_MONO, 7.5)
c.drawString(MARGIN_X + 16, y-14, "ESTADO DE ASSETS")
c.setFillColor(C["ink"]); c.setFont(F_DISP_B, 13)
c.drawString(MARGIN_X + 16, y-30, "Aquí se encuentra el estado de cada uno:")
c.setFillColor(C["amberDim"]); c.setFont(F_MONO_B, 10)
c.drawString(MARGIN_X + 16, y-44, "FreakySuperMesh_Assets")

# =============== PAGE 11 — DESARROLLO ===============
new_page("§07", "DESARROLLO")
y = section_header(c, 7, "Desarrollo", "equipo y producción · sección 07", PH-MARGIN_TOP)

d1 = ("En los siguientes meses el Equipo Dinamita se va a centrar en el desarrollo de Freaky Super "
      "Mesh. Nuestro equipo se conforma por 3 integrantes: un diseñador y artista conceptual, un "
      "modelador 3D y por último un programador de Unity. En el desarrollo también se encuentran la "
      "banda sonora y todas las aplicaciones AI que sean útiles y ayuden a pulir el resultado final.")
y = draw_paragraph(c, d1, MARGIN_X, y, CONTENT_W)
y -= 16

# team cards
card_w = (CONTENT_W - 20) / 3
card_h = 110
team = [
    ("01", "Diseño + Arte Conceptual", "concept · pipeline · storyboards"),
    ("02", "Modelado 3D",              "personajes · escenarios · props"),
    ("03", "Programación Unity",       "gameplay · IA · UI · build"),
]
for i, (n, t, sub) in enumerate(team):
    x = MARGIN_X + i*(card_w + 10)
    # card bg
    c.setFillColor(C["paperAlt"]); c.rect(x, y-card_h, card_w, card_h, fill=1, stroke=0)
    c.setStrokeColor(C["amber"]); c.setLineWidth(2); c.line(x, y-card_h, x, y)
    # number
    c.setFillColor(C["amber"]); c.setFont(F_DISP_B, 28)
    c.drawString(x+14, y-34, n)
    # title
    c.setFillColor(C["ink"]); c.setFont(F_DISP_B, 11)
    # wrap title if needed
    title_lines = wrap_text(c, t, F_DISP_B, 11, card_w-28)
    ty = y-54
    for ln in title_lines:
        c.drawString(x+14, ty, ln); ty -= 14
    # sub
    c.setFillColor(C["muted"]); c.setFont(F_MONO, 7.5)
    sub_lines = wrap_text(c, sub, F_MONO, 7.5, card_w-28)
    for ln in sub_lines:
        c.drawString(x+14, ty, ln); ty -= 10
y -= card_h + 16

d2 = ("Freaky Super Mesh se va a centrar en tener un estilo de arte marcado, memorable y llamativo. "
      "La intención es que el mismo se pueda utilizar tanto en computadoras de alta gama como de "
      "baja (resoluciones de 640x480 pixeles como piso). La vista isométrica que se plantea viene "
      "acompañada por 8 direcciones de movimiento. Todo esto aplicado tanto al protagonista, su "
      "versión Super, como a los enemigos y jefes.")
y = draw_paragraph(c, d2, MARGIN_X, y, CONTENT_W); y -= 6

d3 = ("Se plantean para las primeras versiones jugables un número de 3 tipos enemigos y 1 jefe por "
      "escenario los cuales van a tener un pack básico de animaciones que contienen: movimiento, "
      "ataque y muerte. Por otro lado el protagonista \u201CEmilio\u201D trae un set de movimientos "
      "más extenso ya que viene acompañado por 2 versiones: movimiento, ataque, transformación, "
      "utilización de item y muerte.")
y = draw_paragraph(c, d3, MARGIN_X, y, CONTENT_W); y -= 6

d4 = ("Por fuera de los modelos se desarrollarán tanto animaciones de ataques, objetos, efectos de "
      "ambiente los cuales sumarán a la experiencia. Adicionalmente todo irá acompañado de "
      "cinemáticas, pantalla de título, modificadores de nivel y pantalla de inventario y estadísticas.")
y = draw_paragraph(c, d4, MARGIN_X, y, CONTENT_W)

# =============== PAGE 12 — Desarrollo continua ===============
new_page("§07", "DESARROLLO")
y = PH - MARGIN_TOP

d5 = ("En todo el sector de programación se busca lograr interfaces intuitivas y cómodas, inputs de "
      "movimiento los cuales sean posibles ejecutar con una sola mano y bajo esfuerzo por parte del "
      "usuario. Posteriormente encontrar un balance entre un juego accesible para todo el mundo y un "
      "reto para jugadores con más experiencia, pensando en la dinámica de niveles aleatorios y "
      "características propias por cada escenario.")
y = draw_paragraph(c, d5, MARGIN_X, y, CONTENT_W); y -= 8

d6 = ("La música va a ser indispensable para la experiencia, se busca llegar a producir una banda "
      "sonora ambiental que cuente la historia de cada nivel/mundo mientras el jugador esté en él. "
      "Tanto la canción de apertura como la del resto de la experiencia se va a ver altamente "
      "influenciada por géneros como el french house, ambient house e inspiración en la banda sonora "
      "de Hades compuesta por Darren Korb. Efectos de sonido de pelea, objetos, poderes, movimiento "
      "y similares se tendrán en cuenta también para lograr un feedback acogedor para el jugador.")
y = draw_paragraph(c, d6, MARGIN_X, y, CONTENT_W)
y -= 20

# audio spec card
c.setFillColor(C["dark"]); c.rect(MARGIN_X, y-110, CONTENT_W, 110, fill=1, stroke=0)
c.setStrokeColor(C["amber"]); c.setLineWidth(1.5)
c.line(MARGIN_X, y, PW-MARGIN_X, y)
c.setFillColor(C["amber"]); c.setFont(F_MONO, 8)
c.drawString(MARGIN_X + 16, y-18, "SPEC · AUDIO DIRECTION")
c.setFillColor(C["inkOnDark"]); c.setFont(F_DISP_B, 16)
c.drawString(MARGIN_X + 16, y-38, "Banda sonora ambiental por nivel")
c.setFillColor(HexColor("#B8B3A2")); c.setFont(F_BODY, 10)
c.drawString(MARGIN_X + 16, y-58, "French house  ·  Ambient house  ·  Inspiración: Darren Korb (Hades)")
# genre pills
pills = ["FRENCH HOUSE", "AMBIENT HOUSE", "HADES OST", "SFX PELEA", "SFX OBJETOS"]
px = MARGIN_X + 16
for p in pills:
    w_p = pdfmetrics.stringWidth(p, F_MONO, 8) + 16
    c.setFillColor(C["amberDim"]); c.roundRect(px, y-92, w_p, 16, 4, fill=1, stroke=0)
    c.setFillColor(C["dark"]); c.setFont(F_MONO_B, 7.5)
    c.drawString(px+8, y-86, p)
    px += w_p + 6

# =============== PAGE 13 — REFERENCIAS (1) ===============
new_page("§08", "REFERENCIAS")
y = section_header(c, 8, "Referencias", "inspiración y estudios previos · sección 08", PH-MARGIN_TOP)

refs_top = [
    ("MegaBonk",          "Nos inspiramos en su tono bizarro y su combate de oleadas",   "p13_img1.jpg", "REF.01"),
    ("Vampire Survivors", "Nos inspiramos en su sistema de combate y las masas de enemigos", "p13_img2.jpg", "REF.02"),
]
for name, desc, img, tag in refs_top:
    # title row
    c.setFillColor(C["amber"]); c.setFont(F_MONO, 8)
    c.drawString(MARGIN_X, y, tag)
    c.setFillColor(C["ink"]); c.setFont(F_DISP_B, 14)
    c.drawString(MARGIN_X + 46, y, name)
    y -= 14
    y = draw_paragraph(c, desc, MARGIN_X + 46, y, CONTENT_W-46, color=C["inkSoft"])
    y -= 4
    ip = os.path.join(IMG_DIR, img)
    if os.path.exists(ip):
        y = place_image(c, ip, MARGIN_X + 46, y, 240, bordered=True)
    y -= 10

# =============== PAGE 14 — REFERENCIAS (2) ===============
new_page("§08", "REFERENCIAS")
y = PH - MARGIN_TOP
refs_mid = [
    ("Hotline Miami",     "Nos inspiramos en su tono bizarro y la estrategia para superar escenarios", "p14_img1.jpg", "REF.03"),
    ("Metal Gear Rising", "Nos inspiramos en su mecánica de parry",                                    "p14_img2.jpg", "REF.04"),
]
for name, desc, img, tag in refs_mid:
    c.setFillColor(C["amber"]); c.setFont(F_MONO, 8)
    c.drawString(MARGIN_X, y, tag)
    c.setFillColor(C["ink"]); c.setFont(F_DISP_B, 14)
    c.drawString(MARGIN_X + 46, y, name)
    y -= 14
    y = draw_paragraph(c, desc, MARGIN_X + 46, y, CONTENT_W-46, color=C["inkSoft"])
    y -= 4
    ip = os.path.join(IMG_DIR, img)
    if os.path.exists(ip):
        y = place_image(c, ip, MARGIN_X + 46, y, 240, bordered=True)
    y -= 10

# =============== PAGE 15 — REFERENCIAS (3) ===============
new_page("§08", "REFERENCIAS")
y = PH - MARGIN_TOP
refs_bot = [
    ("Rick and Morty",               "Nos inspiramos en el personaje principal y el viaje entre dimensiones", "p15_img1.jpg", "REF.05"),
    ("Dr. Mundo (League of Legends)","Nos inspiramos en el modelado del personaje para el modo eufórico de emilio", "p15_img2.jpg", "REF.06"),
    ("Heimerdinger (League of Legends)", "Nos inspiramos la personalidad del personaje para el Dr Emilio", "p16_img1.jpg", "REF.07"),
]
for name, desc, img, tag in refs_bot:
    c.setFillColor(C["amber"]); c.setFont(F_MONO, 8)
    c.drawString(MARGIN_X, y, tag)
    c.setFillColor(C["ink"]); c.setFont(F_DISP_B, 14)
    c.drawString(MARGIN_X + 46, y, name)
    y -= 14
    y = draw_paragraph(c, desc, MARGIN_X + 46, y, CONTENT_W-46, color=C["inkSoft"])
    y -= 4
    ip = os.path.join(IMG_DIR, img)
    if os.path.exists(ip):
        y = place_image(c, ip, MARGIN_X + 46, y, 180, bordered=True)
    y -= 6

# =============== PAGE 16 — IA · GEMINI (intro + boceto colorear) ===============
new_page("§09", "USO DE IA")
y = section_header(c, 9, "Uso de IA", "pipeline con gemini y meshy · sección 09", PH-MARGIN_TOP)

# GEMINI subheader
y = sub_header(c, "GEMINI", y, "AI.01 · IMG")
y = draw_paragraph(c, "Primero colorea este boceto mío, dejando el fondo blanco",
                   MARGIN_X, y, CONTENT_W, color=C["inkSoft"], font=F_BODY_I)
y -= 10

# reference / resultado side by side (p17_img1 and p17_img3)
col_w = (CONTENT_W - 16) / 2
ipA = os.path.join(IMG_DIR, "p17_img1.jpg")
ipB = os.path.join(IMG_DIR, "p17_img3.jpg")
yA = place_image(c, ipA, MARGIN_X, y, col_w, caption="REFERENCIA", tag="IN") if os.path.exists(ipA) else y
yB = place_image(c, ipB, MARGIN_X + col_w + 16, y, col_w, caption="RESULTADO", tag="OUT") if os.path.exists(ipB) else y
y = min(yA, yB) - 8

y = draw_paragraph(c, "Ahora hagamos el mismo proceso desde cero pero con la versión Super de Emilio.",
                   MARGIN_X, y, CONTENT_W, color=C["inkSoft"], font=F_BODY_I)
y -= 10

ipC = os.path.join(IMG_DIR, "p17_img2.jpg")
ipD = os.path.join(IMG_DIR, "p18_img1.jpg")
yC = place_image(c, ipC, MARGIN_X, y, col_w, caption="REFERENCIA", tag="IN") if os.path.exists(ipC) else y
yD = place_image(c, ipD, MARGIN_X + col_w + 16, y, col_w, caption="RESULTADO", tag="OUT") if os.path.exists(ipD) else y
y = min(yC, yD)

# =============== PAGE 17 — IA Gemini (poses) ===============
new_page("§09", "USO DE IA · GEMINI")
y = PH - MARGIN_TOP

y = sub_header(c, "Poses de Emilio", y, "AI.02 · POSES")
prompt1 = ("Ahora tomando esta imagen como la posición neutral de Emilio, respetando esquema de "
           "colores, vestimenta y rasgos, te pido lo siguiente:")
y = draw_paragraph(c, prompt1, MARGIN_X, y, CONTENT_W, color=C["inkSoft"], font=F_BODY_I)
y -= 10

ipA = os.path.join(IMG_DIR, "p18_img2.jpg")
if os.path.exists(ipA):
    y = place_image(c, ipA, MARGIN_X, y, 300, caption="REFERENCIA · pose neutral", tag="IN")
y -= 6

prompt2 = ("Tomando como ejemplo la segunda imagen que te adjunto, haz a Emilio en todas esas "
           "posiciones, respeta todo el diseño de Emilio, este atento también a las extremidades.")
y = draw_paragraph(c, prompt2, MARGIN_X, y, CONTENT_W, color=C["inkSoft"], font=F_BODY_I)
y -= 10

col_w = (CONTENT_W - 16) / 2
ipB = os.path.join(IMG_DIR, "p19_img1.jpg")
ipC = os.path.join(IMG_DIR, "p19_img2.jpg")
yB = place_image(c, ipB, MARGIN_X, y, col_w, caption="REFERENCIA", tag="IN") if os.path.exists(ipB) else y
yC = place_image(c, ipC, MARGIN_X + col_w + 16, y, col_w, caption="RESULTADO", tag="OUT") if os.path.exists(ipC) else y
y = min(yB, yC)

# =============== PAGE 18 — IA Gemini Freaky Emilio ===============
new_page("§09", "USO DE IA · GEMINI")
y = PH - MARGIN_TOP

y = sub_header(c, "Freaky Emilio", y, "AI.03 · FREAKY")
y = draw_paragraph(c, "Ahora lo mismo, pero con Freaky emilio",
                   MARGIN_X, y, CONTENT_W, color=C["inkSoft"], font=F_BODY_I)
y -= 10

col_w = (CONTENT_W - 16) / 2
ipA = os.path.join(IMG_DIR, "p20_img1.jpg")
ipB = os.path.join(IMG_DIR, "p20_img2.jpg")
yA = place_image(c, ipA, MARGIN_X, y, col_w, caption="REFERENCIA · 1", tag="IN.A") if os.path.exists(ipA) else y
yB = place_image(c, ipB, MARGIN_X + col_w + 16, y, col_w, caption="REFERENCIA · 2", tag="IN.B") if os.path.exists(ipB) else y
y = min(yA, yB) - 6

ipC = os.path.join(IMG_DIR, "p20_img3.jpg")
if os.path.exists(ipC):
    y = place_image(c, ipC, MARGIN_X, y, 380, caption="RESULTADO", tag="OUT")

# =============== PAGE 19 — MESHY texto a imagen ===============
new_page("§09", "USO DE IA · MESHY")
y = PH - MARGIN_TOP

y = sub_header(c, "MESHY — texto a imagen", y, "AI.04 · T2I")
y = draw_paragraph(c, "Primero definimos la estética a usar:",
                   MARGIN_X, y, CONTENT_W, color=C["inkSoft"], font=F_BODY_I)
y -= 10

# 3 small reference images
col_w = (CONTENT_W - 32) / 3
refs = ["p21_img2.jpg", "p21_img3.jpg", "p21_img4.jpg"]
ys = []
for i, im in enumerate(refs):
    ip = os.path.join(IMG_DIR, im)
    if os.path.exists(ip):
        yy = place_image(c, ip, MARGIN_X + i*(col_w+16), y, col_w, tag=f"AES.{i+1:02d}")
        ys.append(yy)
y = min(ys) - 6 if ys else y

c.setFillColor(C["amberDim"]); c.setFont(F_MONO, 8)
c.drawString(MARGIN_X, y, "REFERENCIAS PARA LA ESTÉTICA")
y -= 14

prompt = ("Ahora que ya tenemos la estética haz 3d este diseño, no lo cambies, se tiene que ver "
          "como un personaje de juego con estilo de animación")
y = draw_paragraph(c, prompt, MARGIN_X, y, CONTENT_W, color=C["inkSoft"], font=F_BODY_I)
y -= 10

col2 = (CONTENT_W - 16) / 2
ipA = os.path.join(IMG_DIR, "p21_img5.jpg")
ipB = os.path.join(IMG_DIR, "p22_img1.jpg")
yA = place_image(c, ipA, MARGIN_X, y, col2, caption="REFERENCIA", tag="IN") if os.path.exists(ipA) else y
yB = place_image(c, ipB, MARGIN_X + col2 + 16, y, col2, caption="RESULTADO", tag="OUT") if os.path.exists(ipB) else y
y = min(yA, yB)

# =============== PAGE 20 — MESHY prompt largo ===============
new_page("§09", "USO DE IA · MESHY")
y = PH - MARGIN_TOP

y = sub_header(c, "Prompt · Freaky Emilio", y, "AI.05 · PROMPT")

long_prompt = ("Un masivo modelo 3D cartoon de un monstruo mutado, con musculatura roja expuesta y "
               "piel gris pálida que denota gran fuerza. La cabeza esquelética con gafas steampunk y "
               "cabello blanco despeinado muestra una sonrisa maníaca. Hombros con armadura de bronce "
               "desgastada y viales verdes luminosos. Viste pantalones negros ajustados, rotos y sucios, "
               "con zapatillas blancas gastadas. Brazos desproporcionadamente largos terminan en garras "
               "afiladas. Renderizado Octane")

# style as code block
box_h = 0
# wrap into lines first to calc height
tmp_lines = wrap_text(c, long_prompt, F_MONO, 8.5, CONTENT_W-28)
box_h = len(tmp_lines)*12 + 20
c.setFillColor(C["dark"]); c.rect(MARGIN_X, y-box_h, CONTENT_W, box_h, fill=1, stroke=0)
c.setStrokeColor(C["amber"]); c.setLineWidth(2); c.line(MARGIN_X, y-box_h, MARGIN_X, y)
c.setFillColor(C["amber"]); c.setFont(F_MONO, 7.5)
c.drawString(MARGIN_X + 14, y-14, "PROMPT.03")
c.setFillColor(C["inkOnDark"]); c.setFont(F_MONO, 8.5)
cy = y-28
for ln in tmp_lines:
    c.drawString(MARGIN_X + 14, cy, ln); cy -= 12
y -= box_h + 14

col2 = (CONTENT_W - 16) / 2
ipA = os.path.join(IMG_DIR, "p22_img2.jpg")
ipB = os.path.join(IMG_DIR, "p22_img3.jpg")
yA = place_image(c, ipA, MARGIN_X, y, col2, caption="REFERENCIA", tag="IN") if os.path.exists(ipA) else y
yB = place_image(c, ipB, MARGIN_X + col2 + 16, y, col2, caption="RESULTADO", tag="OUT") if os.path.exists(ipB) else y
y = min(yA, yB)

# =============== PAGE 21 — MESHY imagen a 3d ===============
new_page("§09", "USO DE IA · MESHY")
y = PH - MARGIN_TOP

y = sub_header(c, "MESHY — imagen a modelado 3D", y, "AI.06 · I23D")
y = draw_paragraph(c, "haz un modelado 3d de este personaje usando la estética cargada como referencias",
                   MARGIN_X, y, CONTENT_W, color=C["inkSoft"], font=F_BODY_I)
y -= 10

col2 = (CONTENT_W - 16) / 2
ipA = os.path.join(IMG_DIR, "p23_img2.jpg")
ipB = os.path.join(IMG_DIR, "p24_img1.jpg")
yA = place_image(c, ipA, MARGIN_X, y, col2, caption="REFERENCIA", tag="IN.A") if os.path.exists(ipA) else y
yB = place_image(c, ipB, MARGIN_X + col2 + 16, y, col2, caption="RESULTADO", tag="OUT.A") if os.path.exists(ipB) else y
y = min(yA, yB) - 10

y = draw_paragraph(c, "haz un modelado 3d de este personaje usando la estetica cargada como referencias",
                   MARGIN_X, y, CONTENT_W, color=C["inkSoft"], font=F_BODY_I)
y -= 10

ipC = os.path.join(IMG_DIR, "p24_img2.jpg")
ipD = os.path.join(IMG_DIR, "p25_img1.jpg")
yC = place_image(c, ipC, MARGIN_X, y, col2, caption="REFERENCIA", tag="IN.B") if os.path.exists(ipC) else y
yD = place_image(c, ipD, MARGIN_X + col2 + 16, y, col2, caption="RESULTADO", tag="OUT.B") if os.path.exists(ipD) else y
y = min(yC, yD)

# =============== PAGE 22 — CHAT GPT (scripts Unity) ===============
new_page("§09", "USO DE IA · CHAT GPT")
y = PH - MARGIN_TOP

y = sub_header(c, "CHAT GPT — guía para scripts en Unity", y, "AI.07 · CODE")
y -= 4

# prompt block (amber framed)
prompt_txt = ("mira chat, tengo un personaje en unity que tiene 2 animaciones \"idle\" para "
              "cuando está quieto y \"walk\" para cuando camina. ¿cómo podría hacer esto? creo que "
              "con un script, además de usar wasd para mover al personaje.")
p_lines = wrap_text(c, prompt_txt, F_BODY_I, 10, CONTENT_W-28)
p_h = len(p_lines)*14 + 28
c.setFillColor(C["paperAlt"]); c.rect(MARGIN_X, y-p_h, CONTENT_W, p_h, fill=1, stroke=0)
c.setStrokeColor(C["amber"]); c.setLineWidth(2); c.line(MARGIN_X, y-p_h, MARGIN_X, y)
c.setFillColor(C["amberDim"]); c.setFont(F_MONO, 7.5)
c.drawString(MARGIN_X+14, y-14, "PROMPT")
c.setFillColor(C["ink"]); c.setFont(F_BODY_I, 10)
py = y-30
for ln in p_lines:
    c.drawString(MARGIN_X+14, py, ln); py -= 14
y -= p_h + 12

# result label
c.setFillColor(C["amberDim"]); c.setFont(F_MONO, 7.5)
c.drawString(MARGIN_X, y, "RESULTADO  ·  PlayerMovement.cs")
y -= 14

# code block (dark)
code_lines = [
    "using UnityEngine;",
    "",
    "public class PlayerMovement : MonoBehaviour",
    "{",
    "    [SerializeField] private float moveSpeed = 3.5f;",
    "    [SerializeField] private float rotationSpeed = 10f;",
    "    [SerializeField] private Animator animator;",
    "    private CharacterController controller;",
    "",
    "    void Awake()",
    "    {",
    "        controller = GetComponent<CharacterController>();",
    "",
    "        if (animator == null)",
    "            animator = GetComponent<Animator>();",
    "    }",
    "",
    "    void Update()",
    "    {",
    "        float h = 0f;",
    "        float v = 0f;",
    "        if (Input.GetKey(KeyCode.A)) h -= 1f;",
    "        if (Input.GetKey(KeyCode.D)) h += 1f;",
    "        if (Input.GetKey(KeyCode.W)) v += 1f;",
    "        if (Input.GetKey(KeyCode.S)) v -= 1f;",
    "",
    "        Vector3 move = new Vector3(h, 0f, v).normalized;",
    "        bool isMoving = move.magnitude > 0.01f;",
    "        animator.SetBool(\"IsMoving\", isMoving);",
    "",
    "        if (isMoving)",
    "        {",
    "            Quaternion targetRotation = Quaternion.LookRotation(move);",
    "            transform.rotation = Quaternion.Slerp(",
    "                transform.rotation, targetRotation,",
    "                rotationSpeed * Time.deltaTime);",
    "        }",
    "",
    "        controller.Move(move * moveSpeed * Time.deltaTime);",
    "    }",
    "}",
]
code_lh = 11
code_h = len(code_lines)*code_lh + 28
c.setFillColor(C["dark"]); c.rect(MARGIN_X, y-code_h, CONTENT_W, code_h, fill=1, stroke=0)
# top-left tag bar
c.setFillColor(C["amber"]); c.setFont(F_MONO, 7.5)
c.drawString(MARGIN_X+14, y-14, "C#  ·  UNITY  ·  PlayerMovement.cs")
# small colored dots (window chrome)
for i, col in enumerate([C["coral"], HexColor("#D4A03C"), C["teal"]]):
    c.setFillColor(col); c.circle(PW-MARGIN_X-14-i*10, y-10, 2.2, fill=1, stroke=0)
# code
cy = y-28
c.setFont(F_MONO, 8.5)
for ln in code_lines:
    # simple syntax: keywords amber, strings teal, brackets muted
    ln_stripped = ln.lstrip()
    indent = len(ln) - len(ln_stripped)
    xx = MARGIN_X + 14 + indent*3.2
    # detect comment/keyword very simply - just color keywords
    parts = ln_stripped
    c.setFillColor(C["inkOnDark"])
    c.drawString(xx, cy, ln_stripped)
    cy -= code_lh

# =============== PAGE 23 — CIERRE ===============
new_page("§10", "CIERRE", dark=True)

# Big closing
c.setFillColor(C["amber"]); c.setFont(F_MONO, 9)
c.drawString(MARGIN_X, PH-140, "END OF DOCUMENT · EQUIPO DINAMITA")

c.setFillColor(C["inkOnDark"]); c.setFont(F_DISP_B, 44)
c.drawString(MARGIN_X, PH-200, "Gracias.")

c.setFillColor(HexColor("#B8B3A2")); c.setFont(F_DISP_R, 13)
wrap_close = wrap_text(c, "Freaky Super Mesh es un proyecto de Equipo Dinamita desarrollado en el marco de la cátedra de Videojuegos - UADE 2026.", F_DISP_R, 13, CONTENT_W-40)
cy = PH-230
for ln in wrap_close:
    c.drawString(MARGIN_X, cy, ln); cy -= 18

# signature block
c.setStrokeColor(C["amber"]); c.setLineWidth(1.5); c.line(MARGIN_X, PH-340, MARGIN_X+120, PH-340)
c.setFillColor(C["amber"]); c.setFont(F_MONO, 8)
c.drawString(MARGIN_X, PH-358, "EQUIPO DINAMITA / v.01 / ES-AR")

# bottom meta grid
meta2 = [
    ("DOC",      "GDD.01"),
    ("BUILD",    "00.1"),
    ("FECHA",    "24.04.2026"),
    ("FORMATO",  "A4 · vertical"),
]
gy = 220
for i, (k, v) in enumerate(meta2):
    col = i % 2
    row = i // 2
    gx = MARGIN_X + col*230
    gyy = gy - row*34
    c.setFillColor(C["amberDim"]); c.setFont(F_MONO, 7.5)
    c.drawString(gx, gyy, k)
    c.setFillColor(C["inkOnDark"]); c.setFont(F_DISP, 12)
    c.drawString(gx, gyy-14, v)

# ---------- SAVE ----------
c.save()
print("WROTE:", OUT_PATH, os.path.getsize(OUT_PATH), "bytes")
