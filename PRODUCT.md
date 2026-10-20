# Product

<!-- impeccable:product-schema 1 -->

## Platform

Windows nativo, Unity. El esquema de la herramienta contempla web/iOS/Android;
ninguno describe esta aplicación y no se registra una plataforma falsa.

## Product Purpose

ABISAL es un survival horror 3D isométrico. La petición humana vigente prioriza
una transformación gráfica inspirada en Mouthwashing, ragdolls y progreso
verificable de los KPI durante una ventana máxima de cinco horas.

## Operating Context

Se juega con teclado y ratón en un ejecutable Windows, o con Play en Unity
6000.5.9f1/URP. La primera misión recorre puerto, almacén, generador, control y
submarino. El equipo colabora mediante GitHub y Git LFS, con PR antes de integrar.

## Capabilities and Constraints

Conservar 3D y vista isométrica, supervivencia, armas, inventario, botín, hordas,
guardado y el protagonista soldado negro con tanque de oxígeno. No cambiar a
pixel art 2D. El usuario rechazó expresamente el diseño anterior. Las fuentes de
Asset Store permanecen locales y el equipo importa sus copias autorizadas.
El prefab ModularAsset_Sandbox contiene cambios del usuario que deben preservarse.

## Brand Commitments

Nombre ABISAL. Referencia visual explícita: Mouthwashing. Sus personajes,
logotipos e imágenes no se incorporan al juego; se trabaja arte original y assets
licenciados. El usuario confirmó PS1 marcado: baja resolución, dithering y textura inestable.
También confirmó construir en Unity y revisar capturas reales.

## Evidence on Hand

v0.4: build Windows, 18 EditMode y 20 PlayMode aprobadas y capturas en
`docs/Validation/NightPort`. No prueban aprobación del diseño: el usuario lo
rechazó. KPI completos en `docs/KPI.md`: faltan sesiones externas, cuatro niveles
y perfilado en GTX1060. PC local identificada con RTX3070Ti y Ryzen5600X.

## Product Principles

- La escena jugable y el ejecutable son la entrega, no una ilustración.
- No declarar cumplido un KPI sin su evidencia y hardware requeridos.
- Mantener los sistemas jugables y el trabajo del usuario al cambiar el arte.
- Versionar fuentes propias, metas, pruebas y documentación reproducible.

## Open Decisions

No se ha definido por el usuario una audiencia etaria ni un estándar de
accesibilidad. No se infieren. Tratamiento y revisión fueron confirmados por el usuario para
esta entrega; no se convierten en preferencias generales ajenas al proyecto.
