using SCAA_API.Models.Authentication;

namespace SCAA_API.Services.Contracts
{
    public interface IAuthService
    {
        Task<string> RegisterAdmin(RegistrationRequestDto registrationRequestDto);
        Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto);
    }
}
