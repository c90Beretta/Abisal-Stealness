# Zombtoy — survival horror isométrico

Proyecto Unity 6 / URP con **mundo 3D y cámara isométrica a resolución nativa**.
Meta de producción: 4 niveles, economía de suero y munición e IA por percepción.
Incluye una **primera versión jugable del puerto**
y el sandbox técnico. El juego completo y sus KPI de producción siguen en desarrollo.

## Jugar sin abrir Unity

En esta copia de trabajo, haz doble clic en **JUGAR ABISAL.cmd** y pulsa
**Comenzar nivel / Enter**. El ejecutable esta en `Builds/Windows/AbisalStealness.exe`.
El paquete Windows debe conservarse completo, incluida su carpeta Data.
Consulta `LEEME PARA JUGAR.txt` para controles y recorrido.

Para el equipo: [descargar el puerto 3D para Windows](https://github.com/c90Beretta/Abisal-Stealness/releases/tag/v0.3.0-3d-playtest).
Extrae el ZIP completo y abre `AbisalStealness.exe`; no requiere Unity instalado.

## Desarrollar en Unity

1. Instala Git, Git LFS y **Unity 6000.5.9f1**, Windows Build Support (Mono).
2. `git clone https://github.com/c90Beretta/Abisal-Stealness.git`; `git lfs install`; `git lfs pull`.
   Mientras se revisa el PR1, usa `git switch codex/first-port-map` y `git lfs pull`
   para trabajar sobre esta entrega; la rama main conserva la versión integrada anterior.
3. Abre esta carpeta (la que contiene `Assets`, `Packages`, `ProjectSettings`) en Unity Hub.
   Importa tus copias autorizadas de los [tres paquetes de entorno](docs/LOCAL_ASSETS.md).
   Sus fuentes de Asset Store permanecen locales; el ejecutable sí contiene el nivel completo.
4. Espera la resolución de paquetes. Usa **Abisal > Abrir puerto y jugar**.
   Play también inicia el puerto aunque estés editando SampleScene.
5. Pulsa **Comenzar nivel**. WASD mover, ratón apuntar, clic disparar, R recargar,
   espacio golpe, Q suero, H botiquín, B venda, F linterna, E interactuar y Esc pausa.
   **1/2/3** cambian pistola, carabina y dispersor; **rueda del ratón** ajusta el zoom.
   Recoge el repuesto del almacén, repara el generador, activa control y embarca.
6. Consulta el [mapa y recorrido](docs/LEVEL01_PORT.md), la [validación del puerto](docs/PORT_VALIDATION.md)
   y la [validación inicial de la fundación](docs/VALIDATION.md).

El nivel incluye dos tipos de infectados con patrulla, vista, oído, búsqueda,
ataque con preparación y muerte; animaciones de espera/carrera; vida, sangrado,
tratamientos con tiempo de uso, botiquines y munición recogibles; HUD, ayuda,
pausa, muerte/victoria y checkpoint del generador. F5 guarda cuando estás a salvo;
F9 recupera. TAB muestra las existencias de equipo, sin inventario por cuadrícula.

El mapa integra muros, suelos, contenedores, estanterías, barriles, maquinaria,
ordenador y puertas de los assets descargados. Cámara ortográfica35.264°/45°,
sombras suaves y MSAA4. Se retiró la ampliación480×270: el mundo se renderiza
directamente al tamaño de la ventana. [Detalle y evidencia de esta entrega](docs/PORT_3D.md).

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
