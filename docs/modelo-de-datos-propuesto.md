Modelo de datos propuesto de NEXUS

1. Objetivo

El objetivo es que NEXUS pueda guardar el progreso del jugador mediante SQL Server, para que pueda cerrar el juego y continuar después sin perder sus avances.

2. Entidades principales

Cadete: almacena la información del jugador, como su nombre, edad, energía, estabilidad, escudo y experiencia.

Partida: representa el progreso guardado, incluyendo la realidad en la que se encuentra el cadete y su posición en el mapa.

Objeto: representa los objetos que se pueden utilizar durante el juego, como células de energía, escudos y analizadores.

Inventario: permite registrar los objetos que tiene el cadete y la cantidad disponible de cada uno.

3. Relaciones entre las entidades

Un cadete puede tener varias partidas guardadas.

Cada partida pertenece a un cadete.

Cada partida puede tener varios objetos en su inventario.

Un mismo tipo de objeto puede aparecer en diferentes partidas.

4. Diagrama del modelo de datos

erDiagram
    CADETE ||--o{ PARTIDA : tiene
    PARTIDA ||--o{ INVENTARIO : contiene
    OBJETO ||--o{ INVENTARIO : aparece_en

5. Guardado y recuperación del progreso

Para guardar: el sistema registraría el estado del cadete, su ubicación, la partida actual y los objetos de su inventario en SQL Server.

Para recuperar: el sistema consultaría la partida seleccionada, cargaría los datos guardados y restauraría el estado del cadete para continuar jugando.

Las operaciones de guardado deberían realizarse de forma segura para evitar que el progreso quede incompleto si ocurre algún error.
