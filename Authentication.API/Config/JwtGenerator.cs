namespace Authentication.API.Config
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Security.Claims;
    using System.Text;
    using Authentication.API.CustomIdentity;
    using Authentication.API.Models;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Tokens;

    public class JwtGenerator
    {
        public static JwtDto GenerateJwtToken(ApplicationUser user, IOptionsSnapshot<JwtOptions> jwtOptions)
        {
            var options = jwtOptions.Value;

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.JwtKey));

            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name,  user.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, $"{user.Firstname} {user.Lastname}"),
            };

            var claimsIdentity = new ClaimsIdentity(claims);
            claimsIdentity.AddClaims(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)).ToList());

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = options.JwtIssuer,
                IssuedAt = DateTime.Now,
                Subject = claimsIdentity,
                Expires = DateTime.Now.AddDays(options.JwtExpireDays),
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return new JwtDto()
            {
                JwtToken = tokenHandler.WriteToken(token),
                UserID = user.Id.ToString(),
                ExpirationDate = DateTime.Now.AddDays(options.JwtExpireDays),
            };
        }

        public static string GenerateSimpleJwt(ApplicationUser user, IOptionsSnapshot<JwtOptions> jwtOptions)
        {
            var options = jwtOptions.Value;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(options.JwtKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
