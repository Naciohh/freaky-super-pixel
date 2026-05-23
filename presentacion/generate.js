const pptxgen = require("pptxgenjs");

const pres = new pptxgen();
pres.layout = "LAYOUT_WIDE"; // 13.3 x 7.5 (horizontal)
pres.title = "Freaky Super Mesh - Análisis Teórico";
pres.author = "Fidmay, Nuñez Di Meo, Hug";

// Palette: sci-fi / dimensional
const C = {
  bg: "0A0E27",
  bgLight: "F5F3FF",
  card: "1A1F3A",
  cardLight: "FFFFFF",
  primary: "7C3AED",
  accent: "00F5D4",
  hot: "FF3D7F",
  gold: "FFD166",
  text: "F5F3FF",
  textDark: "1A1F3A",
  muted: "A78BFA",
  mutedDark: "64748B",
};

const W = 13.3;
const H = 7.5;
const TOTAL = 12;

// ---------- helpers ----------
function drawChrome(slide, { dark = false } = {}) {
  const bar = dark ? C.accent : C.primary;
  slide.addShape(pres.shapes.RECTANGLE, {
    x: 0, y: 0, w: W, h: 0.12, fill: { color: bar }, line: { type: "none" }
  });
  slide.addShape(pres.shapes.RECTANGLE, {
    x: 0, y: 0.12, w: 0.12, h: 1.2, fill: { color: C.hot }, line: { type: "none" }
  });
  slide.addShape(pres.shapes.RECTANGLE, {
    x: W - 0.12, y: H - 1.2, w: 0.12, h: 1.08, fill: { color: C.accent }, line: { type: "none" }
  });
  slide.addShape(pres.shapes.RECTANGLE, {
    x: 0, y: H - 0.12, w: W, h: 0.12, fill: { color: bar }, line: { type: "none" }
  });
}

function footer(slide, n, total, { dark = false } = {}) {
  const tc = dark ? C.muted : C.mutedDark;
  slide.addText("FREAKY SUPER MESH", {
    x: 0.4, y: H - 0.55, w: 4, h: 0.35,
    fontSize: 9, fontFace: "Consolas", color: tc,
    bold: true, charSpacing: 4, margin: 0, valign: "middle"
  });
  slide.addText(`${String(n).padStart(2, "0")} / ${String(total).padStart(2, "0")}`, {
    x: W - 2.4, y: H - 0.55, w: 2, h: 0.35,
    fontSize: 9, fontFace: "Consolas", color: tc,
    align: "right", margin: 0, valign: "middle"
  });
}

function sectionTag(slide, tag, color) {
  slide.addShape(pres.shapes.RECTANGLE, {
    x: 0.6, y: 0.55, w: 0.25, h: 0.25, fill: { color }, line: { type: "none" }
  });
  slide.addText(tag, {
    x: 0.95, y: 0.5, w: 10, h: 0.35,
    fontSize: 11, fontFace: "Consolas", color: color,
    bold: true, charSpacing: 6, margin: 0, valign: "middle"
  });
}

function bigTitle(slide, text, opts = {}) {
  slide.addText(text, {
    x: 0.6, y: 0.9, w: W - 1.2, h: 1.1,
    fontSize: opts.size || 40, fontFace: "Impact",
    color: opts.color || C.textDark, bold: true,
    charSpacing: 1, margin: 0, valign: "middle", ...opts
  });
}

// Draws a bidirectional arrow line between two points (x1,y1)->(x2,y2) in inches,
// shortened by 'inset' at each end so it doesn't overlap node boxes.
function bidirArrow(slide, x1, y1, x2, y2, color, { width = 1.5, inset = 0 } = {}) {
  const dx = x2 - x1, dy = y2 - y1;
  const dist = Math.hypot(dx, dy);
  if (dist === 0) return;
  const ux = dx / dist, uy = dy / dist;
  const sx = x1 + ux * inset, sy = y1 + uy * inset;
  const ex = x2 - ux * inset, ey = y2 - uy * inset;
  // pptxgenjs LINE uses (x,y) as top-left; width/height can be negative
  slide.addShape(pres.shapes.LINE, {
    x: sx, y: sy, w: ex - sx, h: ey - sy,
    line: { color, width, beginArrowType: "triangle", endArrowType: "triangle" }
  });
}

