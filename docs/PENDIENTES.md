# Freaky Super Pixel — Hecho / Pendiente

_Última actualización: 2026-06-21_

---

## ✅ HECHO

### Dificultad
- Selector de 4 niveles (Fácil / Normal / Difícil / Pesadilla) antes del portal. **Por qué:** que el jugador elija el reto. **Impacto:** toda la partida se adapta.
- Escala enemigos: daño que te hacen, vida.
- Escala a Emilio: vida máxima, cooldown de parry, parries para transformar.
- Escala el spawn: cantidad y frecuencia.
- Escala el boss: HP, ventana vulnerable, daño en esa ventana.
- Buffs a Emilio en Difícil/Pesadilla: +daño, +rango de ataque, +daño y +radio del aura. **Por qué:** bancar la horda. **Impacto:** duro pero justo.
- Se guarda en la partida (saves viejos = Normal).

### Enemigos / spawn
- Spawn por **todo el mapa** (entre las 4 paredes), no en cajitas fijas. **Impacto:** salen de todos lados, el carrito ya no del mismo lado.
- Borrado `EnemySpawner2` (estaba dormido, no spawneaba).
- **Ositos minion** en Pesadilla durante la pelea del boss (reproducen el zarpazo del oso grande). **Impacto:** caos extra que te estorba.

### Aura modo Freaky
- Ya no repele (los enemigos se acercan), el daño **aumenta con la cercanía**, sacado el instakill de toque.

### Joystick / gamepad (todo nuevo)
- Mover con stick izquierdo + apuntar con stick derecho (twin-stick).
- Parry con **R1**.
- Pausa con **Options**, volver con **Círculo** (solo donde corresponde; game over no).
- Navegar menús con D-pad/stick; confirmar con **✕ (PS) / A (Xbox) + R1** (posición correcta en todos los controles, no depende del número de botón legacy).
- Hover de botones también con el joystick.
- Lobby: interactuar con **✕ / R1**.
- Detector de dispositivo (teclado vs joystick) + swap automático de carteles **"E" → "X"** (mecanismo listo; falta la foto "X").
- Fix del flood de eventos del **DualShock4** (touchpad/giroscopio) que descartaba input: se quitó el tope `maxEventBytesPerUpdate`.

### Combate / boss (hecho en otro chat)
- Oso más rápido.
- Stun del oso = 2s.
- Transformación bloqueada durante la pelea del boss.
- Cartel "PARRY" reubicado sobre el oso.
- Parry con cooldown + indicador visual.

---

## ⏳ FALTA

### 🎨 Necesita tu arte / fotos
- Contenedor / skin de la **UI de pausa**.
- Contenedor + botones de la **UI de victoria**.
- **Skin del popup de dificultad** (hoy funcional pero básico).
- **Logo** con la estética nueva.
- **Imágenes de cómo jugar**.
- **Íconos de controles** (teclado y joystick) para los hints.
- Foto **"X para interactuar"** (el swap automático ya está hecho).
- **Ícono de parry bloqueado**.
- **Videos de portal por dificultad** (mismo video, portal de otro color; ej. Pesadilla rojo). El código que elige el clip según la dificultad lo hago yo; faltan los videos.

### 🛠️ Puedo hacer ya (lógica / sistemas, sin arte)
- **Sistema de LOGROS**: backend (cuándo se desbloquean + guardado) + popup al desbloquear + menú de logros (desbloqueados / faltantes). El arte de cada logro se suma después.
- **Pantalla de controles** reutilizable (en opciones del inicio y de la pausa) — estructura/lógica; el arte después.
- **Hint de "cómo se juega"** en el menú de inicio con swap automático teclado ↔ joystick (reusa el detector ya hecho).
- **F para ver más** de la descripción de cada dificultad en el popup.
- **Generalizar el swap "E → X"** a todos los carteles que mencionen teclas.
- **Panel de misión/objetivo arriba a la izquierda**: checklist con `[X]` (verde = completado) / `[ ]` (pendiente), estilo la referencia. Se engancha a las fases del nivel (`WaveManager.OnWaveEnd` y `BossBearHealth.OnBossDied`). Objetivos propuestos: `[ ] Sobreviví la oleada` → `[ ] Derrotá al OSO`. **El usuario va a pasar un fondo/arte para ese espacio → arrancar cuando lo tenga.**

### 🐞 Bugs
- **Cámara que se mete dentro del súper** (y otros lugares) en vez de acomodarse contra las paredes — investigar el sistema de cámara.

### ✔️ Decisiones tomadas
- **Arriba a la izquierda**: panel de misión/objetivo (checklist), ver sección 🛠️.
- **Aura**: queda **gradual** (más daño cuanto más cerca), SIN instakill al toque.
- **Zarpazo de los ositos**: reusan la animación de ataque del oso grande (trigger `Attack`). HECHO.

### 📦 Operativo
- **Probar con control físico** (lo único que no se puede testear sin hardware).
- **Commit** de toda la sesión (todavía sin commitear).
- Reabrir la escena de **menú** en el editor (quedó abierta Nivel01).
