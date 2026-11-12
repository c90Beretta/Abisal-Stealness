# Arsenal y encuentros especiales — 0.6

Extensión del primer puerto, manteniendo 3D isométrico y PS1 marcado. Las criaturas son originales; no se importaron modelos ni audio de Left 4 Dead.

## Preparación

El menú inicial añade un armario de selección. Asalto usa la dotación base; Explorador añade una pila y Sanitario un botiquín. Son especialidades del mismo soldado y rig, no tres personajes con modelos distintos.

Se elige arma inicial y un kit: una placa de repuesto, dos vendas o 24 cartuchos. Los beneficios se conceden una vez al comenzar una salida nueva; pausa y continuación no repiten la concesión. No hay árbol de mejoras permanente. Las placas se instalan desde TAB siguiendo las condiciones habituales.

Las seis armas se cambian con 1–6: corta, fusil de asalto, escopeta, precisión, subfusil y ametralladora pesada. Las tres nuevas derivan de la geometría original del fusil, con siluetas escaladas y parámetros propios; no son tres modelos finales detallados. Comparten munición y cargador de 12 de la base. Precisión alcanza 65 m; el resto 25 m. Cambiar arma no crea cartuchos y permanece bloqueado durante acciones. El checkpoint acepta índices 0–5.

## Combate y criaturas

- Espacio prepara el corte: impacto a 0.10 s con cuchillo o 0.18 s con hacha. Se conserva dirección durante el movimiento, se verifica distancia/arco/pared al impactar y el tercer golpe encadenado recibe 25% de daño adicional. Se mantienen costes y cooldowns. El empujón V consume 22 de resistencia, inflige 8 de daño y aturde 0.9 s; aún no desplaza físicamente al enemigo.
- Animación de arma con preparación/barrido/recuperación y capa de brazos sobre locomoción. Los ataques enemigos levantan/barren brazos durante preparación y recuperación.
- Perro infectado: geometría canina procedural, marcha de cuatro patas, 55 HP y velocidad 3.3. Su presentación y cadáver son una primera aproximación, con infraestructura física de la base; falta un rig y ragdoll caninos propios.
- Hinchado: silueta ensanchada, 110 HP, ataque a distancia con línea de visión cada 8 s y explosión al morir que daña dentro de 4 m y emite ruido para atraer enemigos. Las paredes bloquean daño. Todavía no tiene proyectil/salpicadura persistente ni cegado de pantalla.
- Coloso: silueta grande, 380 HP, daño 42 y preparación lenta; acelera durante 2.5 s al detectar una línea despejada, con recarga de 12 s. No lanza objetos del escenario.

Tres encuentros existentes se convierten de forma determinista por nombre, conservando identidades del guardado y límites de horda. No se añaden enemigos infinitos.

## Audio y HUD

Impactos, disparos, gruñidos y estallidos sintetizados originales, emisión espacial con caída de volumen por distancia y atenuación por obstáculos. Ventilación en bucle, pasos con volumen de marcha/carrera y ayuda actualizada. La mezcla necesita escucha y ajuste humano; no se afirma audio final grabado. El menú muestra selección persistente y beneficios antes de iniciar.

La entrega sigue necesitando balance, revisión humana de modelos/animación/sonido y medición de rendimiento. La revisión de armería tiene alcance de interfaz y no certifica todo el combate.
