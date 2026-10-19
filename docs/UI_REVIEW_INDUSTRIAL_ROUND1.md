disposition: rebuild

Inputs no disponibles: imagen local de la referencia oficial y comprobante de la tirada faceb9b7; no se inspeccionó una referencia web porque este revisor trabaja con archivos. No existe comp aprobado ni corresponde exigir comp-diff. Revisión fresca que reemplaza la revisión completa interrumpida; no es un verdict pass.

## persistence

fail. PRODUCT.md, contrato y QUALITY BAR existen. DESIGN.md todavía registra el mundo rechazado: mint, clinical-ink, percepción turquesa y dimensiones anteriores. PRODUCT.md aún presenta el tratamiento retro como decisión abierta pese a la confirmación humana que sí registra el contrato. La semilla faceb9b7 está declarada, pero no llegó evidencia independiente de su generación. La referencia humana prevalece sobre la tirada.

Evidencia válida: se abrieron las doce capturas requeridas y los cinco cuadros industrial-fall-00..04, además del CSV. Hay imágenes de 1280x720 y 1920x1080; el cadáver está separado del jugador y la transición de pie a suelo es visible. La oscuridad es contenido renderizado y se evalúa como defecto del resultado, no como una captura vacía. La composición técnica mundo URP + HUD vivo se acepta con el alcance informado; no demuestra por sí misma equivalencia completa con la presentación temporal del ejecutable. Las 26 pruebas PlayMode comunicadas no certifican estética ni rendimiento.

## fidelity

| Elemento / promesa | Estado | Evidencia visible |
| --- | --- | --- |
| TYPE: tipografía sobria y jerarquía utilitaria | match | Objetivo, vitales, munición y acciones usan una voz condensada consistente; ABISAL tiene título serif. Sin comp aprobado no existe obligación de reproducir la tipografía de Mouthwashing. El texto operativo se puede leer a ambos tamaños. |
| MATERIAL: soldado negro adulto con tanque | contradicted | industrial-soldier y arrival muestran tanque claro y cabeza, pero torso, cadera y piernas se funden en una columna negra. La textura nueva no comunica tela, correas, articulaciones y cuero a la escala final. El material del foco protagonista sigue sin estar resuelto. |
| MATERIAL: interior industrial naval gastado | contradicted | Hay superficies usadas reconocibles en caja, baldosas y algunos paneles. Sin embargo, arrival y warehouse siguen leyendo como plataformas con paneles separados; en control hay aparatos muy oscuros sobre una extensión de baldosa. No se percibe de forma consistente un interior ocupado, contenido y conectado. |
| GROUND: hueso sucio, acero mate, carbón y óxido | adaptation | La temperatura terrosa y el gris del suelo responden al OWN-WORLD. El exterior azul puede responder al puerto y la vista isométrica conservados por el usuario; su exposición dominante como vacío alrededor de cada plataforma contradice la intención de inmersión y se trata en la reconstrucción espacial. |
| 3D isométrico, escala adulta y tanque | match | Se conservan profundidad y perspectiva isométrica. Cabezas y cuerpos no son chibi; el tanque identifica al protagonista. No hay sustitución por sprites. |
| FIRST VIEWPORT: esclusa industrial con luz y ruta legibles | contradicted | La luz revela suelo y caja, pero la pared frontal ocupa gran parte de la escena, se ve el perímetro del tablero y la silueta negra pierde información. Predomina el montaje expuesto sobre la sensación de entrar en una instalación. |
| PS1 marcado | match | Resolución baja, bordes escalonados y dithering son evidentes. Su intensidad es una elección humana vinculante y no es un defecto. La inestabilidad UV y vertex snapping temporales no se pueden certificar con imágenes estáticas. |
| Controles y HUD discreto | adaptation | La posición de objetivo, vitales y munición cumple el contrato y conserva teclado/ratón nativos. Ayuda e inventario usan interrupción y foco justificados por la operación. El aro amarillo segmentado sobre el mundo vuelve a introducir una lectura de depuración. |
| STORY: avance almacén/control/submarino | contradicted | Control indica objetivo 04, pero mantiene abajo «WASD para moverte. Busca el repuesto del almacén». Esa instrucción ya no corresponde al estado. En night-infected persiste además un mensaje de repuestos que no explica el peligro mostrado. |
| Amenaza humana legible | contradicted | En night-infected los cuerpos se confunden con postes, paneles y sombra. La imagen no permite reconocer rápidamente la amenaza ni su preparación de ataque. Mantener terror no exige borrar la silueta. |
| Caída física y cadáver registrable | match | fall-00..04 muestra descenso, giro y asentamiento. El CSV registra 13 cuerpos dinámicos y pelvis de 0.9051 a 0.1488 m. Ragdoll 720/1080 muestra el cuerpo en el suelo sin quedar tapado por el jugador. No demuestra calidad en todas las colisiones o variantes. |
| Inventario, capacidad y recuperación | match | Ambas resoluciones mantienen columnas y acciones completas. El estado de capacidad muestra «Mochila o pila llena. Usa suministros para hacer espacio». Las sobras y la continuidad temporal se explican. |

