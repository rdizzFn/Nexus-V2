# Análisis técnico de NEXUS

## 1. Alternativas evaluadas

### Organización del código

**Alternativa 1: Mantener el código en un solo archivo**

- Ventaja: es más sencillo trabajar con el proyecto cuando todavía es pequeño.
- Desventaja: encontrar y modificar partes específicas se vuelve más complicado a medida que aumenta el código.

**Alternativa 2: Separar el código por responsabilidades**

- Ventaja: facilita el mantenimiento y permite organizar mejor los personajes, las mecánicas y la interfaz.
- Desventaja: requiere reorganizar el código existente y revisar que todo siga funcionando correctamente.

**Decisión recomendada:** separar las responsabilidades gradualmente, conservando las mecánicas que ya funcionan.

### Guardado del progreso

**Alternativa 1: Guardar en archivos**

- Ventaja: es una opción sencilla para almacenar información de una partida.
- Desventaja: administrar varios registros y mantener la información organizada puede resultar más difícil.

**Alternativa 2: Utilizar SQL Server**

- Ventaja: permite organizar el progreso, relacionar la información del cadete con sus partidas y administrar los objetos guardados.
- Desventaja: requiere configurar la base de datos y programar la conexión desde C#.

**Decisión recomendada:** utilizar SQL Server, porque permite organizar los datos del juego y consultar el progreso guardado. Esta integración todavía tendría que implementarse.

## 2. Ventajas y desventajas de la arquitectura propuesta

Separar la interfaz, la lógica del juego y el acceso a datos permitiría modificar cada parte con mayor facilidad.

La principal ventaja es que los cambios en la presentación no tendrían que afectar directamente al combate, las misiones o los personajes. Además, sería más sencillo incorporar nuevas funciones.

La desventaja es que la separación requiere tiempo y pruebas. Si se realiza sin cuidado, podrían aparecer errores en las mecánicas que actualmente funcionan.

## 3. Decisiones técnicas recomendadas

- **Lenguaje:** mantener C#, ya que es el lenguaje utilizado en el proyecto actual.
- **Plataforma:** continuar con .NET 10, indicado en la configuración del proyecto.
- **Interfaz:** conservar la consola por ahora y evaluar una interfaz gráfica únicamente si las necesidades del juego lo justifican.
- **Arquitectura:** separar progresivamente la presentación, las reglas del juego y el almacenamiento.
- **Base de datos:** utilizar SQL Server para guardar y recuperar el progreso.
- **Acceso a datos:** utilizar `Microsoft.Data.SqlClient` para la comunicación entre C# y SQL Server.
- **Seguridad y consistencia:** utilizar consultas parametrizadas y transacciones cuando una operación de guardado involucre varios cambios relacionados.

## 4. Fuentes técnicas consultadas

- [Documentación oficial de .NET 10](https://dotnet.microsoft.com/en-us/download/dotnet/10.0): información sobre la plataforma utilizada por el proyecto.
- [Microsoft.Data.SqlClient](https://github.com/MicrosoftDocs/sql-docs/blob/live/docs/connect/ado-net/microsoft-ado-net-sql-server.md): documentación sobre la conexión de aplicaciones .NET con SQL Server.
- [Transacciones y concurrencia en ADO.NET](https://learn.microsoft.com/en-us/sql/connect/ado-net/transactions-and-concurrency?view=sql-server-ver17): información sobre cómo mantener la consistencia de las operaciones de almacenamiento.

## 5. Verificación de información obtenida con IA

La IA se utilizó como apoyo para organizar las alternativas técnicas y preparar la documentación. Sin embargo, las recomendaciones deben comprobarse antes de aplicarlas al proyecto.

Para verificar la información se deben realizar estas comprobaciones:

- Revisar que las clases y funciones mencionadas existan realmente en el código de NEXUS.
- Comprobar la versión de .NET en el archivo `NexusPOO.csproj`.
- Consultar la documentación oficial de Microsoft para confirmar las recomendaciones sobre C# y SQL Server.
- Probar los cambios en el proyecto antes de considerarlos funcionales.
- Diferenciar las funciones que ya existen de las propuestas que todavía no se han desarrollado.

La IA puede equivocarse o sugerir soluciones que no se ajusten al proyecto. Por eso, sus respuestas deben contrastarse con el código real y las fuentes técnicas oficiales.
