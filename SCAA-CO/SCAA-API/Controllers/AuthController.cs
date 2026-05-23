using Microsoft.AspNetCore.Mvc;
using SCAA_API.Models.Authentication;
using SCAA_API.Services.Contracts;
using Swashbuckle.AspNetCore.Filters;
using System.Net;

namespace SCAA_API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        /// <summary>
        /// Registers a new admin user with the provided registration details.
        /// </summary>
        [HttpPost("registeradmin")]
        [SwaggerRequestExample(typeof(RegistrationRequestDto), typeof(RegistrationRequestDtoExample))]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegistrationRequestDto model)
        {
            var result = await authService.RegisterAdmin(model);

            var response = new CommonResponse()
            {
                Message = "Admin registered successfully",
                IsSuccess = true,
                HttpStatusCode = Convert.ToInt32(HttpStatusCode.Created),
                Result = result
            };

            return StatusCode(response.HttpStatusCode, response);

        }


        /// <summary>
        /// Logins a user with the provided credentials and returns an authentication token if successful.
        /// </summary>
        [HttpPost("login")]
        [SwaggerRequestExample(typeof(LoginRequestDto), typeof(LoginRequestDtoExample))]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
        {
            var result = await authService.Login(model);

            var response = new CommonResponse()
            {
                Message = "Successful authorization",
                IsSuccess = true,
                HttpStatusCode = Convert.ToInt32(HttpStatusCode.OK),
                Result = result
            };

            return StatusCode(response.HttpStatusCode, response);
        }

    }
}
