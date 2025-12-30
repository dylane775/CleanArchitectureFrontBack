namespace Identity.Application.DTOs.Input
{
    /// <summary>
    /// Data transfer object for refreshing access token
    /// </summary>
    public class RefreshTokenDto
    {
        /// <summary>
        /// Refresh token string
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;
    }
}
