using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Dapper;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;

namespace DotnetAPI.Helpers
{
    public class AuthHelper
    {
        private readonly IConfiguration _config;
        private readonly DataContextDapper _dapper;
        public AuthHelper(IConfiguration config)
        {
            _config = config;
            _dapper = new DataContextDapper(config);
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

        public bool setPassword(UserForLoginDto userForSetPassword)
        {
            byte[] passwordSalt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetNonZeroBytes(passwordSalt);
            }

            byte[] passwordHash = this.GetPasswordHash(userForSetPassword.Password, passwordSalt);

            string sqlAddAuth = @"EXEC TutorialAppSchema.spRegistration_Upsert @Email = @EmailParam, @PasswordHash = @PasswordHashParam, @PasswordSalt = @PasswordSaltParam;";
            var parameters = new DynamicParameters();
            parameters.Add("EmailParam", userForSetPassword.Email);
            parameters.Add("PasswordHashParam", passwordHash);
            parameters.Add("PasswordSaltParam", passwordSalt);

            return _dapper.ExecuteSQL(sqlAddAuth, parameters);
        }
    }
}