# Trabajar en equipo

## Primera vez

Git LFS y Unity **6000.5.9f1** con Windows Build Support (Mono). Clonar la carpeta
del proyecto; ejecutar `git lfs install` y `git lfs pull` antes de abrir Unity.
Nunca copiar Library o Temp de otro equipo. Conservar `.meta` junto al asset.
Unity genera GUID; no regenerar metas para resolver conflictos.

## Ramas y revisión

1. Sin cambios locales: `git switch main`; `git pull --ff-only` cuando exista origin.
2. `git switch -c codex/<rol>-<ticket>` (por ejemplo `codex/ai-A01`).
3. Acordar propietario de escenas/prefabs antes de editarlos. Niveles, sistemas
   e iluminación en escenas aditivas. Prefabs pequeños, datos en SO.
4. Hacer commits pequeños con propósito. No mezclar cambios de arte y upgrade URP.
5. Ejecutar `python tools/check_repository.py`, pruebas Unity pertinentes y build
   si cambia runtime. Registrar resultado real en `docs/DAILY.md`.
6. PR con contexto, criterio de salida, validación y screenshot/video si corresponde.
   Revisor distinto del autor; integrar diariamente solo cambios verificados.

No sobrescribir escenas de otra persona con YAML manual. Smart Merge es una ayuda,
no reemplaza la revisión visual. Configuración **local** opcional en PowerShell:

```powershell
git config merge.unityyamlmerge.name 'Unity SmartMerge'
git config merge.unityyamlmerge.driver "'C:/Program Files/Unity/Hub/Editor/6000.5.9f1/Editor/Data/Tools/UnityYAMLMerge.exe' merge -p %O %B %A %A"
git config merge.unityyamlmerge.recursive binary
```

## Publicar la base preparada

Remoto autorizado por el usuario: **c90Beretta/Abisal-Stealness**, público y vacío
al comenzar. Las imágenes/brief originales permanecen locales; el repositorio
contiene el plan revisado, inventarios de enlaces y assets CC0. Para un clon nuevo:

```powershell
git clone https://github.com/c90Beretta/Abisal-Stealness.git
cd Abisal-Stealness
git lfs install
git lfs pull
```

La publicación y el SHA verificado se registran en `docs/VALIDATION.md`.
En el proveedor, proteger main (PR, 1 aprobación, checks), bloquear force-push y
dar acceso por mínimo rol necesario. Configurar licencias Unity de CI mediante
secretos del proveedor si se automatizan builds; nunca guardarlas en archivos.
La validación local no necesita credenciales de GitHub ni de servicios externos.
La protección de main ya está configurada: 1 aprobación, check `repository`,
conversaciones resueltas, sin force-push ni borrado. Admin conserva bypass para
bootstrap/recuperación; el trabajo normal del equipo se revisa por PR.
El workflow `Repository checks` valida assets, metadatos y LFS en GitHub sin licencia
Unity. Las pruebas/build de Unity se ejecutan con `tools/Invoke-Unity.ps1`; no se
declaran ejecutadas en CI sin configurar un runner/licencia de Unity.

```powershell
./tools/Invoke-Unity.ps1 -Task EditMode
./tools/Invoke-Unity.ps1 -Task PlayMode
./tools/Invoke-Unity.ps1 -Task Build
```

Cierra primero esa copia del proyecto en el Editor, o usa `-ProjectPath` con otra
copia de Assets/Packages/ProjectSettings. Los logs y XML quedan en `TestResults`.
No ejecutar `Generate` sobre la escena ya editada; se rechaza para conservar cambios.

## Assets

Intake obligatorio: URL oficial, autor, permiso de uso y redistribución, texto
de licencia, hash, formato, rig/clips, escala, material URP, prefab y ledger.
CC0 puede acompañar el repositorio; paquetes Asset Store restringidos se obtienen
por cuenta/licencia del equipo y se mantienen fuera de repos públicos. No asumir
que el `.gitignore` elimina archivos ya versionados. Los grandes van a LFS:
`git lfs ls-files` y `git lfs fsck` antes de publicar.

No versionar Library, Temp, Logs, Builds, .plastic, tokens o configuraciones privadas.
No editar ThirdParty directamente; usar wrappers en `_Project`.
