using Microsoft.Data.SqlClient;
using System.Data;

namespace MenuDeDocumentos.Utils
{
    public static class EjecucionSpUtils
    {
        /// Ejecuta el procedimiento almacenado de manera asíncrona.
        /// 
        public static async Task ExecuteStoredProcedureAsync(
            string connectionString,
            int codigo,
            Func<SqlDataReader, Task> processReader)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("La cadena de conexión no puede estar vacía.", nameof(connectionString));

            if (processReader == null)
                throw new ArgumentNullException(nameof(processReader));

            await using var conn = new SqlConnection(connectionString);
            await using var cmd = new SqlCommand("[dbo].[sp_PasanteObtenerDocumento]", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@CodigoDocumento", codigo);

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            await processReader(reader);
        }
    }
}
