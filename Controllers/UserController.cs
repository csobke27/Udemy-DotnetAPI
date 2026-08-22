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

    [HttpGet("GetUsers/{userId}")]
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
    public IActionResult AddUser(UserDto user)
    {
        string sql = @"INSERT INTO TutorialAppSchema.Users ([FirstName], [LastName], [Email], [Gender], [Active])
                       VALUES (@FirstName, @LastName, @Email, @Gender, @Active);";

        if(_dapper.ExecuteSQL(sql, new { user.FirstName, user.LastName, user.Email, user.Gender, user.Active }))
        {
            return Ok();
        }
        throw new Exception("Failed to add user.");
    }

    [HttpPut("DeleteUser")]
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
}