using Microsoft.Data.SqlClient;
using System.Data;

namespace MenuDeDocumentos.Utils
{
    public static class EjecucionSpUtils
    {
        /// Ejecuta el procedimiento almacenado de manera asíncrona.
        /// 

        /// <param name="connectionString">La cadena de conexión a la base de datos.</param>
        /// <param name="nombreTabla">El nombre de la tabla.</param>
        /// <param name="codigo">El código del documento.</param>
        /// <param name="processReader">La función para procesar el lector de datos.</param>
        public static async Task ExecuteStoredProcedureAsync(
            string connectionString,
            string nombreTabla,
            int codigo,
            Func<SqlDataReader, Task> processReader)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("La cadena de conexión no puede estar vacía.", nameof(connectionString));

            if (processReader == null)
                throw new ArgumentNullException(nameof(processReader));

            await using var conn = new SqlConnection(connectionString);
            await using var cmd = new SqlCommand("[dbo].[sp_PasanteObtenerDocumento3]", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@CodigoDocumento", codigo);
            cmd.Parameters.AddWithValue("@tabla", nombreTabla);

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            await processReader(reader);
        }
    }
}
