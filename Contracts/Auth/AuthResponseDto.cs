namespace LogiTrack.WebApi.Contracts.Auth
{
    public class AuthResponseDto
    {
        public string UserId { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string? UserName { get; set; }
        public string Token { get; set; } = default!;
        public string[] Roles { get; set; } = Array.Empty<string>();
    }
}

