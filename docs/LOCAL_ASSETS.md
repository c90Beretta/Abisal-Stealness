# Entornos locales de Asset Store

Integrados en el primer puerto 3D por petición del usuario, 18 octubre 2026.

| Paquete / autor | Fuente local de Unity | Uso en el nivel |
|---|---|---|
| [Sci-Fi Construction Kit (Modular)](https://assetstore.unity.com/packages/3d/environments/sci-fi/sci-fi-construction-kit-modular-159280), Sickhead Games | `Assets/SciFi Warehouse Kit` | Muros, ventanas, suelos, estanterías, palet, cajas, barriles, carro |
| [Modular Sci-Fi Corridor](https://assetstore.unity.com/packages/3d/environments/sci-fi/modular-sci-fi-corridor-142811), MagixBox | `Assets/Sci Fi Modular Pack` | Partición industrial del generador |
| [Sci-Fi Old Rusty Props PBR](https://assetstore.unity.com/packages/3d/environments/sci-fi/sci-fi-old-rusty-props-pbr-200267), KMProd | `Assets/Data/Sci-fi_assets/Sci-Fi Old rusty props PBR` | Contenedores, generador, ordenador, puertas, equipos y suministros |

Las fichas de los tres paquetes indican Standard Unity Asset Store EULA.
Referencia: [términos oficiales, Appendix1 §2.2](https://unity.com/legal/as-terms).
Las fuentes originales permanecen locales mediante `.gitignore`; no se publican
los archivos de proveedor en el repositorio abierto. Los materiales adaptados y
escenas propios contienen referencias GUID a esos archivos locales. Los términos
de los paquetes siguen aplicando a su contenido incorporado al juego compilado.

Para trabajar sobre el mapa desde otro equipo, adquirir/importar las copias
autorizadas desde Asset Store / My Assets, conservando los `.meta` originales y
las rutas indicadas. No copiar estos paquetes a un repositorio público. El juego
compilado se distribuye completo para playtests y no requiere importar assets.

El paquete Rusty ya estaba descargado en el cache del usuario. `python
tools/import_local_rusty.py` importa únicamente modelos, texturas, materiales y
prefabs desde ese archivo local, sin scripts ni escenas demo. Su inventario y
SHA256 están en `local-rusty-manifest.json`. No descarga ni extrae credenciales.

Los originales Built-in permanecen sin conversión destructiva. `Port3DBuilder`
crea materiales URP en `_Project/Art/Materials/Imported3D`, reutilizando albedo,
normales, mapas metálicos y oclusión. La geometría estática, colisiones y NavMesh
propios están en `Port_Environment_3D.prefab` y `Port03_NavMesh.asset`.

Las armas pertenecen al subconjunto Kenney CC0 ya versionado, con modelos b/a/c
para pistola/carabina/dispersor. Los paquetes de armas AR/escopeta de la lista HTML
no estaban descargados en este proyecto; esta entrega no afirma haberlos instalado.
