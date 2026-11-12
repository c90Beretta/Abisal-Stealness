---
name: "ABISAL — puerto unificado 0.7"
description: "Sistema industrial PS1 integrado 0.7; U1/U2 resueltos con ship acotado, build final en curso."
colors:
  ink: "rgb(2.5% 2.3% 1.9%)"
  hud-surface: "rgb(2.5% 2.3% 1.9% / 78%)"
  mission-surface: "rgb(2% 1.8% 1.5% / 65%)"
  menu-surface: "rgb(2.2% 2% 1.6%)"
  primary: "rgb(88% 82% 64%)"
  muted: "rgb(76% 74% 66%)"
  text: "rgb(100% 100% 100%)"
  button-surface: "rgb(18% 17% 14%)"
  deployment-surface: "rgb(2.5% 2.3% 1.9% / 98%)"
  deployment-option: "rgb(12% 11.5% 9%)"
  deployment-selected: "rgb(38% 32% 18%)"
  deployment-focus: "rgb(95% 90% 73%)"
  gauge-track: "rgb(22% 21% 18%)"
  warning: "rgb(100% 55% 46%)"
  loot-feedback: "rgb(100% 72% 55%)"
  enemy-health: "rgb(90% 36% 27%)"
  noise-pulse: "rgb(95% 65% 35% / 27%)"
  aged-enamel: "rgb(84% 79% 66%)"
  service-wall: "rgb(20% 24% 22%)"
  dull-steel: "rgb(24% 25% 23%)"
  oxide: "rgb(34% 17% 9.5%)"
  safety-paint: "rgb(63% 39% 12%)"
  concrete: "rgb(34% 32% 28%)"
  service-deck: "rgb(43% 42% 34%)"
  soldier-cloth: "rgb(50% 52% 54%)"
  tactical-plates: "rgb(14% 15% 16%)"
  boot-leather: "rgb(10% 9% 8%)"
  oxygen-tank: "rgb(27% 29% 25%)"
  soldier-boots: "rgb(85% 87% 89%)"
  soldier-plates: "rgb(65% 67% 69%)"
  soldier-knees: "rgb(20% 23% 22%)"
  tactical-seams: "rgb(24% 25% 22%)"
  interior-steel: "rgb(21% 23% 21%)"
  interior-enamel: "rgb(67% 62% 47%)"
  interior-deck: "rgb(36% 35% 28%)"
  crt-case: "rgb(24% 25% 20%)"
  crt-bezel: "rgb(4.5% 6% 4.5%)"
  keyboard-keys: "rgb(22% 24% 20%)"
  phosphor-glass: "rgb(1.2% 3.2% 1.9%)"
  terminal-powered: "rgb(69% 88% 57%)"
  terminal-offline: "rgb(88% 59% 24%)"
typography:
  display:
    fontFamily: "Crimson Text"
    fontSize: "68px"
    fontWeight: 600
  headline:
    fontFamily: "Barlow Condensed"
    fontSize: "26px"
    fontWeight: 500
  title:
    fontFamily: "Barlow Condensed"
    fontSize: "22px"
    fontWeight: 500
  body:
    fontFamily: "Barlow Condensed"
    fontSize: "20px"
    fontWeight: 500
  label:
    fontFamily: "Barlow Condensed"
    fontSize: "17px"
    fontWeight: 500
  ammo:
    fontFamily: "Barlow Condensed"
    fontSize: "42px"
    fontWeight: 500
  interaction:
    fontFamily: "Barlow Condensed"
    fontSize: "23px"
    fontWeight: 500
  terminal:
    fontFamily: "Barlow Condensed"
    fontSize: "36px"
    fontWeight: 500
    lineHeight: 0.9
rounded:
  native: "0px"
spacing:
  panel-inset: "14px"
  menu-label-inset: "16px"
  equipment-title-inset: "24px"
  equipment-column-inset: "26px"
  equipment-action-inset: "10px"
