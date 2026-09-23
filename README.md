# Menu de Documentos

Aplicación web desarrollada en ASP.NET Core MVC para consultar, listar y visualizar documentos almacenados en SQL Server. La solución permite cargar una colección de registros asociados a una tabla y un código padre, y luego abrir cada documento en formato PDF a partir de un archivo binario comprimido en ZLIB.

## Descripción general

El proyecto está pensado para operar como un visor documental sobre un catálogo de documentos persistidos en una base de datos SIGOB. En lugar de mantener archivos físicos en disco, el sistema extrae el contenido desde la base de datos, lo descomprime mediante una librería nativa y lo entrega al navegador como flujo PDF.

La aplicación usa:

- ASP.NET Core MVC
- SQL Server con `Microsoft.Data.SqlClient`
- Carga dinámica de documentos por tabla y código
- Descompresión de archivos con biblioteca nativa `ZLibSIGOB32.dll`
- Vista Razor para navegación por categorías y visor de PDF

## Estructura del proyecto

```text
Menu_Tablas_Documentos/
├── MenuDeDocumentos.sln
├── README.md
├── docs/
│   └── DocumentacionTecnica.md
└── MenuDeDocumentos/
    ├── appsettings.json
    ├── appsettings.Development.json
    ├── Program.cs
    ├── MenuDeDocumentos.csproj
    ├── Controllers/
    │   └── Documentos/
    │       └── DocumentoController.cs
    ├── Models/
    │   └── Documento.cs
    ├── Service/
    │   ├── DocumentoService.cs
    │   └── Interface/
    │       └── IDocumentoService.cs
    ├── Utils/
    │   ├── EjecucionSpUtils.cs
    │   └── ZLIBSIGOB.cs
    ├── Views/
    │   ├── Documento/
    │   │   └── Index.cshtml
    │   ├── Shared/
    │   ├── _ViewImports.cshtml
    │   └── _ViewStart.cshtml
    ├── wwwroot/
    └── Properties/
        └── launchSettings.json
```

## Requisitos

- .NET SDK 8.0
- SQL Server con acceso a la base `SIGOB`
- Librería nativa `ZLibSIGOB32.dll` compatible con arquitectura x86
- Credenciales válidas para la cadena de conexión

## Configuración

La conexión a la base de datos se configura en:

- [MenuDeDocumentos/appsettings.json](MenuDeDocumentos/appsettings.json)

Ejemplo:

```json
"ConnectionStrings": {
  "ConexionSIGOB": "Server=...;Database=SIGOB;User ID=sa;Password=...;TrustServerCertificate=True;"
}
```

Es importante adaptar la cadena de conexión a tu entorno real antes de ejecutar la aplicación.

## Ejecución

Desde la raíz del proyecto, ejecuta:

```bash
dotnet restore
dotnet build
 dotnet run --project MenuDeDocumentos/MenuDeDocumentos.csproj
```

La aplicación usa el patrón de rutas por defecto:

```text
/Documento/NombreTabla/123
```

Donde:

- `NombreTabla`: nombre de la tabla que contiene los documentos
- `123`: código padre asociado a la colección de documentos

## Flujo principal

### 1. Carga de documentos
El controlador `DocumentoController.Index` recibe `nombreTabla` y `codigo`.

- Valida que el código sea mayor que cero.
- Invoca `ObtenerListaDocumentosAsync(codigo, nombreTabla)`.
- El servicio consulta el procedimiento almacenado `sp_PasanteObtenerDocumento3`.
- Se devuelve una lista de documentos con nombre, categoría y un índice interno.
- La vista Razor renderiza la lista agrupada por categoría y carga el primer documento.

### 2. Descarga y visualización del PDF
Cuando el usuario selecciona un documento:

