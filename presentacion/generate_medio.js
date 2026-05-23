const pptxgen = require("pptxgenjs");

const pres = new pptxgen();
pres.layout = "LAYOUT_WIDE"; // 13.3 x 7.5
pres.title = "Freaky Super Mesh - Análisis Teórico";
pres.author = "Fidmay, Nuñez Di Meo, Hug";

// ---------- palette: dark studio, amber accent ----------
const C = {
  bg:        "121722",
  bgAlt:     "0E1219",
  card:      "1C2230",
  cardHi:    "242B3B",
  ink:       "EAE4D4",
  inkSoft:   "B8B3A2",
  amber:     "D4A03C",
  amberDim:  "8A6A28",
  coral:     "E85A4F",
  teal:      "4FB3A9",
  grid:      "2A3142",
  rule:      "3A4256",
  muted:     "6D7688",
  mutedSoft: "8892A5",
};

const W = 13.3;
const H = 7.5;
const TOTAL = 13;

const F = {
  sans: "Calibri",
  sansB: "Calibri",   // bold via opt
  disp: "Segoe UI Semibold",
  dispB: "Segoe UI",  // bold via opt
  mono: "Consolas",
};

// ---------- primitives ----------
function fillBg(s, color) {
  s.background = { color };
}

function rect(s, x, y, w, h, fill, stroke, strokeW) {
  s.addShape(pres.shapes.RECTANGLE, {
    x, y, w, h,
    fill: fill ? { color: fill } : { type: "none" },
    line: stroke ? { color: stroke, width: strokeW || 0.5 } : { type: "none" },
  });
}

function oval(s, x, y, w, h, fill, stroke, strokeW) {
  s.addShape(pres.shapes.OVAL, {
    x, y, w, h,
    fill: fill ? { color: fill } : { type: "none" },
    line: stroke ? { color: stroke, width: strokeW || 0.5 } : { type: "none" },
  });
}

function lineSeg(s, x1, y1, x2, y2, color, width, dashed) {
  const opts = { x: x1, y: y1, w: x2 - x1, h: y2 - y1,
    line: { color, width: width || 0.75 } };
  if (dashed) opts.line.dashType = "dash";
  s.addShape(pres.shapes.LINE, opts);
}

function arrowLine(s, x1, y1, x2, y2, color, width) {
  s.addShape(pres.shapes.LINE, {
    x: x1, y: y1, w: x2 - x1, h: y2 - y1,
    line: { color, width: width || 1, endArrowType: "triangle" }
  });
}

function bidirArrow(s, x1, y1, x2, y2, color, width, inset) {
  inset = inset || 0;
  const dx = x2 - x1, dy = y2 - y1;
  const dist = Math.hypot(dx, dy); if (dist === 0) return;
  const ux = dx/dist, uy = dy/dist;
  const sx = x1 + ux*inset, sy = y1 + uy*inset;
  const ex = x2 - ux*inset, ey = y2 - uy*inset;
  s.addShape(pres.shapes.LINE, {
    x: sx, y: sy, w: ex-sx, h: ey-sy,
    line: { color, width: width || 1,
            beginArrowType: "triangle", endArrowType: "triangle" }
  });
}

function txt(s, str, x, y, w, h, opts) {
  s.addText(str, Object.assign({
    x, y, w, h, margin: 0, valign: "top", fontFace: F.sans,
  }, opts || {}));
}

// ---------- HUD chrome ----------
function cornerMark(s, x, y, size, color, corner) {
  const c = color || C.amber;
  size = size || 0.18;
  if (corner === "tl") {
    lineSeg(s, x, y, x + size, y, c, 1.2);
    lineSeg(s, x, y, x, y + size, c, 1.2);
  } else if (corner === "tr") {
    lineSeg(s, x - size, y, x, y, c, 1.2);
    lineSeg(s, x, y, x, y + size, c, 1.2);
  } else if (corner === "bl") {
    lineSeg(s, x, y, x + size, y, c, 1.2);
    lineSeg(s, x, y - size, x, y, c, 1.2);
  } else if (corner === "br") {
    lineSeg(s, x - size, y, x, y, c, 1.2);
    lineSeg(s, x, y - size, x, y, c, 1.2);
  }
}

function drawChrome(s) {
  const m = 0.4;
  cornerMark(s, m, m, 0.18, C.amber, "tl");
  cornerMark(s, W - m, m, 0.18, C.amber, "tr");
  cornerMark(s, m, H - m, 0.18, C.amber, "bl");
  cornerMark(s, W - m, H - m, 0.18, C.amber, "br");
  lineSeg(s, 0.7, 0.42, W - 0.7, 0.42, C.rule, 0.4);
  lineSeg(s, 0.7, H - 0.42, W - 0.7, H - 0.42, C.rule, 0.4);
}

function footer(s, n, label) {
  txt(s, "FSM · FREAKY SUPER MESH", 0.6, H - 0.35, 4, 0.22, {
    fontSize: 9, fontFace: F.mono, color: C.mutedSoft,
    charSpacing: 2, valign: "middle"
  });
  if (label) {
    txt(s, label, 0.6, H - 0.35, W - 1.2, 0.22, {
      fontSize: 9, fontFace: F.mono, color: C.inkSoft,
      charSpacing: 2, align: "center", valign: "middle"
    });
  }
  txt(s, `${String(n).padStart(2,"0")} / ${String(TOTAL).padStart(2,"0")}  ·  UADE 2026`,
    W - 3.6, H - 0.35, 3, 0.22, {
      fontSize: 9, fontFace: F.mono, color: C.mutedSoft,
      charSpacing: 2, align: "right", valign: "middle"
    });
}

