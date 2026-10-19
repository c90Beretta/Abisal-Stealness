# Puerto industrial PS1 — entrega 0.5

Esta entrega reconstruye interiores y soldado tras la elección explícita del usuario. Conserva 3D isométrico, uniforme negro con tanque y presentación PS1 marcada. Las capturas corresponden a Unity, no a una imagen objetivo.

## Ejecutable y reproducción

- [Descarga Windows 0.5](https://github.com/c90Beretta/Abisal-Stealness/releases/tag/v0.5.0-industrial-playtest). Extraer todo y abrir `JUGAR ABISAL.cmd` o `AbisalStealness.exe`. La copia del proyecto también tiene el lanzador en la raíz.
- Fuente compilada: `0b26d7e649b3301a20f5c14d5a77bd2a4517b3bb`. El tag de entrega incluye además la documentación y esta evidencia; no implica una recompilación con otro SHA.
- Unity **6000.5.9f1**, Windows x64 Mono, versión **0.5.0**, compilado **2026-10-19 13:09:15 UTC**. [Identidad embebida](build-identity.json).
- Build: **0 errores / 1 advertencia**. La advertencia CS0618 procede de `PushPhysics.cs` de una demo del proveedor (`Rigidbody.velocity`). [Resumen](build-summary.txt).
- ZIP: **117947749 bytes**, **203 archivos**. SHA256: `9de69f5dbe5a2048fe23e8078edf792136e6d0411521e641dd11168703309aee`. [Paquete](package.json) y [SHA de cada archivo](build-files-sha256.json); cada entrada del ZIP se verificó contra el build. Incluye avisos y licencias, excluye PDB y carpetas DoNotShip.
- [Sincronización](source-sync.json): escena final, navegación y renderer coinciden byte a byte con la copia de compilación. Runtime, Editor, Tests y arte industrial también comparados. El prefab `ModularAsset_Sandbox` modificado por el usuario permanece intacto y fuera de los commits de esta entrega.

El nivel 0.5 versionado se abre directamente. No ejecutar de nuevo el upgrade industrial encima de la escena ya actualizada. Los colaboradores importan sus copias autorizadas de los tres paquetes de [assets locales](../../LOCAL_ASSETS.md).

## Pruebas y capturas

**26 PlayMode + 18 EditMode aprobadas**, con casos, tiempos y marcas UTC en [tests.json](tests.json). Incluyen ruta completa del puerto, combate, inventario, cuatro arquetipos por dos variantes, rig adulto, ragdoll de 13 cuerpos, botín y diez ciclos de checkpoint. [Registro de ciclos](checkpoint-ten-cycles.csv).

Las **17 capturas** finales de las 13:03 UTC se conservan junto a este documento; [índice, tamaños, hashes y procedencia](captures.json). Se usa `RenderPipeline.SingleCameraRequest` sobre la escena final de la copia de validación, con postprocesado y presentación retro; se compone después el HUD vivo ScreenSpaceOverlay. El CRT WorldSpace permanece en el render 3D. Las fixtures colocan personajes y fases de misión para hacer reproducible cada estado. No se presentan como sesiones humanas ni capturas de rendimiento del ejecutable. Solo se añadió metadato de origen a los PNG; no se editaron sus píxeles.

![Control con consolas y pantalla de estado](industrial-control.png)

![Soldado negro con equipo y piernas diferenciadas](industrial-soldier.png)

[Secuencia física CSV](industrial-fall-sequence.csv): pelvis **0.9056 → 0.1754 m**, **13 cuerpos dinámicos**, entre **0.167 y 2.797 s**, en cinco cuadros `industrial-fall-00..04.png`. Es evidencia de una caída concreta, no de todas las combinaciones de superficie y variante.

## Revisión y límites

Tras dos revisiones `rebuild`, el usuario autorizó esta reconstrucción. La [revisión completa final](../../UI_REVIEW_INDUSTRIAL.md) pidió F1 (piernas/botas) y F2 (cara operativa de control). El [veredicto](../../UI_VERDICT_INDUSTRIAL.md) los cerró como **resolved**; `ship` se limita a esos dos hallazgos. La primera revisión interrumpida por cuota fue reemplazada por una revisión fresca y no cuenta como aprobación.

El [smoke del ejecutable final](runtime-smoke.json) terminó sin excepciones registradas: D3D11, 60 s de calentamiento y 10 s de estrés sintético con hasta 14 infectados, 52 cuerpos físicos y 90 efectos. **Cero muestras de GPU**: salida 3 del instrumento, benchmark rechazado; no se aceptan percentiles ni se calcula FPS. Los [preflight previos](invalid-preflight.json) también fueron rechazados y pertenecen a una compilación anterior.

Siguen pendientes la aceptación estética humana, cinco partidas externas, duración y balance del nivel y una medición válida en la GTX1060 objetivo. Capturas estáticas no certifican temporalidad de UV/snapping/F2 ni confort. El render URP ocurre al tamaño de ventana y luego se reduce/amplía a presentación 270p; no se afirma coste de sombreado de 270p. [KPI originales y estado](../../KPI.md).
