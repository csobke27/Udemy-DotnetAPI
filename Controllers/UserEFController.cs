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

    [HttpGet("GetUsers")]
    public IEnumerable<User> GetUsers()
    {
        var result = _entityFramework.Users.ToList();
        return result;
    }

    [HttpGet("GetUsers/{userId}")]
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

    [HttpPut("DeleteUser")]
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
}