- Se genera una URL con `nombreTabla`, `codigoPadre` e `indiceHijo`.
- El método `DescargarDocumento` consulta el registro correspondiente.
- El servicio obtiene el byte array del documento desde la base de datos.
- Se descomprime mediante `ZLIBSIGOB.DescomprimirArchivoZLIB`.
- El archivo resultante se devuelve al navegador con `application/pdf`.

## Componentes clave

### Controlador
- [MenuDeDocumentos/Controllers/Documentos/DocumentoController.cs](MenuDeDocumentos/Controllers/Documentos/DocumentoController.cs)

Responsable de:

- administrar la ruta base de la aplicación
- recibir parámetros de entrada
- orquestar la carga de documentos
- devolver el PDF al cliente

### Servicio
- [MenuDeDocumentos/Service/DocumentoService.cs](MenuDeDocumentos/Service/DocumentoService.cs)

Encargado de:

- ejecutar consultas a SQL Server
- transformar filas en objetos `Documento`
- recuperar el contenido binario del documento
- descomprimirlo antes de mostrarlo

### Utilidades de acceso a datos
- [MenuDeDocumentos/Utils/EjecucionSpUtils.cs](MenuDeDocumentos/Utils/EjecucionSpUtils.cs)

Centraliza la ejecución del procedimiento almacenado, lo que simplifica el acceso a datos en toda la capa de servicio.

### Wrapper nativo de compresión
- [MenuDeDocumentos/Utils/ZLIBSIGOB.cs](MenuDeDocumentos/Utils/ZLIBSIGOB.cs)

Se comunica con una librería nativa para descomprimir archivos ZLIB. Este enfoque es clave porque el contenido se guarda en la base de datos en formato comprimido.

## Modelo principal

- [MenuDeDocumentos/Models/Documento.cs](MenuDeDocumentos/Models/Documento.cs)

```csharp
public class Documento
{
    public int Codigo { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
}
```

## Vista y experiencia de usuario

- [MenuDeDocumentos/Views/Documento/Index.cshtml](MenuDeDocumentos/Views/Documento/Index.cshtml)

La interfaz presenta:

- un panel lateral con categorías
- una lista de documentos por categoría
- un visor PDF central
- carga dinámica del documento vía JavaScript

## Consideraciones técnicas importantes

- El proyecto está configurado con `PlatformTarget` x86 en el archivo CSPROJ, por lo que requiere una DLL nativa de 32 bits.
- La librería `ZLibSIGOB32.dll` se copia al output de compilación en [MenuDeDocumentos/MenuDeDocumentos.csproj](MenuDeDocumentos/MenuDeDocumentos.csproj).
- La lógica usa un semáforo estático para evitar concurrencias en la librería nativa de compresión.
- El flujo de datos no usa autenticación ni autorización por aplicación; se asume que el acceso está controlado a nivel infraestructura o de red.

## Validación actual

Se comprobó que la solución compila correctamente con `dotnet build`.

Resultado verificado:

- compilación exitosa
- 3 advertencias de nulabilidad y P/Invoke relacionadas con la librería nativa

## Limitaciones y riesgos

1. La entrada `nombreTabla` se pasa directamente al procedimiento almacenado, por lo que debe controlarse muy bien en entorno productivo.
2. La aplicación asume una estructura específica de la base de datos y del procedimiento `sp_PasanteObtenerDocumento3`.
3. La descompresión depende de una DLL específica y de una arquitectura de 32 bits.
4. No hay pruebas automatizadas en la solución en este momento.

## Recomendaciones futuras

- Añadir validación y sanitización más estricta de parámetros de entrada.
- Centralizar configuraciones sensibles con secretos del entorno.
- Añadir pruebas unitarias e integración para servicio y validación de flujo.
- Considerar un mecanismo de registro y trazabilidad para errores de descompresión.
- Evaluar la posibilidad de migrar la librería nativa a una alternativa más moderna y portable.

## Enlaces de documentación

- [docs/DocumentacionTecnica.md](docs/DocumentacionTecnica.md)

