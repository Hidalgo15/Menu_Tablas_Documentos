using MenuDeDocumentos.Models;
using MenuDeDocumentos.Service.Interface;
using MenuDeDocumentos.Utils;
using Microsoft.Data.SqlClient;

namespace MenuDeDocumentos.Service;

public class DocumentoService : IDocumentoService
{
    private readonly string _connectionString;
    // Semáforo estático para permitir que solo UN hilo a la vez use la librería nativa de ZLIB
    private static readonly SemaphoreSlim _zlibLock = new SemaphoreSlim(1, 1);

    public DocumentoService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("ConexionSIGOB")
            ?? throw new InvalidOperationException("La cadena de conexión 'ConexionSIGOB' no existe.");
    }

    public async Task<List<Documento>> ObtenerListaDocumentosAsync(int codigoPadre)
    {
        var lista = new List<Documento>();
        int indice = 0;

        await EjecucionSpUtils.ExecuteStoredProcedureAsync(_connectionString, codigoPadre, async reader =>
        {
            while (await reader.ReadAsync())
            {
                string nombreCrudo = reader["nombre"]?.ToString() ?? "Sin Título";
                string nombreSeguro = string.Join("_", nombreCrudo.Split(Path.GetInvalidFileNameChars()));
                nombreSeguro = nombreSeguro.Replace("\\", "-").Replace("/", "-");

                lista.Add(new Documento
                {
                    Codigo = indice++,
                    Categoria = reader["molde"]?.ToString() ?? "General",
                    Nombre = nombreSeguro
                });
            }
        });

        return lista;
    }

    public async Task<byte[]?> ObtenerDocumentoDesdeBDAsync(int codigoPadre, int indiceHijo, string nombreTabla)
    {
        byte[]? resultado = null;
        int contador = 0;

        await EjecucionSpUtils.ExecuteStoredProcedureAsync(_connectionString, codigoPadre, async reader =>
        {
            while (await reader.ReadAsync())
            {
                if (contador == indiceHijo)
                {
                    string colNombre = HasColumn(reader, "documento") ? "documento" : "document";

                    if (!reader.IsDBNull(reader.GetOrdinal(colNombre)))
                    {
                        resultado = (byte[])reader[colNombre];
                    }
                    break;
                }
                contador++;
            }
        });

        return resultado;
    }

    private static bool HasColumn(SqlDataReader reader, string columnName)
    {
        for (int i = 0; i < reader.FieldCount; i++)
        {
            if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }

    public async Task<byte[]> DescomprimirDocumentoAsync(byte[] archivoComprimido, int codigoPadre, int indiceHijo)
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "VisorDocumentos", Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        string rutaComprimida = Path.Combine(tempDir, $"Doc_{codigoPadre}_{indiceHijo}.zlib");
        string? rutaDescomprimida = null;

        // PROTECCIÓN CON SEMÁFORO: 
        // Esto evita que dos hilos disparen ZLIBSIGOB al mismo tiempo y corrompan la memoria nativa.
        await _zlibLock.WaitAsync();
        try
        {
            await File.WriteAllBytesAsync(rutaComprimida, archivoComprimido);

            rutaDescomprimida = ZLIBSIGOB.DescomprimirArchivoZLIB(rutaComprimida);

            if (string.IsNullOrEmpty(rutaDescomprimida) || !File.Exists(rutaDescomprimida))
            {
                throw new FileNotFoundException("No se generó el archivo descomprimido.");
            }

            return await File.ReadAllBytesAsync(rutaDescomprimida);
        }
        finally
        {
            // Liberamos el semáforo para que la siguiente petición pueda pasar de forma segura
            _zlibLock.Release();

            // Limpieza de temporales
            try
            {
                if (File.Exists(rutaComprimida)) File.Delete(rutaComprimida);
                if (!string.IsNullOrEmpty(rutaDescomprimida) && File.Exists(rutaDescomprimida)) File.Delete(rutaDescomprimida);
                if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
            }
            catch { }
        }
    }
}