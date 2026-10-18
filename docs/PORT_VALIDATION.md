# Validación — primer mapa del puerto

18 octubre 2026, Unity6000.5.9f1 / URP17.5.0, Windows x64 Mono.
Rama: `codex/first-port-map`. El sandbox original se conserva.

| Comprobación | Resultado | Evidencia |
|---|---|---|
| Compilación y build | **0 errores / 0 advertencias** | [Resumen del build](Validation/Port01/build-summary.txt) |
| EditMode | **17/17 PASS** | [Casos y resultados](Validation/Port01/tests.json); dos escenas sin scripts perdidos y referencias obligatorias presentes |
| PlayMode | **5/5 PASS** | Misión ordenada, pasos conectados en NavMesh, suministros no duplicables, paredes bloquean interacción, suelo y recuperación de caída; regresión del sandbox |
| Visual | Revisado | [Plano general](Validation/Port01/port-overview.png), [entrada](Validation/Port01/port-player.png), [render real480×270](Validation/Port01/port-runtime.png) |
| Assets y metadatos | PASS local |133 GUID únicos, sin duplicados ni metas perdidos; licencias/hashes de terceros sin cambios |
| Build distribuible | Generado localmente | `Builds/Windows/AbisalStealness.exe`; [SHA256 de archivos](Validation/Port01/build-files-sha256.json) |

Build arranca en Level01_Port y conserva MVP_Sandbox como segunda escena. El
ejecutable debe acompañarse de su carpeta Data, UnityPlayer, Mono y demás archivos.
Incluye créditos y las cuatro licencias Kenney. No se versiona la carpeta Builds.

Se revisó una captura tomada del RenderTexture durante PlayMode, además de las
vistas de Editor. La captura de Editor usa una solicitud de render URP con
inicialización de estado para conservar los colores desde la primera imagen.

## Alcance de las pruebas

La misión se prueba por interacción y rutas de NavMesh entre objetivos; no se ha
cronometrado una partida humana. El test desactiva enemigos para aislar reglas de
misión y no certifica la dificultad de combate ni el KPI de duración/rendimiento.
No hay todavía checkpoints, inventario por cuadrícula, animaciones finales ni
el tratamiento PSX completo. El corte visual de obstáculos es por ocultación,
no un shader de dither. Es una primera versión para iterar el nivel.

El helper de validación rechaza resultados XML anteriores al inicio de cada
ejecución para impedir que un reporte viejo pase por una prueba nueva.
