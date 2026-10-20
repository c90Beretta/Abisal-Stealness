# Validación — puerto 3D isométrico v0.3

La ampliación nocturna v0.4 tiene evidencia separada en `Validation/NightPort` y mecánicas en [NIGHT_PORT.md](NIGHT_PORT.md). Los resultados siguientes conservan la validación histórica v0.3.


18octubre2026 local, Unity6000.5.9f1 / URP17.5.0 / Windows x64 Mono.
Rama `codex/first-port-map`. La evidencia previa en `Port01` y `FirstLevel02`
describe versiones anteriores; esta revisión usa `Validation/Port3D`.

| Comprobación | Resultado | Evidencia |
|---|---|---|
| EditMode |18/18 PASS|[Casos](Validation/Port3D/tests.json): supervivencia, medicina y escenas con bindings válidos |
| PlayMode |12/12 PASS|Menú/pausa, tratamientos/muerte, checkpoint/arma equipada, misión/NavMesh, suministros/caída, regresión sandbox y armas3D |
| Mundo | Cámara nativa ortográfica35.264°/45°, MSAA4 | Prueba rechaza RenderTexture/RawImage de baja resolución; [HUD real](Validation/Port3D/first-level-hud.png) |
| Modelos |120 instancias de entorno y3 prefabs de armas|[Autoría](Validation/Port3D/port-3d-authoring.txt), [plano](Validation/Port3D/port-overview.png) y [vista de entrada](Validation/Port3D/port-player.png) |
| Armas |3 modelos, disparo, cambio y conservación de munición|[Pistola](Validation/Port3D/weapon-0.png), [carabina](Validation/Port3D/weapon-1.png), [dispersor](Validation/Port3D/weapon-2.png) |
| Windows |0 errores /1 advertencia de API de una demo del proveedor|[Resultado](Validation/Port3D/build-summary.txt) y [SHA256 por archivo](Validation/Port3D/build-files-sha256.json) |
| Repositorio |Metas, GUID, hashes CC0 y paquetes verificados|Fuentes Asset Store locales; datos propios y CC0 versionados |

Las pruebas de armas disparan desde el muzzle real de los tres modelos, comprueban
daño y consumo de1 cartucho incluso para5 perdigones; una pared entre pecho y
cañón bloquea el daño. Cambiar de arma no crea munición ni evita una curación/pausa.
El checkpoint conserva también la selección de arma; los guardados previos usan
pistola por defecto. JSON corrupto se rechaza.

La advertencia CS0618 corresponde a `SciFi Warehouse Kit/Demo/Scripts/PushPhysics.cs`
por usar Rigidbody.velocity, sustituido por linearVelocity en Unity6. La demo no
está en las escenas habilitadas del build; se conservó el código original del paquete.

La prueba de misión desactiva enemigos para aislar reglas y comprueba rutas
completas de NavMesh entre objetivos. Una prueba separada activa un infectado
y verifica preparación, esquiva, daño y pared. La navegación se rehorneó para
el mobiliario añadido, conservando colisiones en objetos ocultos por la cámara.

Las capturas se producen con la escena/canvases reales del motor a1280×720,
renderizando el mundo a resolución completa para componerlo con el HUD en batch.
Las tres vistas de armas usan zoom de inspección2.4, distinto al zoom normal del
juego, para revisar orientación y postura. No son una partida humana ni un perfilado.
Los estados de salud/munición mostrados en las capturas son datos de prueba.

El acceso del escritorio y `JUGAR ABISAL.cmd` apuntan al directorio Windows
completo. El ZIP incluye Data/UnityPlayer/Mono, créditos, licencias Kenney y aviso
de Asset Store; excluye símbolos y carpetas DoNotShip. Para editar desde otro
equipo hacen falta las copias autorizadas de `LOCAL_ASSETS.md`; el ejecutable
contiene ya los modelos del mapa.

Pendientes: validación humana de duración/dificultad/economía, perfilado en el
hardware objetivo, inventario por cuadrícula, variantes avanzadas, fases mentales
audiovisuales y niveles2–4. Grúa, agua y casco del submarino siguen siendo formas
3D propias de prototipo. Esta entrega no certifica los KPI globales.
