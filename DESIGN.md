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
typography:
  display:
    fontSize: "52px"
  title:
    fontSize: "27px"
  body:
    fontSize: "20px"
  label:
    fontSize: "16px"
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
---

# Design System: ABISAL — primer puerto jugable

## Overview

Registro descriptivo del HUD de Unity uGUI implementado en `Assets/_Project/Runtime/Gameplay/GameHud.cs`, con presentación del mundo en `IsometricView.cs`. Conserva el lenguaje clínico oscuro, verde y rojo de la interfaz existente. No constituye una dirección creativa nueva ni aprobada por entrevista; no existe un PRODUCT.md de referencia.

**Key Characteristics:**
- Mundo de baja resolución y HUD independiente para lectura.
- Paneles oscuros rectangulares, texto blanco/gris y foco visual verde menta.
- Avisos de hemorragia y salud enemiga en rojo, acompañados de texto o barras.

## Colors

### Primary
- **Menta:** títulos, interacción, objetivo y relleno de vida/suero.

### Secondary
- **Advertencia:** vida inferior a 30 o hemorragia; la etiqueta incluye HEMORRAGIA y la tecla B.
- **Salud enemiga:** relleno de las barras de infectados.

### Neutral
- **Tinta clínica:** respaldo de paneles y marcador sobre el mundo.
- **Blanco:** información principal y botones. **Gris verdoso:** información auxiliar.
- **Pista de indicador:** fondo de barras. **Verde de botón:** superficie interactiva.

## Typography

La implementación usa `LegacyRuntime.ttf` de Unity, sin familia externa. Es una dependencia provisional del prototipo, no una elección de marca para futuras superficies. Los tamaños del frontmatter representan unidades de referencia del Canvas, expresadas como px para intercambio; no son medidas del mundo.

Jerarquía observada: ABISAL (display), encabezado del menú (title), ayuda y botones (body), marcador/estadísticas (label). Existen tamaños de apoyo de 14–24 y munición de 32. Los textos permiten salto de línea y recortan el desbordamiento vertical: cualquier ampliación de contenido requiere verificar el render.

## Layout

Canvas HUD superpuesto, orden 100, referencia 1280 × 720, `ScaleWithScreenSize` y coincidencia ancho/alto 0.5. El mundo usa RenderTexture 480 × 270 con filtro Point y Canvas de orden −100. Mantener ambas capas independientes.

Objetivo en franja superior de 86 unidades; signos vitales abajo a la izquierda (310 × 126); munición y recursos abajo a la derecha (322 × 126). Interacciones, tratamiento y mensajes ocupan el centro inferior. Menú central de 900 × 542; ayuda de 860 × 580; equipo de 650 × 250. No hay breakpoints específicos: la única evidencia visual de esta revisión es 1280 × 720.

El marcador de objetivo sigue la proyección de cámara con anclajes limitados a x 0.13–0.87 e y 0.27–0.83. Su respaldo oscuro mantiene la etiqueta legible sobre el puerto.

## Elevation & Depth

La separación usa superficies oscuras y velos translúcidos, sin sombras decorativas. El menú oscurece el mundo; la ayuda añade un velo negro de mayor opacidad. Las barras flotantes pertenecen al HUD y siguen posiciones del mundo.

## Shapes

Paneles, botones y barras rectangulares, sin esquinas redondeadas ni contornos ornamentales. Las barras horizontales se rellenan de izquierda a derecha.

## Components

- **Menú:** comenzar por botón o Enter; continuar durante pausa; reintentar al morir o terminar. El checkpoint queda deshabilitado cuando no existe guardado. Incluye ayuda y salida.
- **Botón:** fondo verde oscuro y etiqueta blanca. El ColorBlock de Unity multiplica RGB por 1.4 al resaltar/seleccionar y aplica (0.45, 0.5, 0.48) al deshabilitar; el motor conserva su transición nativa.
- **Signos vitales:** valor y barra de vida; suero e infección. La hemorragia cambia el texto a advertencia e indica B.
- **Munición y recursos:** cargador, reserva, recarga R y cantidades de botiquines, vendas y dosis.
- **Objetivo:** instrucción superior y distancia flotante sobre respaldo oscuro. El glifo actual es deuda del prototipo, no un sistema de iconos.
- **Ayuda F1:** controles y ruta ALMACÉN 03 → GENERADOR → CONTROL → SUBMARINO. Ayuda y equipo bloquean controles de combate mediante `BlocksCombat`; no equivalen a pausar la simulación.
- **Equipo TAB:** resumen textual de cantidades, munición y repuesto; no es un inventario de cuadrícula y avisa que no pausa.
- **Salud enemiga:** barra de 62 × 6; aparece en infectados vivos, activos, alertados o heridos y dentro de los límites de pantalla definidos en código.

## Do's and Don'ts

### Do:
- **Do** mantener el HUD separado del render de baja resolución.
- **Do** conservar el respaldo oscuro del marcador y verificar su contraste sobre el mundo.
- **Do** mostrar etiquetas y teclas junto a estados y cantidades.

### Don't:
- **Don't** presentar este registro como identidad nueva aprobada o como norma para todos los niveles.
- **Don't** convertir la fuente provisional o el glifo del marcador en un sistema de marca.
- **Don't** afirmar adaptación a otras resoluciones o accesibilidad certificada con estas tres capturas.