components:
  button:
    backgroundColor: "{colors.button-surface}"
    textColor: "{colors.text}"
    typography: "{typography.body}"
    rounded: "{rounded.native}"
    width: "382px"
    height: "48px"
  vitals:
    backgroundColor: "{colors.hud-surface}"
    textColor: "{colors.text}"
    rounded: "{rounded.native}"
    width: "276px"
    height: "122px"
  mission:
    backgroundColor: "{colors.mission-surface}"
    textColor: "{colors.primary}"
    typography: "{typography.title}"
    width: "510px"
    height: "52px"
  ammunition:
    backgroundColor: "{colors.hud-surface}"
    textColor: "{colors.text}"
    typography: "{typography.ammo}"
    width: "198px"
    height: "122px"
  objective-marker:
    backgroundColor: "{colors.ink}"
    textColor: "{colors.primary}"
    typography: "{typography.label}"
    width: "164px"
    height: "27px"
  equipment:
    backgroundColor: "{colors.ink}"
    textColor: "{colors.text}"
    rounded: "{rounded.native}"
    width: "1000px"
    height: "550px"
  equipment-action:
    backgroundColor: "{colors.button-surface}"
    textColor: "{colors.text}"
    typography: "{typography.label}"
    rounded: "{rounded.native}"
    width: "224px"
    height: "38px"
  equipment-weapon:
    backgroundColor: "{colors.button-surface}"
    textColor: "{colors.text}"
    typography: "{typography.label}"
    width: "143px"
    height: "38px"
  loot-take:
    backgroundColor: "{colors.button-surface}"
    textColor: "{colors.text}"
    typography: "{typography.label}"
    width: "410px"
    height: "42px"
  equipment-feedback:
    textColor: "{colors.loot-feedback}"
    typography: "{typography.label}"
    width: "410px"
    height: "44px"
  deployment-panel:
    backgroundColor: "{colors.deployment-surface}"
    textColor: "{colors.primary}"
    rounded: "{rounded.native}"
    width: "665px"
    height: "670px"
  deployment-option:
    backgroundColor: "{colors.deployment-option}"
    textColor: "{colors.primary}"
    rounded: "{rounded.native}"
  deployment-option-selected:
    backgroundColor: "{colors.deployment-selected}"
    textColor: "{colors.primary}"
    rounded: "{rounded.native}"
  terminal-surface:
    backgroundColor: "{colors.phosphor-glass}"
    textColor: "{colors.terminal-powered}"
    typography: "{typography.terminal}"
    rounded: "{rounded.native}"
    width: "320px"
    height: "180px"
---

# Design System: ABISAL — puerto unificado 0.7

## Overview

**Creative North Star: "Desembarco de cuarentena abandonado"**

ABISAL conserva el survival horror 3D isométrico y al soldado negro con tanque de oxígeno. La dirección humana toma Mouthwashing como referencia: escala adulta, industria gastada, superficies mate y luz práctica. El usuario confirmó PS1 marcado —baja resolución, dithering y textura inestable— y construcción directa en Unity con capturas reales. Después de dos revisiones rebuild eligió expresamente «Reconstruir interiores y soldado con más detalle»; este registro sustituye la documentación intermedia bajo esa autorización.

La reconstrucción final organiza mamparos, bancos y equipo como interiores contenidos; diferencia textil, rodilleras y botas; y presenta consolas CRT con teclado físico y estado de misión. [La revisión completa](docs/UI_REVIEW_INDUSTRIAL.md) emitió fix y [el veredicto posterior](docs/UI_VERDICT_INDUSTRIAL.md) cerró F1 y F2 como resolved, con disposition: ship exclusivamente para esa lista. No hubo una nueva auditoría integral ni aceptación estética humana. Las diecisiete capturas del 19 de octubre de 2026, 13:03 UTC, y el CSV de caída documentan el estado revisado; pruebas funcionales y capturas no cierran los KPI externos.

Integración 0.7 (12 de noviembre de 2026): cuatro supervivientes, seis armas de fuego, cuatro armas cuerpo a cuerpo y tres kits conviven en un nivel ampliado. Conserva la dirección industrial PS1; los cambios paralelos de personajes, combate, mapa y hordas se documentan como sistema actual. La [revisión unificada](docs/UI_REVIEW_UNIFIED.md) emitió fix U1/U2 y su [veredicto acotado](docs/UI_VERDICT_UNIFIED.md) los cerró como resolved, ship solo para esa lista. Cantidades, dotación e identificación del arma inicial están verificadas en código y captura. Los cierres anteriores siguen siendo históricos.

**Key Characteristics:**

