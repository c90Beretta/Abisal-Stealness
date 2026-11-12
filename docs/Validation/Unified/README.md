# Version unica 0.7

Fuente compilada `76379d38f07c9be47f6874d8789b668d9fdf62d0`, Unity6000.5.9f1 Windows x64, build2026-11-12 08:36:03UTC. El tag añade documentacion/evidencia. [Identidad](build-identity.json), [pruebas](tests.json), [build](build-summary.txt), [paquete](package.json).

**27 PlayMode +18 EditMode aprobadas**, mas recaptura final con la prueba de integracion aprobada. Esta ultima verifica personaje, grado AP-I y consumos de estaciones tras guardar/cargar; comprueba rutas NavMesh completas hacia siete puntos de los tres sectores nuevos. La navegacion se recalcula despues de crear su geometria. No sustituye una partida humana recorriendo el nivel.

Build: **0 errores /1 advertencia** de una demo del proveedor sobre `Rigidbody.velocity`. ZIP **118089374 bytes**, SHA256 `509d68a6ba3789231aba7dcc8a2f5ea75947233e16124f4494dec07c6045dddd`; integridad CRC de todas las entradas comprobada. Sin PDB/logs/DoNotShip; incluye creditos/licencias.

![Armeria unificada](arsenal-menu.png)

Captura real Unity a1280×720 con fixture reproducible y HUD nativo; no es imagen generada ni evidencia de FPS. Conserva metadato de origen. [Revision completa](../../UI_REVIEW_UNIFIED.md) y [veredicto](../../UI_VERDICT_UNIFIED.md): **ship limitado a U1/U2 resueltos**, sin aprobacion global de juego/arte/audio.

La copia principal ya contiene el runtime integrado y escena/navegacion. Su ejecutable `Builds/Windows` se reemplazo por este build; los anteriores y las fuentes previas tienen respaldo local. Ambos lanzadores y el acceso de escritorio apuntan a esta misma prueba. Un archivo auxiliar local sin referencias (`IndustrialPropFactory.cs`) se preservo sin incorporarlo al build; no altera esta version.

[Descarga unica0.7](https://github.com/c90Beretta/Abisal-Stealness/releases/tag/v0.7.0-unified-playtest). Extraer todo y abrir `JUGAR ABISAL.cmd`. La entrega contiene los cambios paralelos finales de personajes, modelos, combate, HUD, hordas y sectores junto al arsenal/especiales. [Controles y alcance](../../UNIFIED_PLAYTEST.md).

Siguen pendientes cinco sesiones externas, balance, valoracion humana de modelos/animacion/audio y perfilado valido en GPU objetivo. La integracion no los convierte en KPI cumplidos. No se ha fusionado automaticamente el PR en main.
