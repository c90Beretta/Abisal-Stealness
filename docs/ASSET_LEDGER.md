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

**OWN-02 — Puerto inicial:** geometría procedural propia (muelle, contenedores,
almacén/control, grúa, pasarela, submarino), materiales URP, NavMesh y prefab
Port_Environment. Usa las tres skins Kenney ya incluidas en MVP-CHR; no añade
descargas ni licencias de terceros. Véase `LEVEL01_PORT.md`.

Asignar ID, registrar autor/URL/licencia exacta y hash, validar permisos para este
repositorio público, rig/clips/escala y URP; crear wrapper nuevo en `_Project`,
probar escena y build; actualizar manifest/ledger/créditos en el mismo PR.
Los modelos de Sketchfab, Zara, Perfect Pixel, Pixabay,
Mixamo y Zombtoy externo permanecen **NO INSTALADOS**.

**DER-01 — Clips del puerto:** `_Project/Art/Animations/Idle.anim` y `Run.anim`
son copias derivadas de `Root|Idle` y `Root|Run` de los FBX Kenney Survivors ya
auditados (CC0). Se convierten a Legacy/Loop y se enlazan al nodo Root del esqueleto;
se excluyen los clips Targeting Pose. No se modifican archivos del proveedor ni
sus hashes. Protagonista y seis infectados comparten las dos animaciones.

**LOCAL-03 — Entornos 3D del usuario:** Sci-Fi Construction Kit (Sickhead Games),
Modular Sci-Fi Corridor (MagixBox) y Sci-Fi Old Rusty Props PBR (KMProd), locales
bajo Asset Store EULA. Datos y referencias en `LOCAL_ASSETS.md`; inventario SHA256
de las69 entradas Rusty importadas en `local-rusty-manifest.json`. No se publican
sus fuentes. Materiales URP propios conservan las referencias originales. El puerto
incluye120 instancias estáticas de estos modelos, además de los3 prefabs de armas
Kenney b/a/c ya incluidos en el subconjunto CC0. No hay nuevas descargas de armas.