- Mundo 3D de escala adulta, interiores industriales contenidos y cadáveres con caída física.
- Rig de soldado compartido, atlas por zonas y equipo volumétrico; cuatro perfiles cambian tintes y accesorios sobre esa base.
- Render PS1 marcado en el mundo; HUD operativo uGUI a resolución nativa.
- Crimson Text para ABISAL y Barlow Condensed para información operativa.
- Iluminación práctica cálida/fría y consolas diegéticas que muestran estado real.

Autoridad: [PRODUCT.md](PRODUCT.md), [contrato industrial](.impeccable/industrial-horror.md), [QUALITY BAR](.impeccable/quality-industrial.md), código y evidencia Unity Windows. No hay comp aprobado ni comp-diff. La semilla faceb9b7 está anotada sin recibo independiente; no se repite ni sustituye la dirección humana. El primer intento de revisor terminó por cuota y fue reemplazado, sin pass inferido. El historial permanece en [ROUND1](docs/UI_REVIEW_INDUSTRIAL_ROUND1.md) y [ROUND2](docs/UI_REVIEW_INDUSTRIAL_ROUND2.md).

## Colors

Hueso cálido, acero terroso, carbón y óxido con luz práctica localizada. Los porcentajes del frontmatter conservan los canales numéricos de Unity Color; los tintes de material se multiplican por textura y luz y no describen colores finales de captura.

### Primary

`primary`, hueso cálido, organiza misión, títulos, marca y barras. El nombre de variable heredado Green no significa verde menta. `safety-paint`, ámbar de precaución, es pintura del entorno.

### Secondary

`terminal-powered`, fósforo verde apagado, comunica red activa o acceso abierto. `terminal-offline`, ámbar de bloqueo, comunica ausencia de red. No extienden su saturación al HUD completo.

### Neutral

`ink`, carbón cálido, respalda ayuda, mochila y marcador; `hud-surface` y `mission-surface` son translúcidos, `menu-surface` opaco. `text` sostiene lectura principal y `muted` información secundaria. `button-surface` y `gauge-track` distinguen acciones y reserva vacía.

`aged-enamel`, `service-wall`, `dull-steel`, `concrete`, `service-deck` y los tres `interior-*` corresponden a albedos industriales. `soldier-cloth` tiñe el atlas táctico; `soldier-plates`, `soldier-knees`, `soldier-boots` y `tactical-seams` separan equipo. El tinte claro de bota se multiplica por cuero negro del atlas: no prescribe botas blancas. `tactical-plates` y `boot-leather` conservan usos de otros actores. `oxygen-tank` mantiene tanque mate. `crt-case`, `crt-bezel`, `keyboard-keys` y `phosphor-glass` distinguen carcasa, cristal y operación.

`deployment-surface` respalda el armario; `deployment-option` distingue opciones disponibles y `deployment-selected` marca una elección persistente en cada grupo. Son tintes base de Image; el ColorBlock de Unity conserva sus estados de interacción. No heredan el multiplicador explícito del menú izquierdo. `deployment-focus` identifica foco de navegación con Outline independiente del estado elegido.

`warning` acompaña HERIDA [B] o vida inferior a 30; `enemy-health` marca daño reciente. `loot-feedback` comunica éxito o rechazo de recogida. `noise-pulse` es un pulso breve de ruido; `oxide` representa corrosión material.

## Typography

**Display Font:** Crimson Text SemiBold. **Body Font:** Barlow Condensed Medium. Se cargan de `Assets/ThirdParty/Fonts/crimsontext/CrimsonText-SemiBold.ttf` y `Assets/ThirdParty/Fonts/barlowcondensed/BarlowCondensed-Medium.ttf`; cada carpeta conserva OFL.txt. LegacyRuntime.ttf es fallback técnico cuando falta asignación, no la identidad elegida.

El contraste es un título serif sobrio frente a lectura operativa condensada. Display identifica ABISAL; headline presenta menú/ayuda; title organiza misión y columnas; body sirve a botones y ayuda; label a equipo y feedback; ammo jerarquiza la cifra de munición y usa reserva en rich text de 23. Detalle de mochila usa 18–19 unidades y controles periféricos 15–16. Interaction identifica prompt E. No se inventan tracking o interlineado globales.

