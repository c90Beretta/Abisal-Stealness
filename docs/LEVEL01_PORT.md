# Nivel 01 — Puerto carguero

Primera versión jugable de trazado y misión. Escena:
`Assets/_Project/Scenes/Level01_Port.unity`. Unity6000.5.9f1 / URP.
La entrega vigente es el [puerto3D nativo v0.3](PORT_3D.md), con assets del usuario
y tres modelos de armas; las evidencias0.1/0.2 son históricas.

## Recorrido

1. **Desembarco:** esquina suroeste, sin enemigos al alcance inmediato. Leer HUD y probar movimiento/apuntado.
2. **Patio de carga:** seis contenedores forman calles y rutas alternativas. Caja opcional al sur: +12 cartuchos y +1 dosis, una sola vez.
3. **Almacén 03:** entrada sur abierta; recoger el repuesto con E.
4. **Generador:** patio norte. E instala el repuesto, hace ruido, abre la puerta de control y guarda un checkpoint.
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
- Cámara ortográfica35.264°/45° a resolución nativa, zoom por rueda y MSAA4. Paredes y contenedores que tapan al jugador se ocultan temporalmente; sus colliders permanecen.
- Cuatro Shambler y dos Runner, definidos por ScriptableObjects. Patrullan, investigan ruido, persiguen con NavMesh, buscan y preparan el golpe antes de atacar. Los golpes pueden esquivarse y las paredes bloquean la vista y el daño.
- Animaciones Kenney de espera/carrera, reacción al impacto, caída al morir y barras de vida de infectados alertados/heridos.
- Inicio: salud100, suero65, 1 dosis, 2 botiquines, 3 vendas, 12 cartuchos cargados +24 de reserva. Tres cajas adicionales de medicina/munición; recursos opcionales de un solo uso.
- H aplica botiquín (+40 vida, 1.6s); B venda (detiene sangrado, 1.2s); Q suero (2s). Un golpe interrumpe el tratamiento. Recargar tarda1.5s; no se dispara durante tratamientos/recarga. Espacio da un golpe cercano; F alterna linterna.
- Menú inicial, HUD nativo, F1 ayuda, Esc pausa, pantallas de muerte/victoria. TAB muestra existencias de equipo; todavía no es un inventario por cuadrícula.
- Checkpoint JSON local al reparar generador o F5 lejos de enemigos; F9 o Continuar recuperan salud, posición, munición, suministros, infectados abatidos, objetivos y puertas.
- Barandillas y controles físicos delimitan las rutas. Si se cae al agua se vuelve al desembarco y se pierden10 puntos de salud, para evitar un bloqueo por caída.
- La duración se debe medir en playtests. Esta versión no acredita aún los10–15 minutos ni el balance de economía del MVP.

## Editar en equipo

Geometría y decoración: `Assets/_Project/Prefabs/Levels/Port_Environment_3D.prefab`.
Navegación: `Assets/_Project/Navigation/Port03_NavMesh.asset`.
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
./tools/Invoke-Unity.ps1 -Task UpgradePort -ProjectPath <copia-de-validacion>
./tools/Invoke-Unity.ps1 -Task EditMode -ProjectPath <copia-de-validacion>
./tools/Invoke-Unity.ps1 -Task PlayMode -ProjectPath <copia-de-validacion>
./tools/Invoke-Unity.ps1 -Task CapturePort -ProjectPath <copia-de-validacion>
./tools/Invoke-Unity.ps1 -Task Build -ProjectPath <copia-de-validacion>
```

La configuración de build coloca el puerto primero y conserva el sandbox como
segunda escena de pruebas. El build de Windows incluye ambas y comienza en el puerto.
El puerto actual utiliza los tres paquetes de entorno del usuario y armas Kenney;
ver `LOCAL_ASSETS.md`. El agua, grúa y submarino conservan geometría propia de
prototipo. La presentación pixelada del plan inicial se sustituyó por3D nativo.

`GameplayUpgrade.Upgrade` es una migración explícita de sistemas del puerto;
reutiliza clips y definiciones propios, sin regenerar la geometría. Solo ejecutarla
en una copia limpia o con los cambios de escena guardados/versionados. Los clips
derivados comparten referencias y no se duplican dentro de la escena.
