using Dapper;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UserCompleteController : ControllerBase
{
    DataContextDapper _dapper;
    public UserCompleteController(IConfiguration config)
    {
        _dapper = new DataContextDapper(config);
    }

    // User Endpoints
    [HttpGet("GetUsers/{userId}/{isActive}")]
    public IEnumerable<UserComplete> GetUsers(int userId, bool isActive)
    {
        string sql = @"EXEC TutorialAppSchema.spUsers_Get";
        var parameters = new DynamicParameters();
        if(userId != 0)
        {
            sql += " @UserId = @UserIdParam,";
            parameters.Add("UserIdParam", userId);
        }
        if(isActive)
        {
            sql += " @Active = @ActiveParam,";
            parameters.Add("ActiveParam", isActive);
        }
        // Remove trailing comma if it exists
        sql = _dapper.TrimEndComma(sql);

        IEnumerable<UserComplete> result = _dapper.LoadDataWithParams<UserComplete>(sql, parameters);
        return result;
    }

    [HttpPut("UpsertUser")]
    public IActionResult UpsertUser(UserComplete user)
    {
        string sql = @"EXEC TutorialAppSchema.spUser_Upsert
                       @FirstName = @FirstNameParam,
                       @LastName = @LastNameParam,
                       @Email = @EmailParam,
                       @Gender = @GenderParam,
                       @Active = @ActiveParam,
                       @JobTitle = @JobTitleParam,
                       @Department = @DepartmentParam,
                       @Salary = @SalaryParam,
                       @UserId = @UserIdParam;";
        var parameters = new DynamicParameters();
        parameters.Add("FirstNameParam", user.FirstName);
        parameters.Add("LastNameParam", user.LastName);
        parameters.Add("EmailParam", user.Email);
        parameters.Add("GenderParam", user.Gender);
        parameters.Add("ActiveParam", user.Active);
        parameters.Add("JobTitleParam", user.JobTitle);
        parameters.Add("DepartmentParam", user.Department);
        parameters.Add("SalaryParam", user.Salary);
        parameters.Add("UserIdParam", user.UserId);

        if(_dapper.ExecuteSQL(sql, parameters))
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