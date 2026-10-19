# Validación — primer nivel del puerto

18 octubre 2026 local / Unity6000.5.9f1 / URP17.5.0 / Windows x64 Mono.
Rama `codex/first-port-map`. La evidencia anterior de trazado permanece en
`Validation/Port01`; el incremento de supervivencia está en `Validation/FirstLevel02`.

| Comprobación | Resultado | Evidencia |
|---|---|---|
| Compilación Windows | 0 errores / 0 advertencias | [Resumen](Validation/FirstLevel02/build-summary.txt) |
| EditMode | 18/18 PASS | [Casos](Validation/FirstLevel02/tests.json): suero, manía, límites, medicina/sangrado y escenas con bindings válidos |
| PlayMode | 10/10 PASS | Menú inicia por botón; pausa congela; tratamiento tarda, se interrumpe y consume; muerte; guardado/carga; ataque preparable/esquivable, paredes; misión ordenada y rutas NavMesh; suministros no duplicables; caída; regresión sandbox |
| Visual | 3 estados inspeccionados a1280×720 | [Menú](Validation/FirstLevel02/first-level-menu.png), [HUD](Validation/FirstLevel02/first-level-hud.png), [ayuda](Validation/FirstLevel02/first-level-help.png) |
| Recursos | PASS |146 GUID únicos; hashes/licencias de terceros y lock de paquetes sin cambios |
| Distribución | Build local completo + paquete Windows | `JUGAR ABISAL.cmd`, acceso del escritorio y release de prueba v0.2.0-port-playtest |

Las capturas muestran canvases uGUI reales y el RenderTexture del mundo, compuestos
por una cámara temporal durante PlayMode. El Editor batch no presenta un backbuffer;
por eso no se usa ScreenCapture del escritorio. No son una partida manual ni un
registro de rendimiento. Se corrigió el contraste del marcador y se revisaron de
nuevo los tres estados; ver [alcance de revisión UI](UI_REVIEW.md).

La prueba de misión desactiva enemigos para aislar objetivos y navegación. La
prueba de combate activa un infectado para comprobar preparación, esquiva, daño y
bloqueo por pared. El guardado usa rutas temporales de prueba y no escribe el save
normal del jugador. Comprueba salud, recursos, objetivos, puertas, suministros y
enemigos abatidos; rechaza JSON corrupto. No equivale aún a los10 ciclos de aceptación
del KPI de checkpoint.

El juego arranca con menú en Level01_Port y conserva MVP_Sandbox como segunda
escena. El paquete incluye UnityPlayer, Data, Mono, créditos y cuatro licencias
Kenney; se excluyen símbolos y carpetas DoNotShip. El EXE requiere todo su directorio.
Los hashes de archivos del build están en
[build-files-sha256.json](Validation/FirstLevel02/build-files-sha256.json).

Pendientes: partida humana completa, ajuste de duración/dificultad/economía,
perfilado en el hardware objetivo, inventario por cuadrícula, más armas y variantes,
fases mentales audiovisuales, shader PSX completo y niveles2–4. La ocultación de
obstáculos conserva colliders; no es todavía corte por dither. Ningún KPI global
queda certificado por esta entrega.
