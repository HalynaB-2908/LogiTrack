namespace LogiTrack.WebApi.Contracts.Auth
{
    public class RegisterDto
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string? UserName { get; set; }
        public string Role { get; set; } = "User";
    }
}
