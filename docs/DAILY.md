# Bitácora

## 18 octubre 2026 — preparación

- Inspección: plantilla URP sin gameplay; Git sin historial ni remoto; Unity6000.5.9f1 abierto.
- Baseline `3b225ed`, rama `codex/mvp-foundation`, Git LFS inicializado localmente.
- Plan revisado con F0, MVP de un nivel y meta final separados; KPI con métodos
  de aceptación, backlog y puertas hasta 13 diciembre. No se marcan KPI finales cumplidos.
- Inventario original: 43 enlaces en cinco HTML; fuentes preservadas; orden de
  imágenes corregido. Imágenes y brief originales conservados localmente, ignorados
  al publicar en repo público.
- Intake: cuatro packs CC0 oficiales Kenney, 26 archivos + cuatro licencias,
  manifiesto con SHA256 del ZIP y de cada archivo. Sin compras ni código de Zara/Zombtoy.
- Dependencias fijadas: añadir Cinemachine3.1.7 y Addressables2.9.1. Eliminar del
  manifest herramientas sin uso del template (AI Assistant preview, Inference,
  Pipeline experimental, Multiplayer Center, Collab Proxy y Visual Scripting).
  Se conservan en el commit baseline si se necesitan; Git es el flujo del equipo.
- Sandbox y generador Editor, reglas de suero en SO, import settings y pruebas.
  Validación real y limitaciones se registran en `VALIDATION.md` al concluir.
- Usuario proporcionó remoto: https://github.com/c90Beretta/Abisal-Stealness.git;
  estaba vacío y público. Sin sobrescribir historial ajeno.
- Siguiente trabajo de producto: G01–G04, inventario, save, IA NavMesh y pipeline
  PSX; asignar personas/revisores y ejecutar el primer playtest de la semana1.
- Validación cerrada: 16 EditMode + 2 PlayMode pasan; build Windows con 0 errores
  y 2 advertencias de API obsoleta. Captura revisada. Checkout LF y Git LFS verificados.
- Rama publicada y CI GitHub/Ubuntu verde en `da6892c`. Protección main aplicada
  (1 revisión, check repository, conversaciones, sin force-push/borrado; admin con bypass).
  Etiqueta de preparación: v0.1.0-foundation. Los límites pendientes figuran en VALIDATION.

## 18 octubre 2026 — primer mapa de puerto

- Rama `codex/first-port-map`, a partir de la fundación publicada.
- Escena Level01_Port con muelle56×44m, patio de seis contenedores, almacén,
  generador, control, pasarela y submarino. Geometría en prefab propio y NavMesh horneado.
- Misión: repuesto → energía → autorización en control → embarque. Caja opcional
  de munición/suero de un solo uso. Seis encuentros, con búsqueda de ruta sobre NavMesh.
- Interacciones bloqueadas por paredes, objetivos fuera de orden rechazados,
  recuperación de caída al agua y ocultación de muros/contenedores que tapan al jugador.
- Se conserva el sandbox; el build arranca en el puerto. Sin nuevas dependencias
  ni assets externos. Pruebas, captura y build se registran en PORT_VALIDATION.

## 18 octubre 2026 — primer nivel accesible y sistemas de supervivencia

- Acceso `JUGAR ABISAL.cmd`, instrucciones de juego y menú Abisal en Unity;
  Play inicia el puerto aun desde SampleScene. Build Windows en ventana1280×720.
- Menú inicial, HUD de salud/sangrado/suero/munición, equipo, ayuda, pausa,
  muerte y victoria. Marcador de objetivo con respaldo oscuro tras revisión visual.
- Botiquines/vendas, tiempos de aplicación, interrupción por daño, melee y
  linterna. Suministros recogibles de una sola vez y recarga con tiempo de uso.
- IA con patrulla, investigación, persecución, búsqueda, preparación de ataque,
  recuperación, impacto y muerte. Dos definiciones de infectados y clips Idle/Run.
- Checkpoint del generador, F5/F9, validación de JSON y recuperación de objetivos,
  puertas, recursos y enemigos abatidos.
- 18 EditMode +10 PlayMode pasan; build con0 errores/0 advertencias. Capturas
  compuestas desde canvases reales; no se completó partida humana de validación.
- Continúan pendientes inventario por cuadrícula, arsenal, efectos PSX completos,
  fases mentales audiovisuales y aceptación de KPI de duración/rendimiento/balance.
