using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using MultiRacingApi.Controllers.ViewModels;
using MultiRacingApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MultiRacingApi.Services
{
    public class JwtService
    {
        private const int EXPIRATION_HOURS = 24;

        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<AuthResponse> CreateTokenAsync(User user)
        {
            var expiration = DateTime.UtcNow.AddHours(EXPIRATION_HOURS);

            var token = CreateJwtToken(
                await CreateClaims(user),
                CreateSigningCredentials(),
                expiration
            );

            return new AuthResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                UserId = user.Id
            };
        }

        private JwtSecurityToken CreateJwtToken(Claim[] claims, SigningCredentials credentials, DateTime expiration) =>
            new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: expiration,
                signingCredentials: credentials
            );

        private async Task<Claim[]> CreateClaims(User user)
        {
            var claims = new List<Claim> {
                new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
            };

            //var roles = await _userManager.GetRolesAsync(user);

            //foreach (var role in roles)
                //claims.Add(new Claim("role", role));

            return claims.ToArray();
        }


        private SigningCredentials CreateSigningCredentials() =>
            new SigningCredentials(
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])
                ),
                SecurityAlgorithms.HmacSha256
            );
    }
}
