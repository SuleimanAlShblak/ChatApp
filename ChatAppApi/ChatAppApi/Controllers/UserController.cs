using ChatAppApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace ChatAppApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly DataService.DataService _dataService;

    public UserController(DataService.DataService dataService)
    {
        _dataService = dataService;
    }


    [HttpPost("login/{userName}")]
    public IActionResult Login(string userName)
    {
        var users = GetUsers();
        foreach (var user in users)
        {
            if (user.UserName == userName)
            {
                return Ok(new { message = "User exists", userId = user.Id });
            }
        }
        //var userId = Guid.NewGuid().ToString();
        //var userCreate = new Models.User
        //{
        //    Id = userId,
        //    UserName = userName,
        //    DisplayName = userName,
        //    Status = "online"
        //};

        //_dataService.users[userId] = userCreate;
        return Ok(new { message = "User added"/*, userId = /*userId*/});

    }

    [HttpPost("logout/{userId}")]
    public string Logout(string userId)
    {
        var users = GetUsers();
        foreach (var user in users)
        {
            if (user.Status == "online" && user.Id == userId)
            {
                var userUpdate = new User
                {
                    Id = userId,
                    UserName = user.UserName,
                    DisplayName = user.DisplayName,
                    Status = "offline"
                };
                _dataService.users[userId] = userUpdate;
                return "User logged out";
            }
            else if (user.Status == "offline" && user.Id == userId)
            {
                return "User already logged out";
            }
        }
        return "User ID not found";
    }

    [HttpGet("all")]
    public List<User> GetAllUsers()
    {
        return GetUsers();
    }

    [HttpGet("{userId}")]
    public IActionResult GetUser(string userId)
    {
        if (_dataService.users.TryGetValue(userId, out var user))
        {
            return Ok(user);
        }
        return NotFound();
    }

    private List<User> GetUsers()
    {
        var users = _dataService.users.Values.ToList();
        return users;
    }
}
