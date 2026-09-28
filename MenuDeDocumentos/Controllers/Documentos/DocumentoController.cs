using MenuDeDocumentos.Models;
using MenuDeDocumentos.Service.Interface;
using MenuDeDocumentos.Utils;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace MenuDeDocumentos.Controllers.Documentos
{
    [Route("")]
    [Route("Documento")]
    public class DocumentoController : Controller
    {
        private readonly IDocumentoService _documentoService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _turnstileSecretKey;
        private readonly string _turnstileSiteKey;

        public DocumentoController(
            IDocumentoService documentoService,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _documentoService = documentoService;
            _httpClientFactory = httpClientFactory;

            // Leer las claves configuradas en appsettings.json
            _turnstileSiteKey = configuration["CloudflareTurnstile:SiteKey"]
                ?? throw new InvalidOperationException("No se ha configurado 'SiteKey' en appsettings.json.");

            _turnstileSecretKey = configuration["CloudflareTurnstile:SecretKey"]
                ?? throw new InvalidOperationException("No se ha configurado 'SecretKey' en appsettings.json.");
        }

        /// <summary>
        /// Muestra la vista principal con la lista de documentos y 
        /// el primer documento PDF cargado
        /// </summary>
        /// <param name="nombreTabla"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        [HttpGet("")]
        [HttpGet("Index")]
        [HttpGet("/{nombreTabla:regex(^[[a-zA-Z0-9_]]+$)}/{codigo:int}")]
        public async Task<IActionResult> Index(string? nombreTabla, int? codigo)
        {
            // Pasar la SiteKey a la vista mediante ViewBag
            ViewBag.TurnstileSiteKey = _turnstileSiteKey;

            if (codigo.HasValue && codigo.Value > 0)
            {
                var listaDocumentos = await _documentoService.ObtenerListaDocumentosAsync(codigo.Value, nombreTabla ?? "");
                ViewBag.Documentos = listaDocumentos;

                if (listaDocumentos.Any())
                {
                    var docSeleccionado = listaDocumentos.First();

                    ViewBag.PdfUrl = Url.Action("DescargarDocumento", "Documento", new
                    {
                        nombreTabla = nombreTabla ?? "",
                        codigoPadre = codigo.Value,
                        indiceHijo = docSeleccionado.Codigo
                    });
                    ViewBag.NombreDocumentoActual = docSeleccionado.Nombre;
                }
            }
            else
            {
                ViewBag.Documentos = new List<Documento>();
            }

            return View();
        }

        /// <summary>
        /// Descarga el documento PDF desde la base de datos y lo devuelve al cliente
        /// </summary>
        /// <param name="nombreTabla"></param>
        /// <param name="codigoPadre"></param>
        /// <param name="indiceHijo"></param>
        /// <returns></returns>
        [HttpGet("DescargarDocumento/{nombreTabla:regex(^[[a-zA-Z0-9_]]+$)}/{codigoPadre:int}/{indiceHijo:int}")]
        public async Task<IActionResult> DescargarDocumento(string? nombreTabla, int codigoPadre, int indiceHijo)
        {
            // 1. Validar Token de Cloudflare desde el Header HTTP
            string? turnstileToken = Request.Headers["X-Turnstile-Token"];

            if (string.IsNullOrEmpty(turnstileToken))
            {
                return BadRequest("El token de verificación de seguridad es requerido.");
            }

            bool esCaptchaValido = await ValidarTurnstileAsync(turnstileToken);
            if (!esCaptchaValido)
            {
                return StatusCode(403, "Validación de seguridad fallida o expirada.");
            }

            // 2. Validación de parámetros del documento
            if (codigoPadre <= 0 || indiceHijo < 0)
            {
                return BadRequest("Parámetros de documento inválidos.");
            }

            // 3. Obtener el archivo comprimido desde la base de datos
            byte[]? archivoBytes = await _documentoService.ObtenerDocumentoDesdeBDAsync(codigoPadre, indiceHijo, nombreTabla ?? "");

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

        /// <summary>
        /// Método auxiliar para validar el token de Turnstile contra la API de Cloudflare
        /// </summary>
        private async Task<bool> ValidarTurnstileAsync(string token)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();

                var values = new Dictionary<string, string>
                {
                    { "secret", _turnstileSecretKey },
                    { "response", token }
                };

                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync("https://challenges.cloudflare.com/turnstile/v0/siteverify", content);

                if (!response.IsSuccessStatusCode)
                    return false;

                var jsonString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<TurnstileResponse>(jsonString);

                return result?.Success ?? false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}