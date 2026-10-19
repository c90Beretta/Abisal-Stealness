---
name: "ABISAL — primer puerto jugable"
description: "Registro del HUD nativo existente; alcance limitado al primer nivel de Windows."
colors:
  clinical-ink: "rgba(2.5%, 5.5%, 5.5%, 0.96)"
  mint: "rgb(55%, 94%, 72%)"
  muted: "rgb(67%, 77%, 74%)"
  text: "#ffffff"
  warning: "rgb(100%, 55%, 46%)"
  enemy-health: "rgb(90%, 36%, 27%)"
  gauge-track: "rgb(15%, 25%, 23%)"
  button: "rgb(14%, 28%, 23%)"
  equipment-feedback: "rgb(100%, 72%, 55%)"
  perception-ring: "rgba(25%, 70%, 65%, 0.28)"
  noise-pulse: "rgba(90%, 56%, 28%, 0.55)"
typography:
  display:
    fontSize: "52px"
  title:
    fontSize: "27px"
  body:
    fontSize: "20px"
  label:
    fontSize: "16px"
  night-status:
    fontSize: "15px"
  equipment-heading:
    fontSize: "22px"
  equipment-body:
    fontSize: "18px"
  equipment-action:
    fontSize: "17px"
  loot-body:
    fontSize: "19px"
rounded:
  square: "0px"
components:
  button:
    backgroundColor: "{colors.button}"
    textColor: "{colors.text}"
    rounded: "{rounded.square}"
    width: "382px"
    height: "48px"
  panel:
    backgroundColor: "{colors.clinical-ink}"
    rounded: "{rounded.square}"
  objective-marker:
    backgroundColor: "{colors.clinical-ink}"
    textColor: "{colors.mint}"
    width: "200px"
    height: "30px"
  vitals:
    backgroundColor: "{colors.clinical-ink}"
    rounded: "{rounded.square}"
    width: "310px"
    height: "190px"
  equipment:
    backgroundColor: "{colors.clinical-ink}"
    rounded: "{rounded.square}"
    width: "1000px"
    height: "550px"
  equipment-action:
    backgroundColor: "{colors.button}"
    textColor: "{colors.text}"
    typography: "{typography.equipment-action}"
    rounded: "{rounded.square}"
    width: "224px"
    height: "38px"
  equipment-weapon:
    backgroundColor: "{colors.button}"
    textColor: "{colors.text}"
    typography: "{typography.equipment-action}"
    rounded: "{rounded.square}"
    width: "143px"
    height: "38px"
  loot-take:
    backgroundColor: "{colors.button}"
    textColor: "{colors.text}"
    typography: "{typography.equipment-action}"
    rounded: "{rounded.square}"
    width: "410px"
    height: "42px"
  equipment-feedback:
    textColor: "{colors.equipment-feedback}"
    typography: "{typography.equipment-action}"
    width: "410px"
    height: "44px"
---

# Design System: ABISAL — primer puerto jugable

## Overview

Registro descriptivo del HUD de Unity uGUI implementado en `Assets/_Project/Runtime/Gameplay/GameHud.cs` y `NightHud.cs`, con presentación del mundo en `IsometricView.cs` y percepción nocturna en `NightSurvival.cs`. Conserva el lenguaje clínico oscuro, verde y rojo de la interfaz existente. No constituye una dirección creativa nueva ni aprobada por entrevista; no existe un PRODUCT.md de referencia.

La extensión nocturna conserva esta identidad: mochila y botín en dos columnas, supervivencia legible en el HUD y mundo oscuro con linterna y círculo próximo. La autoridad es el contrato `.impeccable/night-equipment.md`, el código y las capturas del runtime Windows; las previews HTML del sidecar solo ilustran componentes nativos.

**Key Characteristics:**
- Mundo3D a resolución nativa y HUD independiente para lectura.
- Paneles oscuros rectangulares, texto blanco/gris y foco visual verde menta.
- Avisos de hemorragia y salud enemiga en rojo, acompañados de texto o barras.
- Supervivencia nocturna con resistencia, O2, luz, ruido y hordas; mochila operable mientras el mundo avanza.

