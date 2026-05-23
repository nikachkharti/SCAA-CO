using SCAA_API.Models.Authentication;
using Swashbuckle.AspNetCore.Filters;

namespace SCAA_API
{
    public sealed record RegistrationRequestDtoExample : IExamplesProvider<RegistrationRequestDto>
    {
        public RegistrationRequestDto GetExamples()
        {
            return new RegistrationRequestDto() { Email = "admin@gmail.com", Password = "Password123!" };
        }
    }

    public sealed record LoginRequestDtoExample : IExamplesProvider<LoginRequestDto>
    {
        public LoginRequestDto GetExamples()
        {
            return new LoginRequestDto() { UserName = "admin@gmail.com", Password = "Password123!" };
        }
    }


}
