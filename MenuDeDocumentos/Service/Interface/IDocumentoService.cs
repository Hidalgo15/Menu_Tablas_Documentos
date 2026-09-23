using MenuDeDocumentos.Models;

namespace MenuDeDocumentos.Service.Interface
{
    /// <summary>
    /// Interface for the DocumentoService, providing methods
    /// to retrieve and process documents from the database.
    /// </summary>
    public interface IDocumentoService
    {
        Task<List<Documento>> ObtenerListaDocumentosAsync(int codigoPadre, string nombreTabla);
        Task<byte[]?> ObtenerDocumentoDesdeBDAsync(int codigoPadre, int indiceHijo, string nombreTabla);
        Task<byte[]> DescomprimirDocumentoAsync(byte[] archivoComprimido, int codigoPadre, int indiceHijo);
    }
}
