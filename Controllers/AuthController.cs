using System.Security.Cryptography;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetAPI.Helpers;

namespace DotnetAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly DataContextDapper _dapper;
        private readonly AuthHelper _authHelper;

        public AuthController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
            _authHelper = new AuthHelper(config);
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public IActionResult Register(UserForRegistrationDto userForRegistrationDto)
        {
            // Validate that the password and password confirm match
            if (userForRegistrationDto.Password != userForRegistrationDto.PasswordConfirm)
            {
                return BadRequest("Passwords do not match");
            }

            // Validate that the user is not already registered
            string validateEmailSql = @"SELECT COUNT(*) FROM TutorialAppSchema.Auth WHERE Email = @Email;";
            if (_dapper.LoadDataSingle<int>(validateEmailSql, new { Email = userForRegistrationDto.Email }) > 0)
            {
                return BadRequest("User already exists");
            }

            byte[] passwordSalt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetNonZeroBytes(passwordSalt);
            }

            byte[] passwordHash = _authHelper.GetPasswordHash(userForRegistrationDto.Password, passwordSalt);

            string sqlAddAuth = @"INSERT INTO TutorialAppSchema.Auth (Email, PasswordHash, PasswordSalt)
                                VALUES (@Email, @PasswordHash, @PasswordSalt);";

            if (_dapper.ExecuteSQL(sqlAddAuth, new { Email = userForRegistrationDto.Email, PasswordHash = passwordHash, PasswordSalt = passwordSalt }))
            {
                // insert user into Users table
                string sqlAddUser = @"INSERT INTO TutorialAppSchema.Users (FirstName, LastName, Email, Gender, Active)
                                    VALUES (@FirstName, @LastName, @Email, @Gender, @Active);";
                if (_dapper.ExecuteSQL(sqlAddUser, new { FirstName = userForRegistrationDto.FirstName, LastName = userForRegistrationDto.LastName, Email = userForRegistrationDto.Email, Gender = userForRegistrationDto.Gender, Active = true }))
                {
                    return Ok();
                }
                else
                {
                    return BadRequest("Failed to register user");
                }
            }
            return BadRequest("Failed to register user");
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login(UserForLoginDto userForLoginDto)
        {
            string sqlGetUserPassAndSalt = @"SELECT [PasswordHash], [PasswordSalt] FROM TutorialAppSchema.Auth WHERE Email = @Email;";
            var userAuthData = _dapper.LoadDataSingle<UserForLoginConfirmationDto>(sqlGetUserPassAndSalt, new { Email = userForLoginDto.Email });
            if (userAuthData == null)
            {
                return Unauthorized("Invalid email or password");
            }

            if (userAuthData.PasswordHash == null || userAuthData.PasswordSalt == null)
            {
                return Unauthorized("Invalid email or password");
            }
            byte[] attemptedPasswordHash = _authHelper.GetPasswordHash(userForLoginDto.Password, userAuthData.PasswordSalt);
            if (!attemptedPasswordHash.SequenceEqual(userAuthData.PasswordHash))
            {
                return Unauthorized("Invalid email or password");
            }

            int userId = _dapper.LoadDataSingle<int>(@"SELECT [UserId] FROM TutorialAppSchema.Users WHERE Email = @Email;", new { Email = userForLoginDto.Email });

            return Ok(new Dictionary<string, string> { { "token", _authHelper.CreateToken(userId) } });
        }

        [HttpGet("RefreshToken")]
        public IActionResult RefreshToken()
        {
            string? userId = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Invalid token");
            }

            int dbUserId = _dapper.LoadDataSingle<int>(@"SELECT [UserId] FROM TutorialAppSchema.Users WHERE UserId = @UserId;", new { UserId = userId });
            return Ok(new Dictionary<string, string> { { "token", _authHelper.CreateToken(dbUserId) } });
        }
    }
}