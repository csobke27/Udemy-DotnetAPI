using DotnetAPI.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    DataContextDapper _dapper;
    public UserController(IConfiguration config)
    {
        _dapper = new DataContextDapper(config);
    }

    // User Endpoints
    [HttpGet("GetUsers")]
    public IEnumerable<User> GetUsers()
    {
        string sql = @"SELECT [UserId],
                        [FirstName],
                        [LastName],
                        [Email],
                        [Gender],
                        [Active]
                    FROM TutorialAppSchema.Users;";

        var result = _dapper.LoadData<User>(sql);
        return result;
    }

    [HttpGet("GetUser/{userId}")]
    public User GetSingleUser(int userId)
    {
        string sql = @"SELECT [UserId],
                        [FirstName],
                        [LastName],
                        [Email],
                        [Gender],
                        [Active]
                    FROM TutorialAppSchema.Users
                    WHERE [UserId] = @UserId;";

        var result = _dapper.LoadDataSingle<User>(sql, new { UserId = userId });
        return result;
    }

    [HttpPut("EditUser")]
    public IActionResult EditUser(User user)
    {
        string sql = @"UPDATE TutorialAppSchema.Users
                       SET [FirstName] = @FirstName,
                           [LastName] = @LastName,
                           [Email] = @Email,
                           [Gender] = @Gender,
                           [Active] = @Active
                       WHERE [UserId] = @UserId;";

        if(_dapper.ExecuteSQL(sql, new { user.UserId, user.FirstName, user.LastName, user.Email, user.Gender, user.Active }))
        {
            return Ok();
        }
        throw new Exception("Failed to update user.");
    }

    [HttpPost("AddUser")]
    public IActionResult AddUser(UserToAddDto user)
    {
        string sql = @"INSERT INTO TutorialAppSchema.Users ([FirstName], [LastName], [Email], [Gender], [Active])
                       VALUES (@FirstName, @LastName, @Email, @Gender, @Active);";

        if(_dapper.ExecuteSQL(sql, new { user.FirstName, user.LastName, user.Email, user.Gender, user.Active }))
        {
            return Ok();
        }
        throw new Exception("Failed to add user.");
    }

    [HttpDelete("DeleteUser")]
    public IActionResult DeleteUser(int userId)
    {
        string sql = @"DELETE FROM TutorialAppSchema.Users
                       WHERE [UserId] = @UserId;";

        if(_dapper.ExecuteSQL(sql, new { UserId = userId }))
        {
            return Ok();
        }
        throw new Exception("Failed to delete user.");
    }

    // Salary Endpoints
    [HttpGet("UserSalary/{userId}")]
    public UserSalary GetUserSalary(int userId)
    {
        string sql = @"SELECT [UserId],
                        [Salary],
                        [AvgSalary]
                    FROM TutorialAppSchema.UserSalary
                    WHERE [UserId] = @UserId;";

        var result = _dapper.LoadDataSingle<UserSalary>(sql, new { UserId = userId });
        return result;
    }

    [HttpPut("EditUserSalary")]
    public IActionResult EditUserSalary(UserSalary userSalary)
    {
        string sql = @"UPDATE TutorialAppSchema.UserSalary
                       SET [Salary] = @Salary,
                           [AvgSalary] = @AvgSalary
                       WHERE [UserId] = @UserId;";

        if(_dapper.ExecuteSQL(sql, new { userSalary.UserId, userSalary.Salary, userSalary.AvgSalary }))
        {
            return Ok();
        }
        throw new Exception("Failed to update user salary.");
    }

    [HttpPost("AddUserSalary")]
    public IActionResult AddUserSalary(UserSalary userSalary)
    {
        string sql = @"INSERT INTO TutorialAppSchema.UserSalary ([UserId], [Salary], [AvgSalary])
                       VALUES (@UserId, @Salary, @AvgSalary);";

        if(_dapper.ExecuteSQL(sql, new { userSalary.UserId, userSalary.Salary, userSalary.AvgSalary }))
        {
            return Ok();
        }
        throw new Exception("Failed to add user salary.");
    }

    [HttpDelete("DeleteUserSalary")]
    public IActionResult DeleteUserSalary(int userId)
    {
        string sql = @"DELETE FROM TutorialAppSchema.UserSalary
                       WHERE [UserId] = @UserId;";

        if(_dapper.ExecuteSQL(sql, new { UserId = userId }))
        {
            return Ok();
        }
        throw new Exception("Failed to delete user salary.");
    }

    // Job Info Endpoints
    [HttpGet("UserJobInfo/{userId}")]
    public UserJobInfo GetUserJobInfo(int userId)
    {
        string sql = @"SELECT [UserId],
                        [JobTitle],
                        [Department]
                    FROM TutorialAppSchema.UserJobInfo
                    WHERE [UserId] = @UserId;";

        var result = _dapper.LoadDataSingle<UserJobInfo>(sql, new { UserId = userId });
        return result;
    }

    [HttpPut("EditUserJobInfo")]
    public IActionResult EditUserJobInfo(UserJobInfo userJobInfo)
    {
        string sql = @"UPDATE TutorialAppSchema.UserJobInfo
                       SET [JobTitle] = @JobTitle,
                           [Department] = @Department
                       WHERE [UserId] = @UserId;";

        if(_dapper.ExecuteSQL(sql, new { userJobInfo.UserId, userJobInfo.JobTitle, userJobInfo.Department }))
        {
            return Ok();
        }
        throw new Exception("Failed to update user job info.");
    }

    [HttpPost("AddUserJobInfo")]
    public IActionResult AddUserJobInfo(UserJobInfo userJobInfo)
    {
        string sql = @"INSERT INTO TutorialAppSchema.UserJobInfo ([UserId], [JobTitle], [Department])
                       VALUES (@UserId, @JobTitle, @Department);";

        if(_dapper.ExecuteSQL(sql, new { userJobInfo.UserId, userJobInfo.JobTitle, userJobInfo.Department }))
        {
            return Ok();
        }
        throw new Exception("Failed to add user job info.");
    }

    [HttpDelete("DeleteUserJobInfo")]
    public IActionResult DeleteUserJobInfo(int userId)
    {
        string sql = @"DELETE FROM TutorialAppSchema.UserJobInfo
                       WHERE [UserId] = @UserId;";

        if(_dapper.ExecuteSQL(sql, new { UserId = userId }))
        {
            return Ok();
        }
        throw new Exception("Failed to delete user job info.");
    }
}