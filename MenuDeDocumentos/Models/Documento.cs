namespace MenuDeDocumentos.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class Documento
    {
        /// <summary>
        /// Gets or sets the unique code of the document.
        /// </summary>
        public int Codigo { get; set; }

        /// <summary>
        /// Gets or sets the name of the document.
        /// </summary
        public string Nombre { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
    }
}
