# disposition

ship

Veredicto posterior a la corrección: **R1 resolved**. Este cierre puntúa exclusivamente el único hallazgo de la revisión fresca inicial, cuya disposición fue fix. No constituye una segunda auditoría general ni amplía las garantías funcionales.

# material findings

**R1 — resolved.** Hallazgo original: en ambas capturas de mochila, linterna, O2 y placas estaban al 100%, pero «Cambiar pila», «Recargar oxigeno» y «Cambiar placa» tenían apariencia activa. `NightSurvival.UseSupply` rechazaba cargas superiores al 70%. Su explicación se enviaba al mensaje del HUD de combate, situado detrás de la tarjeta de equipo casi opaca. La misma ruta ocultaba la explicación de `LootContainer.Take` cuando no cabía botín o se alcanzaba un límite de pila. El jugador podía pulsar un control aparentemente disponible y no descubrir por qué no respondía; esto afectaba directamente al circuito principal de esta extensión.

Archivos/acciones: `Assets/_Project/Runtime/Gameplay/NightHud.cs:19`, `Assets/_Project/Runtime/Gameplay/NightHud.cs:56`, `Assets/_Project/Runtime/Gameplay/NightSurvival.cs:95`, `Assets/_Project/Runtime/Gameplay/LootContainer.cs:21`; el mensaje cubierto se crea en `Assets/_Project/Runtime/Gameplay/GameHud.cs:66` y la tarjeta que lo cubre en `Assets/_Project/Runtime/Gameplay/GameHud.cs:96`. Dar estado no disponible a los suministros según carga, existencias y bloqueo; comunicar la regla real **≤70%** en la mochila y mostrar allí el resultado o motivo de rechazo de recoger. No basta con añadir más mensajes a la capa de combate cubierta. Mantenerlo dentro de las dos columnas existentes, sin solaparlo con acciones o texto.

Aceptación: a 100%, los suministros deben comunicar por qué no están disponibles; un intento de recoger con mochila/pila llena debe dejar una explicación legible mientras la mochila permanece abierta. Verificar a 1280×720 y 1920×1080.

Comprobación de la corrección: `NightHud.LateUpdate` ahora deshabilita los tres suministros según carga >70%, falta de repuesto, sesión no activa, acción en curso o bloqueo. En las dos resoluciones sus superficies se distinguen de las acciones disponibles. La regla real de carga <=70% y repuesto disponible aparece dentro de la mochila. El feedback tiene una etiqueta propia bajo «Recoger lo que quepa»; `night-inventory-feedback.png` muestra «Mochila o pila llena. Usa suministros para hacer espacio.» en dos líneas completas, legibles y separadas del texto auxiliar. `NightSurvival.UseSupply` comunica también el éxito. Se satisface el criterio acotado de R1.

No se elevan a hallazgos materiales la fuente LegacyRuntime, el glifo del objetivo ni la suavidad heredada de las etiquetas: el contrato conserva esos recursos. La tarjeta oculta parcialmente el HUD de fondo; no se exige rediseñar ese HUD para resolver R1.

# evidence checked

- Leídos `.impeccable/night-equipment.md`, `DESIGN.md`, `docs/NIGHT_PORT.md`, las referencias `craft-floor.md` y el criterio de cierre de `new-work.md`.
- Leídos `GameHud.cs`, `NightHud.cs`, `NightSurvival.cs`, `LootContainer.cs` y `FieldInventory.cs`; consultados los cierres de paneles y pausa de `SandboxSession.cs`.
- Abiertas individualmente las seis capturas de `Library/ZombtoyValidationClean/TestResults/`: `night-hud.png`, `night-inventory.png`, `night-inventory-1080.png`, `night-soldier.png`, `night-infected.png`, `first-level-help.png`.
- Las capturas corresponden al contenido que nombran, no están vacías y muestran runtime. Las cinco vistas de 1280×720 y la mochila de 1920×1080 son evidencia utilizable. No se necesita recapture por invalidez.
- En las vistas de combate se distinguen el círculo próximo, el protagonista negro y el tanque; la vista próxima permite reconocer el equipo sin eliminar la oscuridad pedida. La vista de infectados muestra modelos 3D y diferencias de silueta. La ayuda expone controles de combate, supervivencia, mochila y ruta. Las dos columnas de mochila contienen carga, recursos, equipo, botín y acciones, sin recorte del contenido principal.
- Confirmada la presencia de la escena generada `Library/ZombtoyValidationClean/Assets/_Project/Scenes/Level01_Port.unity`. Los encabezados XML registran 19/19 PlayMode y 18/18 EditMode aprobados. No se ejecutó Unity durante esta revisión.
- Para el veredicto se abrieron las siete capturas nuevas y válidas de `docs/Validation/NightPort/`: `night-hud.png`, `night-inventory.png`, `night-inventory-1080.png`, `night-soldier.png`, `night-infected.png`, `first-level-help.png` y `night-inventory-feedback.png`. Se releyeron los cambios de `NightHud.cs` y `NightSurvival.UseSupply`; solo se puntuó R1. El XML actualizado de PlayMode registra 20/20 y `EquipmentDisablesUnusableSuppliesAndShowsLootFeedback` Passed. Ese resultado no amplía el alcance visual de este veredicto.

# scope/limits

Evaluación visual y de coherencia de acciones por lectura de código en el alcance señalado. No hay comp aprobado; mandan contrato, código e identidad incumbente. No se ejecutó detector HTML/CSS porque el artefacto es uGUI nativo Windows. No aplica guía de iOS/Android.

Las imágenes no certifican audio, navegación, oleadas, combate, desmembramiento, persistencia, balance, funcionamiento integral de cada botón ni rendimiento/60 fps. El XML prueba el resultado de esas ejecuciones automatizadas, sin ampliar por sí solo esta revisión visual. El recorte próximo del soldado complementa la vista normal; no sustituye una prueba de lectura en movimiento.

# verdict/followup

**ship — R1 resolved.** El único hallazgo original quedó resuelto en la tanda recibida. No quedan acciones pendientes dentro de esta lista de correcciones. Procede continuar al registro documental final. La formulación respaldada es «el revisor puntuó R1 como resuelto»; este veredicto no certifica rendimiento ni funcionamiento integral del juego, y no abre una búsqueda nueva de defectos.
