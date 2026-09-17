using System.Runtime.InteropServices;
using System.Text;

namespace MenuDeDocumentos.Utils
{
    public static class ZLIBSIGOB
    {

        /// <summary>

        /// Provee operaciones para comprimir/descomprimir archivos en un formato ZLIB.

        /// </summary>

        private static extern void zlibComprimirArchivo(string Archivo, string Directorio, StringBuilder Resultado);

        [DllImport("ZLibSIGOB32", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]

        private static extern void zlibDescomprimirArchivo(string Archivo, StringBuilder Resultado);



        /// <summary>

        ///

        /// </summary>

        /// <param name="Archivo"></param>

        /// <param name="Directorio"></param>

        /// <returns></returns>

        public static string ComprimirArchivoZLIB(string Archivo, string Directorio)

        {

            var resultado = new StringBuilder(1024);

            zlibComprimirArchivo(Archivo, Directorio, resultado);

            return resultado.ToString();

        }



        /// <summary>

        ///

        /// </summary>

        /// <param name="Archivo"></param>

        /// <returns></returns>

        public static string ComprimirArchivoZLIB(string Archivo)

        {

            var resultado = new StringBuilder(1024);

            string Directorio;

            var Info = new FileInfo(Archivo);

            Directorio = Info.DirectoryName;

            zlibComprimirArchivo(Archivo, Directorio, resultado);

            return resultado.ToString();

        }



        public static string DescomprimirArchivoZLIB(string Archivo)

        {

            var resultado = new StringBuilder(1024);

            var Info = new FileInfo(Archivo);

            zlibDescomprimirArchivo(Archivo, resultado);

            return resultado.ToString();

        }



    }

}

