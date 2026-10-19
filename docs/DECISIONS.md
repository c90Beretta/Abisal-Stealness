# Decisiones

## ADR-001 — Alcance de la preparación (18 octubre 2026)

El usuario pide plan, assets mínimos y control de versiones. El documento adjunto
es fuente de requisitos y propuestas; sus instrucciones de Claude/agentes/MCP
no son autorización independiente. No se crean siete agentes ni se inicia trabajo
autónomo de ocho semanas. Se entrega fundación reproducible, MVP acotado y roadmap.
El catálogo completo permanece como objetivo final, con recortes explícitos.

## ADR-002 — Editor y paquetes

Instalado: **6000.5.9f1 (Unity 6.5, no LTS)**. Preservarlo ahora evita migrar un
proyecto abierto durante el intake. 6.5 ya alcanzó fin de soporte según su anuncio;
antes de producir contenido validar una copia con LTS vigente, URP y build Windows.
No se declara cumplimiento del requisito LTS. No actualizar editor de forma implícita.

Conservar URP17.5.0, Input1.20.0, Navigation2.0.14, TestFramework1.7.0 ya instalados.
Añadir Cinemachine3.1.7 y Addressables2.9.1, versiones publicadas para 6000.5;
fijar lockfile. Shader Graph llega como dependencia URP.
Addressables se prepara como herramienta; el sandbox usa referencias directas.
Para reducir dependencias del template se retiraron AI Assistant preview,
Inference, Pipeline experimental, Multiplayer Center, Collab Proxy y Visual
Scripting (sin consumidores en el proyecto vacío). El baseline conserva el estado
anterior. UI usa `UnityEngine.UI` y cámara fija propia en este sandbox;
Cinemachine está instalado, aún no enlazado al rig de producción.

Fuentes consultadas el 18 octubre:
- https://unity.com/releases/editor/whats-new/6000.5.9f1
- https://discussions.unity.com/t/unity-6-5-is-now-available/1723176
- https://docs.unity3d.com/6000.5/Documentation/Manual/com.unity.cinemachine.html
- https://docs.unity3d.com/6000.5/Documentation/Manual/com.unity.addressables.html
- https://packages.unity.com/com.unity.cinemachine
- https://packages.unity.com/com.unity.addressables

## ADR-003 — Assets de fundación

Los HTML solo contienen URLs. No hay paquetes Asset Store en la caché consultada.
Se seleccionan subconjuntos **CC0 de Kenney** para entorno modular, armas,
personajes y audio, más primitivas propias. No se compra nada. No se importa todo
el catálogo, demos, scripts de terceros ni formatos duplicados. Registrar ZIP SHA256,
archivos seleccionados y licencia original. No asignar licencias por nombre de modelo.
Los archivos de referencia adjuntos se usan para orientación interna, no como arte del juego.

## ADR-004 — Núcleo propio y Zara

Núcleo propio con SO y módulos pequeños, según opción B del brief. Zara queda como
spike pendiente del paquete, no como prueba fallida ni aprobado. Su ficha indica
1.06 (2021), Unity2019.4 y EULA de Asset Store; gratuidad no prueba compatibilidad.
Aceptar solo si compila en editor/URP acordados, cámara ortográfica/personaje propio,
inventario y save controlables por datos. Límite: 2 días. No bloquear MVP por ello.
https://assetstore.unity.com/packages/templates/systems/zara-survival-engine-182386

## ADR-005 — Automatización y puente Unity

Evaluados documentalmente CoplayDev/unity-mcp e IvanMurzak/Unity-MCP. Ambos ofrecen
escenas/objetos/console/tests; agregan dependencias de editor/servidor y revisión
de versión/licencia. No son necesarios para este intake. No se instala un servicio
ni se modifica la configuración global del asistente. Scripts Editor y batchmode
son la vía reproducible inicial. MCP se reevalúa si reduce trabajo real del equipo.
No se ha confirmado un MCP oficial incluido en esta instalación.
- https://github.com/CoplayDev/unity-mcp
- https://github.com/IvanMurzak/Unity-MCP

## ADR-006 — Versiones y distribución

Git con baseline recuperable, rama `codex/mvp-foundation`, LFS para binarios
y YAML textual con metas. El usuario indicó `c90Beretta/Abisal-Stealness`; se
confirmó remoto vacío y público. Conservar imágenes/brief originales solo localmente;
publicar fuentes propias, inventarios de enlaces y assets CC0. No inventar permisos
ni invitaciones. Licencia del código propio queda
reservada al titular hasta que el equipo decida; CC0 de Kenney se conserva.

## ADR-007 — Referencias y presentación

Los números del brief están invertidos respecto a los adjuntos: imagen1 es
legibilidad doméstica, imagen2 es sala verde del submarino. Se preservan con nombres
descriptivos. No se adjuntaron capturas de Endoparasitic: guía UI provisional,
sin afirmar análisis de capturas inexistentes. El sandbox no certifica look PSX final.

## ADR-008 — Mundo 3D isométrico nativo y assets del usuario

Petición posterior del usuario, 18octubre2026: implementar el mapa con sus assets
descargados y modelos de armas; descartar el aspecto pixelado2D y mantener vista
isométrica3D. Esta petición sustituye la presentación480×270/point del brief inicial.
El mundo renderiza directamente a la ventana con cámara ortográfica35.264°/45°,
zoom por rueda, texturas PBR, sombras suaves y MSAA4; el HUD sigue independiente.

Se integran los paquetes locales Warehouse/Modular Corridor/Old Rusty Props y tres
modelos Kenney de armas. Se conservan colisiones, misión y encuentros del puerto,
con NavMesh rehecho para el mobiliario añadido. Asset Store queda local, con guías
de importación para colaboradores; fuentes propias y CC0 siguen en Git público.
`LOCAL_ASSETS.md` recoge rutas, autores y términos. El binario integra el nivel.
El nuevo look no certifica todavía rendimiento, dificultad ni duración.
