# Plan de trabajo — revisión ejecutable

Fecha: 18 octubre 2026. Ventana solicitada: **19 octubre–13 diciembre 2026**.
Fuente: `Refs/ORIGINAL_BRIEF.md`; decisiones de ejecución: `DECISIONS.md`.

Actualización de presentación por petición del usuario: **3D isométrico a resolución
nativa**, con los paquetes de entorno descargados y tres armas visibles. Se retira
el objetivo de ampliación pixelada480×270 de la entrega vigente; ver ADR-008 y
`PORT_3D.md`. Las menciones PSX de la planificación original son históricas y no
deben reintroducir el render pixelado sin nueva petición del usuario.

Estado del primer mapa: `Level01_Port` ya tiene trazado, misión repuesto/energía/
control/embarque, suministros opcionales, seis encuentros y NavMesh. Véanse
`LEVEL01_PORT.md` y `PORT_VALIDATION.md`. L01 avanza como prototipo; su duración,
balance y aceptación del MVP requieren playtests humanos.

## Entregas separadas

**F0, preparación actual:** proyecto preservado en Git, assets mínimos importables,
escena sandbox reproducible, paquetes fijados, pruebas y guías de equipo. Se acepta
con importación/compilación, escena sin scripts perdidos y un build de Windows.
No equivale al MVP de producto.

**MVP, puerta del 15 noviembre:** una ruta cerrada puerto → control → extracción,
10–15 minutos en una sesión, 2 enemigos (Shambler y Runner) × 2 variantes,
1 arma funcional, melee, ruido, loot limitado, inventario 4×3, suero con cuatro
fases y manía, puerta/llave/terminal, objetivo, muerte/reinicio, pausa y checkpoint.
Usar geometría provisional coherente. No exigir modelos específicos para validar mecánicas.

**Entrega completa, 13 diciembre:** cuatro niveles (puerto + tres secciones de
submarino), 3 armas, 13 arquetipos + Rider, 2 variantes por tipo, guardado/carga,
60–100 minutos y auditoría de créditos. El KPI completo sigue abierto hasta pasar
la matriz de `KPI.md`; recortarlo requiere registrar el cambio de meta.

## Responsabilidad y arquitectura

Los roles son personas asignables, no agentes ejecutados automáticamente.
Un responsable por tarea y un revisor distinto. Una persona puede cubrir varios roles.
Carpetas: `Runtime/Core` (datos y reglas), `Runtime/Gameplay`, `Runtime/AI`,
`Runtime/Presentation`, `Editor`, `Tests`, `Art`, `Audio`, `Prefabs`, `Scenes`.
No editar `ThirdParty` salvo intake; crear materiales, variantes y prefabs propios.
No editar una misma escena simultáneamente: separar nivel, luces y sistemas.

Cada ticket tiene ID, propietario, dependencias, criterio de salida y evidencia.
Máximo una tarea activa por persona; integrar por PR, con revisiones pequeñas.

## Backlog inicial

