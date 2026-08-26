using DotnetAPI.Data;
using DotnetAPI.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotnetAPI.Helpers;
using Dapper;
using AutoMapper;
using DotnetAPI.Models;

namespace DotnetAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly DataContextDapper _dapper;
        private readonly AuthHelper _authHelper;
        private readonly ReusableSql _reusableSql;
        private readonly IMapper _mapper;

        public AuthController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
            _authHelper = new AuthHelper(config);
            _reusableSql = new ReusableSql(config);
            _mapper = new Mapper(new MapperConfiguration(cfg => {
                cfg.CreateMap<UserForRegistrationDto, UserComplete>();
             }));
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
            if (_dapper.LoadDataSingleWithParams<int>(validateEmailSql, new { Email = userForRegistrationDto.Email }) > 0)
            {
                return BadRequest("User already exists");
            }

            if (_authHelper.setPassword(new UserForLoginDto { Email = userForRegistrationDto.Email, Password = userForRegistrationDto.Password }))
            {
                UserComplete userComplete = _mapper.Map<UserComplete>(userForRegistrationDto);
                userComplete.Active = true;
                if(_reusableSql.UpsertUser(userComplete))
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

        [HttpPut("ResetPassword")]
        public IActionResult ResetPasword(UserForLoginDto userForResetPassword)
        {
            if (_authHelper.setPassword(userForResetPassword))
            {
                return Ok();
            }
            return BadRequest("Failed to reset password");
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login(UserForLoginDto userForLoginDto)
        {
            string sqlGetUserPassAndSalt = @"EXEC TutorialAppSchema.spLoginConfirmation_Get @Email = @EmailParam;";
            var userParameters = new DynamicParameters();
            userParameters.Add("EmailParam", userForLoginDto.Email);
            var userAuthData = _dapper.LoadDataSingleWithParams<UserForLoginConfirmationDto>(sqlGetUserPassAndSalt, userParameters);
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

            int userId = _dapper.LoadDataSingleWithParams<int>(@"SELECT [UserId] FROM TutorialAppSchema.Users WHERE Email = @Email;", new { Email = userForLoginDto.Email });

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

            int dbUserId = _dapper.LoadDataSingleWithParams<int>(@"SELECT [UserId] FROM TutorialAppSchema.Users WHERE UserId = @UserId;", new { UserId = userId });
            return Ok(new Dictionary<string, string> { { "token", _authHelper.CreateToken(dbUserId) } });
        }
    }
}