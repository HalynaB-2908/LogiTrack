using LogiTrack.WebApi.Models;
using System.Threading.Tasks;

namespace LogiTrack.WebApi.Services.Abstractions
{
    public interface ITokenService
    {
        Task<string> CreateTokenAsync(ApplicationUser user);
    }
}