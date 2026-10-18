# Assets instalados — fundación

Fecha: 18 octubre 2026. Fuente verificable de archivos y hashes:
[`asset-install-manifest.json`](asset-install-manifest.json).
26 archivos de contenido seleccionados + 4 textos de licencia. No se importaron
los 43 candidatos del inventario original. Descargar ≠ importar ≠ aceptar para producción.

| ID | Pack / autor / fuente | Licencia | Selección instalada | Estado técnico |
|---|---|---|---|---|
| MVP-ENV | [Modular Space Kit](https://kenney.nl/assets/modular-space-kit), Kenney | CC0-1.0; License.txt incluido | 9 FBX: corridor/corner/junction, gate-door, room-small, stairs, floor, wall, cables + colormap | Modelo estático; wrappers URP propios y prueba en sandbox. Colisiones de nivel por graybox; no NavMesh certificado. |
| MVP-WPN | [Blaster Kit](https://kenney.nl/assets/blaster-kit), Kenney | CC0-1.0; License.txt incluido | 3 blasters, crate-small, clip-small, colormap | Placeholders originales de ciencia ficción; no modelos finales de MP5/rifle/escopeta. Collider/daño pertenecen al gameplay. |
| MVP-CHR | [Animated Characters Survivors](https://kenney.nl/assets/animated-characters-survivors), Kenney | CC0-1.0; License.txt incluido | characterMedium.fbx, idle/run.fbx, skins survivorMaleB/zombieA/zombieC | Import Generic; no declarar rig Humanoid. El sandbox usa maniquí sin animador activo; animación/retarget pendientes. |
| MVP-SFX | [Impact Sounds](https://kenney.nl/assets/impact-sounds), Kenney | CC0-1.0; License.txt incluido | 2 pasos concreto, metal ligero e impacto genérico OGG | Mono, prueba mediante AudioSource; impacto de disparo provisional. No es audio final ni mezcla certificada. |
| OWN-01 | Graybox y wrappers, proyecto | Derechos del titular | Contenedores/deck/terminal/extracción; materiales URP, prefabs visuales, Survival_Default SO | Generados por FoundationBuilder, fuera de ThirdParty. Escala normalizada por altura (jugador1.7m), colisiones independientes del mesh. |

CC0 permite su uso y distribución conforme a la declaración incluida por el autor;
se conserva crédito voluntario. No hacer extensiva esa licencia al código propio.
ZIP originales quedan en Library (ignorados). No se reescriben archivos del proveedor.

## Ajustes de importación

`KenneyImportSettings.cs`: sin cámaras/luces/colliders importados, materiales
del proveedor desactivados; Generic para Survivors; texturas point sin mipmaps;
audio mono. El sandbox usa materiales Lit de URP propios y wrappers normalizados.
Los colormaps/skins quedan disponibles para el trabajo de arte posterior.
Modelo estático no necesita avatar. Los FBX de animación pueden no tener renderer;
no se instancian como personaje. Rig/clips medidos se reportan en la evidencia de importación.

## Antes de sustituir un placeholder

Asignar ID, registrar autor/URL/licencia exacta y hash, validar permisos para este
repositorio público, rig/clips/escala y URP; crear wrapper nuevo en `_Project`,
probar escena y build; actualizar manifest/ledger/créditos en el mismo PR.
Los modelos de Sketchfab, paquetes Asset Store, Zara, Perfect Pixel, Pixabay,
Mixamo y Zombtoy externo permanecen **NO INSTALADOS**.
