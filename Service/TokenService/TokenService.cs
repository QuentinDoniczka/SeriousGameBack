using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Model.Data;

namespace Service.TokenService
{
    public class TokenService : ITokenService
    {
        private readonly UserManager<User> _userManager;
        private readonly string _key;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expiration;

        public TokenService(
            UserManager<User> userManager, 
            string key, 
            string issuer, 
            string audience, 
            string expiration)
        {
            _userManager = userManager;
            _key = key;
            _issuer = issuer;
            _audience = audience;
            _expiration = int.Parse(expiration);
        }

        public async Task<string> GenerateJwtToken(User user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var roleClaims = roles.Select(r => new Claim(ClaimTypes.Role, r));
            var email = user.Email ?? "";
            var username = user.UserName ?? "";

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim("custom:userId", user.Id),
                new Claim("email", email),
                new Claim(JwtRegisteredClaimNames.Name, username),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddMinutes(_expiration);

            var token = new JwtSecurityToken(
                _issuer,
                _audience,
                claims,
                expires: expires,
                signingCredentials: cred
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}