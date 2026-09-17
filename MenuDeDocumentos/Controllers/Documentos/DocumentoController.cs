using MenuDeDocumentos.Models;
using MenuDeDocumentos.Service.Interface;
using Microsoft.AspNetCore.Mvc;

namespace MenuDeDocumentos.Controllers.Documentos
{
    [Route("")]
    [Route("Documento")]
    public class DocumentoController : Controller
    {
        private readonly IDocumentoService _documentoService;
        private const string NombreTabla = "cbs01";

        public DocumentoController(IDocumentoService documentoService)
        {
            _documentoService = documentoService;
        }

        [HttpGet("")]
        [HttpGet("Index")]
        [HttpGet("{codigo:int}")]
        public async Task<IActionResult> Index(int? codigo)
        {
            if (codigo.HasValue && codigo.Value > 0)
            {
                var listaDocumentos = await _documentoService.ObtenerListaDocumentosAsync(codigo.Value);
                ViewBag.Documentos = listaDocumentos;

                if (listaDocumentos.Any())
                {
                    var docSeleccionado = listaDocumentos.First();

                    ViewBag.PdfUrl = Url.Action("DescargarDocumento", "Documento", new { codigoPadre = codigo.Value, indiceHijo = docSeleccionado.Codigo });
                    ViewBag.NombreDocumentoActual = docSeleccionado.Nombre;
                }
            }
            else
            {
                ViewBag.Documentos = new List<Documento>();
            }

            return View();
        }

        // Recibe ambos parámetros de manera limpia y sin depender de sesiones
        [HttpGet("DescargarDocumento/{codigoPadre:int}/{indiceHijo:int}")]
        public async Task<IActionResult> DescargarDocumento(int codigoPadre, int indiceHijo)
        {
            if (codigoPadre <= 0 || indiceHijo < 0)
            {
                return BadRequest("Parámetros de documento inválidos.");
            }

            byte[]? archivoBytes = await _documentoService.ObtenerDocumentoDesdeBDAsync(codigoPadre, indiceHijo, NombreTabla);

            if (archivoBytes == null || archivoBytes.Length == 0)
            {
                return NotFound("No se encontró el archivo comprimido en la BD.");
            }

            try
            {
                byte[] pdfBytes = await _documentoService.DescomprimirDocumentoAsync(archivoBytes, codigoPadre, indiceHijo);
                return File(pdfBytes, "application/pdf");
            }
            catch (Exception)
            {
                return NotFound("No se pudo procesar o descomprimir el archivo.");
            }
        }
    }
}