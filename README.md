# NEXUS — Sistema de Simulación y Exploración de Realidades

## Descripción del proyecto

NEXUS es un proyecto desarrollado en C# que simula un sistema de entrenamiento y exploración de diferentes realidades. El usuario puede crear un cadete, consultar su estado y realizar distintas acciones dentro de un entorno de simulación.

El sistema combina menús interactivos, mapas, exploración, análisis de anomalías y gestión de recursos como la energía y la estabilidad del enlace.

## Objetivo

Desarrollar una aplicación de consola que permita simular misiones de exploración, gestionar el estado de un cadete e interactuar con diferentes zonas y situaciones dentro del sistema NEXUS.

## Funcionalidades principales

- Registro y validación de los datos del cadete.
- Creación de un entorno de entrenamiento.
- Exploración de diferentes realidades.
- Visualización de mapas y zonas.
- Detección y análisis de anomalías.
- Diagnóstico del estado del enlace.
- Recuperación de energía y estabilización.
- Consulta del inventario y las misiones.
- Sistema de niveles y experiencia.
- Menús interactivos para navegar por las opciones del programa.

## Tecnologías utilizadas

- **Lenguaje:** C#
- **Entorno de desarrollo:** Visual Studio
- **Tipo de aplicación:** Consola
- **Conceptos de programación:** clases, objetos, métodos, propiedades, colecciones y operaciones de LINQ.

## Estructura general

El proyecto está organizado en clases que cumplen diferentes responsabilidades:

- **Program:** controla el inicio de la aplicación, la validación de datos y el menú principal.
- **Cadete:** representa al usuario de la simulación y administra sus características y estado.
- **Nexus:** coordina elementos del sistema y las acciones de la simulación.
- **Zona:** representa los mapas y las áreas disponibles para explorar.
- **Exploracion:** contiene la lógica relacionada con el desplazamiento y la exploración.
- **Arte:** gestiona elementos visuales y presentaciones de la consola.

La organización exacta puede variar según los archivos y las clases adicionales que formen parte del proyecto.

## Requisitos

- Tener instalado Visual Studio o un entorno compatible con C#.
- Contar con el SDK o las herramientas de desarrollo correspondientes al proyecto.
- Disponer de los archivos y recursos necesarios para ejecutar la aplicación.

## Ejecución

1. Abrir la solución o el proyecto de NEXUS en Visual Studio.
2. Verificar que los archivos del proyecto estén incluidos correctamente.
3. Seleccionar el proyecto como elemento de inicio.
4. Compilar la solución para comprobar que no existan errores.
5. Ejecutar la aplicación desde Visual Studio.

## Uso del sistema

Al iniciar el programa, se presenta la pantalla de bienvenida y se solicitan los datos del cadete. Después de validar la información, el sistema muestra el resumen inicial y permite acceder al menú principal.

Desde el menú se pueden realizar acciones como explorar realidades, analizar anomalías, revisar el enlace, recuperar energía, estabilizar la conexión, consultar el inventario y revisar las misiones.

## Documentación

Los documentos del proyecto se encuentran en la carpeta `docs/` del repositorio.

- `Manual_NEXUS_H3.pdf`: manual técnico del proyecto.
- `diseno-del-juego.md`: diseño y mecánicas del juego.
- `arquitectura-actual.md`: organización actual del código.
- `arquitectura-propuesta.md`: propuesta de organización del sistema.
- `modelo-de-datos-propuesto.md`: propuesta para guardar el progreso.
- `analisis-tecnico.md`: análisis de las alternativas técnicas.
- `evidencias/`: bocetos y diagramas del proyecto.
