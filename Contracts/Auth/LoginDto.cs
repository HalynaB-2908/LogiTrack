namespace LogiTrack.WebApi.Contracts.Auth
{
    public class LoginDto
    {
        public string EmailOrUserName { get; set; } = default!;
        public string Password { get; set; } = default!;
    }
}
