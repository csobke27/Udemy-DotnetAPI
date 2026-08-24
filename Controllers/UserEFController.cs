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
    // private readonly DataContextEF _entityFramework;
    IUserRepository _userRepository;

    IMapper _mapper;
    public UserEFController(IConfiguration configuration, IUserRepository userRepository)
    {
        // _entityFramework = new DataContextEF(configuration);
        _userRepository = userRepository;
        _mapper = new Mapper(new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<UserToAddDto, User>();
        }));
    }

    // User Endpoints
    [HttpGet("GetUsers")]
    public IEnumerable<User> GetUsers()
    {
        return _userRepository.GetUsers();
    }

    [HttpGet("GetUser/{userId}")]
    public User GetSingleUser(int userId)
    {
        return _userRepository.GetSingleUser(userId);
    }

    [HttpPut("EditUser")]
    public IActionResult EditUser(User user)
    {
        User? existingUser = _userRepository.GetSingleUser(user.UserId);

        existingUser.FirstName = user.FirstName;
        existingUser.LastName = user.LastName;
        existingUser.Email = user.Email;
        existingUser.Gender = user.Gender;
        existingUser.Active = user.Active;

        if(_userRepository.SaveChanges())
        {
            return Ok();
        }
        throw new Exception("Failed to update user.");
    }

    [HttpPost("AddUser")]
    public IActionResult AddUser(UserToAddDto user)
    {
        var newUser = _mapper.Map<User>(user);

        _userRepository.AddEntity<User>(newUser);
        if(_userRepository.SaveChanges())
        {
            return Ok();
        }
        throw new Exception("Failed to add user.");
    }

    [HttpDelete("DeleteUser")]
    public IActionResult DeleteUser(int userId)
    {
        var existingUser = _userRepository.GetSingleUser(userId);

        _userRepository.RemoveEntity<User>(existingUser);
        if(_userRepository.SaveChanges())
        {
            return Ok();
        }
        throw new Exception("Failed to delete user.");
    }

    // Salary Endpoints
    [HttpGet("GetUserSalary/{userId}")]
    public UserSalary GetUserSalary(int userId)
    {        
        return _userRepository.GetSingleUserSalary(userId);
    }

    [HttpPut("EditUserSalary")]
    public IActionResult EditUserSalary(UserSalary userSalary)
    {
        UserSalary? existingUserSalary = _userRepository.GetSingleUserSalary(userSalary.UserId);

        _mapper.Map(userSalary, existingUserSalary);
        if(_userRepository.SaveChanges())
        {
            return Ok();
        }
        throw new Exception("Failed to update user salary.");
    }

    [HttpPost("AddUserSalary")]
    public IActionResult AddUserSalary(UserSalary userSalary)
    {
        _userRepository.AddEntity<UserSalary>(userSalary);
        if(_userRepository.SaveChanges())
        {
            return Ok();
        }
        throw new Exception("Failed to add user salary.");
    }

    [HttpDelete("DeleteUserSalary")]
    public IActionResult DeleteUserSalary(int userId)
    {
        var existingUserSalary = _userRepository.GetSingleUserSalary(userId);

        _userRepository.RemoveEntity<UserSalary>(existingUserSalary);
        if(_userRepository.SaveChanges())
        {
            return Ok();
        }
        throw new Exception("Failed to delete user salary.");
    }

    // Job Info Endpoints
    [HttpGet("GetUserJobInfo/{userId}")]
    public UserJobInfo GetUserJobInfo(int userId)
    {
        return _userRepository.GetSingleUserJobInfo(userId);
    }

    [HttpPut("EditUserJobInfo")]
    public IActionResult EditUserJobInfo(UserJobInfo userJobInfo)
    {
        UserJobInfo? existingUserJobInfo = _userRepository.GetSingleUserJobInfo(userJobInfo.UserId);

        _mapper.Map(userJobInfo, existingUserJobInfo);
        if(_userRepository.SaveChanges())
        {
            return Ok();
        }
        throw new Exception("Failed to update user job info.");
    }

    [HttpPost("AddUserJobInfo")]
    public IActionResult AddUserJobInfo(UserJobInfo userJobInfo)
    {
        _userRepository.AddEntity<UserJobInfo>(userJobInfo);
        if(_userRepository.SaveChanges())
        {
            return Ok();
        }
        throw new Exception("Failed to add user job info.");
    }

    [HttpDelete("DeleteUserJobInfo")]
    public IActionResult DeleteUserJobInfo(int userId)
    {
        var existingUserJobInfo = _userRepository.GetSingleUserJobInfo(userId);

        _userRepository.RemoveEntity<UserJobInfo>(existingUserJobInfo);
        if(_userRepository.SaveChanges())
        {
            return Ok();
        }
        throw new Exception("Failed to delete user job info.");
    }
}