// ================== SLIDE 1 — PORTADA ==================
{
  const s = pres.addSlide();
  s.background = { color: C.bg };
  drawChrome(s, { dark: true });

  // portal top-right
  s.addShape(pres.shapes.OVAL, {
    x: W - 3.6, y: -1.3, w: 4.8, h: 4.8,
    fill: { color: C.primary, transparency: 35 }, line: { type: "none" }
  });
  s.addShape(pres.shapes.OVAL, {
    x: W - 2.95, y: -0.65, w: 3.5, h: 3.5,
    fill: { color: C.hot, transparency: 45 }, line: { type: "none" }
  });
  s.addShape(pres.shapes.OVAL, {
    x: W - 2.2, y: 0.1, w: 2, h: 2,
    fill: { color: C.accent, transparency: 35 }, line: { type: "none" }
  });
  // small gold dot bottom-right
  s.addShape(pres.shapes.OVAL, {
    x: W - 0.85, y: H - 0.95, w: 0.5, h: 0.5,
    fill: { color: C.gold, transparency: 20 }, line: { type: "none" }
  });

  s.addText("ANÁLISIS TEÓRICO · DISEÑO DE VIDEOJUEGO", {
    x: 0.8, y: 1.4, w: 8, h: 0.4,
    fontSize: 13, fontFace: "Consolas", color: C.accent,
    bold: true, charSpacing: 6, margin: 0
  });

  s.addText("FREAKY", {
    x: 0.7, y: 1.9, w: 10, h: 1.6,
    fontSize: 110, fontFace: "Impact", color: C.text,
    bold: true, charSpacing: 4, margin: 0, valign: "middle"
  });
  s.addText("SUPER MESH", {
    x: 0.7, y: 3.4, w: 10, h: 1.6,
    fontSize: 110, fontFace: "Impact", color: C.hot,
    bold: true, charSpacing: 4, margin: 0, valign: "middle"
  });

  s.addShape(pres.shapes.RECTANGLE, {
    x: 0.8, y: 5.2, w: 1.5, h: 0.06, fill: { color: C.accent }, line: { type: "none" }
  });

  s.addText([
    { text: "INTEGRANTES", options: { bold: true, color: C.muted, fontSize: 11, charSpacing: 4, breakLine: true } },
    { text: "Fidmay Agustín", options: { color: C.text, fontSize: 14, breakLine: true } },
    { text: "Nuñez Di Meo Dante", options: { color: C.text, fontSize: 14, breakLine: true } },
    { text: "Hug Juan Ignacio", options: { color: C.text, fontSize: 14 } },
  ], { x: 0.8, y: 5.45, w: 4.5, h: 1.3, fontFace: "Calibri", margin: 0, paraSpaceAfter: 2 });

  s.addText([
    { text: "PROFESORES", options: { bold: true, color: C.muted, fontSize: 11, charSpacing: 4, breakLine: true } },
    { text: "Aparicio Lucas", options: { color: C.text, fontSize: 14, breakLine: true } },
    { text: "Ulrich Gonzalo Ezequiel", options: { color: C.text, fontSize: 14 } },
  ], { x: 5.4, y: 5.45, w: 4, h: 1.3, fontFace: "Calibri", margin: 0, paraSpaceAfter: 2 });

  s.addText([
    { text: "UADE", options: { bold: true, color: C.accent, fontSize: 12, charSpacing: 4, breakLine: true } },
    { text: "Primer Cuatrimestre · Turno Tarde", options: { color: C.text, fontSize: 13, breakLine: true } },
    { text: "2026  ·  24 / 04 / 26", options: { color: C.gold, fontSize: 13, bold: true } },
  ], { x: 9.3, y: 5.45, w: 3.5, h: 1.3, fontFace: "Calibri", margin: 0, paraSpaceAfter: 2, align: "right" });

  footer(s, 1, TOTAL, { dark: true });
}

// ================== SLIDE 2 — RESUMEN ==================
{
  const s = pres.addSlide();
  s.background = { color: C.bgLight };
  drawChrome(s);

  sectionTag(s, "RESUMEN DEL JUEGO", C.primary);
  bigTitle(s, "Action Rogue-lite dimensional");

  s.addShape(pres.shapes.RECTANGLE, {
    x: 0.6, y: 2.1, w: 7.5, h: 4.5,
    fill: { color: C.cardLight }, line: { color: C.primary, width: 0.75 },
    shadow: { type: "outer", color: "000000", blur: 12, offset: 3, angle: 135, opacity: 0.08 }
  });
  s.addShape(pres.shapes.RECTANGLE, {
    x: 0.6, y: 2.1, w: 0.12, h: 4.5, fill: { color: C.hot }, line: { type: "none" }
  });

  s.addText([
    { text: "PLATAFORMA   ", options: { bold: true, color: C.primary, fontSize: 11, charSpacing: 3 } },
    { text: "PC · 1 jugador · Offline", options: { color: C.textDark, fontSize: 13, breakLine: true } },
    { text: " ", options: { fontSize: 6, breakLine: true } },
    { text: "PROTAGONISTA   ", options: { bold: true, color: C.primary, fontSize: 11, charSpacing: 3 } },
    { text: "Dr. Emilio", options: { color: C.textDark, fontSize: 13, bold: true, breakLine: true } },
    { text: "Científico atrapado entre dimensiones tras un experimento de portales fallido.", options: { color: C.textDark, fontSize: 12, italic: true, breakLine: true } },
    { text: " ", options: { fontSize: 6, breakLine: true } },
    { text: "CÁMARA   ", options: { bold: true, color: C.primary, fontSize: 11, charSpacing: 3 } },
    { text: "Cenital inclinada · estilo isométrico", options: { color: C.textDark, fontSize: 13, breakLine: true } },
    { text: " ", options: { fontSize: 6, breakLine: true } },
    { text: "LOOP   ", options: { bold: true, color: C.primary, fontSize: 11, charSpacing: 3 } },
    { text: "Atravesar mundos absurdos, derrotar enemigos temáticos y jefes finales.", options: { color: C.textDark, fontSize: 13 } },
  ], { x: 0.95, y: 2.35, w: 7, h: 4.1, fontFace: "Calibri", margin: 0, paraSpaceAfter: 4 });

  s.addText("PILARES DE DISEÑO", {
    x: 8.5, y: 2.15, w: 4.3, h: 0.4,
    fontSize: 11, fontFace: "Consolas", color: C.hot,
    bold: true, charSpacing: 5, margin: 0
  });

  const pillars = [
    { t: "Combate frenético", c: C.hot },
    { t: "Mecánicas simples", c: C.primary },
    { t: "Alta rejugabilidad", c: C.accent },
    { t: "Humor visual", c: C.gold },
    { t: "Progreso por habilidad", c: C.hot },
  ];
  pillars.forEach((p, i) => {
    const y = 2.65 + i * 0.78;
    s.addShape(pres.shapes.RECTANGLE, {
      x: 8.5, y, w: 4.3, h: 0.65,
      fill: { color: C.cardLight }, line: { color: p.c, width: 0.75 }
    });
    s.addShape(pres.shapes.RECTANGLE, {
      x: 8.5, y, w: 0.12, h: 0.65, fill: { color: p.c }, line: { type: "none" }
    });
    s.addText(p.t, {
      x: 8.75, y, w: 4, h: 0.65,
      fontSize: 14, fontFace: "Calibri", color: C.textDark, bold: true,
      valign: "middle", margin: 0
    });
  });

  footer(s, 2, TOTAL);
}