## Colors

### Primary
- **Menta:** títulos, interacción, objetivo y relleno de vida/suero.

### Secondary
- **Advertencia:** vida inferior a 30 o hemorragia; la etiqueta incluye HEMORRAGIA y la tecla B.
- **Salud enemiga:** relleno de las barras de infectados.
- **Feedback de equipo:** resultado o motivo de rechazo visible dentro de la mochila, bajo recoger.
- **Percepción y ruido:** círculo verde azulado y pulso naranja translúcidos en el mundo; sus colores no sustituyen las etiquetas del HUD.

### Neutral
- **Tinta clínica:** respaldo de paneles y marcador sobre el mundo.
- **Blanco:** información principal y botones. **Gris verdoso:** información auxiliar.
- **Pista de indicador:** fondo de barras. **Verde de botón:** superficie interactiva.

## Typography

La implementación usa `LegacyRuntime.ttf` de Unity, sin familia externa. Es una dependencia provisional del prototipo, no una elección de marca para futuras superficies. Los tamaños del frontmatter representan unidades de referencia del Canvas, expresadas como px para intercambio; no son medidas del mundo.

Jerarquía observada: ABISAL (display), encabezado del menú (title), ayuda y botones (body), marcador/estadísticas (label). Existen tamaños de apoyo de 14–24 y munición de 32. Los textos permiten salto de línea y recortan el desbordamiento vertical: cualquier ampliación de contenido requiere verificar el render.

La extensión registra roles propios de estado nocturno, encabezado de columna, contenido de mochila, acciones/feedback y contenido de botín en el frontmatter. NightHud usa la misma fuente provisional; no introduce una nueva familia ni un sistema de iconos.

## Layout

Canvas HUD superpuesto, orden100, referencia1280×720, `ScaleWithScreenSize` y coincidencia ancho/alto0.5. Desde v0.3 el mundo3D renderiza directamente a la ventana con cámara ortográfica35.264°/45°, MSAA4 y zoom. Se retiraron el RenderTexture480×270 y el Canvas del mundo, por petición del usuario. El HUD permanece separado.

Objetivo en franja superior de86 unidades; signos vitales abajo a la izquierda con geometría `components.vitals`, ampliada para los estados nocturnos; arma, munición y recursos abajo a la derecha (322×154). Interacciones, tratamiento y mensajes ocupan el centro inferior. Menú central de900×542; ayuda de860×580. No hay breakpoints específicos; se conserva la evidencia anterior de `docs/Validation/Port3D`.

El panel central de equipo usa `components.equipment`: mochila, recursos y equipo a la izquierda; botín cercano y recogida a la derecha. Las acciones de suministro/equipo, selección de arma y recogida tienen variantes de geometría separadas en el frontmatter. El feedback tiene su propio rectángulo bajo recoger, antes de la ayuda contextual; no depende del mensaje de combate cubierto por la tarjeta. Las posiciones nativas y la capacidad de24kg se registran en las extensiones del sidecar. La tarjeta puede cubrir parcialmente el HUD de fondo.

La evidencia nocturna `docs/Validation/NightPort` incluye HUD, mochila, feedback, ayuda y vistas próximas a1280×720, además de mochila a1920×1080. El Canvas escala la misma composición; no se ha implementado un cambio de columnas por resolución.

El marcador de objetivo sigue la proyección de cámara con anclajes limitados a x 0.13–0.87 e y 0.27–0.83. Su respaldo oscuro mantiene la etiqueta legible sobre el puerto.

## Elevation & Depth

La separación usa superficies oscuras y velos translúcidos, sin sombras decorativas. El menú oscurece el mundo; la ayuda añade un velo negro de mayor opacidad. Las barras flotantes pertenecen al HUD y siguen posiciones del mundo.

El puerto nocturno conserva el mundo3D: linterna con sombras, luz próxima, círculo de percepción recortado por obstáculos y pulso de ruido. La oscuridad permite reconocer al soldado negro con tanque en las capturas; las vistas próximas complementan la lectura normal y no prueban lectura en movimiento. El pulso representa ruido reciente, no un minimapa de enemigos.

