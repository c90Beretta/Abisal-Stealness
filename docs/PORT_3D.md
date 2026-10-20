# Puerto 3D isométrico — v0.3

La escena jugable sigue siendo `Assets/_Project/Scenes/Level01_Port.unity`.
Abrir con `JUGAR ABISAL.cmd`, el acceso del escritorio o el menú Abisal de Unity.
En el menú, pulsar Comenzar nivel/Enter.

Se eliminó la presentación pixelada480×270. `IsometricView` renderiza el mundo
directamente al tamaño de la ventana; proyección ortográfica35.264°/45°, zoom por
rueda, MSAA4 y sombras suaves. El HUD no escala una imagen del mundo.

El puerto incorpora120 instancias de los tres paquetes locales: almacén/control
con muros y ventanas, suelos texturizados, contenedores industriales, cajas,
palet, barriles, estanterías, carro, generador, ordenador, puertas y equipos.
Se retiraron los cubos verdes de suministros en favor de cajas3D. La base del
muelle, grúa y casco del submarino siguen siendo geometría propia de prototipo.

Las teclas1/2/3 equipan pistola, carabina y dispersor, con los modelos3D Kenney
b/a/c, posición en las manos, punto de salida y retroceso visual. Cada arma tiene
daño, cadencia y dispersión por ScriptableObject; el dispersor lanza5 perdigones.
Las tres comparten el cargador12/reserva en esta versión. El cambio de arma no
crea munición ni permite saltarse tratamientos o recarga. Los checkpoints guardan
el arma equipada y siguen leyendo los guardados anteriores como pistola.

Se conserva la ruta repuesto → generador → autorización → submarino. Navegación
rehornada en `Port03_NavMesh.asset` y arte en `Port_Environment_3D.prefab`.
Las colisiones siguen presentes al ocultarse un objeto que tapa al personaje.

## Mantener y reproducir

Importar primero las copias autorizadas descritas en `LOCAL_ASSETS.md`. El mapa
público contiene referencias GUID; las fuentes originales de proveedor quedan
excluidas de Git. Los modelos de armas Kenney sí están versionados.

`Port3DBuilder.Build` es una migración explícita desde el puerto0.2, en una copia
de validación. Rechaza una escena que ya tenga `Port_Environment_3D` para conservar
ediciones. Para iterar, editar el nuevo prefab/escena; no volver a generar encima.

```powershell
./tools/Invoke-Unity.ps1 -Task PlayMode -ProjectPath <copia-de-validacion>
./tools/Invoke-Unity.ps1 -Task EditMode -ProjectPath <copia-de-validacion>
./tools/Invoke-Unity.ps1 -Task CapturePort -ProjectPath <copia-de-validacion>
./tools/Invoke-Unity.ps1 -Task Build -ProjectPath <copia-de-validacion>
./tools/Package-Playtest.ps1 -Version 0.3.0-3d-playtest
```

Pruebas y capturas actuales en `Validation/Port3D`; resultados finales y límites
en `PORT_VALIDATION.md`. El balance de las tres armas, duración y rendimiento
requieren playtests/perfilado; no se certifican por inspección del Editor.