function sectionId(s, label) {
  txt(s, label, 0.6, 0.65, 8, 0.3, {
    fontSize: 10, fontFace: F.mono, color: C.amber,
    charSpacing: 3, bold: true, valign: "middle"
  });
  rect(s, 0.6, 0.98, 0.35, 0.04, C.amber);
}

function bigTitle(s, str, size, color, y) {
  size = size || 38;
  color = color || C.ink;
  y = y || 1.15;
  txt(s, str, 0.6, y, W - 1.2, 1.0, {
    fontSize: size, fontFace: F.disp, bold: true,
    color, valign: "middle", charSpacing: 1
  });
}

function techLabel(s, str, x, y, size, color) {
  txt(s, str, x, y, 6, 0.25, {
    fontSize: size || 9, fontFace: F.mono, color: color || C.amber,
    charSpacing: 3, bold: true, valign: "middle"
  });
}

// ====================== SLIDE 1 · PORTADA ======================
{
  const s = pres.addSlide();
  fillBg(s, C.bg);
  drawChrome(s);

  // faint grid lines
  for (let i = 0; i < 8; i++) {
    lineSeg(s, 0, 0.6 + i * 0.9, W, 0.6 + i * 0.9, C.grid, 0.25);
  }

  // top strip
  txt(s, "PROYECTO  /  DISEÑO DE VIDEOJUEGOS", 0.8, 0.65, 8, 0.3, {
    fontSize: 10, fontFace: F.mono, color: C.amber, bold: true,
    charSpacing: 3, valign: "middle"
  });
  txt(s, "DIM · 03", W - 2.4, 0.65, 2, 0.3, {
    fontSize: 10, fontFace: F.mono, color: C.mutedSoft,
    charSpacing: 3, bold: true, align: "right", valign: "middle"
  });

  // title
  txt(s, "FREAKY", 0.75, 1.4, 12, 1.6, {
    fontSize: 88, fontFace: F.disp, bold: true, color: C.ink,
    charSpacing: 3, valign: "middle"
  });
  txt(s, "SUPER MESH", 0.75, 2.8, 12, 1.6, {
    fontSize: 88, fontFace: F.disp, bold: true, color: C.amber,
    charSpacing: 3, valign: "middle"
  });

  // tagline
  txt(s, "Action Rogue-lite dimensional  ·  análisis teórico de diseño",
    0.8, 4.3, 12, 0.45, {
      fontSize: 16, italic: true, color: C.inkSoft
    });

  // divider tri
  rect(s, 0.8, 4.85, 1.4, 0.04, C.amber);
  rect(s, 2.3, 4.85, 0.4, 0.04, C.coral);
  rect(s, 2.8, 4.85, 0.2, 0.04, C.teal);

  // info blocks
  function block(x, w, label, lines) {
    txt(s, label, x, 5.1, w, 0.3, {
      fontSize: 9, fontFace: F.mono, color: C.amber,
      bold: true, charSpacing: 3, valign: "middle"
    });
    lines.forEach((ln, i) => {
      txt(s, ln, x, 5.45 + i * 0.3, w, 0.3, {
        fontSize: 13, color: C.ink
      });
    });
  }
  block(0.8, 4.0, "INTEGRANTES",
    ["Fidmay, Agustín", "Nuñez Di Meo, Dante", "Hug, Juan Ignacio"]);
  block(5.1, 4.0, "CÁTEDRA",
    ["Prof. Aparicio, Lucas", "Prof. Ulrich, Gonzalo Ezequiel"]);
  block(9.4, 3.5, "UADE · 2026",
    ["Diseño de Videojuegos", "Primer Cuatrimestre · Tarde",
     "Entrega: 24 / 04 / 2026"]);

  // stamp
  lineSeg(s, 0.8, 6.6, W - 0.8, 6.6, C.rule, 0.5);
  txt(s, "EQUIPO DINAMITA", 0.8, 6.7, 6, 0.3, {
    fontSize: 11, fontFace: F.mono, color: C.amber, bold: true,
    charSpacing: 3, valign: "middle"
  });
  txt(s, "BUILD  00.1", W - 2.4, 6.7, 2, 0.3, {
    fontSize: 10, fontFace: F.mono, color: C.mutedSoft,
    charSpacing: 3, align: "right", valign: "middle"
  });

  footer(s, 1);
}