Las unidades px del intercambio representan unidades del Canvas de referencia. Terminal utiliza un Canvas WorldSpace de 320 × 180, escala 0.0036, texto 36 e interlineado 0.9: no son 36 píxeles finales de HUD. Señalética física TextMesh usa fontSize 64 con characterSize proporcional a la pieza.

El despliegue 0.7 reutiliza Barlow Condensed: título 24, encabezados y resumen 16, opciones 17 unidades Canvas. El resumen separa identidad, pasiva, dotación de salida, arsenal y kit. Se conserva espacio para beneficios y cantidades sin reducir el texto; la captura Unity corregida a 720p conserva las cinco líneas y footer sin recorte visible.

**The Native Readout Rule.** El HUD de pantalla permanece fuera del pass retro; el CRT pertenece al mundo y recibe su tratamiento PS1.

## Layout

ScreenSpaceOverlay, sortingOrder 100, ScaleWithScreenSize, referencia 1280 × 720 y matchWidthOrHeight 0.5. La evidencia cubre Windows 1280 × 720 y 1920 × 1080, ambos 16:9; no establece adaptación móvil o navegador.

Misión arriba a la izquierda en (26, -22); vitales abajo a la izquierda en (26, 150), pivote superior izquierdo; arma abajo a la derecha en (-224, 150). Vida mide 246 × 4 y suero 246 × 3. Prompt centrado abajo en y 119 y mensaje en y 72; son contenedores independientes. Marcador de objetivo solo a menos de 9 m, limitado a anclas x 0.13–0.87 / y 0.27–0.83.

Menú de 470 × 590 a x 46, centrado verticalmente, paso de botones 54. Ayuda centrada de 860 × 580. Mochila centrada con columnas x 26 y x 536; suministros/equipo a x 26/260 y filas y -294/-340/-386. Armas a y -480 con paso 151. Botín y feedback permanecen en la columna derecha, con helper debajo. La simulación continúa mientras ayuda o mochila bloquean combate; ESC pausa.

El armario 0.7 se ancla al centro derecho, margen 30, panel 665 × 670, sortingOrder 101 sobre HUD 100; referencia 1280 × 720 y match 0.5. Inset 22. Personajes: dos columnas y dos filas (302 × 38, paso 312/42, y 76). Armas: dos columnas y tres filas (302 × 38, y 194). Melee: dos columnas y dos filas (302 × 36, paso 312/40, y 352). Kits: tres columnas (198 × 40, paso 208, y 464). Resumen 620 × 112 en y 514; footer y 634. La tarjeta izquierda conserva inicio/continuar/ayuda. La composición densa se verifica en captura nativa; no se infiere adaptación a otras relaciones de aspecto.

Cámara ortográfica: tamaño inicial 2.85, zoom 2.6–11, pitch 32°, yaw 45° y framingOffset (1.15, 0, 1.15). Sigue al actor a altura base 0.9 y distancia 28. Los interiores agrupan banco, estantería y equipo contra mamparos, con cubierta soldada y luz de trabajo. InteriorCutaway oculta nearShell al entrar en los límites de cada cuarto mediante forceRenderingOff; no cambia colliders ni visión de IA.

**The Occupied Room Rule.** El corte del muro próximo cambia visibilidad, conserva colisiones y deja reconocibles los límites y la función del recinto.

## Elevation & Depth

Geometría 3D, materiales usados, oclusión, sombras y luz práctica construyen profundidad. El HUD es plano: rectángulos oscuros y jerarquía de texto sin sombra decorativa. El soldado combina atlas por zonas anatómicas, bolsillos/correas y protecciones volumétricas. F1 quedó resuelto al separar muslo, rodilla y bota en vistas jugables sin subir exposición global. Una luz puntual baja del actor aporta intensidad 0.55 y alcance1.8 m; la luz de inspección aporta 0.75 y 2.7 m.

RetroRenderFeature utiliza RenderGraph después de postprocesado: toma el color nativo, reduce a altura 270 y ancho proporcional, y reescala con Point. A 16:9 entrega 480 ×270; esto no acredita coste de geometría/shading a 270p. RetroScreen aplica Bayer 4 ×4 y 32 niveles por canal sRGB. RetroSurface ajusta vértices a rejilla fija 480 ×270 y desplaza UV con amplitud 0.0015 a 8 Hz. La rejilla de vértices no adapta su ancho con el framebuffer. El HUD overlay queda fuera.