// ================== SLIDE 3 — MDA FLOW ==================
{
  const s = pres.addSlide();
  s.background = { color: C.bgLight };
  drawChrome(s);

  sectionTag(s, "FRAMEWORK MDA · HUNICKE, LEBLANC & ZUBEK", C.primary);
  bigTitle(s, "Del diseñador al jugador", { size: 36 });
  s.addText("La dirección del flujo es la clave del análisis MDA", {
    x: 0.6, y: 1.85, w: 11, h: 0.4, fontSize: 14, color: C.mutedDark, italic: true, margin: 0
  });

  const nodes = [
    { t: "MECHANICS",  b: "Reglas, sistemas,\ncomponentes del juego.",  sub: "Lo que diseñamos", c: C.hot },
    { t: "DYNAMICS",   b: "Comportamiento\nen tiempo real al jugar.",   sub: "Lo que emerge",    c: C.primary },
    { t: "AESTHETICS", b: "Respuestas emocionales\ndel jugador.",       sub: "Lo que se siente", c: C.accent },
  ];
  const startX = 0.7, startY = 2.6;
  const nodeW = 3.6, nodeH = 3.3, gap = 0.55;
  nodes.forEach((n, i) => {
    const cx = startX + i * (nodeW + gap);
    s.addShape(pres.shapes.RECTANGLE, {
      x: cx, y: startY, w: nodeW, h: nodeH,
      fill: { color: C.cardLight }, line: { color: n.c, width: 1 },
      shadow: { type: "outer", color: "000000", blur: 12, offset: 3, angle: 135, opacity: 0.08 }
    });
    s.addShape(pres.shapes.RECTANGLE, {
      x: cx, y: startY, w: nodeW, h: 0.45, fill: { color: n.c }, line: { type: "none" }
    });
    s.addText(`0${i + 1}`, {
      x: cx + 0.2, y: startY, w: 0.6, h: 0.45,
      fontSize: 11, fontFace: "Consolas", color: C.cardLight, bold: true,
      valign: "middle", margin: 0, charSpacing: 2
    });
    s.addText(n.t, {
      x: cx, y: startY, w: nodeW, h: 0.45,
      fontSize: 13, fontFace: "Consolas", color: C.cardLight, bold: true,
      align: "center", valign: "middle", margin: 0, charSpacing: 4
    });
    s.addText(n.sub, {
      x: cx + 0.3, y: startY + 0.6, w: nodeW - 0.6, h: 0.4,
      fontSize: 11, fontFace: "Consolas", color: n.c, bold: true,
      charSpacing: 2, margin: 0
    });
    s.addText(n.b, {
      x: cx + 0.3, y: startY + 1.1, w: nodeW - 0.6, h: 1.8,
      fontSize: 14, fontFace: "Calibri", color: C.textDark, bold: true, margin: 0
    });
  });

  // Arrows between nodes
  for (let i = 0; i < 2; i++) {
    const ax = startX + (i + 1) * nodeW + i * gap;
    bidirArrow(s, ax + 0.05, startY + nodeH / 2, ax + gap - 0.05, startY + nodeH / 2,
               C.primary, { width: 2.5 });
  }

  // Bottom strip — dual perspective
  s.addShape(pres.shapes.RECTANGLE, {
    x: 0.6, y: 6.2, w: 12.1, h: 0.7,
    fill: { color: C.bg }, line: { type: "none" }
  });
  s.addText("DISEÑADOR →  parte de Mechanics  ·  JUGADOR ←  experimenta desde Aesthetics", {
    x: 0.6, y: 6.2, w: 12.1, h: 0.7,
    fontSize: 13, fontFace: "Consolas", color: C.accent, bold: true,
    align: "center", valign: "middle", margin: 0, charSpacing: 3
  });

  footer(s, 3, TOTAL);
}

// ================== SLIDE 4 — MECHANICS ==================
{
  const s = pres.addSlide();
  s.background = { color: C.bgLight };
  drawChrome(s);

  sectionTag(s, "MDA · 01 MECHANICS", C.hot);
  bigTitle(s, "Mecánicas", { size: 48 });
  s.addText("Las reglas del sistema", {
    x: 0.6, y: 1.85, w: 10, h: 0.4, fontSize: 14, color: C.mutedDark, italic: true, margin: 0
  });

  const items = [
    "Movimiento WASD", "Apuntado con mouse", "Lanzamiento de probetas",
    "Bloqueo con Shift", "Enemigos por oleadas", "Jefes de dimensión",
    "Sistema de salud", "Estado Eufórico", "Mejora temporal de stats",
    "Recuperación en euforia",
  ];
  const cols = 2;
  const startX = 0.6, startY = 2.6;
  const cardW = 6.0, cardH = 0.6, gapX = 0.25, gapY = 0.18;
  items.forEach((t, i) => {
    const cx = startX + (i % cols) * (cardW + gapX);
    const cy = startY + Math.floor(i / cols) * (cardH + gapY);
    s.addShape(pres.shapes.RECTANGLE, {
      x: cx, y: cy, w: cardW, h: cardH,
      fill: { color: C.cardLight }, line: { color: "E5E7EB", width: 0.5 }
    });
    s.addShape(pres.shapes.OVAL, {
      x: cx + 0.12, y: cy + 0.12, w: 0.36, h: 0.36,
      fill: { color: C.hot }, line: { type: "none" }
    });
    s.addText(String(i + 1).padStart(2, "0"), {
      x: cx + 0.12, y: cy + 0.12, w: 0.36, h: 0.36,
      fontSize: 10, fontFace: "Consolas", color: C.cardLight, bold: true,
      align: "center", valign: "middle", margin: 0
    });
    s.addText(t, {
      x: cx + 0.6, y: cy, w: cardW - 0.7, h: cardH,
      fontSize: 14, fontFace: "Calibri", color: C.textDark, bold: true,
      valign: "middle", margin: 0
    });
  });

  s.addShape(pres.shapes.RECTANGLE, {
    x: 0.6, y: 6.15, w: 12.1, h: 0.7,
    fill: { color: C.bg }, line: { type: "none" }
  });
  s.addText([
    { text: "OBJETIVO   ", options: { color: C.accent, bold: true, fontSize: 12, charSpacing: 5 } },
    { text: "Superar dimensiones y sobrevivir.", options: { color: C.text, fontSize: 16, bold: true } },
  ], { x: 0.9, y: 6.15, w: 12, h: 0.7, fontFace: "Calibri", valign: "middle", margin: 0 });

  footer(s, 4, TOTAL);
}