// ====================== SLIDE 2 · RESUMEN ======================
{
  const s = pres.addSlide();
  fillBg(s, C.bg);
  drawChrome(s);
  sectionId(s, "[ 01 ]  ·  RESUMEN DEL JUEGO");
  bigTitle(s, "Action Rogue-lite dimensional");

  txt(s, "El Dr. Emilio queda atrapado entre mundos tras un experimento fallido. " +
        "Debe sobrevivir oleadas, vencer jefes y volver a casa.",
    0.6, 2.2, 7.5, 0.65, {
      fontSize: 15, italic: true, color: C.inkSoft
    });

  techLabel(s, "PROPUESTA", 0.6, 2.95);
  txt(s,
    "Freaky Super Mesh es un Action Rogue-lite single-player para PC. " +
    "Desde una cámara cenital inclinada, el jugador atraviesa dimensiones " +
    "absurdas derrotando enemigos temáticos y a un jefe final por mundo. " +
    "El combate se construye sobre tres pilares: movimiento, bloqueo " +
    "y un Estado Eufórico que recompensa la ofensiva continua.",
    0.6, 3.3, 7.5, 1.9, {
      fontSize: 13, color: C.ink, paraSpaceAfter: 2
    });

  techLabel(s, "LOOP DE JUEGO", 0.6, 5.3);
  const loop = ["Explorar", "Oleadas", "Euforia", "Boss", "Nueva dim."];
  let xx = 0.6;
  loop.forEach((ch, i) => {
    const w = 1.1;
    rect(s, xx, 5.55, w, 0.4, null, C.amber, 0.6);
    txt(s, ch, xx, 5.55, w, 0.4, {
      fontSize: 11, bold: true, color: C.ink,
      align: "center", valign: "middle"
    });
    xx += w + 0.12;
    if (i < loop.length - 1) {
      arrowLine(s, xx - 0.1, 5.75, xx + 0.02, 5.75, C.amber, 0.8);
      xx += 0.1;
    }
  });

  // ficha tecnica
  rect(s, 8.5, 2.2, 4.25, 4.7, C.card, C.rule, 0.5);
  rect(s, 8.5, 2.2, 4.25, 0.4, C.cardHi);
  techLabel(s, "FICHA TÉCNICA", 8.7, 2.23, 9, C.amber);

  const fields = [
    ["GÉNERO", "Action Rogue-lite"],
    ["PLATAFORMA", "PC"],
    ["JUGADORES", "1 · Offline"],
    ["PERSONAJE", "Dr. Emilio"],
    ["CÁMARA", "Cenital 53° (iso)"],
    ["INPUT", "WASD + Mouse + Shift"],
    ["ENGINE", "Unity"],
  ];
  fields.forEach((f, i) => {
    const yy = 2.75 + i * 0.56;
    txt(s, f[0], 8.75, yy, 3.8, 0.25, {
      fontSize: 9, fontFace: F.mono, color: C.amber,
      bold: true, charSpacing: 2, valign: "middle"
    });
    txt(s, f[1], 8.75, yy + 0.22, 3.8, 0.3, {
      fontSize: 13, bold: true, color: C.ink
    });
    if (i < fields.length - 1) {
      lineSeg(s, 8.75, yy + 0.52, 12.5, yy + 0.52, C.rule, 0.4);
    }
  });

  footer(s, 2, "RESUMEN");
}

// ====================== SLIDE 3 · MDA FLOW ======================
{
  const s = pres.addSlide();
  fillBg(s, C.bg);
  drawChrome(s);
  sectionId(s, "[ 02 ]  ·  FRAMEWORK MDA");
  bigTitle(s, "Del diseñador al jugador", 36);
  txt(s, "Hunicke, LeBlanc & Zubek  ·  la dirección del flujo es la clave del análisis.",
    0.6, 2.2, 12, 0.4, {
      fontSize: 14, italic: true, color: C.inkSoft
    });

  const nodes = [
    ["01", "MECHANICS", "Reglas, sistemas, componentes del juego.",
     "Lo que diseñamos", "INPUT"],
    ["02", "DYNAMICS", "Comportamiento emergente en tiempo real.",
     "Lo que emerge", "PROCESS"],
    ["03", "AESTHETICS", "Respuestas emocionales del jugador.",
     "Lo que se siente", "OUTPUT"],
  ];
  const startX = 0.6, startY = 2.95, nodeW = 3.85, nodeH = 3.2, gap = 0.43;
  nodes.forEach((n, i) => {
    const cx = startX + i * (nodeW + gap);
    rect(s, cx, startY, nodeW, nodeH, C.card, C.rule, 0.6);
    rect(s, cx, startY, nodeW, 0.4, C.cardHi);
    txt(s, n[0], cx + 0.25, startY, 1, 0.4, {
      fontSize: 10, fontFace: F.mono, color: C.amber,
      bold: true, charSpacing: 2, valign: "middle"
    });
    txt(s, n[1], cx + 0.8, startY, nodeW - 1, 0.4, {
      fontSize: 12, bold: true, color: C.ink,
      charSpacing: 3, valign: "middle"
    });
    rect(s, cx + 0.25, startY + 0.6, 0.4, 0.03, C.amber);
    txt(s, n[3], cx + 0.25, startY + 0.75, nodeW - 0.5, 0.3, {
      fontSize: 10, fontFace: F.mono, color: C.amber,
      charSpacing: 2, bold: true
    });
    txt(s, n[2], cx + 0.25, startY + 1.2, nodeW - 0.5, 1.4, {
      fontSize: 14, bold: true, color: C.ink, paraSpaceAfter: 2
    });
    lineSeg(s, cx + 0.25, startY + nodeH - 0.4,
            cx + nodeW - 0.25, startY + nodeH - 0.4, C.rule, 0.4);
    txt(s, n[4], cx + 0.25, startY + nodeH - 0.35, nodeW - 0.5, 0.25, {
      fontSize: 9, fontFace: F.mono, color: C.mutedSoft,
      charSpacing: 2, valign: "top"
    });
  });

  for (let i = 0; i < 2; i++) {
    const ax = startX + (i + 1) * nodeW + i * gap;
    arrowLine(s, ax + 0.05, startY + nodeH / 2,
              ax + gap - 0.08, startY + nodeH / 2, C.amber, 1.5);
  }

  rect(s, 0.6, 6.3, W - 1.2, 0.55, C.cardHi);
  txt(s, "→  DISEÑADOR  parte de Mechanics", 0.6, 6.3, 6.25, 0.55, {
    fontSize: 11, fontFace: F.mono, color: C.amber, bold: true,
    charSpacing: 1.5, align: "center", valign: "middle"
  });
  txt(s, "JUGADOR  experimenta desde Aesthetics  ←", 6.85, 6.3, 6.25, 0.55, {
    fontSize: 11, fontFace: F.mono, color: C.teal, bold: true,
    charSpacing: 1.5, align: "center", valign: "middle"
  });

  footer(s, 3, "MDA · FLUJO");
}

