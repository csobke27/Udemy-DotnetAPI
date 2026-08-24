using System.Security.Cryptography;
using System.Text;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;

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
                return Ok();
            }
            return BadRequest("Failed to register user");
        }

        [HttpPost("Login")]
        public IActionResult Login(UserForLoginDto userForLoginDto)
        {
            string sqlGetUserPassAndSalt = @"SELECT [PasswordHash], [PasswordSalt] FROM TutorialAppSchema.Auth WHERE Email = @Email;";
            var userAuthData = _dapper.LoadDataSingle<UserForLoginConfirmationDto>(sqlGetUserPassAndSalt, new { Email = userForLoginDto.Email });
            if(userAuthData.PasswordHash == null || userAuthData.PasswordSalt == null)
            {
                return Unauthorized("Invalid email or password");
            }

            byte[] attemptedPasswordHash = GetPasswordHash(userForLoginDto.Password, userAuthData.PasswordSalt);
            if(!attemptedPasswordHash.SequenceEqual(userAuthData.PasswordHash))
            {
                return Unauthorized("Invalid email or password");
            }

            return Ok("Login successful");
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
    }
}