using ChatAppApi.Hubs;
using ChatAppApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace ChatAppApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly DataService.DataService _dataService;
    private readonly IHubContext<ChatHub> _hubContext;
    public UserController(DataService.DataService dataService, IHubContext<ChatHub> hubContext)
    {
        _dataService = dataService;
        _hubContext = hubContext;
    }

    /// <summary>
    /// Logs in a user with the specified username.
    /// </summary>
    [HttpPost("login/{userName}")]
    public IActionResult Login(string userName)
    {
        var existingUser = _dataService.users.Values.FirstOrDefault(u => u.UserName == userName);
        if (existingUser != null)
        {
            return Ok(new { message = "User exists", userId = existingUser.Id });
        }

        var userId = Guid.NewGuid().ToString();
        var newUser = new Models.User
        {
            Id = userId,
            UserName = userName,
            DisplayName = userName,
            Status = Status.Offline
        };

        _dataService.users[userId] = newUser;
        return Ok(new { message = "User added", userId = userId });

    }

    /// <summary>
    /// Logs out a user with the specified user ID.
    /// </summary>
    [HttpPost("logout/{userId}")]
    public async Task<IActionResult> Logout(string userId)
    {
        if (!_dataService.users.TryGetValue(userId, out var user))
        {
            return NotFound("User ID not found");
        }

        if (user.Status == Status.Online)
        {
            return Ok("User logged out successfully");
        }

        user.Status = Status.Offline;
        user.ConnectionId = null;

        await _hubContext.Clients.All.SendAsync("UserListUpdated", _dataService.users.Values.ToList());

        return Ok("User logged out");

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