// ================== SLIDE 5 — DYNAMICS ==================
{
  const s = pres.addSlide();
  s.background = { color: C.bgLight };
  drawChrome(s);

  sectionTag(s, "MDA · 02 DYNAMICS", C.primary);
  bigTitle(s, "Dinámicas emergentes", { size: 46 });
  s.addText("Lo que ocurre cuando el jugador se encuentra con las reglas", {
    x: 0.6, y: 1.85, w: 11, h: 0.4, fontSize: 14, color: C.mutedDark, italic: true, margin: 0
  });

  const dynamics = [
    { n: "01", t: "Movimiento constante", d: "Esquivar y reposicionarse es obligatorio" },
    { n: "02", t: "Posicionamiento", d: "Lectura del espacio frente a hordas" },
    { n: "03", t: "Bloqueo preciso", d: "Defensa activa como recurso estratégico" },
    { n: "04", t: "Incentivo ofensivo", d: "Matar rápido = activar modo eufórico" },
    { n: "05", t: "Aprendizaje", d: "Leer patrones enemigos y jefes" },
    { n: "06", t: "Presión creciente", d: "El sistema sube el ritmo progresivamente" },
  ];
  dynamics.forEach((d, i) => {
    const col = i % 3;
    const row = Math.floor(i / 3);
    const cx = 0.6 + col * 4.15;
    const cy = 2.55 + row * 1.8;
    s.addShape(pres.shapes.RECTANGLE, {
      x: cx, y: cy, w: 3.95, h: 1.55,
      fill: { color: C.cardLight }, line: { color: C.primary, width: 0.5 },
      shadow: { type: "outer", color: "000000", blur: 10, offset: 2, angle: 135, opacity: 0.06 }
    });
    s.addText(d.n, {
      x: cx + 0.25, y: cy + 0.1, w: 1.5, h: 0.5,
      fontSize: 26, fontFace: "Impact", color: C.primary, bold: true, margin: 0
    });
    s.addText(d.t, {
      x: cx + 0.25, y: cy + 0.55, w: 3.5, h: 0.4,
      fontSize: 15, fontFace: "Calibri", color: C.textDark, bold: true, margin: 0
    });
    s.addText(d.d, {
      x: cx + 0.25, y: cy + 0.95, w: 3.5, h: 0.55,
      fontSize: 11, fontFace: "Calibri", color: C.mutedDark, italic: true, margin: 0
    });
  });

  const steps = ["Inicio simple", "Más enemigos", "Presión creciente", "Jefe final", "Nueva dimensión"];
  const stripY = 6.2;
  const stepW = 12.1 / steps.length;
  steps.forEach((st, i) => {
    const sx = 0.6 + i * stepW;
    s.addShape(pres.shapes.RECTANGLE, {
      x: sx, y: stripY, w: stepW - 0.08, h: 0.7,
      fill: { color: i === steps.length - 1 ? C.hot : C.bg }, line: { type: "none" }
    });
    s.addText(st, {
      x: sx, y: stripY, w: stepW - 0.08, h: 0.7,
      fontSize: 11, fontFace: "Consolas", color: C.text, bold: true,
      align: "center", valign: "middle", margin: 0, charSpacing: 2
    });
  });

  footer(s, 5, TOTAL);
}