// ====================== SLIDE 4 · MECHANICS ======================
{
  const s = pres.addSlide();
  fillBg(s, C.bg);
  drawChrome(s);
  sectionId(s, "[ 03 ]  ·  MDA · 01 MECHANICS");
  bigTitle(s, "Mecánicas del sistema", 40);
  txt(s, "Las reglas con las que se construye el juego.",
    0.6, 2.2, 12, 0.4, {
      fontSize: 14, italic: true, color: C.inkSoft
    });

  const items = [
    ["Movimiento", "WASD en cuatro direcciones."],
    ["Apuntado", "Dirección con el mouse."],
    ["Ataque", "Lanzamiento de probetas."],
    ["Defensa", "Bloqueo con Shift."],
    ["Enemigos", "Oleadas temáticas por dimensión."],
    ["Bosses", "Jefe al cierre de cada dimensión."],
    ["Salud", "Sistema numérico con daño recibido."],
    ["Estado Eufórico", "Se activa al combinar bloqueo y kills."],
    ["Buff temporal", "Stats mejoradas durante la euforia."],
    ["Regeneración", "Recuperación de vida en euforia."],
  ];
  const cols = 2, cardW = 6.05, cardH = 0.7, gapX = 0.25, gapY = 0.1;
  const startX = 0.6, startY = 2.85;
  items.forEach((it, i) => {
    const cx = startX + (i % cols) * (cardW + gapX);
    const cy = startY + Math.floor(i / cols) * (cardH + gapY);
    rect(s, cx, cy, cardW, cardH, C.card, C.rule, 0.4);
    rect(s, cx, cy, 0.04, cardH, C.amber);
    const id = `M.${String(i + 1).padStart(2, "0")}`;
    txt(s, id, cx + 0.2, cy, 0.7, cardH, {
      fontSize: 9, fontFace: F.mono, color: C.amber,
      bold: true, charSpacing: 1.5, valign: "middle"
    });
    txt(s, it[0], cx + 0.95, cy + 0.08, cardW - 1.1, 0.3, {
      fontSize: 13, bold: true, color: C.ink, valign: "middle"
    });
    txt(s, it[1], cx + 0.95, cy + 0.38, cardW - 1.1, 0.3, {
      fontSize: 11, color: C.inkSoft
    });
  });

  footer(s, 4, "MDA · MECHANICS");
}

// ====================== SLIDE 5 · DYNAMICS ======================
{
  const s = pres.addSlide();
  fillBg(s, C.bg);
  drawChrome(s);
  sectionId(s, "[ 04 ]  ·  MDA · 02 DYNAMICS");
  bigTitle(s, "Dinámicas emergentes", 40);
  txt(s, "Lo que ocurre cuando el jugador entra en contacto con las reglas.",
    0.6, 2.2, 12, 0.4, {
      fontSize: 14, italic: true, color: C.inkSoft
    });

  const items = [
    ["01", "Movimiento constante", "Esquivar y reposicionarse es parte del combate."],
    ["02", "Posicionamiento", "Lectura del espacio frente a hordas."],
    ["03", "Bloqueo estratégico", "Defensa activa como recurso de supervivencia."],
    ["04", "Incentivo ofensivo", "Eliminar rápido acelera el estado eufórico."],
    ["05", "Aprendizaje", "Lectura de patrones de enemigos y jefes."],
    ["06", "Presión creciente", "El ritmo aumenta partida a partida."],
  ];
  items.forEach((it, i) => {
    const col = i % 3, row = Math.floor(i / 3);
    const cx = 0.6 + col * 4.15, cy = 2.85 + row * 1.7;
    rect(s, cx, cy, 3.95, 1.5, C.card, C.rule, 0.5);
    rect(s, cx, cy, 3.95, 0.3, C.cardHi);
    txt(s, `D.${it[0]}`, cx + 0.2, cy, 1, 0.3, {
      fontSize: 9, fontFace: F.mono, color: C.amber,
      bold: true, charSpacing: 1.5, valign: "middle"
    });
    txt(s, it[1], cx + 0.2, cy + 0.4, 3.6, 0.35, {
      fontSize: 14, bold: true, color: C.ink
    });
    txt(s, it[2], cx + 0.2, cy + 0.8, 3.6, 0.6, {
      fontSize: 11, italic: true, color: C.inkSoft
    });
  });

  techLabel(s, "RITMO DE PARTIDA", 0.6, 6.3);
  const steps = ["Inicio", "Más enemigos", "Presión creciente", "Jefe final", "Nueva dimensión"];
  const stepW = (W - 1.2) / steps.length;
  steps.forEach((st, i) => {
    const sx = 0.6 + i * stepW;
    const last = i === steps.length - 1;
    const col = last ? C.coral : C.amber;
    oval(s, sx + 0.04, 6.62, 0.12, 0.12, col);
    txt(s, st, sx + 0.25, 6.55, stepW - 0.3, 0.3, {
      fontSize: 11, color: last ? C.ink : C.inkSoft,
      bold: last
    });
    if (i < steps.length - 1) {
      lineSeg(s, sx + 0.2, 6.68, sx + stepW - 0.1, 6.68, C.rule, 0.5);
    }
  });

  footer(s, 5, "MDA · DYNAMICS");
}

