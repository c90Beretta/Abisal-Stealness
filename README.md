# Zombtoy — survival horror isométrico

Proyecto Unity 6 / URP. Meta de producción: 4 niveles, economía de suero y munición,
IA por percepción y estética PSX. Incluye una **primera versión jugable del puerto**
y el sandbox técnico. El juego completo y sus KPI de producción siguen en desarrollo.

## Empezar

1. Instala Git, Git LFS y **Unity 6000.5.9f1**, Windows Build Support (Mono).
2. `git clone https://github.com/c90Beretta/Abisal-Stealness.git`; `git lfs install`; `git lfs pull`.
3. Abre esta carpeta (la que contiene `Assets`, `Packages`, `ProjectSettings`) en Unity Hub.
4. Espera la resolución de paquetes. Abre `Assets/_Project/Scenes/Level01_Port.unity` y pulsa Play.
5. WASD mover, ratón apuntar, clic disparar, R recargar, Q suero, E interactuar y Esc pausa.
   Recoge el repuesto del almacén, repara el generador, activa control y embarca.
6. Consulta el [mapa y recorrido](docs/LEVEL01_PORT.md), la [validación del puerto](docs/PORT_VALIDATION.md)
   y la [validación inicial de la fundación](docs/VALIDATION.md).

Remoto del equipo: https://github.com/c90Beretta/Abisal-Stealness.
Revisa `docs/VALIDATION.md` para el estado verificado de la publicación.

## Documentación del equipo

- [Plan, alcance mínimo, backlog y responsables](docs/WORKPLAN.md)
- [KPI, método de medición y evidencia requerida](docs/KPI.md)
- [Auditoría de los 43 candidatos](docs/ASSET_AUDIT.md)
- [Assets realmente instalados y licencias](docs/ASSET_LEDGER.md)
- [Decisiones técnicas y excepciones al documento original](docs/DECISIONS.md)
- [Colaboración, Git LFS y publicación del remoto](CONTRIBUTING.md)
- [Bitácora](docs/DAILY.md), [créditos](CREDITS.md), [licencias](LICENSES.md)

La documentación original está preservada localmente en `docs/Refs/ORIGINAL_BRIEF.md`
(no se distribuye en el repositorio público; ver `docs/Refs/README.md`).
Contiene propuestas de automatización y agentes; no configura herramientas por sí sola.
