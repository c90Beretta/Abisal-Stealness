# Evidencia / puerto nocturno 0.4

`tests.json`: 18 EditMode y 20 PlayMode aprobadas. Incluyen la ruta de misión
completa, arma/munición y paredes, tratamientos, guardado, percepción/luz,
capacidad y persistencia de botín, extremidades, hordas finitas, melee,
armadura/O2 y estados/feedback de mochila.

`build-summary.txt`: Windows x64 Mono, 0 errores y 1 advertencia del demo de un
paquete externo (`PushPhysics.cs`, Rigidbody.velocity obsoleto). Ese demo no es
una escena habilitada del juego. No se modificó el archivo original del proveedor.

Se comprobó el arranque del ejecutable en modo headless durante 12 segundos,
sin excepciones registradas; se detuvo únicamente ese proceso auxiliar. No fue
una sesión humana ni una comprobación visual del jugador Windows.

Capturas runtime nativas a 1280×720 y mochila adicional a 1920×1080. La inspección
visual está en `docs/UI_REVIEW_NIGHT.md`: ship, R1 resolved, solo al alcance de la
corrección de estado y feedback. No se certifican balance, accesibilidad ni FPS.

`package.json` y `build-files-sha256.json` identifican el ZIP completo y sus 200
archivos. Se comprobaron todos los hashes del ZIP, los cuatro avisos Kenney y el
aviso Asset Store; se excluyeron símbolos, logs y carpetas DoNotShip.

La entrega está en la rama `codex/first-port-map`, PR1; los fuentes de Asset Store
permanecen locales. Las piezas físicas/manchas temporales no se serializan.