// ====================== SLIDE 6 · AESTHETICS ======================
{
  const s = pres.addSlide();
  fillBg(s, C.bg);
  drawChrome(s);
  sectionId(s, "[ 05 ]  ·  MDA · 03 AESTHETICS");
  bigTitle(s, "Las 8 categorías de Hunicke", 38);
  txt(s, "Identificamos cuáles aplican al juego y cuáles quedan fuera de alcance.",
    0.6, 2.2, 12, 0.4, {
      fontSize: 14, italic: true, color: C.inkSoft
    });

  const items = [
    ["SENSATION",  "Combate frenético, feedback del lanzamiento y del estado eufórico.", true],
    ["FANTASY",    "Universo bizarro: un laboratorio que se rebela contra su dueño.",     true],
    ["NARRATIVE",  "Dr. Emilio intenta volver a casa tras un experimento fallido.",       true],
    ["CHALLENGE",  "Supervivencia frente a oleadas y jefes con dificultad creciente.",    true],
    ["FELLOWSHIP", "No aplica: propuesta single-player offline sin interacción social.",  false],
    ["DISCOVERY",  "Cada dimensión introduce nuevos enemigos y escenarios.",              true],
    ["EXPRESSION", "No contemplada explícitamente en el GDD (sin customización).",        false],
    ["SUBMISSION", "Loop arcade y rejugabilidad: la partida corta como propuesta.",       true],
  ];
  const cols = 4;
  const startX = 0.6, startY = 2.85;
  const cardW = 3.0, cardH = 1.95, gapX = 0.13, gapY = 0.18;
  items.forEach((it, i) => {
    const ci = i % cols, ri = Math.floor(i / cols);
    const cx = startX + ci * (cardW + gapX);
    const cy = startY + ri * (cardH + gapY);
    const col = it[2] ? C.amber : C.coral;
    rect(s, cx, cy, cardW, cardH, C.card, C.rule, 0.5);
    rect(s, cx, cy, 0.04, cardH, col);
    // SÍ/NO pill
    const pillW = 0.6, pillH = 0.3;
    rect(s, cx + cardW - pillW - 0.15, cy + 0.15, pillW, pillH,
         it[2] ? col : C.card, col, 0.7);
    txt(s, it[2] ? "SÍ" : "NO",
        cx + cardW - pillW - 0.15, cy + 0.15, pillW, pillH, {
      fontSize: 10, fontFace: F.mono, bold: true,
      color: it[2] ? C.bg : col,
      charSpacing: 2, align: "center", valign: "middle"
    });
    txt(s, `A.${String(i + 1).padStart(2, "0")}`,
        cx + 0.2, cy + 0.2, 1, 0.3, {
      fontSize: 9, fontFace: F.mono, color: col,
      charSpacing: 2, bold: true
    });
    txt(s, it[0], cx + 0.2, cy + 0.55, cardW - 0.4, 0.4, {
      fontSize: 16, bold: true, color: it[2] ? C.ink : C.inkSoft,
      charSpacing: 1
    });
    txt(s, it[1], cx + 0.2, cy + 1.0, cardW - 0.4, 0.95, {
      fontSize: 10, color: it[2] ? C.inkSoft : C.muted
    });
  });

  footer(s, 6, "MDA · AESTHETICS");
}

// ====================== SLIDE 7 · CADENA MDA ======================
{
  const s = pres.addSlide();
  fillBg(s, C.bg);
  drawChrome(s);
  sectionId(s, "[ 06 ]  ·  CADENA COMPLETA MDA");
  bigTitle(s, "Caso: Estado Eufórico", 40);
  txt(s, "Cómo una mecánica concreta se transforma en experiencia estética.",
    0.6, 2.2, 12, 0.4, {
      fontSize: 14, italic: true, color: C.inkSoft
    });

  const chain = [
    ["M", "MECÁNICA", "Eliminar enemigos y bloquear ataques activa el estado eufórico."],
    ["D", "DINÁMICA", "El jugador combina ofensiva y defensa para acelerar su activación."],
    ["A", "ESTÉTICA", "Challenge · Sensation · Submission."],
  ];
  const startY = 2.85;
  chain.forEach((c, i) => {
    const cx = 0.6 + i * 4.23;
    rect(s, cx, startY, 3.95, 3.35, C.card, C.rule, 0.6);
    rect(s, cx, startY, 3.95, 0.4, C.cardHi);
    txt(s, `STEP 0${i + 1}`, cx + 0.2, startY, 1.5, 0.4, {
      fontSize: 9, fontFace: F.mono, color: C.amber,
      bold: true, charSpacing: 2, valign: "middle"
    });
    txt(s, c[1], cx + 0.2, startY, 3.55, 0.4, {
      fontSize: 11, bold: true, color: C.ink,
      align: "right", charSpacing: 3, valign: "middle"
    });
    oval(s, cx + 3.95 / 2 - 0.5, startY + 0.8, 1.0, 1.0, C.cardHi, C.amber, 1.2);
    txt(s, c[0], cx, startY + 0.78, 3.95, 1.05, {
      fontSize: 46, fontFace: F.disp, bold: true, color: C.amber,
      align: "center", valign: "middle"
    });
    lineSeg(s, cx + 0.4, startY + 2.2, cx + 3.55, startY + 2.2, C.rule, 0.5);
    txt(s, c[2], cx + 0.3, startY + 2.35, 3.35, 0.9, {
      fontSize: 12, color: C.ink
    });
    if (i < 2) {
      arrowLine(s, cx + 4.0, startY + 3.35 / 2, cx + 4.2, startY + 3.35 / 2,
                C.amber, 1.5);
    }
  });

  rect(s, 0.6, 6.4, W - 1.2, 0.5, C.cardHi);
  txt(s, "Una misma acción alimenta defensa, ofensiva y recompensa emocional.",
      0.6, 6.4, W - 1.2, 0.5, {
    fontSize: 12, bold: true, color: C.amber,
    align: "center", valign: "middle"
  });

  footer(s, 7, "MDA · CADENA");
}

