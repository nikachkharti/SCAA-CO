using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SCAA_API.Data;
using SCAA_API.Entities.Authentication;
using SCAA_API.Models.Authentication;
using SCAA_API.Services.Contracts;

namespace SCAA_API.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IMapper _mapper;
        private const string _adminRole = "admin";

        public AuthService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IMapper mapper,
            IJwtTokenGenerator jwtTokenGenerator)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
            _jwtTokenGenerator = jwtTokenGenerator;
        }


        public async Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto)
        {
            var user = await _context.ApplicationUsers
                .FirstOrDefaultAsync(x => x.UserName.ToLower() == loginRequestDto.UserName.ToLower());

            if (!user.LockoutEnabled)
                throw new UnauthorizedAccessException("Unable to sign in with locked account");

            bool isValid = await _userManager.CheckPasswordAsync(user, loginRequestDto.Password);

            if (user == null || !isValid)
                throw new UnauthorizedAccessException("User not found or password is incorrect");


            //If user was found generate token
            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtTokenGenerator.GenerateToken(user, roles);

            return new LoginResponseDto() { AccessToken = token };
        }

        public async Task<string> RegisterAdmin(RegistrationRequestDto registrationRequestDto)
        {
            var user = _mapper.Map<ApplicationUser>(registrationRequestDto);
            var result = await _userManager.CreateAsync(user, registrationRequestDto.Password);

            if (result.Succeeded)
            {
                var userToReturn = await _context.ApplicationUsers
                    .FirstAsync(x => x.Email.ToLower() == registrationRequestDto.Email.ToLower());

                if (userToReturn != null)
                {
                    if (!await _roleManager.RoleExistsAsync(_adminRole))
                        await _roleManager.CreateAsync(new IdentityRole(_adminRole));

                    await _userManager.AddToRoleAsync(userToReturn, _adminRole);
                }

                return userToReturn.Id;
            }
            else
            {
                throw new BadHttpRequestException(result.Errors.FirstOrDefault().Description);
            }

        }
    }
}
