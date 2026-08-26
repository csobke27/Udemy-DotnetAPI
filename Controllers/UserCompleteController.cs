using Dapper;
using DotnetAPI.Data;
using DotnetAPI.Helpers;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class UserCompleteController : ControllerBase
{
    private readonly DataContextDapper _dapper;
    private readonly ReusableSql _reusableSql;
    public UserCompleteController(IConfiguration config)
    {
        _dapper = new DataContextDapper(config);
        _reusableSql = new ReusableSql(config);
    }

    // User Endpoints
    [HttpGet("GetUsers/{userId}/{isActive}")]
    public IEnumerable<UserComplete> GetUsers(int userId, bool? isActive)
    {
        var arguments = new List<string>();
        var parameters = new DynamicParameters();
        if(userId != 0)
        {
            arguments.Add("@UserId = @UserIdParam");
            parameters.Add("UserIdParam", userId);
        }
        if(isActive.HasValue)
        {
            arguments.Add("@Active = @ActiveParam");
            parameters.Add("ActiveParam", isActive.Value);
        }
        string sql = $"EXEC TutorialAppSchema.spUsers_Get {string.Join(", ", arguments)}";

        IEnumerable<UserComplete> result = _dapper.LoadDataWithParams<UserComplete>(sql, parameters);
        return result;
    }

    [HttpPut("UpsertUser")]
    public IActionResult UpsertUser(UserComplete user)
    {
        if(_reusableSql.UpsertUser(user))
        {
            return Ok();
        }
        throw new Exception("Failed to update user.");
    }

    [HttpDelete("DeleteUser")]
    public IActionResult DeleteUser(int userId)
    {
        string sql = @"EXEC TutorialAppSchema.spUser_Delete @UserId = @UserIdParam;";
        var parameters = new DynamicParameters();
        parameters.Add("UserIdParam", userId);
        if(_dapper.ExecuteSQL(sql, parameters))
        {
            return Ok();
        }
        throw new Exception("Failed to delete user.");
    }
}