// ====================== SLIDE 8 · TÉTRADA DE SCHELL ======================
{
  const s = pres.addSlide();
  fillBg(s, C.bg);
  drawChrome(s);
  sectionId(s, "[ 07 ]  ·  TÉTRADA DE SCHELL");
  bigTitle(s, "Los cuatro elementos y sus relaciones", 32);
  txt(s, "Diagrama de influencias recíprocas entre los elementos del juego.",
    0.6, 2.2, 12, 0.4, {
      fontSize: 14, italic: true, color: C.inkSoft
    });

  const nodes = {
    "MECÁNICA":   { cx: 4.5, cy: 3.1,
      content: ["Combate en tiempo real", "Bosses por dimensión", "Estado eufórico"] },
    "ESTÉTICA":   { cx: 7.0, cy: 4.85,
      content: ["Humor absurdo", "Mundos bizarros", "Feedback frenético"] },
    "TECNOLOGÍA": { cx: 4.5, cy: 6.6,
      content: ["Unity · Modelos 3D", "Cámara cenital 53°", "PC · Offline"] },
    "HISTORIA":   { cx: 2.0, cy: 4.85,
      content: ["Dr. Emilio", "Portal fallido", "Volver a casa"] },
  };
  const nodeW = 2.6, nodeH = 1.05;

  const pairs = [
    ["MECÁNICA","ESTÉTICA"], ["MECÁNICA","TECNOLOGÍA"],
    ["MECÁNICA","HISTORIA"], ["ESTÉTICA","TECNOLOGÍA"],
    ["ESTÉTICA","HISTORIA"], ["TECNOLOGÍA","HISTORIA"]
  ];
  pairs.forEach(([a, b]) => {
    const na = nodes[a], nb = nodes[b];
    bidirArrow(s, na.cx, na.cy, nb.cx, nb.cy, C.amber, 1.2, 0.72);
  });

  oval(s, 4.5 - 0.15, 4.85 - 0.15, 0.3, 0.3, C.amber);
  oval(s, 4.5 - 0.08, 4.85 - 0.08, 0.16, 0.16, C.bg);

  Object.entries(nodes).forEach(([name, n]) => {
    const cx = n.cx - nodeW / 2, cy = n.cy - nodeH / 2;
    rect(s, cx, cy, nodeW, nodeH, C.card, C.amber, 1.1);
    rect(s, cx, cy, nodeW, 0.3, C.cardHi);
    txt(s, name, cx, cy, nodeW, 0.3, {
      fontSize: 11, fontFace: F.mono, bold: true, color: C.amber,
      charSpacing: 3, align: "center", valign: "middle"
    });
    n.content.forEach((it, j) => {
      txt(s, "· " + it, cx + 0.18, cy + 0.35 + j * 0.22, nodeW - 0.3, 0.22, {
        fontSize: 9, color: C.ink
      });
    });
  });

  // right panel
  const px = 8.8, py = 2.85, pw = 4.0, ph = 4.1;
  rect(s, px, py, pw, ph, C.card, C.rule, 0.6);
  rect(s, px, py, pw, 0.35, C.cardHi);
  txt(s, "INFLUENCIAS RECÍPROCAS", px, py, pw, 0.35, {
    fontSize: 10, fontFace: F.mono, bold: true, color: C.amber,
    align: "center", valign: "middle", charSpacing: 3
  });

  const infl = [
    ["HISTORIA  →  MECÁNICA",   "El portal fallido justifica oleadas y bosses por dimensión."],
    ["MECÁNICA  →  ESTÉTICA",   "El combate frenético y la euforia generan sensation y submission."],
    ["TECNOLOGÍA  →  MECÁNICA", "La cámara isométrica en Unity habilita el combate con apuntado."],
    ["ESTÉTICA  →  HISTORIA",   "El tono absurdo construye el universo bizarro del laboratorio."],
  ];
  let yy = py + 0.55;
  infl.forEach(it => {
    rect(s, px + 0.25, yy + 0.05, 0.06, 0.7, C.amber);
    txt(s, it[0], px + 0.45, yy, pw - 0.65, 0.22, {
      fontSize: 9, fontFace: F.mono, color: C.amber,
      charSpacing: 1.5, bold: true
    });
    txt(s, it[1], px + 0.45, yy + 0.22, pw - 0.65, 0.6, {
      fontSize: 10, color: C.ink
    });
    yy += 0.88;
  });

  footer(s, 8, "TÉTRADA DE SCHELL");
}

// ====================== SLIDE 9 · RELACIÓN ======================
{
  const s = pres.addSlide();
  fillBg(s, C.bg);
  drawChrome(s);
  sectionId(s, "[ 08 ]  ·  RELACIÓN ENTRE ELEMENTOS");
  bigTitle(s, "Coherencia del sistema", 42);
  txt(s, "La articulación entre narrativa, sistemas y presentación.",
    0.6, 2.2, 12, 0.4, {
      fontSize: 14, italic: true, color: C.inkSoft
    });

  const rels = [
    ["HISTORIA", "DIMENSIONES", "La narrativa justifica cada mundo y sus enemigos."],
    ["MECÁNICA", "DESAFÍO", "Cada dimensión se traduce en un puzzle jugable."],
    ["ESTÉTICA", "MEMORIA", "El universo absurdo vuelve al juego inolvidable."],
    ["TECNOLOGÍA", "FLUIDEZ", "Unity garantiza una jugabilidad sólida y estable."],
  ];
  let y = 2.85;
  rels.forEach(r => {
    rect(s, 0.6, y, 8.2, 0.8, C.card, C.rule, 0.5);
    rect(s, 0.6, y, 0.05, 0.8, C.amber);
    txt(s, r[0], 0.85, y, 2.1, 0.8, {
      fontSize: 12, fontFace: F.mono, bold: true, color: C.amber,
      charSpacing: 3, valign: "middle"
    });
    arrowLine(s, 2.95, y + 0.4, 3.4, y + 0.4, C.amber, 0.9);
    txt(s, r[1], 3.75, y, 2.3, 0.8, {
      fontSize: 12, bold: true, color: C.ink,
      charSpacing: 2, valign: "middle"
    });
    txt(s, r[2], 6.1, y, 2.55, 0.8, {
      fontSize: 12, italic: true, color: C.inkSoft, valign: "middle"
    });
    y += 0.92;
  });

  rect(s, 9.1, 2.85, 3.65, 3.55, C.cardHi, C.amber, 0.8);
  rect(s, 9.1, 2.85, 3.65, 0.4, C.amber);
  txt(s, "CONCLUSIÓN", 9.1, 2.85, 3.65, 0.4, {
    fontSize: 10, fontFace: F.mono, bold: true, color: C.bg,
    charSpacing: 3, align: "center", valign: "middle"
  });
  txt(s, "Coherencia", 9.3, 3.4, 3.3, 0.7, {
    fontSize: 32, fontFace: F.disp, bold: true, color: C.amber
  });
  txt(s, "entre narrativa, sistemas y presentación visual.",
    9.3, 4.3, 3.3, 1.3, {
      fontSize: 14, color: C.ink
    });
  rect(s, 9.3, 5.75, 0.8, 0.04, C.coral);
  txt(s, "— FSM TEAM", 9.3, 5.9, 3.3, 0.25, {
    fontSize: 10, fontFace: F.mono, color: C.mutedSoft, charSpacing: 2
  });

  footer(s, 9, "RELACIÓN");
}

