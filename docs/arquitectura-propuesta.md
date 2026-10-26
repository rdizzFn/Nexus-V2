Arquitectura propuesta de NEXUS

1. Objetivo de la propuesta

La idea de esta propuesta es organizar mejor el código de NEXUS para que sea más fácil de entender, mantener y ampliar. Actualmente, gran parte del funcionamiento está concentrado en Program.cs, por lo que sería conveniente separar las responsabilidades en diferentes partes.

La propuesta consiste en dividir el proyecto en tres áreas principales: la interfaz gráfica, la lógica del juego y el acceso a datos. Esta organización permitiría modificar una parte sin tener que cambiar todo el sistema.

Esta es una mejora planteada para el futuro. El proyecto actual sigue funcionando desde la consola y la separación todavía tendría que desarrollarse.

2. Separación de componentes

2.1. Interfaz gráfica

Esta parte se encargaría de mostrar la información al jugador y recibir sus acciones. Aquí estarían los menús, los mapas, las barras de energía, el inventario y los mensajes de las misiones.

En el proyecto actual, varias de estas tareas las realiza la clase Ui, que utiliza textos, colores y símbolos en la consola.

Si más adelante se decide incorporar una interfaz gráfica, esta capa podría adaptarse sin tener que reescribir toda la lógica del juego.

2.2. Lógica del juego

Esta sería la parte principal de NEXUS, donde se controlarían las reglas y las acciones de la partida.

Aquí se encontrarían los personajes, los enemigos, los combates, los mapas, las misiones, los objetos y el progreso del cadete. También se controlarían los cambios en la energía, la estabilidad y la experiencia.

Las clases actuales, como Personaje, Cadete, Robot y Enemigo, servirían como base para organizar esta parte. De esta manera, las reglas del juego no dependerían directamente de cómo se muestran los menús o los mensajes.

2.3. Acceso a datos

Esta parte se encargaría de guardar y recuperar información del juego, como el progreso del cadete, la experiencia, los objetos del inventario y las misiones completadas.

Actualmente, la propuesta contempla incorporar un sistema de guardado que permita continuar una partida sin empezar desde cero. Para ello, se podría utilizar una base de datos o archivos de almacenamiento, dependiendo de las necesidades del proyecto.

Esta capa se comunicaría con la lógica del juego para guardar o recuperar información cuando sea necesario. No se plantea como una función que ya esté implementada.

3. Diagrama de arquitectura propuesta

El siguiente diagrama muestra cómo se organizarían las tres partes y cómo se comunicarían entre sí.

flowchart TD
    A[Jugador] --> B[Interfaz gráfica]
    B --> C[Lógica del juego]
    C --> D[Acceso a datos]
    D --> E[(Base de datos o archivos)]
    E --> D
    D --> C
    C --> B
    B --> A

El jugador interactuaría con la interfaz, que enviaría sus acciones a la lógica del juego. Esta última comprobaría las reglas y actualizaría el estado de la partida. Cuando fuera necesario guardar o recuperar información, se comunicaría con el acceso a datos.

Después, la interfaz mostraría el resultado al jugador.

4. Organización propuesta del proyecto

Para aplicar esta arquitectura, se podría organizar el código de la siguiente manera:

Interfaz/: contendría los menús, la presentación de mapas y los elementos visuales.

Logica/: contendría los personajes, los combates, las misiones, los objetos y las reglas del juego.

Datos/: contendría las funciones para guardar y recuperar el progreso.

Program.cs: se encargaría de iniciar el juego y conectar sus componentes.

Esta organización es una propuesta de estructura. No significa que las carpetas ya existan ni que el código actual se haya dividido de esta forma.

5. Ventajas de la propuesta

La separación de responsabilidades tendría varias ventajas para el desarrollo de NEXUS:

Código más ordenado: sería más sencillo encontrar las clases y los métodos que se necesitan modificar.

Mantenimiento más fácil: los cambios en los menús no tendrían que afectar directamente a las reglas del juego.

Posibilidad de guardar partidas: el jugador podría conservar su progreso mediante un sistema de almacenamiento.

Ampliación del juego: sería más sencillo agregar nuevas misiones, enemigos, objetos o realidades.

Mejor organización del trabajo: cada parte tendría una responsabilidad más clara.
