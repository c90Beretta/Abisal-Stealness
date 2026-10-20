# KPI y evidencia

Los objetivos son metas, no resultados medidos. Estado inicial: pendiente salvo
los resultados explícitos de `VALIDATION.md`. Los cambios de umbral llevan ADR.

| KPI | MVP (15 nov) | Final (13 dic) | Cómo se acepta |
|---|---|---|---|
| Ejecutable | Windows x64, iniciar/salir/reiniciar | Mismo; paquete distribuible | Equipo limpio, registrar SHA/versión/fecha y log |
| Progreso | 1 nivel, 10–15 min | 4, total 60–100 min, normalmente 15–25 por nivel | 5 sesiones externas; ≥4/5 completan sin ayuda ni softlock |
| Enemigos | 2 tipos × 2 variantes | 13 tipos × 2 + Rider × 2; Kaiju opcional | Catálogo de SO + encuentro demostrable por tipo; variantes alteran datos |
| Supervivencia | Suero, salud, ruido y 1 arma | Sangrado y 3 armas completos | Tests de transiciones y playtest; ninguna munición/suero infinitos |
| Suero | 4 fases y manía | Igual, integrado en 4 niveles | 1 punto/6 s; +35/2 s; temblor20 s; 2 inyecciones/60 s → manía30 s, bloqueo10 s; límites y pausa probados |
| Economía | 10–20% de dosis al extraer ruta eficiente | Por nivel, con semilla/versionado | Mediana de 5 rutas exitosas; registrar disponibles/usadas/desperdiciadas |
| Checkpoint | Reinicio conserva estado | En los 4 niveles y transiciones | 10 ciclos guardar/cargar; salud, loot, enemigos, misión y timers sin duplicación; corrupción manejada |
| Rendimiento | Sin objetivo certificado antes de perfilado | 1080p en GTX1060; 60 FPS | Build Release, VSync off, ruta peor caso 10 min, warmup60 s; p95 frame≤16.67 ms, p99≤25 ms; CPU/GPU/hardware declarados |
| Calidad | 0 P0 y 0 errores de consola de juego | 0 P0, bugs P1 explícitamente aceptados | Lista de bugs con severidad, test y evidencia; caída/softlock/pérdida de save = P0 |
| Licencias | 100% de assets importados en ledger | 100%, créditos incluidos en build | Autor, URL, texto de licencia, hash, obligación y permiso de distribución por asset |
| Colaboración | Clone + LFS + importación por segunda persona | Build reproducible desde tag | Nueva carpeta/equipo; lock de paquetes idéntico; ningún Library/Temp ni secreto versionado |

## Registro de sesiones

Guardar CSV en `docs/Playtests/` (sin datos personales):

`build_sha,session_id,level_id,duration_s,completed,deaths,ammo_start,ammo_looted,ammo_used,serum_doses_available,serum_doses_used,serum_wasted,phase0_s,phase1_s,phase2_s,phase3_s,mania_count,save_cycles,p0_count,notes`

Para perfilado adjuntar CSV/Profiler capture en artefactos de release, con enlace
desde VALIDATION. Las mediciones del Editor o de otra GPU no certifican GTX1060.

## Estado de la base industrial 0.5 — 19 oct 2026

| Criterio | Evidencia disponible | Estado |
|---|---|---|
| Nivel y progresión | Ruta NavMesh completa, fusible→generador→control→extracción probada | Integrado; duración y4/5 completan sin ayuda pendientes de cinco personas |
| Enemigos MVP | Cuatro arquetipos×dos variantes de datos, cuerpos adultos y encuentros fixture | Catálogo supera mínimo; legibilidad visual aceptada por revisor, balance externo pendiente |
| Supervivencia | Suero, salud, sangrado, armas, ruido, recursos y pausa con regresiones | Pruebas funcionales; no balance certificado |
| Checkpoint | Diez ciclos: pose19huesos, orientación, salud, loot, recursos, temporizadores y contadores; corrupción rechazada | Cumple el escenario automatizado; sesiones externas y migración real de saves históricos pendientes |
| Economía | CSV local por partida con origen/build, recursos y fases | Instrumentación integrada; faltan cinco rutas humanas exitosas para la mediana10–20% |
| Rendimiento | Preflight D3D11/D3D12 oculto devuelve cero muestras GPU, rechazado automáticamente | Sin FPS válido; ruta10min/GTX1060 pendiente |
| Calidad gráfica pedida | Reconstrucción adicional autorizada y capturas Unity720p/1080p | Revisión completa posterior fix; verdict ship soloF1/F2 resueltos. Aceptación estética humana pendiente |
| Colaboración | Rama/PR, LFS, fuentesOFL, ledger, packs privados fuera del Git público | Comprobaciones locales disponibles; segunda persona/equipo pendiente |

Los26PlayMode y18EditMode no sustituyen aceptación estética, FPS ni playtests.
Consultar `Validation/IndustrialPort/README.md` para hashes y alcance de evidencia.