// ====================== SLIDE 10 · REPARTO GDD ======================
{
  const s = pres.addSlide();
  fillBg(s, C.bg);
  drawChrome(s);
  sectionId(s, "[ 09 ]  ·  REPARTO DE TAREAS");
  bigTitle(s, "Autoría por sección del GDD", 34);
  txt(s, "Secciones del Game Design Document  ·  completar con el integrante responsable.",
    0.6, 2.2, 12, 0.4, {
      fontSize: 14, italic: true, color: C.inkSoft
    });

  const sections = [
    ["01", "Concepto General"], ["02", "Narrativa y contexto"],
    ["03", "Mecánicas principales"], ["04", "Progresión y estructura de niveles"],
    ["05", "Listado de assets"], ["06", "Desarrollo"],
    ["07", "Referencias"], ["08", "Uso de IA"],
  ];
  const cols = 2, rowsPerCol = 4;
  const startX = 0.6, startY = 2.85, colW = 6.1, gapX = 0.3, rowH = 0.9;
  sections.forEach((sec, i) => {
    const col = Math.floor(i / rowsPerCol);
    const row = i % rowsPerCol;
    const cx = startX + col * (colW + gapX);
    const cy = startY + row * rowH;
    rect(s, cx, cy, colW, rowH - 0.15, C.card, C.rule, 0.5);
    rect(s, cx, cy, 0.7, rowH - 0.15, C.cardHi);
    txt(s, sec[0], cx, cy, 0.7, rowH - 0.15, {
      fontSize: 22, fontFace: F.disp, bold: true, color: C.amber,
      align: "center", valign: "middle"
    });
    txt(s, sec[1], cx + 0.9, cy + 0.08, colW - 1.1, 0.35, {
      fontSize: 13, bold: true, color: C.ink, valign: "middle"
    });
    txt(s, "INTEGRANTE", cx + 0.9, cy + 0.42, 1.2, 0.25, {
      fontSize: 8, fontFace: F.mono, bold: true, color: C.amber,
      charSpacing: 2, valign: "middle"
    });
    txt(s, "___________________________", cx + 2.0, cy + 0.42, colW - 2.2, 0.3, {
      fontSize: 11, fontFace: F.mono, color: C.inkSoft
    });
  });

  lineSeg(s, 0.6, 6.5, W - 0.6, 6.5, C.rule, 0.5);
  txt(s, "EQUIPO DINAMITA  ·  FIDMAY  ·  NUÑEZ DI MEO  ·  HUG",
    0.6, 6.6, W - 1.2, 0.3, {
      fontSize: 10, fontFace: F.mono, bold: true, color: C.amber,
      charSpacing: 3, align: "center", valign: "middle"
    });

  footer(s, 10, "REPARTO GDD");
}

// ====================== SLIDE 11 · REFERENCIAS ======================
{
  const s = pres.addSlide();
  fillBg(s, C.bg);
  drawChrome(s);
  sectionId(s, "[ 10 ]  ·  REFERENCIAS E INFLUENCIAS");
  bigTitle(s, "El ADN del juego", 42);

  const refs = [
    ["MegaBonk", "Diversión arcade inmediata y acción directa."],
    ["Vampire Survivors", "Supervivencia frente a hordas, progresión y rejugabilidad."],
    ["Hotline Miami", "Ritmo frenético, presión constante y combate intenso."],
    ["Rick and Morty", "Humor absurdo y dimensiones alternativas."],
    ["Dr. Mundo — LoL", "Científico descontrolado, poder bruto."],
    ["Heimerdinger — LoL", "Inventor científico, estética tecnológica."],
  ];
  refs.forEach((r, i) => {
    const col = i % 3, row = Math.floor(i / 3);
    const cx = 0.6 + col * 4.15, cy = 2.4 + row * 2.1;
    rect(s, cx, cy, 3.95, 1.85, C.card, C.rule, 0.5);
    rect(s, cx, cy, 3.95, 0.35, C.cardHi);
    txt(s, `REF.${String(i + 1).padStart(2, "0")}`,
        cx + 0.2, cy, 2, 0.35, {
      fontSize: 9, fontFace: F.mono, bold: true, color: C.amber,
      charSpacing: 3, valign: "middle"
    });
    txt(s, r[0], cx + 0.25, cy + 0.5, 3.45, 0.6, {
      fontSize: 18, bold: true, color: C.ink
    });
    rect(s, cx + 0.25, cy + 1.08, 0.4, 0.03, C.amber);
    txt(s, r[1], cx + 0.25, cy + 1.2, 3.45, 0.6, {
      fontSize: 11, italic: true, color: C.inkSoft
    });
  });

  footer(s, 11, "REFERENCIAS");
}

