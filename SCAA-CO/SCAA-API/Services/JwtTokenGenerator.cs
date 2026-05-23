using Microsoft.IdentityModel.Tokens;
using SCAA_API.Entities.Authentication;
using SCAA_API.Services.Contracts;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SCAA_API.Services
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly string _secret;
        private readonly string _issuer;
        private readonly string _audience;

        public JwtTokenGenerator(IConfiguration configuration)
        {
            _secret = configuration.GetValue<string>("JwtOptions:Secret");
            _issuer = configuration.GetValue<string>("JwtOptions:Issuer");
            _audience = configuration.GetValue<string>("JwtOptions:Audience");
        }

        public string GenerateToken(ApplicationUser applicationUser, IEnumerable<string> roles)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secret);
            var claimList = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Email,applicationUser.Email),
                new Claim(JwtRegisteredClaimNames.Sub,applicationUser.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Name,applicationUser.UserName)
            };

            claimList.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Audience = _audience,
                Issuer = _issuer,
                Subject = new ClaimsIdentity(claimList),
                Expires = DateTime.Now.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
