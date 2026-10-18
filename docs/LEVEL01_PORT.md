# Nivel 01 — Puerto carguero

Primera versión jugable de trazado y misión. Escena:
`Assets/_Project/Scenes/Level01_Port.unity`. Unity6000.5.9f1 / URP.

## Recorrido

1. **Desembarco:** esquina suroeste, sin enemigos al alcance inmediato. Leer HUD y probar movimiento/apuntado.
2. **Patio de carga:** seis contenedores forman calles y rutas alternativas. Caja opcional al sur: +12 cartuchos y +1 dosis, una sola vez.
3. **Almacén 03:** entrada sur abierta; recoger el repuesto con E.
4. **Generador:** patio norte. E instala el repuesto, hace ruido y abre la puerta de control.
5. **Control:** edificio noreste sin techo; E en la terminal autoriza el embarque y abre el control del muelle.
6. **Submarino B-07:** salida al este por pasarela protegida; E junto al marcador completa el nivel. Enter reinicia.

El HUD indica el objetivo actual y la acción disponible al acercarse. La interacción
comprueba distancia y línea de visión. La terminal sin energía y el embarque sin
autorización se rechazan aunque el jugador llegue allí fuera de orden.

## Plano de orientación

```text
                          NORTE
   ┌────────────────────────────────────────────────┐
   │ ALMACEN 03        GRUA / GENERADOR    CONTROL   │
   │   repuesto             energía       terminal  │
   │   [entrada]                           [puerta] │
   │                                                │
   │  [contenedor]  [contenedor]  [contenedor]        │
   │       PATIO: calles y pasos alternativos        │
   │  [contenedor]  [contenedor]  [contenedor]        │
   │                                                │
   │        suministros                    [control]╞══PASARELA══B-07
   │ DESEMBARCO                                     │
   └────────────────────────────────────────────────┘
                          SUR
```

## Composición y límites

- Muelle principal: **56×44 m**, con agua, bordes, bolardos, grúa, almacén, torre de control y submarino.
- Cámara ortográfica30°/45°, RT480×270 point. Paredes y contenedores que tapan al jugador se ocultan temporalmente; sus colliders permanecen.
- Cuatro Shambler y dos Runner. Persiguen con rutas de NavMesh y mantienen percepción de vista/oído del prototipo.
- Inicio: salud100, suero65, 1 dosis, 12 cartuchos cargados +24 de reserva. Recursos opcionales limitados.
- Barandillas y controles físicos delimitan las rutas. Si se cae al agua se vuelve al desembarco y se pierden10 puntos de salud, para evitar un bloqueo por caída.
- La duración se debe medir en playtests. Esta versión no acredita aún los10–15 minutos ni el balance de economía del MVP.

## Editar en equipo

Geometría y decoración: `Assets/_Project/Prefabs/Levels/Port_Environment.prefab`.
Navegación: `Assets/_Project/Navigation/Port01_NavMesh.asset`.
El NavMesh es un binario horneado y se versiona en Git LFS.
La escena contiene jugador/cámara, encuentros e interactuables de misión. Los
controles móviles quedan fuera del prefab estático; usan obstáculos con carving
para actualizar la navegación al abrirse.

Editar el prefab con un propietario asignado. Si cambian los colliders, volver a
hornear el componente NavMeshSurface y comprobar el recorrido. La estructura base
se reproduce con `PortMapBuilder.Generate` en una copia sin la escena/navdata
generados; el comando rechaza sobrescribir una escena existente.

```powershell
./tools/Invoke-Unity.ps1 -Task GeneratePort -ProjectPath <copia-de-validacion>
./tools/Invoke-Unity.ps1 -Task EditMode -ProjectPath <copia-de-validacion>
./tools/Invoke-Unity.ps1 -Task PlayMode -ProjectPath <copia-de-validacion>
./tools/Invoke-Unity.ps1 -Task CapturePort -ProjectPath <copia-de-validacion>
./tools/Invoke-Unity.ps1 -Task Build -ProjectPath <copia-de-validacion>
```

La configuración de build coloca el puerto primero y conserva el sandbox como
segunda escena de pruebas. El build de Windows incluye ambas y comienza en el puerto.
Sin nuevos assets externos: primitivas propias y skins del pack Kenney ya auditado.
Modelos/animaciones, agua, sonido, UI y efectos PSX siguen siendo provisionales.
