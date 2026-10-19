# Puerto nocturno / v0.4

Primera integración jugable de los sistemas nocturnos solicitados. El nivel conserva
la ruta almacén → generador → control → submarino y el render 3D isométrico nativo.

## Luz y sonido

La iluminación ambiental y lunar es mínima. F enciende una linterna con sombras,
alcance de 18 m y carga consumible. Una luz próxima permite orientarse sin linterna.
El círculo de percepción tiene 2.7 m y se recorta contra obstáculos; el pulso naranja
representa el ruido reciente (se dibuja hasta 7 m para mantener legible el mapa).
El HUD muestra el alcance real del ruido. No es un minimapa de enemigos.

Los infectados se muestran dentro del círculo o del cono frontal iluminado, con
comprobación de línea de visión. Sus gemidos espaciales permiten oírlos antes de
verlos. Las paredes atenúan el oído; correr, disparar, recargar y tratar heridas
producen ruido. Apagar la linterna reduce la distancia a la que te ven.
Reparar el generador activa luces de servicio en almacén, control y muelle.
Vignette, grano, bloom y ACES se ejecutan mediante un perfil URP.

## Infectados y hordas

Hay seis patrullas y dieciséis reservas colocadas en puntos navegables del puerto.
Las reservas despiertan en su posición, a más de 9 m del jugador: no aparecen en
su luz. Cada oleada activa hasta cuatro enemigos, con 24 s entre oleadas y un
límite de catorce infectados vivos activos. No se reponen los abatidos.
El ruido acumulado, el generador y los gritos pueden atraer estas reservas.

| Variante | Cuerpo y equipo | Comportamiento |
|---|---|---|
| Errante | Cuerpo medio | Patrulla, investiga sonidos y busca tras perder contacto |
| Corredor | Malla estrecha | Persigue a 2.8 m/s |
| Acorazado | Malla ancha, protección y casco | 180 de vida; reduce daño balístico al torso; golpe más lento y fuerte |
| Aullador | Garganta y crecimiento dorsal expuestos | Grita al perseguir; atrae aliados y reservas |

Las cuatro mallas propias se derivan del cuerpo CC0 de Kenney y comparten su rig;
no son cuatro paquetes de personajes comprados ni modelos fotorrealistas.
La navegación usa el NavMesh del puerto y separación próxima entre enemigos.

## Combate y gore

Las tres armas conservan modelos 3D y comparten munición de prototipo. Añaden
destello, luz breve, humo, casquillos, impactos y sacudida de cámara. Apuntar sobre
el cuerpo con el ratón permite variar la altura del impacto. Un disparo suficiente
a la cabeza la separa y mata. El acorazado tiene protección en el torso.

ESPACIO usa el cuchillo o hacha elegidos en TAB. Consume resistencia, tiene tiempo
de recuperación y comprueba ángulo, alcance y paredes. El hacha puede alcanzar
hasta tres enemigos. Correr también gasta resistencia.

El desmembramiento separa triángulos ponderados de cabeza, brazos o piernas, crea
una pieza física temporal y tapa la herida. Perder piernas ralentiza al infectado;
perder brazos debilita su ataque. Sangre y piezas tienen una duración limitada;
los efectos de combate mantienen un máximo de 90 objetos gestionados.
Es desmembramiento de malla por regiones, sin simulación anatómica ni ragdoll completo.

## Soldado, equipo y botín

El protagonista lleva ropa negra, chaleco, casco, máscara, arnés, tubo y el tanque
Oxy_ballon del paquete local Old Rusty Props. La máscara equipada consume oxígeno;
protege de la fuga tóxica junto al generador. Sin oxígeno o sin máscara, la zona
produce daño. El chaleco absorbe el 35% del daño físico hasta agotar sus placas.

TAB abre una mochila de 24 kg con munición, tratamientos, pilas, oxígeno y placas.
Permite cambiar las tres armas, el arma cuerpo a cuerpo, máscara y chaleco, además
de sustituir consumibles cuando su carga es inferior o igual al 70%.
El mundo sigue avanzando mientras está abierta; ESC pausa.

E registra cuerpos abatidos y cuatro contenedores: desembarco, almacén, servicio
y control. «Recoger lo que quepa» respeta peso y límites de pila; las sobras quedan
en el contenedor. Las cajas antiguas y suministros de misión también comprueban
la capacidad. Los tratamientos mantienen sus teclas H, B y Q.

## Guardado y colaboración

El checkpoint versión 2 conserva mochila, cargas, equipo, oleadas y reservas
liberadas, contenido restante de cajas y cuerpos, estado y miembros de infectados.
Los guardados versión 1 siguen siendo legibles; sus sistemas nuevos empiezan con
los valores iniciales, porque esos datos no existían en la versión anterior.
Las piezas físicas, manchas temporales y sonidos no se serializan.

La migración explícita `NightPortBuilder.Upgrade` parte de una escena v0.3 y evita
aplicarse dos veces. No debe ejecutarse sobre el puerto nocturno ya editado.
`FinalizeNight` sincroniza referencias de audio/material y colocación de cajas.
El audio nuevo se sintetiza con `python tools/generate_night_audio.py`, con semilla
fija y sin grabaciones externas. Las fuentes Asset Store continúan locales según
[LOCAL_ASSETS.md](LOCAL_ASSETS.md); cada colaborador importa su copia autorizada.

La evidencia automática de esta entrega se registra en `docs/Validation/NightPort`.
Las pruebas funcionales y capturas por lote no sustituyen una sesión humana de
balance ni certifican todavía los KPI de producción del juego completo.
