# Validación de la fundación

18 octubre 2026. Código/escena verificados: **`da6892c`**. Editor **6000.5.9f1**,
URP17.5.0, Windows x64 Mono. Se validó en una copia separada de
Assets/Packages/ProjectSettings; el Editor original del usuario permaneció abierto.
Esta evidencia acredita la preparación técnica, no la aceptación del MVP completo.

| Comprobación | Resultado | Evidencia |
|---|---|---|
| Resolución de paquetes y compilación | PASS, sin errores C# | Lockfile versionado; log local `Library/ZombtoyValidationClean/TestResults/Generate.log` |
| Importación | 17 FBX, 4 OGG y 5 texturas; 4 licencias | [Informe de meshes, clips y avatares](Validation/asset-import.csv), manifest con hashes |
| EditMode | **16/16 PASS** | [Resultados por caso](Validation/test-results.json) |
| PlayMode | **2/2 PASS** | Ruta requiere terminal antes de extracción; inyección espera2s; recarga conserva munición; RT480×270 point |
| Repositorio local | PASS | 110 GUID únicos, metas completos, archivos/hashes/licencias y paquetes fijados |
| Checkout con LF | PASS | Checkout del índice con `core.autocrlf=false`; mismos hashes y metas; licencias preservan bytes |
| Git LFS | PASS | `git lfs fsck`; 27 binarios publicados (antes de añadir la captura propia) |
| GitHub / Ubuntu | **PASS** | [Repository checks, código da6892c](https://github.com/c90Beretta/Abisal-Stealness/actions/runs/39175640620) |
| Build Windows | **Succeeded, 0 errores, 2 advertencias** | [Resumen](Validation/build-summary.txt); salida local `Builds/Windows/AbisalStealness.exe` |
| Revisión visual | Captura revisada, sin materiales magenta | [Sandbox graybox](Validation/foundation-scene.png). Maniquíes y luces provisionales; look final no aprobado. |

SHA256 del ejecutable generado:
`36c5c9f13481406382a8e9ef8fc0ea7cdf055c43bb12fc8fd545b07c199cd277`.
El directorio de distribución incluye CREDITS.md, LICENSES.md y las cuatro
licencias CC0; se excluye el directorio BurstDebugInformation_DoNotShip.
Conservar la carpeta completa junto al EXE. El build se genera localmente;
no se publica como release ni se versionan sus DLL/ejecutables.

## Control de versiones y publicación

Remoto: https://github.com/c90Beretta/Abisal-Stealness. Baseline `3b225ed`, código
de fundación `da6892c`, evidencia en commit posterior y etiqueta
**`v0.1.0-foundation`**. Rama de preparación: `codex/mvp-foundation`;
primera integración por fast-forward en main, sin force-push ni historia ajena.

Protección configurada en main: PR con 1 aprobación, descartar aprobaciones tras
cambios, conversaciones resueltas y check `repository` actualizado; force-push y
borrado bloqueados. Los administradores conservan bypass para bootstrap/recuperación;
la primera publicación del proyecto usa ese rol. Dar acceso a colaboradores y
designar revisores requiere los nombres de las personas; no se enviaron invitaciones.
GitHub ejecuta verificaciones del repositorio; Unity se valida localmente, no en ese job.

## Límites conocidos y siguiente puerta

- Las 2 advertencias del build son CS0618 en `FindObjectsByType(...FindObjectsSortMode)`:
  API aún funcional en el editor fijado, pendiente migrar al overload de Unity6.5.
- Se verificaron reglas y ruta con tests en el Editor. No se ejecutó una sesión
  humana del EXE ni una partida de 10–15 min. No se midió rendimiento GTX1060.
- Cámara fija propia y RT point. Cinemachine está instalado, pero su rig final,
  dithering/quantización, shader de muros y visión ocluida siguen pendientes.
- Enemigos persiguen directo con CharacterController y colisión, sin NavMesh ni
  flanqueo, patrulla o búsqueda completos. Variantes provisionales de color/tamaño/estadísticas.
- Suero tiene consumo, inyección temporizada, fases por datos y timers de manía;
  faltan efectos audiovisuales de las fases, inversión de controles y conexión al inventario.
- Faltan inventario con arrastre, melee, sangrado, checkpoint, loot persistente,
  menú principal y niveles de producción. Los maniquíes no tienen animador activo;
  los clips se importaron como Generic y no se certifica retarget humanoide.
- Audio de impacto provisional compartido entre paso/disparo. Afinar contraste e
  iluminación antes de la puerta visual. No se certifica semejanza final a las referencias.
- Unity6.5 no es LTS y su rama ya no tiene soporte; evaluar migración en copia.

Siguiente aceptación: ejecutar las tareas G01–G04/A01/L01 del plan y registrar
playtests reales. Ningún KPI final se marca cumplido por estas 18 pruebas.