// ================== SLIDE 6 — AESTHETICS (8 CATEGORÍAS HUNICKE) ==================
{
  const s = pres.addSlide();
  s.background = { color: C.bg };
  drawChrome(s, { dark: true });

  sectionTag(s, "MDA · 03 AESTHETICS", C.accent);
  bigTitle(s, "Las 8 categorías (Hunicke et al.)", { size: 36, color: C.text });
  s.addText("Cuáles buscamos generar en el jugador y cuáles no aplican", {
    x: 0.6, y: 1.85, w: 11, h: 0.4, fontSize: 14, color: C.muted, italic: true, margin: 0
  });

  const items = [
    { t: "SENSATION",  d: "Combate frenético, feedback audiovisual del lanzamiento de probetas y del estado eufórico.", ok: true },
    { t: "FANTASY",    d: "Universo bizarro: laboratorio que se rebela contra su dueño; dimensiones absurdas.",        ok: true },
    { t: "NARRATIVE",  d: "Dr. Emilio intenta volver a casa tras el fallo de su portal dimensional.",                  ok: true },
    { t: "CHALLENGE",  d: "Supervivencia frente a oleadas y jefes; dificultad creciente partida a partida.",           ok: true },
    { t: "FELLOWSHIP", d: "No aplica: el juego es single-player offline, sin interacción social.",                     ok: false },
    { t: "DISCOVERY",  d: "Cada dimensión (desierto, supermercado, océano) trae nuevos enemigos y jefes.",             ok: true },
    { t: "EXPRESSION", d: "No se contempla explícitamente en el GDD (sin customización declarada).",                   ok: false },
    { t: "SUBMISSION", d: "Loop arcade + rejugabilidad: \"una partida más\" es la propuesta central.",                 ok: true },
  ];
  const cols = 4;
  const startX = 0.6, startY = 2.55;
  const cardW = 3.0, cardH = 2.1, gapX = 0.13, gapY = 0.2;
  items.forEach((it, i) => {
    const ci = i % cols;
    const ri = Math.floor(i / cols);
    const cx = startX + ci * (cardW + gapX);
    const cy = startY + ri * (cardH + gapY);
    const col = it.ok ? C.accent : C.mutedDark;
    s.addShape(pres.shapes.RECTANGLE, {
      x: cx, y: cy, w: cardW, h: cardH,
      fill: { color: C.card }, line: { color: col, width: 1 }
    });
    // SÍ / NO pill
    const pillW = 0.75;
    s.addShape(pres.shapes.RECTANGLE, {
      x: cx + cardW - pillW - 0.15, y: cy + 0.15, w: pillW, h: 0.28,
      fill: { color: it.ok ? col : C.card }, line: { color: col, width: 0.75 }
    });
    s.addText(it.ok ? "SÍ" : "NO", {
      x: cx + cardW - pillW - 0.15, y: cy + 0.15, w: pillW, h: 0.28,
      fontSize: 10, fontFace: "Consolas", color: it.ok ? C.bg : C.mutedDark,
      bold: true, align: "center", valign: "middle", margin: 0, charSpacing: 3
    });
    s.addText(it.t, {
      x: cx + 0.2, y: cy + 0.5, w: cardW - 0.4, h: 0.5,
      fontSize: 18, fontFace: "Impact", color: it.ok ? col : C.muted, bold: true,
      charSpacing: 1, valign: "middle", margin: 0
    });
    s.addText(it.d, {
      x: cx + 0.2, y: cy + 1.05, w: cardW - 0.4, h: 1.0,
      fontSize: 10, fontFace: "Calibri", color: it.ok ? C.text : C.muted,
      margin: 0, valign: "top"
    });
  });

  footer(s, 6, TOTAL, { dark: true });
}

// ================== SLIDE 7 — CADENA MDA ==================
{
  const s = pres.addSlide();
  s.background = { color: C.bgLight };
  drawChrome(s);

  sectionTag(s, "CADENA COMPLETA MDA", C.gold);
  bigTitle(s, "Ejemplo: Estado Eufórico", { size: 42 });
  s.addText("Cómo una mecánica se convierte en emoción", {
    x: 0.6, y: 1.85, w: 11, h: 0.4, fontSize: 14, color: C.mutedDark, italic: true, margin: 0
  });

  const chain = [
    { lbl: "MECÁNICA", body: "Eliminar enemigos y bloquear ataques activa el estado eufórico.", c: C.hot, icon: "M" },
    { lbl: "DINÁMICA", body: "El jugador combina ofensiva y defensa para activarlo rápidamente.", c: C.primary, icon: "D" },
    { lbl: "ESTÉTICA", body: "Challenge · Sensation · Expression.", c: C.accent, icon: "A" },
  ];

  chain.forEach((c, i) => {
    const cx = 0.6 + i * 4.23;
    const cy = 2.55;
    s.addShape(pres.shapes.RECTANGLE, {
      x: cx, y: cy, w: 3.95, h: 3.5,
      fill: { color: C.cardLight }, line: { color: c.c, width: 1 },
      shadow: { type: "outer", color: "000000", blur: 14, offset: 4, angle: 135, opacity: 0.1 }
    });
    s.addShape(pres.shapes.RECTANGLE, {
      x: cx, y: cy, w: 3.95, h: 0.4, fill: { color: c.c }, line: { type: "none" }
    });
    s.addText(c.lbl, {
      x: cx, y: cy, w: 3.95, h: 0.4,
      fontSize: 12, fontFace: "Consolas", color: C.cardLight, bold: true,
      align: "center", valign: "middle", charSpacing: 6, margin: 0
    });
    s.addText(c.icon, {
      x: cx + 0.1, y: cy + 0.6, w: 3.75, h: 1.7,
      fontSize: 96, fontFace: "Impact", color: c.c, bold: true,
      align: "center", valign: "middle", margin: 0
    });
    s.addText(c.body, {
      x: cx + 0.3, y: cy + 2.35, w: 3.35, h: 1.05,
      fontSize: 13, fontFace: "Calibri", color: C.textDark,
      align: "center", valign: "top", margin: 0
    });
    if (i < 2) {
      s.addText(">", {
        x: cx + 3.85, y: cy + 1.4, w: 0.55, h: 0.7,
        fontSize: 36, fontFace: "Impact", color: C.gold, bold: true,
        align: "center", valign: "middle", margin: 0
      });
    }
  });

  s.addShape(pres.shapes.RECTANGLE, {
    x: 0.6, y: 6.3, w: 12.1, h: 0.55,
    fill: { color: C.bg }, line: { type: "none" }
  });
  s.addText("Una misma acción alimenta defensa, ofensiva y recompensa emocional.", {
    x: 0.6, y: 6.3, w: 12.1, h: 0.55,
    fontSize: 13, fontFace: "Calibri", color: C.accent, italic: true, bold: true,
    align: "center", valign: "middle", margin: 0
  });

  footer(s, 7, TOTAL);
}

