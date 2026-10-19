# Puerto industrial / rediseño 0.5

## Scope

Modo Experience en el mundo3D y Operate en su HUD. Referencia humana vinculante:
Mouthwashing. Cámara isométrica y soldado negro con tanque se conservan.
El usuario rechazó el diseño0.4 y delegó trabajo continuado durante hasta5h.
Respuesta explícita del usuario: PS1 marcado, baja resolución, dithering y textura
inestable; construir en Unity y revisar capturas reales del juego. Esta
sesión implementa ese tratamiento en el mundo3D y conserva el HUD legible.
No se guarda esta elección como preferencia permanente ni como aprobación visual.

## Direction contract

THESIS: un desembarco de cuarentena abandonado: cuerpos humanos y materia
industrial gastada, con oscuridad interrumpida por focos y fluorescentes. Evitar
el diorama de figuras cabezonas sobre un tablero y el panel HUD de depuración.
OWN-WORLD: esmalte hueso sucio, acero gris mate, carbón textil y óxido oscuro;
ámbar de precaución, blanco legible y rojo solo para peligro. Tipografía sobria,
señalética física y texturas auténticas sobre geometría3D, sin neón plástico.
STORY: desembarcar, entender la cuarentena, encontrar recursos, oír a los
infectados y atravesar almacén/generador/control hasta el submarino.
FIRST VIEWPORT: soldado de escala adulta dentro de una esclusa industrial
abierta por arriba. Paredes/paneles y tuberías definen el espacio. Una luz cálida
corta la penumbra; la linterna dirige la mirada. Objetivo discreto arriba a la
izquierda, vitales compactos abajo y munición aislada a la derecha.
FORM: dirección fijada por la referencia del usuario; semilla faceb9b7, índice5.
La restricción humana gana a la tirada. Siete anclajes considerados: cuarentena,
sala de máquinas, terminal de carga, plano de evacuación, puesto de guardia
nocturno, archivo de incidentes y mantenimiento de casco. El quinto aporta la
lectura compacta y sobria del HUD; no se trasladan los juguetes/PC98/cerámica/
neón/zine del catálogo. Instrumentos nocturnos aporta jerarquía por urgencia,
no decoración de seis relojes. Se trabaja con código y capturas reales, sin comp
raster aprobado ni presentación de una ilustración como resultado ejecutable.
FINISH: unreviewed and undocumented is unfinished; this build ends with the finish review, the verdict, DESIGN.md, and every shipping raster carrying its provenance

## Evidence gate

Comparar con0.4: proporción humana, materiales mate/gastados, escena cerrada y
puntos de luz claros, HUD más pequeño, caída física de cadáveres. Windows720p y
1080p; capturas de entrada, almacén/control, combate, ragdoll, ayuda e inventario.
Dos rondas propias y revisión fresca posterior. KPI ajenos a lo medido quedan
abiertos; la similitud estética requiere valoración humana, no se autocertifica.

## Respuesta humana y reconstrucción

El acabado PS1 marcado fue confirmado el19octubre2026. La revisión independiente
validó la evidencia de caída física tras recaptura, pero señaló fidelidad insuficiente:
formas de botas/manos en bloque, ropa plana, habitaciones que parecen paneles sobre
un tablero, negro que borra materia y monitores verdes vacíos. Se reconstruyen
esas regiones, además de integrar baja resolución, dithering y oscilación de UV.
La revisión completa se repetirá sobre la reconstrucción; no hay verdict ship.


## Reconstrucción adicional autorizada

Tras ver la captura del almacén y ambas revisiones rebuild, el usuario eligió
explícitamente «Reconstruir interiores y soldado con más detalle» (19oct2026).
Se conserva isometría, soldado negro con tanque y PS1 marcado. Se rehacen las
regiones focales: atlas táctico por zonas anatómicas, volumen de bolsillos y
protecciones, mamparos interiores visibles, bancos y equipos agrupados, corte
completo de los muros próximos al ocupar una habitación, luces locales y
encuadre que muestre la parte posterior del recinto. La evidencia vuelve a
ser la escena Unity y la matriz completa, incluida caída y UI; sin comp aprobado.