## ceiling

No alcanzado. Dispositivos examinados: Windows 720p y 1080p; no corresponden reglas iOS/Android ni detector HTML/CSS. Quedan sin resolver recursos propios del mundo elegido: continuidad de mamparos y umbrales, densidad funcional del interior, iluminación práctica que separe materiales negros, siluetas humanas y señalética integrada en la arquitectura. El pixelado fuerte está presente; no sustituye esos recursos. Movimiento de UV, oscilación de vértices, animación de ataque, F2 y FPS quedan fuera de lo demostrable con estas capturas. La caída sí tiene evidencia secuencial.

## material_fixes

1. Reconstruir las regiones focales soldado/esclusa/almacén/control: producir un conjunto 3D conectado de mamparos, marcos, umbrales y equipos con materiales gastados, y una presentación del soldado con tela, chaleco, guantes, rodillas y botas distinguibles a 480x270; reutilizar los assets licenciados donde sirvan y producir atlas raster originales de ropa/desgaste donde falten. La salida debe leerse como instalación naval y cuerpo vestido, manteniendo cámara isométrica, negro del uniforme, tanque y PS1 fuerte. MATERIAL contradicho en el foco activa rebuild.
2. Recomponer luces prácticas y valores sobre esas regiones para recuperar costuras, articulaciones, equipo y siluetas sin levantar toda la escena; demostrarlo en las mismas tomas arrival, soldier, warehouse, control e infected. No resolver con quitar dithering o subir resolución: el usuario confirmó ambos.
3. Sustituir o retirar el aro segmentado permanente del suelo; si representa una mecánica necesaria, limitar su aparición al momento de información accionable y documentar su propósito. El contrato rechaza el aspecto de depuración.
4. Vincular el mensaje inferior al progreso y al evento vigente; eliminar la instrucción inicial cuando ya se alcanzó control y evitar mensajes de inventario caducados durante el encuentro. STORY y estados deben comunicar la acción actual.
5. Tras la reconstrucción, recapturar los mismos archivos y someter el conjunto a revisión completa; conservar la secuencia de caída y añadir evidencia temporal real si se pretende declarar comprobados UV inestable, vertex snapping, ataque o modo F2. No presentar pruebas funcionales como aprobación visual o de FPS.
6. Actualizar PRODUCT.md para registrar la confirmación de esta sesión y documentar el mundo final en DESIGN.md después de la revisión; adjuntar comprobante existente de la semilla o reconocer que no se conserva. No volver a sortear ni hacer de la tirada una condición que sustituya el brief humano.

## keep

Mantener 3D isométrico, proporción adulta, soldado negro con tanque, PS1 marcado confirmado, HUD operativo legible y caída física visible; no declarar aprobación estética humana ni KPI de rendimiento a partir de esta revisión.
