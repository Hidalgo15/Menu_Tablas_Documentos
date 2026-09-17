using MenuDeDocumentos.Models;

namespace MenuDeDocumentos.Service.Interface
{
    public interface IDocumentoService
    {
        Task<List<Documento>> ObtenerListaDocumentosAsync(int codigoPadre);
        Task<byte[]?> ObtenerDocumentoDesdeBDAsync(int codigoPadre, int indiceHijo, string nombreTabla);
        Task<byte[]> DescomprimirDocumentoAsync(byte[] archivoComprimido, int codigoPadre, int indiceHijo);
    }
}
