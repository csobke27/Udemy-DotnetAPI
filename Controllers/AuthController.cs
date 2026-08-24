using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace DotnetAPI.Controllers
{
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly DataContextDapper _dapper;

        public AuthController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
            _config = config;
        }

        [HttpPost("Register")]
        public IActionResult Register(UserForRegistrationDto userForRegistrationDto)
        {
            // Validate that the password and password confirm match
            if(userForRegistrationDto.Password != userForRegistrationDto.PasswordConfirm)
            {
                return BadRequest("Passwords do not match");
            }

            // Validate that the user is not already registered
            string validateEmailSql = @"SELECT COUNT(*) FROM TutorialAppSchema.Auth WHERE Email = @Email;";
            if(_dapper.LoadDataSingle<int>(validateEmailSql, new { Email = userForRegistrationDto.Email }) > 0)
            {
                return BadRequest("User already exists");
            }

            byte[] passwordSalt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetNonZeroBytes(passwordSalt);
            }

            byte[] passwordHash = GetPasswordHash(userForRegistrationDto.Password, passwordSalt);

            string sqlAddAuth = @"INSERT INTO TutorialAppSchema.Auth (Email, PasswordHash, PasswordSalt)
                                VALUES (@Email, @PasswordHash, @PasswordSalt);";

            if(_dapper.ExecuteSQL(sqlAddAuth, new { Email = userForRegistrationDto.Email, PasswordHash = passwordHash, PasswordSalt = passwordSalt }))
            {
                // insert user into Users table
                string sqlAddUser = @"INSERT INTO TutorialAppSchema.Users (FirstName, LastName, Email, Gender, Active)
                                    VALUES (@FirstName, @LastName, @Email, @Gender, @Active);";
                if(_dapper.ExecuteSQL(sqlAddUser, new { FirstName = userForRegistrationDto.FirstName, LastName = userForRegistrationDto.LastName, Email = userForRegistrationDto.Email, Gender = userForRegistrationDto.Gender, Active = true }))
                {
                    return Ok();
                } else
                {
                    return BadRequest("Failed to register user");
                }
            }
            return BadRequest("Failed to register user");
        }

        [HttpPost("Login")]
        public IActionResult Login(UserForLoginDto userForLoginDto)
        {
            string sqlGetUserPassAndSalt = @"SELECT [PasswordHash], [PasswordSalt] FROM TutorialAppSchema.Auth WHERE Email = @Email;";
            var userAuthData = _dapper.LoadDataSingle<UserForLoginConfirmationDto>(sqlGetUserPassAndSalt, new { Email = userForLoginDto.Email });
            if(userAuthData == null)
            {
                return Unauthorized("Invalid email or password");
            }
            
            if(userAuthData.PasswordHash == null || userAuthData.PasswordSalt == null)
            {
                return Unauthorized("Invalid email or password");
            }
            byte[] attemptedPasswordHash = GetPasswordHash(userForLoginDto.Password, userAuthData.PasswordSalt);
            if(!attemptedPasswordHash.SequenceEqual(userAuthData.PasswordHash))
            {
                return Unauthorized("Invalid email or password");
            }

            int userId = _dapper.LoadDataSingle<int>(@"SELECT [UserId] FROM TutorialAppSchema.Users WHERE Email = @Email;", new { Email = userForLoginDto.Email });

            return Ok(new Dictionary<string, string> { { "token", CreateToken(userId) } });
        }

        private byte[] GetPasswordHash(string password, byte[] salt)
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

        private string CreateToken(int userId)
        {
            Claim[] claims = new Claim[]
            {
                new Claim("userId", userId.ToString())
            };

            string? tokenKey = _config.GetSection("AppSettings:TokenKey").Value;
            if(string.IsNullOrEmpty(tokenKey))
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