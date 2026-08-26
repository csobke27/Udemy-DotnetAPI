using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;

namespace DotnetAPI.Helpers
{
    public class AuthHelper
    {
        private readonly IConfiguration _config;
        public AuthHelper(IConfiguration config)
        {
            _config = config;
        }
        // Add helper methods related to authentication here
        public byte[] GetPasswordHash(string password, byte[] salt)
        {
            string paswordSaltPlusString = _config.GetSection("AppSettings:PasswordKey").Value + Convert.ToBase64String(salt);
            byte[] passwordHash = KeyDerivation.Pbkdf2(
                password: password,
                salt: Encoding.ASCII.GetBytes(paswordSaltPlusString),
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8
            );
            return passwordHash;
        }

        public string CreateToken(int userId)
        {
            Claim[] claims = new Claim[]
            {
                new Claim("userId", userId.ToString())
            };

            string? tokenKey = _config.GetSection("AppSettings:TokenKey").Value;
            if (string.IsNullOrEmpty(tokenKey))
            {
                throw new Exception("Token key is not configured");
            }
            SymmetricSecurityKey tokenKeyObj = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));
            SigningCredentials creds = new SigningCredentials(tokenKeyObj, SecurityAlgorithms.HmacSha512Signature);
            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            JwtSecurityToken token = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}