Materiales propios: smoothness 0.035, metallic 0; tanque 0.08 y lentes 0.18 como excepciones. El adaptador de imports usa 0.06/0.03. Retro Surface implementa Simple Lit y specular negro por defecto; conservar parámetros heredados no certifica fidelidad PBR.

Acabado extraído: vignette 0.23/smoothness 0.62, grano 0.055, bloom 0.16/threshold 1.6, tonemapping Neutral, exposición 0.15, contraste 10, saturación -12. FinishLighting fija cielo (0.09,0.085,0.07), ecuador (0.04,0.045,0.035), suelo (0.025,0.024,0.021) y niebla 0.018. Luces cálidas de trabajo y control más frío organizan zonas. No se prescribe ocultar materia con negro.

F2 implementa lectura clara: desactiva pass retro, fuerza _AbisalRetroStrength a 0 y apaga grano/bloom; CameraTrauma consulta ReducedEffects para la sacudida. Código observado no equivale a prueba temporal completa de F2, snapping o UV. La secuencia de caída sí aporta evidencia temporal física limitada.

**The Local Matter Rule.** La separación del uniforme y del equipo se conserva con atlas, geometría y luz local, manteniendo negro e intensidad PS1 confirmados.

## Shapes

Paneles, botones y barras uGUI son rectángulos sin radio. El mundo conserva cuerpos adultos ponderados, cubierta con juntas, mamparos y maquinaria con volumen. El collider adulto mide 1.82 m, centro 0.91 m y radio 0.29 m. El CRT presenta caja posterior, bisel, soporte, teclas elevadas y superficie de operación: su función se reconoce incluso cuando los glifos pequeños reciben el tratamiento retro.

## Components

- **Menú:** comenzar/Enter, continuar/Esc o reintentar según estado; checkpoint deshabilitado cuando falta guardado. ColorBlock explícito multiplica RGB por 1.4 al resaltar/seleccionar y por (0.45,0.43,0.38) al deshabilitar; otros estados conservan Unity.
- **Vitales y arma:** vida, herida, suero/infección, O2/luz/resistencia; fuga o ruido alto solo cuando corresponde. Nombre de arma y cifra son textos separados.
- **Interacción y objetivo:** prompt E, distancia próxima y progreso de acción. Marcador textual; no reaparece glifo provisional o aro de percepción permanente. El pulso de ruido usa 65 puntos, ancho 0.045 m, radio representado 1–7 m y corte por colisiones; visible en Playing con NoiseRadius ≥2, sin revelar posiciones enemigas.
- **Ayuda F1 y mochila TAB:** controles, supervivencia y ruta; BlocksCombat impide combate sin pausar la simulación. Inventario 24 kg. Cambiar pila/O2/placa exige carga ≤70%, repuesto, sesión activa, sin acción ni bloqueo. NightHud conserva ColorBlock de Unity, distinto del menú.
- **Botín y recuperación:** E a menos de 2.5 m; capacidad y pila limitan recogida, sobras permanecen. El feedback vive dentro de la mochila sobre el helper. No se infiere interacción completa de una captura.
- **Infectado y cadáver:** barra 62 ×6 solo vivo, percibido, activo y dañado en últimos 3 s. Rig 19 huesos, 13 Rigidbody y 12 ConfigurableJoint. Diez ciclos automatizados de checkpoint comprueban pose, botín y recursos; formato de guardado v3 admite v1/v2. No son diez sesiones humanas.
- **Consola CRT:** WorldSpace integrado al mundo, texto y lámpara actualizados desde PowerOn/BoardingAuthorized. Estados B-07 / SIN RED / BLOQUEO, B-07 / RED OK / CONTROL y B-07 / ACCESO / ABIERTO. Carcasa usada, pantalla con estructura y teclado físico orientados a cámara resolvieron F2; no se promete lectura nativa de cada glifo.

### Sistema actual 0.7: despliegue unificado

Cuatro grupos de rectángulos planos conservan la paleta del armario. `[X]`/`[ ]` y los tintes `deployment-selected`/`deployment-option` identifican una elección persistente por grupo; Outline `deployment-focus`, effectDistance (2, 2), identifica foco independientemente. Unity ColorBlock mantiene el tinte de interacción.

