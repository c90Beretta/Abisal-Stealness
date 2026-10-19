disposition: fix

Alcance: revisión completa fresca de la reconstrucción adicional elegida por el usuario, con evidencia Unity Windows del 19 de octubre de 2026, 12:53–12:54 UTC. No es un verdict pass ni aprobación estética humana. Se conservan UI_REVIEW_INDUSTRIAL_ROUND1.md y UI_REVIEW_INDUSTRIAL_ROUND2.md. La referencia es Mouthwashing reinterpretado en 3D isométrico, con soldado negro y tanque y PS1 marcado confirmados; no se exige primera persona, imagen limpia ni una copia de su composición.

## persistence

pass para la revisión; FINISH todavía abierto. PRODUCT.md, contrato industrial y QUALITY BAR registran la dirección y la autorización humana de reconstruir interiores y soldado con más detalle. La semilla faceb9b7 está declarada sin comprobante independiente: no se vuelve a sortear y no sustituye el brief humano. No hay comp aprobado; no corresponde comp-diff. Se inspeccionó la imagen oficial local mouthwashing-official-interior.jpg y su .provenance.json: es referencia crítica interna de Steam / Wrong Organ, no un asset del juego.

Evidencia válida: se abrieron las doce capturas obligatorias —arrival, soldier, warehouse, control, ragdoll, ragdoll-1080, menú, ayuda, inventario, inventario-1080, feedback e infected— y los cinco cuadros fall-00..04, todos en .impeccable/review/industrial/. Se leyó industrial-fall-sequence.csv. Los estados corresponden a sus nombres; hay salida 1280×720 y 1920×1080; el cuerpo caído está separado del protagonista y su trayectoria no queda oculta. Las regiones oscuras son contenido renderizado y se juzgan como tal. No corresponde recapture.

El paquete declara captura de mundo URP y HUD vivo mediante cámara UI; esta revisión visual se aplica a esa escena y esos estados, no acredita por sí sola el comportamiento temporal completo del ejecutable. La escena capturada procede del checkout de validación Library/ZombtoyValidationClean; la sincronización final a Assets y la identidad del build distribuible se deben registrar antes de entregar. Las 26 PlayMode y 18 EditMode comunicadas son evidencia funcional, no aceptación estética ni medición de rendimiento.

DESIGN.md y .impeccable/design.json ya describen una etapa industrial 0.5, pero registran el estado intermedio anterior a esta reconstrucción. Requieren refresco tras resolver esta revisión; no son autoridad para conservar sus defectos. El launcher considera web una plataforma que no reconoce; aquí prevalecen el producto y la implementación Unity Windows. No se ejecutó detector HTML/CSS ni se aplicaron reglas iOS/Android.

## fidelity

| Elemento / promesa | Estado | Evidencia visible |
| --- | --- | --- |
| TYPE y HUD sobrio | match | Título serif y texto operativo condensado mantienen jerarquía. Objetivo arriba, vitales abajo a la izquierda y arma a la derecha dejan el centro al juego. La información operativa se lee en las dos salidas capturadas. No se exige copiar el diálogo serif de Mouthwashing. |
| 3D isométrico, escala adulta, uniforme negro y tanque | match | Hay volumen y profundidad 3D, cabeza y extremidades de proporción adulta y tanque reconocible en arrival/warehouse/control. soldier muestra ahora el frente; no se juzga la ausencia del tanque desde ese ángulo como incumplimiento. |
| MATERIAL: soldado vestido y equipo distinguible | drift | soldier muestra correas claras, borde de chaleco, bolsillos, articulación de brazos y máscara; ya no es una única columna sin detalle. Sin embargo, muslos, rodillas, pantorrillas y botas quedan casi al mismo valor oscuro. En las vistas jugables arrival/warehouse/control, el tramo cadera–suelo vuelve a leerse como bloque continuo. El negro es correcto; la separación entre textil, protección y cuero sigue pendiente. F1. |
| MATERIAL: interior industrial gastado | adaptation | arrival ya muestra recinto reconocible con pared posterior, marco de acceso, tuberías, banco, caja y equipo. warehouse organiza banco y estantería contra mamparo, con área de trabajo iluminada y paso libre. Juntas, manchas y textura de caja aportan materia. La cámara comunica habitación contenida; no persiste la contradicción general de paneles aislados sobre un tablero. |
| MATERIAL / función de control | drift | control conserva recinto, suelo usado y luz fría práctica. Pero los tres aparatos de la pared se presentan principalmente como carcasas altas cerradas: no se distingue una cara de operación con pantalla, mandos o estado. La región se lee más como almacenamiento de gabinetes que como puesto de control. Es una región localizada dentro de un interior que ya funciona espacialmente. F2. |
| GROUND y luz práctica | match | Hueso gastado, acero verdoso apagado, suelo terroso y negro textil responden al contrato. El foco cálido del banco y la temperatura más fría de control distinguen zonas. Oscuridad y exterior azul corresponden al puerto nocturno; no se pide elevar toda la exposición. |
| FIRST VIEWPORT: entrar en instalación abandonada | adaptation | arrival ofrece umbral al fondo, pared continua y objetos anclados al recinto; el protagonista ocupa una escala útil. El corte isométrico del muro cercano es compatible con el alcance humano. Ya no domina una pared frontal opaca ni el contorno de una plataforma expuesta. El uniforme se evalúa en F1. |
| PS1 marcado | match | Baja resolución, escalonado y dithering son inequívocos. Su intensidad viene del usuario y se mantiene. Las imágenes no acreditan UV inestable ni snapping en movimiento. |
| STORY y estados del HUD | match | Control muestra objetivo 04 y no arrastra el tutorial de buscar el repuesto. warehouse muestra la interacción de recogerlo. infected no mantiene mensaje de botín caducado. No reaparece un aro segmentado permanente de depuración. |
| Amenaza humana y cadáver | match | infected permite reconocer cuatro figuras separadas con ropa y escala diferentes. Ragdoll 720/1080 identifica un cuerpo humano y ofrece Registrar Cuerpo: Errante. La sombra larga se distingue del cadáver; el jugador no lo tapa. |
| Caída física | match, alcance limitado | fall-00..04 muestra figura de pie, pérdida de equilibrio, descenso y asentamiento con rotación de torso y extremidades. CSV: 13 cuerpos dinámicos; pelvis de 0.9193 a 0.1605 m entre 0.167 y 2.796 s. Acredita esta caída; no todas las variantes, superficies o colisiones. |
| Menú, ayuda, inventario y recuperación | match | Menú expone comenzar, checkpoint, ayuda y salir. Ayuda explica controles, supervivencia y ruta. Inventario conserva columnas, acciones y armas sin recortar en 720p/1080p; feedback expresa límite y recuperación mediante suministros. Las interrupciones tienen propósito operativo. No se certifican foco, hover o toda la interacción a partir de imágenes. |