// ====================== SLIDE 12 · EVIDENCIAS ======================
{
  const s = pres.addSlide();
  fillBg(s, C.bg);
  drawChrome(s);
  sectionId(s, "[ 11 ]  ·  DESARROLLO DEL PROYECTO");
  bigTitle(s, "Evidencias de producción", 40);
  txt(s, "Material visual del trabajo: concept art, escenas en Unity y gameplay.",
    0.6, 2.2, 12, 0.4, {
      fontSize: 14, italic: true, color: C.inkSoft
    });

  const placeholders = [
    ["PERSONAJES", "Dr. Emilio · enemigos por dimensión"],
    ["ESCENAS EN UNITY", "Capturas de editor y escenarios"],
    ["GAMEPLAY", "Videos de combate y bosses"],
    ["ITERACIÓN DE DISEÑO", "Pruebas, bocetos, prototipos"],
  ];
  const cols = 2;
  const startX = 0.6, startY = 2.85;
  const cardW = 6.05, cardH = 1.95, gapX = 0.2, gapY = 0.2;
  placeholders.forEach((p, i) => {
    const ci = i % cols, ri = Math.floor(i / cols);
    const cx = startX + ci * (cardW + gapX);
    const cy = startY + ri * (cardH + gapY);
    // dashed frame
    s.addShape(pres.shapes.RECTANGLE, {
      x: cx, y: cy, w: cardW, h: cardH,
      fill: { type: "none" },
      line: { color: C.amber, width: 0.9, dashType: "dash" }
    });
    // inner card
    rect(s, cx + 0.08, cy + 0.08, cardW - 0.16, cardH - 0.16, C.card);
    // corner marks
    cornerMark(s, cx + 0.18, cy + 0.22, 0.12, C.amber, "tl");
    cornerMark(s, cx + cardW - 0.18, cy + 0.22, 0.12, C.amber, "tr");
    cornerMark(s, cx + 0.18, cy + cardH - 0.22, 0.12, C.amber, "bl");
    cornerMark(s, cx + cardW - 0.18, cy + cardH - 0.22, 0.12, C.amber, "br");
    txt(s, `SLOT.${String(i + 1).padStart(2, "0")}`,
        cx + 0.45, cy + 0.3, 2, 0.3, {
      fontSize: 9, fontFace: F.mono, bold: true, color: C.amber,
      charSpacing: 3, valign: "middle"
    });
    txt(s, p[0], cx + 0.45, cy + 0.6, cardW - 0.9, 0.4, {
      fontSize: 16, bold: true, color: C.ink, charSpacing: 1
    });
    rect(s, cx + 0.45, cy + 1.05, 0.4, 0.03, C.amber);
    txt(s, p[1], cx + 0.45, cy + 1.15, cardW - 0.9, 0.4, {
      fontSize: 12, italic: true, color: C.inkSoft
    });
    txt(s, "[  INSERTAR CAPTURA / VIDEO  ]",
        cx + cardW - 2.3, cy + cardH - 0.45, 2.1, 0.3, {
      fontSize: 9, fontFace: F.mono, color: C.mutedSoft,
      charSpacing: 2, align: "right", valign: "middle"
    });
  });

  footer(s, 12, "EVIDENCIAS");
}

// ====================== SLIDE 13 · CIERRE ======================
{
  const s = pres.addSlide();
  fillBg(s, C.bg);
  drawChrome(s);

  for (let i = 0; i < 8; i++) {
    lineSeg(s, 0, 0.6 + i * 0.9, W, 0.6 + i * 0.9, C.grid, 0.25);
  }

  txt(s, "END  /  FIN DE LA PRESENTACIÓN", 0.6, 1.9, W - 1.2, 0.35, {
    fontSize: 10, fontFace: F.mono, bold: true, color: C.amber,
    charSpacing: 6, align: "center", valign: "middle"
  });
  rect(s, W / 2 - 0.75, 2.45, 1.5, 0.04, C.amber);

  txt(s, "Freaky Super Mesh", 0.6, 2.75, W - 1.2, 1.3, {
    fontSize: 56, fontFace: F.disp, bold: true, color: C.ink,
    align: "center", valign: "middle"
  });

  txt(s, "Sistemas claros.   Acción intensa.   Identidad propia.",
    0.6, 4.2, W - 1.2, 0.5, {
      fontSize: 18, italic: true, color: C.amber, align: "center"
    });

  lineSeg(s, W / 2 - 3, 5.05, W / 2 + 3, 5.05, C.rule, 0.6);

  txt(s, "GRACIAS", 0.6, 5.2, W - 1.2, 0.7, {
    fontSize: 28, fontFace: F.disp, bold: true, color: C.amber,
    charSpacing: 6, align: "center", valign: "middle"
  });
  txt(s, "EQUIPO DINAMITA", 0.6, 6.1, W - 1.2, 0.3, {
    fontSize: 10, fontFace: F.mono, bold: true, color: C.amber,
    charSpacing: 3, align: "center", valign: "middle"
  });
  txt(s, "Fidmay  ·  Nuñez Di Meo  ·  Hug", 0.6, 6.35, W - 1.2, 0.3, {
    fontSize: 13, color: C.ink, align: "center", valign: "middle"
  });
  txt(s, "UADE  ·  Diseño de Videojuegos  ·  2026", 0.6, 6.65, W - 1.2, 0.25, {
    fontSize: 10, fontFace: F.mono, color: C.mutedSoft,
    charSpacing: 2, align: "center", valign: "middle"
  });

  footer(s, 13);
}

pres.writeFile({ fileName: "Freaky_Super_Mesh_Medio.pptx" }).then(f => {
  console.log("WROTE:", f);
});