Los personajes comparten rig y base de soldado; `SurvivorRoster` cambia materiales y accesorios procedurales. Kael «Vanguardia» / Asalto Tactico incorpora montaje NVG y antena, recarga de 1.15 s y +15% daño balístico; Viktor «Sombra» / Explorador usa capucha, ópticas rojas y vainas, con movilidad, menor ruido y recuperación al matar en melee; Dra. Vance «Bio-Control» / Sanitario usa capucha/visor bio y depósitos, con menor degradación de suero y curación mejorada; Elena «Bastion» / Ingeniera usa pantalla de soldadura, hombreras y mochila, con blindaje y capacidad mayores. Son apariencias del mismo esqueleto, no cuatro rigs originales independientes. Las pasivas cuantitativas se leen del perfil activo y no se convierten en tokens de estilo.

La dotación de salida distingue pasiva de suministro: Kael conserva base; Viktor +1 pila y +1 placa; Vance +1 botiquín y +1 suero; Elena +2 vendas. Los kits añaden una sola vez por salida nueva +1 placa de repuesto, +2 vendas o +24 cartuchos compartidos, en mochila y sujetos a límites. Continuar checkpoint conserva recursos, superviviente, arma, tipo de munición y usos/restantes temporales de sectores; no vuelve a conceder el kit.

El arma elegida es la equipada inicialmente: se llevan las seis, con reserva compartida y cambio 1–6 en partida. `ModelUpgradeBuilder` construye geometría procedural diferenciada para Pistola DP-9, Carabina C3, Escopeta S12, Fusil de precision, Subfusil y Ametralladora pesada; reemplaza las simples variantes escaladas del fusil de 0.6. Cuatro modelos melee: Cuchillo KM-9, Hacha de abordaje, Machete sierra y Mazo neumatico. La geometría y sus accesorios no constituyen arte premium final aprobado. El perro procedural y humanoides alterados mantienen esa misma limitación.

Los encabezados separan atajos en partida de la elección de menú: F3/F4 personaje, 1–6 arma, T munición FMJ/HP/AP-I, C melee, Espacio combo, G golpe pesado y clic derecho bloqueo/parry; Alt/X esquiva. La ayuda/HUD conserva controles y estados de combate. El resumen corregido muestra identidad, pasiva, dotación, arma/melee con reserva compartida y cantidad/destino del kit.

### Nivel integrado y evidencia actual

`PortMapExpander` agrega Sector 07 Helipuerto Sur y Laberinto de Contenedores, Sector 08 Ala Oeste de Bio-Cuarentena y Sector 09 Subestacion Electrica Norte. Reutiliza materia industrial, luces prácticas y señalética WorldSpace. Los prompts E comunican usos restantes de armería/bio/suministros y estado de recarga del pilón; sus recursos y temporizadores persisten. La construcción abre pasos y reconstruye NavMesh en runtime. La comprobación funcional reportada conecta siete puntos muestreados; no certifica todas las trayectorias o calidad de encuadre. HordeDirector integra presión/oleadas y reservas en sectores ampliados, con límite activo de 14.

La [revisión unificada](docs/UI_REVIEW_UNIFIED.md) identificó U1 (cantidades/dotación) y U2 (arma inicial, seis disponibles, reserva compartida y teclas en partida). El [veredicto unificado](docs/UI_VERDICT_UNIFIED.md) cierra ambos como resolved, ship exclusivo para U1/U2. La recaptura nativa 1280 × 720 muestra Viktor, Arma pesada, Cuchillo KM-9 y Reserva de municion elegidos, con foco separado en Arma corta. Se verificaron visualmente marcadores, dotación +1 pila/+1 placa, kit +24 cartuchos compartidos, pasiva y footer; las demás descripciones se contrastaron con código. La [captura canónica](docs/Validation/Unified/arsenal-menu.png) es la entrega visual de este estado.

El equipo reporta 27 PlayMode completas aprobadas sobre gameplay unificado y una prueba de integración filtrada aprobada después del ajuste de copy/recaptura, que incluye save/load de personaje, AP-I, usos de estación y conexión de siete puntos de sectores. También aprobaron 18 EditMode; resultados en [tests.json](docs/Validation/Unified/tests.json). Build 0.7 de fuente `76379d3` en curso al documentar. Pruebas y veredicto visual son evidencias separadas: no se emite aprobación integral del juego ni certificación de modelos, combate, audio, animación, FPS, accesibilidad, recorrido manual completo por teclado u otras resoluciones; aceptación estética humana pendiente.