// ================== SLIDE 8 — TÉTRADA DE SCHELL (DIAGRAMA) ==================
{
  const s = pres.addSlide();
  s.background = { color: C.bgLight };
  drawChrome(s);

  sectionTag(s, "TÉTRADA DE SCHELL", C.primary);
  bigTitle(s, "Los cuatro elementos y sus relaciones", { size: 32 });
  s.addText("Diagrama de influencias recíprocas entre los elementos del juego", {
    x: 0.6, y: 1.85, w: 11, h: 0.4, fontSize: 14, color: C.mutedDark, italic: true, margin: 0
  });

  // Diamond nodes (centers in inches)
  const nodes = {
    "MECÁNICA":   { cx: 4.5, cy: 2.85, col: C.hot,     content: ["Combate en tiempo real", "Bosses por dimensión", "Estado eufórico"] },
    "ESTÉTICA":   { cx: 7.0, cy: 4.65, col: C.gold,    content: ["Humor absurdo", "Mundos bizarros", "Feedback frenético"] },
    "TECNOLOGÍA": { cx: 4.5, cy: 6.45, col: C.accent,  content: ["Unity · Modelos 3D", "Cámara cenital 53°", "PC · Offline"] },
    "HISTORIA":   { cx: 2.0, cy: 4.65, col: C.primary, content: ["Dr. Emilio", "Portal fallido", "Volver a casa"] },
  };
  const nodeW = 2.6, nodeH = 1.05;

  // All 6 bidirectional arrows between pairs
  const pairs = [
    ["MECÁNICA","ESTÉTICA"], ["MECÁNICA","TECNOLOGÍA"],
    ["MECÁNICA","HISTORIA"], ["ESTÉTICA","TECNOLOGÍA"],
    ["ESTÉTICA","HISTORIA"], ["TECNOLOGÍA","HISTORIA"]
  ];
  pairs.forEach(([a, b]) => {
    const na = nodes[a], nb = nodes[b];
    bidirArrow(s, na.cx, na.cy, nb.cx, nb.cy, C.primary, { width: 1.8, inset: 0.72 });
  });

  // Central hub
  s.addShape(pres.shapes.OVAL, {
    x: 4.5 - 0.2, y: 4.65 - 0.2, w: 0.4, h: 0.4,
    fill: { color: C.bgLight }, line: { type: "none" }
  });
  s.addShape(pres.shapes.OVAL, {
    x: 4.5 - 0.15, y: 4.65 - 0.15, w: 0.3, h: 0.3,
    fill: { color: C.primary }, line: { type: "none" }
  });

  // Nodes on top
  Object.entries(nodes).forEach(([name, n]) => {
    const cx = n.cx - nodeW / 2;
    const cy = n.cy - nodeH / 2;
    s.addShape(pres.shapes.RECTANGLE, {
      x: cx, y: cy, w: nodeW, h: nodeH,
      fill: { color: C.cardLight }, line: { color: n.col, width: 1.5 }
    });
    s.addShape(pres.shapes.RECTANGLE, {
      x: cx, y: cy, w: nodeW, h: 0.3, fill: { color: n.col }, line: { type: "none" }
    });
    s.addText(name, {
      x: cx, y: cy, w: nodeW, h: 0.3,
      fontSize: 12, fontFace: "Consolas", color: C.cardLight, bold: true,
      align: "center", valign: "middle", margin: 0, charSpacing: 4
    });
    n.content.forEach((it, j) => {
      s.addText("· " + it, {
        x: cx + 0.15, y: cy + 0.32 + j * 0.22, w: nodeW - 0.3, h: 0.22,
        fontSize: 9, fontFace: "Calibri", color: C.textDark, margin: 0
      });
    });
  });

  // Right panel — influencias recíprocas
  const panelX = 8.9, panelY = 2.55, panelW = 3.9, panelH = 4.35;
  s.addShape(pres.shapes.RECTANGLE, {
    x: panelX, y: panelY, w: panelW, h: panelH,
    fill: { color: C.bg }, line: { type: "none" }
  });
  s.addShape(pres.shapes.RECTANGLE, {
    x: panelX, y: panelY, w: panelW, h: 0.35, fill: { color: C.primary }, line: { type: "none" }
  });
  s.addText("INFLUENCIAS RECÍPROCAS", {
    x: panelX, y: panelY, w: panelW, h: 0.35,
    fontSize: 10, fontFace: "Consolas", color: C.cardLight, bold: true,
    align: "center", valign: "middle", margin: 0, charSpacing: 4
  });

  const infl = [
    { lbl: "HISTORIA → MECÁNICA",    d: "El portal fallido justifica las oleadas y los bosses por dimensión.", c: C.primary },
    { lbl: "MECÁNICA → ESTÉTICA",    d: "El combate frenético y la euforia generan sensation y submission.",   c: C.hot },
    { lbl: "TECNOLOGÍA → MECÁNICA",  d: "La cámara isométrica en Unity habilita el combate con apuntado.",     c: C.accent },
    { lbl: "ESTÉTICA → HISTORIA",    d: "El tono absurdo construye el universo bizarro del laboratorio.",      c: C.gold },
  ];
  infl.forEach((it, i) => {
    const y = panelY + 0.55 + i * 0.95;
    s.addShape(pres.shapes.RECTANGLE, {
      x: panelX + 0.2, y: y + 0.05, w: 0.08, h: 0.75,
      fill: { color: it.c }, line: { type: "none" }
    });
    s.addText(it.lbl, {
      x: panelX + 0.38, y: y, w: panelW - 0.5, h: 0.25,
      fontSize: 9, fontFace: "Consolas", color: it.c, bold: true, charSpacing: 2, margin: 0
    });
    s.addText(it.d, {
      x: panelX + 0.38, y: y + 0.25, w: panelW - 0.6, h: 0.65,
      fontSize: 10, fontFace: "Calibri", color: C.text, margin: 0
    });
  });

  footer(s, 8, TOTAL);
}

