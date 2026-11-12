# Prueba unificada 0.7

Un solo `Level01_Port` combina el arsenal con el trabajo paralelo finalizado: cuatro perfiles (Kael, Viktor, Vance y Elena), apariencias y pasivas, seis armas de fuego, cuatro armas cuerpo a cuerpo, munición FMJ/HP/AP-I, combos, golpe pesado, bloqueo/parry, esquiva, enemigos especiales, hordas y HUD ampliados.

El puerto conserva la ruta repuesto→generador→control→submarino y añade helipuerto/laberinto sur, bio-cuarentena oeste y subestación norte. Se puede caminar entre sectores. F6/F7/F8 permiten trasladarse a ellos durante pruebas; no son la única forma de acceder. Se recalcula navegación una vez al construir los sectores y se desactivan colliders decorativos antes de hornearla.

El checkpoint conserva personaje, arma y grado de munición, arma cuerpo a cuerpo, inventario, botín y recursos restantes de las estaciones, además del estado previo del nivel y cadáveres. Los guardados anteriores sin personaje/grado usan los valores iniciales. Los recursos de salida se conceden una sola vez y no se repiten al continuar.

## Abrir la prueba

En la copia principal, `JUGAR ABISAL.cmd` y el acceso de escritorio apuntan al mismo ejecutable actualizado en `Builds/Windows`. El lanzador antiguo de Arsenal se redirige a esa misma versión. Las compilaciones anteriores se conservan como respaldo; no son otras pruebas actuales.

En el ZIP distribuido, extraer todo y abrir `JUGAR ABISAL.cmd`. Seleccionar personaje, arma inicial, melee y kit; después Comenzar/Enter. Las seis armas siguen disponibles mediante1–6 y comparten reserva. Los kits son suministros iniciales sujetos a límites, no mejoras permanentes.

- WASD mover; Shift correr; ratón apuntar; clic izquierdo disparar; R recargar.
- Espacio combo; G golpe pesado; clic derecho bloquear/parry; Alt/X esquivar; V aturdir; C cambiar melee.
- T grado de munición; F3/F4 personaje; Tab equipo; E interactuar; F linterna; F1 ayuda; F2 efectos.
- F5 guardar a salvo; F9 continuar; Esc pausa. F6–F8 accesos rápidos a los sectores durante pruebas.

La versión se conserva en `codex/arsenal-expansion` y PR2 para revisión; no se fusiona automáticamente. El trabajo paralelo fue tomado como base sin eliminar sus archivos. Se incorporó su último cambio de sectores y se añadieron las correcciones necesarias de integración y persistencia.

Ver `Validation/Unified/README.md` para identidad, pruebas y paquete. La revisión de armería se limita a su interfaz. Siguen pendientes valoración humana de gráficos/audio/animación, balance, sesiones externas y rendimiento en hardware objetivo. Los modelos son geometría procedural propia sobre la base de rig existente; no se promete arte final ni cuatro rigs independientes.
