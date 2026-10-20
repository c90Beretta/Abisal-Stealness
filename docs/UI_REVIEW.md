# Revisión visual del primer puerto

Registro histórico de v0.2. La presentación vigente3D nativa y las capturas de
armas/mundo se describen en `PORT_3D.md`; no se extrapola este veredicto a v0.3.

Alcance: HUD nativo de Unity para Windows, extensión del lenguaje clínico verde/rojo existente. Fuentes: `Assets/_Project/Runtime/Gameplay/GameHud.cs`, `IsometricView.cs` y controles de `SandboxSession.cs`. El código prevalece sobre este registro.

Evidencia visual inspeccionada a 1280 × 720, publicada bajo `docs/Validation/FirstLevel02/`:

- `first-level-menu.png`: inicio por botón/Enter; checkpoint sin guardado deshabilitado; ayuda y salida.
- `first-level-hud.png`: objetivo superior, vida con hemorragia, suero/infección, munición y recursos; marcador con respaldo oscuro.
- `first-level-help.png`: controles, ruta del nivel y cierre por F1.

Las capturas son composiciones automatizadas del motor. No hubo prueba manual de juego; la automatización de escritorio se detuvo. No se ejecutó detector web sobre código nativo.

La revisión de acabado externa encontró inicialmente un problema material de contraste en el marcador y no identificó solapamientos. Se añadió su respaldo oscuro y se recapturaron las mismas tres vistas. El veredicto posterior fue **ship** para esa corrección: contraste resuelto, sin regresiones introducidas observadas. Ese veredicto puntúa la corrección indicada; no certifica la totalidad de la interfaz ni otras resoluciones.

El HUD usa Canvas de referencia 1280 × 720 sobre un mundo 480 × 270 con filtro Point. F1 muestra ayuda; TAB muestra cantidades y repuesto, sin cuadrícula y sin pausar la partida. Las barras enemigas y los estados restantes se documentan a partir del código, no de estas tres imágenes.

Deuda no canonizada ni reparada en este pase documental: `LegacyRuntime.ttf` como fuente provisional y glifo del marcador como sustituto de un recurso de icono. No hay dirección nueva aprobada, investigación de usuarios, certificación de accesibilidad ni medición de KPI que respaldar.