// ================== SLIDE 9 — RELACIÓN ENTRE ELEMENTOS ==================
{
  const s = pres.addSlide();
  s.background = { color: C.bgLight };
  drawChrome(s);

  sectionTag(s, "RELACIÓN ENTRE LOS ELEMENTOS", C.accent);
  bigTitle(s, "Todo conecta", { size: 48 });
  s.addText("La coherencia como columna vertebral del diseño", {
    x: 0.6, y: 1.85, w: 11, h: 0.4, fontSize: 14, color: C.mutedDark, italic: true, margin: 0
  });

  const rels = [
    { a: "HISTORIA", b: "→", c: "DIMENSIONES", d: "La narrativa justifica cada mundo.", col: C.primary },
    { a: "MECÁNICA", b: "→", c: "DESAFÍO", d: "Cada dimensión es un puzzle jugable.", col: C.hot },
    { a: "ESTÉTICA", b: "→", c: "MEMORIA", d: "El universo se vuelve inolvidable.", col: C.gold },
    { a: "TECNOLOGÍA", b: "→", c: "FLUIDEZ", d: "Unity garantiza una jugabilidad sólida.", col: C.accent },
  ];

  rels.forEach((r, i) => {
    const cy = 2.55 + i * 0.85;
    s.addShape(pres.shapes.RECTANGLE, {
      x: 0.6, y: cy, w: 8.1, h: 0.72,
      fill: { color: C.cardLight }, line: { color: "E5E7EB", width: 0.5 }
    });
    s.addShape(pres.shapes.RECTANGLE, {
      x: 0.6, y: cy, w: 0.12, h: 0.72, fill: { color: r.col }, line: { type: "none" }
    });
    s.addText([
      { text: r.a, options: { bold: true, color: r.col, fontSize: 13, charSpacing: 3 } },
      { text: "   " + r.b + "   ", options: { color: C.mutedDark, fontSize: 13 } },
      { text: r.c, options: { bold: true, color: C.textDark, fontSize: 13, charSpacing: 3 } },
    ], {
      x: 0.9, y: cy, w: 4.2, h: 0.72,
      fontFace: "Consolas", valign: "middle", margin: 0
    });
    s.addText(r.d, {
      x: 5.15, y: cy, w: 3.5, h: 0.72,
      fontSize: 12, fontFace: "Calibri", color: C.mutedDark, italic: true,
      valign: "middle", margin: 0
    });
  });

  s.addShape(pres.shapes.RECTANGLE, {
    x: 9.0, y: 2.55, w: 3.75, h: 3.85,
    fill: { color: C.bg }, line: { type: "none" }
  });
  s.addText("CONCLUSIÓN", {
    x: 9.25, y: 2.75, w: 3.35, h: 0.4,
    fontSize: 11, fontFace: "Consolas", color: C.accent,
    bold: true, charSpacing: 5, margin: 0
  });
  s.addText("Coherencia", {
    x: 9.25, y: 3.2, w: 3.35, h: 0.7,
    fontSize: 34, fontFace: "Impact", color: C.text, bold: true, margin: 0
  });
  s.addText("entre narrativa,\nsistemas y\npresentación visual.", {
    x: 9.25, y: 4.0, w: 3.35, h: 1.6,
    fontSize: 15, fontFace: "Calibri", color: C.text, margin: 0
  });
  s.addShape(pres.shapes.RECTANGLE, {
    x: 9.25, y: 5.9, w: 0.8, h: 0.08, fill: { color: C.hot }, line: { type: "none" }
  });

  footer(s, 9, TOTAL);
}

// ================== SLIDE 10 — REPARTO GDD ==================
{
  const s = pres.addSlide();
  s.background = { color: C.bgLight };
  drawChrome(s);

  sectionTag(s, "REPARTO DE TAREAS · EQUIPO DINAMITA", C.primary);
  bigTitle(s, "Quién desarrolló cada sección del GDD", { size: 30 });
  s.addText("Secciones del Game Design Document · completar con el integrante responsable", {
    x: 0.6, y: 1.85, w: 12, h: 0.4, fontSize: 14, color: C.mutedDark, italic: true, margin: 0
  });

  const sections = [
    ["01", "Concepto General"],
    ["02", "Narrativa y contexto"],
    ["03", "Mecánicas principales"],
    ["04", "Progresión y estructura de niveles"],
    ["05", "Listado de assets"],
    ["06", "Desarrollo"],
    ["07", "Referencias"],
    ["08", "Uso de IA"],
  ];
  const cols = 2, rowsPerCol = 4;
  const startX = 0.6, startY = 2.55;
  const colW = 6.1, gapX = 0.3, rowH = 0.9;
  sections.forEach(([n, sect], i) => {
    const col = Math.floor(i / rowsPerCol);
    const row = i % rowsPerCol;
    const cx = startX + col * (colW + gapX);
    const cy = startY + row * rowH;
    s.addShape(pres.shapes.RECTANGLE, {
      x: cx, y: cy, w: colW, h: rowH - 0.15,
      fill: { color: C.cardLight }, line: { color: "E5E7EB", width: 0.5 }
    });
    s.addShape(pres.shapes.RECTANGLE, {
      x: cx, y: cy, w: 0.75, h: rowH - 0.15,
      fill: { color: C.primary }, line: { type: "none" }
    });
    s.addText(n, {
      x: cx, y: cy, w: 0.75, h: rowH - 0.15,
      fontSize: 22, fontFace: "Impact", color: C.cardLight, bold: true,
      align: "center", valign: "middle", margin: 0
    });
    s.addText(sect, {
      x: cx + 0.95, y: cy + 0.08, w: colW - 1.1, h: 0.4,
      fontSize: 14, fontFace: "Calibri", color: C.textDark, bold: true,
      valign: "middle", margin: 0
    });
    s.addText([
      { text: "Integrante:  ", options: { color: C.primary, fontSize: 10, bold: true, charSpacing: 2, fontFace: "Consolas" } },
      { text: "___________________________", options: { color: C.mutedDark, fontSize: 12, fontFace: "Consolas" } },
    ], {
      x: cx + 0.95, y: cy + 0.42, w: colW - 1.1, h: 0.3,
      margin: 0, valign: "top"
    });
  });

  s.addShape(pres.shapes.RECTANGLE, {
    x: 0.6, y: 6.45, w: 12.1, h: 0.4,
    fill: { color: C.bg }, line: { type: "none" }
  });
  s.addText("Equipo Dinamita · Fidmay · Nuñez Di Meo · Hug", {
    x: 0.6, y: 6.45, w: 12.1, h: 0.4,
    fontSize: 11, fontFace: "Consolas", color: C.accent, bold: true,
    align: "center", valign: "middle", margin: 0, charSpacing: 4
  });

  footer(s, 10, TOTAL);
}

