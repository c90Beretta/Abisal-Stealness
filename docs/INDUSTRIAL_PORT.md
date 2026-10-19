# Puerto industrial 0.5

El usuario rechazó la presentación 0.4 y pidió acercarla a Mouthwashing. La
dirección cambia a cuarentena industrial, proporciones adultas, materiales mates
gastados y luces prácticas cálidas. Se conserva la cámara isométrica 3D. Es una
reinterpretación propia; no se han extraído modelos, texturas ni personajes de
Mouthwashing. La aceptación estética por el usuario continúa pendiente.

## Implementación

- Esclusa inicial con paneles, zócalos, marcos, tuberías, drenaje y señalización.
  Envolventes de mamparos y cubiertas transitables conectan cuarentena, almacén
  y control; las aberturas respetan la ruta de navegación. Se reutilizan los
  carros, estantes, conductos, ordenadores y equipos de los paquetes locales.
  Vestuario negro, casco, respirador y tanque del soldado. Cuerpos originales con
  19 huesos, malla ponderada y clips de locomoción propios.
- Cuatro arquetipos de infectado, dos variantes de datos por arquetipo presentes
  en el nivel: errante, corredor, acorazado y aullador. No equivalen a los trece
  tipos del objetivo final.
- Cada infectado tiene 13 cuerpos físicos y 12 articulaciones. El impacto
  activa el ragdoll; se inmoviliza al reposar o tras cinco segundos para acotar
  el coste. Los colliders de miembros perdidos quedan desactivados. El botín
  sigue a la pelvis del cadáver.
- Desmembramiento conserva materiales y UV; las piezas separadas usan una malla
  compacta y física temporal. Sangre, casquillos, humo e impacto tienen duración
  limitada; máximo 90 objetos de efectos gestionados.
- Tres armas de fuego y dos armas cuerpo a cuerpo con modelos originales de
  escala humana, bocas de cañón reales y materiales mates.
- HUD compacto, fuentes Barlow Condensed y Crimson Text, inventario opaco y
  daño en los bordes. Mundo PS1 a480×270, dithering ordenado, vértices cuantizados
  y UV oscilante; HUD nativo. F2 desactiva esos efectos, grano, bloom y sacudidas
  durante esa ejecución. El pulso del suelo informa ruido emitido y desaparece
  al cesar; no queda un aro permanente en reposo. Cámara32°/45°, zoom inicial2.85.
- Checkpoint v3 conserva orientación del actor, pose del cadáver y métricas de
  recursos. Lee partidas v1/v2; sus cadáveres sin pose vuelven a caer físicamente.

## Medición y pruebas

`IndustrialLevelTests` comprueba anatomía, variantes, caída física, límites de
pose, miembros sin colisión, botín, diez ciclos de guardado con recursos/timers y
rechazo de cuaterniones corruptos. `PortMapTests` comprueba la ruta navegable y
la secuencia de objetivos. Las capturas proceden de la escena y HUD de Unity.

Cada partida escribe un CSV anónimo **local**, sin envío de red, en
`Application.persistentDataPath/Playtests` al terminar, morir, reiniciar o salir.
Incluye SHA de compilación, origen, tiempo activo/simulado, munición encontrada,
disparos, dosis encontradas/usadas, suero desperdiciado, fases y cargas. Editor
y benchmark se identifican expresamente; no cuentan como sesiones externas.
Un checkpoint restaura los contadores para evitar contabilizar de nuevo botín
ya recogido. Los saves antiguos se marcan sin línea base de economía fiable.

El modo optativo `-abisal-benchmark 600 -abisal-output <carpeta>` recorre seis
zonas con teletransporte programado, 14 infectados vivos, cuatro cadáveres
reciclados y efectos repetidos. Calienta 60 segundos y mide 600 segundos a1080p
con VSync desactivado y FPS sin límite. Es estrés sintético del render/IA/física,
no una partida humana ni una medición del balance. Produce JSON de percentiles
CPU/GPU/frame y CSV por segundo; exige GPU activa y resolución correcta.

Los preflight ocultos D3D11/D3D12 de esta sesión devolvieron cero muestras GPU;
el instrumento los rechazó y sus percentiles no se usan como FPS del juego.
Para medir con la ventana visible, ejecutar manualmente
`./tools/Measure-Industrial.ps1 -Seconds 600 -ShowWindow` y mantener el juego
renderizando. Solo un resultado válido, con hardware y versión exactos, es
evidencia de rendimiento; una ventana oculta no lo garantiza.

Los criterios originales siguen en `KPI.md`: cinco partidas externas y hardware
GTX1060 son condiciones separadas. Una medición con RTX3070Ti no certifica esa
GPU. No se añaden esperas artificiales para aparentar diez minutos de nivel.

## Autoría y reproducción

`HumanCharacterBuilder`, `IndustrialWeaponsBuilder` e `IndustrialPortBuilder`
contienen el arte geométrico y el montaje. El upgrade se ejecuta una vez sobre
la escena0.4 en una copia de validación, conservando los GUID de los assets.
La escena0.5 versionada se abre directamente; no volver a ejecutar el upgrade
encima de ella. Los paquetes privados siguen las instrucciones de
`LOCAL_ASSETS.md`; no se publican sus fuentes.

Estado visual: dos revisiones solicitaron `rebuild`; el usuario autorizó
reconstruir interiores y soldado. La revisión completa posterior pidió dos fixes,
resueltos según `UI_VERDICT_INDUSTRIAL.md`: separación de pantalón/rodillas/botas
y cara operativa de control. Su `ship` cierra F1/F2, sin certificar aceptación
estética humana ni una nueva auditoría global. Informes históricos en
`UI_REVIEW_INDUSTRIAL_ROUND1.md` y `UI_REVIEW_INDUSTRIAL_ROUND2.md`.
La primera revisión interrumpida por cuota fue reemplazada por una revisión
fresca, sin contarla como aprobación.

Las cuatro texturas originales —esmalte, rostro, tejido gastado y atlas táctico— están en `_Project/Art/Industrial/Textures`, con
prompts exactos embebidos en PNG y archivos `.provenance.json`. Modo: generación
nueva con la herramienta integrada `image_gen`, fondo opaco, sin referencias
de imágenes. Fuentes y licencias OFL: `font-install-manifest.json`.

`InteriorRebuild` crea mamparos visibles, bancos y equipo agrupado, cubiertas
pintadas y luces locales. `InteriorCutaway` oculta las mallas próximas mientras
se ocupa el cuarto: colisiones y líneas de visión siguen activas. La cámara
adelanta su foco1.15m enX/Z. El atlas del soldado separa regiones anatómicas;
botas, protección y textil tienen materiales propios. Las consolas usan pantalla
en el mundo3D y teclado geométrico; `TerminalDisplay` refleja PowerOn y
BoardingAuthorized reales, sin sustituir la información principal del HUD.

El render URP se produce al tamaño de la ventana; después se reduce a altura270
y se amplía con filtro Point/dithering. Es una presentación PS1, no una promesa
de coste de sombreado equivalente a270p. El HUD se compone a resolución nativa.
