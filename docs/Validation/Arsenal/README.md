# Arsenal 0.6 — evidencia de entrega

Fuente compilada: `dda2f8ee753d156fbc79282670e6b634cf039573`. El tag de entrega añade documentación. [Identidad](build-identity.json), [resultados](tests.json), [compilación](build-summary.txt) y [paquete](package.json).

**27 PlayMode +18 EditMode aprobadas** en la escena actual con navegación rehecha. La prueba de armería selecciona especialidad, arma y kit, mueve el foco a otra opción, comienza la misión y comprueba arma equipada y concesión única de munición incluso tras pausa. Las regresiones comprueban las seis armas, bloqueo por paredes y tratamiento, preparación del corte, gasto de resistencia, guardados y ruta de objetivos. El caso de desmembramiento humano selecciona un humanoide, pues el perro no tiene anatomía humana visible.

Build Windows 0.6.0: **0 errores /1 advertencia**, correspondiente a código obsoleto `Rigidbody.velocity` de una demo del proveedor. No se presenta como medición de FPS ni como prueba de escucha humana.

ZIP: **118068596 bytes**. SHA256: `47456dc3021df34ce19e60812d41df6aa2a8dbd0a6dcc32da20999f9305a5ea5`. Se verificó la integridad CRC de todas las entradas. Contiene créditos/licencias y excluye PDB, logs y carpetas DoNotShip.

![Armería en Unity](arsenal-menu.png)

Captura real de Unity 6000.5.9f1 a1280×720, escena actual y HUD compuesto por `FirstLevelTests.Capture`. La fixture selecciona Explorador, Pesada y Reserva, y coloca foco de teclado en Corta. No es imagen generada ni sesión humana. El PNG conserva metadato de origen; no se editaron sus píxeles. La captura inválida temporal de fuentes LFS sin materializar fue descartada; la entrega usa assets materializados y sus pruebas pasan.

[Revisión](../../UI_REVIEW_ARSENAL.md) y [veredicto](../../UI_VERDICT_ARSENAL.md): **ship solo para F1/F2 resueltos**, selección y beneficios. No certifica estética global, audio, modelos, animación ni rendimiento.

El trabajo confirmado en paralelo sobre `SurvivorRoster.cs` y posteriores cambios de combate permanece fuera de esta rama/compilación. No se restauró ni sobrescribió ese trabajo. La rama `codex/arsenal-expansion` mantiene sus propios commits, escena actual y NavMesh Port06. Los cambios que quedaron en la copia compartida deben reconciliarse con el otro trabajo antes de integrarlos.

Para jugar esta compilación en la copia principal: `JUGAR ARSENAL.cmd`, que abre `Builds/Arsenal-0.6/AbisalStealness.exe`. Para el equipo: [descarga0.6](https://github.com/c90Beretta/Abisal-Stealness/releases/tag/v0.6.0-arsenal-playtest). Extraer ZIP completo y abrir `JUGAR ABISAL.cmd`. Ver [funciones y límites](../../ARSENAL.md): especialidades del mismo soldado, tres armas derivadas, munición compartida, perro procedural y mejoras iniciales de suministros; faltan personajes propios e integración del roster paralelo, rig canino final, progresión permanente, balance y validación humana/GPU.