## Shapes

Paneles, botones y barras rectangulares, sin esquinas redondeadas ni contornos ornamentales. Las barras horizontales se rellenan de izquierda a derecha.

## Components

- **Menú:** comenzar por botón o Enter; continuar durante pausa; reintentar al morir o terminar. El checkpoint queda deshabilitado cuando no existe guardado. Incluye ayuda y salida.
- **Botón:** fondo verde oscuro y etiqueta blanca. El ColorBlock de Unity multiplica RGB por 1.4 al resaltar/seleccionar y aplica (0.45, 0.5, 0.48) al deshabilitar; el motor conserva su transición nativa.
- **Signos vitales:** valor y barra de vida; suero e infección. La hemorragia cambia el texto a advertencia e indica B. El bloque nocturno añade resistencia, O2, estado/carga de luz F, ruido en metros y hordas/reserva; en fuga tóxica la última línea informa máscara/fuga.
- **Munición y recursos:** arma equipada con teclas1/2/3, cargador, reserva, recarga R y cantidades de botiquines, vendas y dosis. El título del arma y las cifras ocupan textos separados para evitar recortes de línea.
- **Objetivo:** instrucción superior y distancia flotante sobre respaldo oscuro. El glifo actual es deuda del prototipo, no un sistema de iconos.
- **Ayuda F1:** controles y ruta ALMACÉN 03 → GENERADOR → CONTROL → SUBMARINO. Ayuda y equipo bloquean controles de combate mediante `BlocksCombat`; no equivalen a pausar la simulación.
- **Equipo TAB:** mochila de24kg en dos columnas de texto y acciones; no es un inventario de cuadrícula. Muestra carga, cantidades, protección, máscara, O2, linterna y arma cuerpo a cuerpo. Permite cambiar suministros, equipo y tres armas. El mundo sigue avanzando y ESC pausa. El resumen textual anterior queda como fallback cuando no existe NightSurvival.
- **Suministros:** las tres acciones se deshabilitan si la carga correspondiente supera70%, falta repuesto, la sesión no está activa, hay acción en curso o el inventario está bloqueado. Con carga≤70% y repuesto disponible pueden usarse. NightHud conserva el ColorBlock predeterminado de Unity; el multiplicador1.4 del botón de menú no se aplica a esta variante.
- **Botín cercano:** E abre cuerpos y cajas al alcance. Recoger respeta peso y límites de pila; las sobras permanecen. Su botón se deshabilita sin contenido accesible, durante acción o bloqueo. La etiqueta de feedback dentro de la tarjeta muestra el resultado o motivo de rechazo, incluidos mochila/pila llena. La regla≤70% permanece en la ayuda de la columna derecha.
- **Salud enemiga:** barra de 62 × 6; aparece en infectados vivos, activos, alertados o heridos y dentro de los límites de pantalla definidos en código.

El cierre de `docs/UI_REVIEW_NIGHT.md` es **ship — R1 resolved**, limitado al estado de suministros y feedback visible de la corrección. No es una nueva auditoría general ni certifica todas las acciones, balance, audio, persistencia, navegación o rendimiento. No se aplica detector HTML/CSS al runtime uGUI nativo Windows.

## Do's and Don'ts

### Do:
- **Do** mantener el HUD separado del render3D nativo.
- **Do** conservar el respaldo oscuro del marcador y verificar su contraste sobre el mundo.
- **Do** mostrar etiquetas y teclas junto a estados y cantidades.
- **Do** mantener el resultado de recoger visible en la columna de botín y comunicar la condición≤70% junto a los suministros.

### Don't:
- **Don't** presentar este registro como identidad nueva aprobada o como norma para todos los niveles.
- **Don't** convertir la fuente provisional o el glifo del marcador en un sistema de marca.
- **Don't** afirmar adaptación a otras resoluciones o accesibilidad certificada con estas tres capturas.
- **Don't** ampliar el cierre acotado de R1 a una certificación funcional o visual integral del puerto nocturno.
