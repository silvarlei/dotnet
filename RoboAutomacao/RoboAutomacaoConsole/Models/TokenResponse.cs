using Newtonsoft.Json;

namespace RoboAutomacaoConsole.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class TokenResponse
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("id_token")]
        public string TokenId { get; set; }

        /// <summary>
        /// o token em si
        /// </summary>
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        /// <summary>
        /// quantos segundos o token é válido
        /// </summary>
        [JsonProperty("expires_in")]
        public string ExpiresIn { get; set; }

        /// <summary>
        /// sempre "Bearer"
        /// </summary>
        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        /// <summary>
        /// todos os scopes pedidos no connect/authorize
        /// </summary>
        [JsonProperty("scope")]
        public string Scope { get; set; }

        /// <summary>
        /// Utilizado para obtenção de novos tokens de acesso
        /// </summary>
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }
    }
}
