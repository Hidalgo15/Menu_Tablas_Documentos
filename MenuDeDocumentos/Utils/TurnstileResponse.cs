using System.Text.Json.Serialization;

namespace MenuDeDocumentos.Utils
{
    /// <summary>
    /// Modelo para deserializar la respuesta de Cloudflare Turnstile
    /// </summary>
    public class TurnstileResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("error-codes")]
        public string[]? ErrorCodes { get; set; }

        [JsonPropertyName("challenge_ts")]
        public string? ChallengeTs { get; set; }

        [JsonPropertyName("hostname")]
        public string? Hostname { get; set; }
    }
}