Historial 0.6: [veredicto del arsenal](docs/UI_VERDICT_ARSENAL.md), [captura](docs/Validation/Arsenal/arsenal-menu.png), [tests](docs/Validation/Arsenal/tests.json) y build Windows documentan aquella entrega de tres especialidades, seis armas y tres kits: F1/F2 resueltos con ship solo para su lista; 27 PlayMode y 18 EditMode aprobadas. La imagen transitoria sin texto por LFS se excluyó. Esta evidencia no verifica las regresiones U1/U2 del menú integrado.

Evidencia final del lote industrial anterior: doce capturas principales en `docs/Validation/IndustrialPort/` —industrial-arrival, industrial-soldier, industrial-warehouse, industrial-control, industrial-ragdoll, industrial-ragdoll-1080, first-level-menu, first-level-help, night-inventory, night-inventory-1080, night-inventory-feedback y night-infected— más industrial-fall-00..04 y [CSV](docs/Validation/IndustrialPort/industrial-fall-sequence.csv). Archivos finales de 13:03 UTC. El CSV actual registra 13 cuerpos dinámicos, pelvis 0.9056→0.1754 m entre 0.167 y 2.797 s; no reutilizar las cifras de capturas previas. Acredita una caída, no todas las variantes/superficies/colisiones.

26 PlayMode y 18 EditMode aprobadas acompañan el lote funcional. Faltan aceptación estética humana, cinco sesiones externas y perfilado válido para los KPI pendientes. No hay medición válida de FPS, ni certificación de confort, accesibilidad, ataque o temporalidad UV/F2 por esta revisión visual. La escena de las capturas procede de `Library/ZombtoyValidationClean`; la sincronización de Assets y la identidad del ejecutable se registran en [la evidencia de entrega](docs/Validation/IndustrialPort/README.md), separadas del verdict visual. No aplica detector HTML/CSS ni reglas de plataforma iOS/Android a Unity Windows. Los snippets HTML del sidecar ilustran uGUI y CRT, no ejecutan juego ni son evidencia de render nativo.

Los cuatro albedos originales —AgedEnamel_v1, InfectedFace_v1, WornCloth_v1 y TacticalAtlas_v2— conservan PNG fuente y `.provenance.json` en `Assets/_Project/Art/Industrial/Textures/`, con generador, propósito y prompt. Importación Point con mipmaps: esmalte 256/Repeat, cara 128/Clamp, tejido 128/Repeat y atlas 256/Clamp. Se conservan fuentes licenciadas locales de mobiliario. [Mouthwashing oficial local](.impeccable/review/references/mouthwashing-official-interior.jpg) y su procedencia son referencia crítica de Steam/Wrong Organ, solo QA; no se incorporan como arte del juego.

## Do's and Don'ts

### Do:

- **Do** conservar 3D isométrico, escala adulta, base de soldado con tanque y PS1 marcado; distinguir los cuatro perfiles integrados mediante tintes/accesorios legibles.
- **Do** mantener el HUD fuera del pass retro y mostrar teclas junto a acciones y estados.
- **Do** conservar lectura local de muslo, rodilla y bota, equipo volumétrico y cuartos organizados contra mamparos.
- **Do** alimentar la pantalla CRT desde PowerOn y BoardingAuthorized, con superficie de operación y teclado visibles.
- **Do** conservar feedback de botín y la condición de repuestos ≤70% junto a suministros.
- **Do** juzgar futuras variaciones en capturas Unity de la cámara jugable a 720p y 1080p.

### Don't:

- **Don't** presentar ship de F1/F2 como aprobación estética humana, auditoría integral o cierre de KPI no medidos.
- **Don't** convertir el negro que borra materia o los interiores como plataforma en mandatos de diseño.
- **Don't** reducir el PS1 confirmado para disimular deficiencias de materiales o encuadre.
- **Don't** importar personajes, logos o imágenes de Mouthwashing ni restablecer el mundo 0.4 rechazado.
- **Don't** afirmar que geometría e iluminación se calculan a 270p: la reducción ocurre después del postprocesado nativo.
- **Don't** inferir verificación temporal de UV, snapping, F2 o confort a partir de capturas estáticas.
