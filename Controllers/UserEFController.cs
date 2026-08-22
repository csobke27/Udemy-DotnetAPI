using AutoMapper;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UserEFController : ControllerBase
{
    private readonly DataContextEF _entityFramework;

    IMapper _mapper;
    public UserEFController(IConfiguration configuration)
    {
        _entityFramework = new DataContextEF(configuration);
        _mapper = new Mapper(new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<UserToAddDto, User>();
        }));
    }

    // User Endpoints
    [HttpGet("GetUsers")]
    public IEnumerable<User> GetUsers()
    {
        var result = _entityFramework.Users.ToList();
        return result;
    }

    [HttpGet("GetUser/{userId}")]
    public User GetSingleUser(int userId)
    {
        User? user = _entityFramework.Users.FirstOrDefault(u => u.UserId == userId);
        if(user == null)
        {
            throw new Exception("User not found.");
        }
        return user;
    }

    [HttpPut("EditUser")]
    public IActionResult EditUser(User user)
    {
        User? existingUser = _entityFramework.Users.FirstOrDefault(u => u.UserId == user.UserId);
        if (existingUser == null)
        {
            throw new Exception("User not found.");
        }

        existingUser.FirstName = user.FirstName;
        existingUser.LastName = user.LastName;
        existingUser.Email = user.Email;
        existingUser.Gender = user.Gender;
        existingUser.Active = user.Active;

        if(_entityFramework.SaveChanges() > 0)
        {
            return Ok();
        }
        throw new Exception("Failed to update user.");
    }

    [HttpPost("AddUser")]
    public IActionResult AddUser(UserToAddDto user)
    {
        var newUser = _mapper.Map<User>(user);

        _entityFramework.Users.Add(newUser);
        if(_entityFramework.SaveChanges() > 0)
        {
            return Ok();
        }
        throw new Exception("Failed to add user.");
    }

    [HttpDelete("DeleteUser")]
    public IActionResult DeleteUser(int userId)
    {
        var existingUser = _entityFramework.Users.FirstOrDefault(u => u.UserId == userId);
        if (existingUser == null)
        {
            throw new Exception("User not found.");
        }

        _entityFramework.Users.Remove(existingUser);
        if(_entityFramework.SaveChanges() > 0)
        {
            return Ok();
        }
        throw new Exception("Failed to delete user.");
    }

    // Salary Endpoints
    [HttpGet("GetUserSalary/{userId}")]
    public UserSalary GetUserSalary(int userId)
    {
        UserSalary? userSalary = _entityFramework.UserSalary.FirstOrDefault(u => u.UserId == userId);
        if(userSalary == null)
        {
            throw new Exception("User salary not found.");
        }
        return userSalary;
    }

    [HttpPut("EditUserSalary")]
    public IActionResult EditUserSalary(UserSalary userSalary)
    {
        UserSalary? existingUserSalary = _entityFramework.UserSalary.FirstOrDefault(u => u.UserId == userSalary.UserId);
        if (existingUserSalary == null)
        {
            throw new Exception("User salary not found.");
        }

        // existingUserSalary.Salary = userSalary.Salary;
        // existingUserSalary.AvgSalary = userSalary.AvgSalary;
        _mapper.Map(userSalary, existingUserSalary);
        if(_entityFramework.SaveChanges() > 0)
        {
            return Ok();
        }
        throw new Exception("Failed to update user salary.");
    }

    [HttpPost("AddUserSalary")]
    public IActionResult AddUserSalary(UserSalary userSalary)
    {
        _entityFramework.UserSalary.Add(userSalary);
        if(_entityFramework.SaveChanges() > 0)
        {
            return Ok();
        }
        throw new Exception("Failed to add user salary.");
    }

    [HttpDelete("DeleteUserSalary")]
    public IActionResult DeleteUserSalary(int userId)
    {
        var existingUserSalary = _entityFramework.UserSalary.FirstOrDefault(u => u.UserId == userId);
        if (existingUserSalary == null)
        {
            throw new Exception("User salary not found.");
        }

        _entityFramework.UserSalary.Remove(existingUserSalary);
        if(_entityFramework.SaveChanges() > 0)
        {
            return Ok();
        }
        throw new Exception("Failed to delete user salary.");
    }

    // Job Info Endpoints
    [HttpGet("GetUserJobInfo/{userId}")]
    public UserJobInfo GetUserJobInfo(int userId)
    {
        UserJobInfo? userJobInfo = _entityFramework.UserJobInfo.FirstOrDefault(u => u.UserId == userId);
        if(userJobInfo == null)
        {
            throw new Exception("User job info not found.");
        }
        return userJobInfo;
    }

    [HttpPut("EditUserJobInfo")]
    public IActionResult EditUserJobInfo(UserJobInfo userJobInfo)
    {
        UserJobInfo? existingUserJobInfo = _entityFramework.UserJobInfo.FirstOrDefault(u => u.UserId == userJobInfo.UserId);
        if (existingUserJobInfo == null)
        {
            throw new Exception("User job info not found.");
        }

        // existingUserJobInfo.JobTitle = userJobInfo.JobTitle;
        // existingUserJobInfo.Department = userJobInfo.Department;
        _mapper.Map(userJobInfo, existingUserJobInfo);

        if(_entityFramework.SaveChanges() > 0)
        {
            return Ok();
        }
        throw new Exception("Failed to update user job info.");
    }

    [HttpPost("AddUserJobInfo")]
    public IActionResult AddUserJobInfo(UserJobInfo userJobInfo)
    {
        _entityFramework.UserJobInfo.Add(userJobInfo);
        if(_entityFramework.SaveChanges() > 0)
        {
            return Ok();
        }
        throw new Exception("Failed to add user job info.");
    }

    [HttpDelete("DeleteUserJobInfo")]
    public IActionResult DeleteUserJobInfo(int userId)
    {
        var existingUserJobInfo = _entityFramework.UserJobInfo.FirstOrDefault(u => u.UserId == userId);
        if (existingUserJobInfo == null)
        {
            throw new Exception("User job info not found.");
        }

        _entityFramework.UserJobInfo.Remove(existingUserJobInfo);
        if(_entityFramework.SaveChanges() > 0)
        {
            return Ok();
        }
        throw new Exception("Failed to delete user job info.");
    }
}