| ID | P | Responsable | Depende de | Entrega / aceptación |
|---|---|---|---|---|
| F01 | P0 | Integración | — | Git inicial, LFS, README y fuentes originales preservadas |
| F02 | P0 | Arte técnico | F01 | Selección mínima CC0, hash, licencia, escala y materiales URP |
| F03 | P0 | Gameplay/QA | F02 | Sandbox reproducible, compilación y build Windows |
| F04 | P0 | Integración | F01 | Publicar en c90Beretta/Abisal-Stealness, protección main y permisos |
| G01 | P0 | Gameplay | F03 | Movimiento isométrico, apuntado, disparo, recarga y melee |
| G02 | P0 | Gameplay | G01 | Salud, sangrado, ruido y daño; paredes bloquean disparos |
| G03 | P0 | Gameplay/UI | G01 | Inventario 4×3, rotación/arrastre, stacks y rechazo si está lleno |
| G04 | P0 | Gameplay | G02 | Suero en SO, 4 fases, inyección cancelable y manía; tests de fronteras |
| A01 | P0 | IA | G02 | Shambler/Runner; vista con oclusión, oído, búsqueda; NavMesh |
| A02 | P0 | IA/arte | A01 | 2 variantes por datos; sin duplicar lógica |
| L01 | P0 | Niveles | G01 | Puerto graybox: ruta, llave, terminal y salida; sin softlocks |
| S01 | P0 | Gameplay | G03,G04,L01 | Checkpoint versionado, estado de loot/enemigos/objetivo y archivo corrupto |
| U01 | P0 | UI/audio | G02,G04 | HUD legible, pausa, menú, feedback de inyección/daño/ruido |
| Q01 | P0 | QA | A02,S01,U01 | 5 sesiones externas, 10–15 min, evidencia y bugs; aceptar MVP |
| R01 | P0 | Arte técnico | F03 | 480×270 point, quantización/dither URP, muros y visión; aprobación visual |
| L02 | P0 | Niveles | Q01,R01 | 3 secciones submarino, objetivos y transiciones persistentes |
| A03 | P0 | IA | A02 | Civil, Bloater, Perro, Decraniated, Shy Guy y Humano |
| A04 | P1 | IA | A03,L02 | Glitchbug, Ant, Starfish, Mothman, Shadowstalker y Rider |
| G05 | P0 | Gameplay/audio | G01,Q01 | MP5/rifle/escopeta, tablas de loot y audio por superficie |
| Q02 | P0 | QA/arte | L02,A04,G05 | Balance, rendimiento GTX 1060, accesibilidad y partida completa |
| REL | P0 | Integración/QA | Q02 | RC, créditos, cero bloqueantes y build final reproducible |

## Calendario de ocho semanas

| Semana | Fechas | Resultado y puerta |
|---|---|---|
| 1 | 19–25 oct | Fundación, pipeline de importación, controlador y sala de control. Validar look y congelar versión del editor. Zara: spike de máximo 2 días **solo si se obtiene el paquete**; núcleo propio es la base provisional. |
| 2 | 26 oct–1 nov | G01–G04, A01 y ruta L01. Prueba de 10 min; combate, ruido y caída de suero observables. |
| 3 | 2–8 nov | A02, S01, U01, R01; build diario Windows. Pruebas de checkpoint y límites del inventario. |
| 4 | 9–15 nov | Q01: aceptar MVP en puerto. Luego ampliar primeros infectados y armas. Si falla, detener contenido nuevo. |
| 5 | 16–22 nov | Niveles 2–3 en bruto, A03, tres armas y mezcla de audio. Validar fases mentales y claridad espacial. |
| 6 | 23–29 nov | Nivel 4, A04 y Rider si hay capacidad. Congelar contenido el 29 nov. Puerta: cuatro niveles conectados y completables. |
| 7 | 30 nov–6 dic | Economía, cinco partidas completas, perfilado GTX 1060, controles y subtítulos. Puerta: 60–100 min sin bloqueos. |
| 8 | 7–13 dic | Solo correcciones. RC 9 dic; entrega 13 dic con matriz KPI, commit/tag y créditos. |

## Presupuesto de recursos verificable

Con caída de 1 punto/6 s se consumen **150–250 puntos** en 15–25 min.
Con inicio en 100 y piso deseado 40, necesidad nominal:
`max(0, ceil(((duración_s / 6) - 60) / 35))` dosis = **3–6 dosis**.
No sumar simplemente 10% al número de dosis: simular pérdidas por clamp a 100,
animación, exploración e inyecciones juntas. Ajustar loot para que al extraer quede
10–20% de las dosis disponibles en la ruta eficiente. Medir también muertes por
escasez y tiempo en cada fase. Ningún bot sustituye el playtest humano del terror.

## Puertas y recortes

Revisión humana del look (25 oct), control/suero (1 nov), MVP (15 nov),
cuatro niveles (29 nov), partida completa (6 dic) y RC (9 dic).
Revisión con build y evidencia concreta, no con promesas o capturas aisladas.
Recortar Kaiju, afijos, localización y rotación antes que guardado o legibilidad.
Quitar Mothman/Starfish o acortar nivel 4 altera KPI de contenido/duración:
registrar una nueva meta en `DECISIONS.md`; nunca marcar el KPI original cumplido.

## Definición de terminado

PR revisado, Unity compila, pruebas pertinentes pasan, escena/prefab sin referencias
rotas, licencia registrada, build cuando afecta runtime, evidencia vinculada,
commit y entrada en DAILY. No publicar assets de licencia restringida en un repo público.