// ================== SLIDE 11 — REFERENCIAS ==================
{
  const s = pres.addSlide();
  s.background = { color: C.bgLight };
  drawChrome(s);

  sectionTag(s, "REFERENCIAS E INFLUENCIAS", C.hot);
  bigTitle(s, "El ADN del juego", { size: 44 });

  const refs = [
    { t: "MegaBonk", d: "Diversión arcade inmediata y acción directa.", c: C.hot },
    { t: "Vampire Survivors", d: "Supervivencia frente a hordas, progresión, rejugabilidad.", c: C.primary },
    { t: "Hotline Miami", d: "Ritmo frenético, presión constante, combate intenso.", c: C.gold },
    { t: "Rick and Morty", d: "Humor absurdo y dimensiones alternativas.", c: C.accent },
    { t: "Dr. Mundo — LoL", d: "Científico descontrolado, poder bruto.", c: C.hot },
    { t: "Heimerdinger — LoL", d: "Inventor científico, estética tecnológica.", c: C.primary },
  ];
  refs.forEach((r, i) => {
    const col = i % 3;
    const row = Math.floor(i / 3);
    const cx = 0.6 + col * 4.15;
    const cy = 2.3 + row * 2.2;
    s.addShape(pres.shapes.RECTANGLE, {
      x: cx, y: cy, w: 3.95, h: 2.0,
      fill: { color: C.cardLight }, line: { color: "E5E7EB", width: 0.5 },
      shadow: { type: "outer", color: "000000", blur: 10, offset: 2, angle: 135, opacity: 0.06 }
    });
    s.addShape(pres.shapes.RECTANGLE, {
      x: cx, y: cy, w: 3.95, h: 0.35, fill: { color: r.c }, line: { type: "none" }
    });
    s.addText(`REF · 0${i + 1}`, {
      x: cx + 0.2, y: cy, w: 3.7, h: 0.35,
      fontSize: 10, fontFace: "Consolas", color: C.cardLight, bold: true,
      charSpacing: 5, valign: "middle", margin: 0
    });
    s.addText(r.t, {
      x: cx + 0.2, y: cy + 0.55, w: 3.55, h: 0.65,
      fontSize: 22, fontFace: "Impact", color: C.textDark, bold: true,
      charSpacing: 1, margin: 0, valign: "middle"
    });
    s.addText(r.d, {
      x: cx + 0.2, y: cy + 1.2, w: 3.55, h: 0.75,
      fontSize: 11, fontFace: "Calibri", color: C.mutedDark, italic: true,
      margin: 0, valign: "top"
    });
  });

  footer(s, 11, TOTAL);
}

// ================== SLIDE 12 — CIERRE ==================
{
  const s = pres.addSlide();
  s.background = { color: C.bg };
  drawChrome(s, { dark: true });

  // corner decorations
  s.addShape(pres.shapes.OVAL, {
    x: -0.6, y: H - 2.6, w: 3.2, h: 3.2,
    fill: { color: C.primary, transparency: 55 }, line: { type: "none" }
  });
  s.addShape(pres.shapes.OVAL, {
    x: W - 2.6, y: -0.7, w: 3.2, h: 3.2,
    fill: { color: C.hot, transparency: 55 }, line: { type: "none" }
  });
  s.addShape(pres.shapes.OVAL, {
    x: 0.7, y: 0.8, w: 0.8, h: 0.8,
    fill: { color: C.accent, transparency: 40 }, line: { type: "none" }
  });
  s.addShape(pres.shapes.OVAL, {
    x: W - 1.5, y: H - 1.6, w: 0.8, h: 0.8,
    fill: { color: C.gold, transparency: 40 }, line: { type: "none" }
  });

  s.addText("FIN DE LA PRESENTACIÓN", {
    x: 0.8, y: 1.6, w: 12, h: 0.4,
    fontSize: 12, fontFace: "Consolas", color: C.accent,
    bold: true, charSpacing: 6, margin: 0, align: "center"
  });
  s.addText("Sistemas claros.", {
    x: 0.6, y: 2.3, w: 12, h: 1.1,
    fontSize: 64, fontFace: "Impact", color: C.text, bold: true,
    align: "center", valign: "middle", margin: 0
  });
  s.addText("Acción intensa.", {
    x: 0.6, y: 3.4, w: 12, h: 1.1,
    fontSize: 64, fontFace: "Impact", color: C.accent, bold: true,
    align: "center", valign: "middle", margin: 0
  });
  s.addText("Identidad propia.", {
    x: 0.6, y: 4.5, w: 12, h: 1.1,
    fontSize: 64, fontFace: "Impact", color: C.hot, bold: true,
    align: "center", valign: "middle", margin: 0
  });
  s.addShape(pres.shapes.RECTANGLE, {
    x: 5.9, y: 5.85, w: 1.5, h: 0.06, fill: { color: C.gold }, line: { type: "none" }
  });
  s.addText("GRACIAS", {
    x: 0.6, y: 6.0, w: 12, h: 0.6,
    fontSize: 22, fontFace: "Impact", color: C.gold, bold: true,
    align: "center", charSpacing: 10, margin: 0
  });
  s.addText("Freaky Super Mesh · UADE · 2026", {
    x: 0.6, y: 6.55, w: 12, h: 0.35,
    fontSize: 12, fontFace: "Consolas", color: C.muted,
    align: "center", charSpacing: 3, margin: 0
  });

  footer(s, 12, TOTAL, { dark: true });
}

pres.writeFile({ fileName: "Freaky_Super_Mesh.pptx" }).then(f => {
  console.log("WROTE:", f);
});