La referencia oficial aporta superficies con historia, capas de equipo y cuerpo vestido bajo render retro. Esos principios ya llegan a la arquitectura reconstruida; llegan parcialmente al soldado y a los aparatos de control. No se exige igualar el número de píxeles que ocupa una persona en una cámara en primera persona. Una textura existente en el atlas no resuelve la materia si su región queda sin lectura en la cámara jugable.

## ceiling

No alcanzado, por dos regiones delimitadas. La reconstrucción ha resuelto la contradicción espacial amplia de las revisiones anteriores: los cuartos contienen y organizan la escena, y el frente del soldado muestra equipo. No se justifica otra reconstrucción completa con esta evidencia. Queda trabajo material comprobable para que la mitad inferior del uniforme conserve su anatomía y para que control muestre su función al jugador. Esto impide ship en esta revisión.

La matriz cubre los tamaños Windows solicitados, no otras relaciones de aspecto ni todas las condiciones de iluminación. No se certifican FPS, confort, ataque, F2, oscilación de UV o snapping temporal. No hay GPU timing válido ni sesiones humanas externas; los KPI correspondientes permanecen abiertos. La caída sí cuenta con evidencia secuencial y conserva el alcance limitado descrito arriba.

## material_fixes

| ID | Corrección necesaria | Criterio verificable de resolución |
| --- | --- | --- |
| F1 | Recuperar separación local en la mitad inferior del soldado. Ajustar distribución de valores del atlas y respuesta a la luz de muslo, rodillera, pantorrilla y bota; conservar negro, geometría adulta, tanque y PS1 a altura 270. Conservar correas y chaleco ya visibles. No requiere elevar exposición global ni quitar dithering. | En soldier de frente y arrival/warehouse/control jugables se deben reconocer transición muslo–rodilla, separación de ambas piernas y límite de bota respecto a pantalón, con al menos tela y protección/cuero distinguibles. El detalle debe sobrevivir a la salida final sin depender de abrir el atlas o acercar una cámara adicional. |
| F2 | Dar una cara funcional visible al conjunto de control. Reorientar o resolver los aparatos próximos y sus superficies de trabajo para que desde la cámara jugable se reconozcan pantalla/mandos y un estado concreto del equipo, con materia usada coherente. Puede conservarse mobiliario y distribución; no hace falta rehacer el recinto. | industrial-control debe permitir identificar un puesto que opera la instalación, con al menos una superficie de operación visible y diferenciada de una carcasa cerrada a resolución PS1 final. Evitar una pantalla uniforme luminosa como único sustituto de contenido. |

Aplicar F1 y F2 en un lote, reconstruir una vez y recapturar los mismos archivos y tamaños; volver al mismo revisor para puntuar cada ID como resolved, partial o unresolved. No ampliar la lista con otra caza de defectos. Refrescar después DESIGN.md y design.json con resultado y procedencia del cuarto albedo TacticalAtlas_v2, manteniendo historial y límites de evidencia. La revisión no autoriza a declarar aceptación humana ni cerrar KPI no medidos.

## keep

Conservar la nueva arquitectura contenida, banco y estanterías contra paredes, juntas y desgaste visibles, marco de acceso de llegada, iluminación cálida/fría localizada, 3D isométrico, escala adulta, uniforme negro con tanque, frente del soldado con correas y bolsillos, PS1 marcado, amenaza humana legible, caída dinámica visible, HUD compacto y feedback recuperable.

Conclusión verificable: fix, con F1 y F2 abiertos. La evidencia nueva es válida y demuestra progreso material; no sostiene ship ni otra directiva de reconstrucción total. La aceptación visual final sigue perteneciendo al usuario.
