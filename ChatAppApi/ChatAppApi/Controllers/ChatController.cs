using ChatAppApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace ChatAppApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly DataService.DataService _dataService;

    public ChatController(DataService.DataService dataService)
    {
        _dataService = dataService;
    }


    [HttpPost("message/{senderId}/{receiverId}/{content}")]
    public IActionResult SendMessage(string senderId, string receiverId, string content)
    {
        if (!_dataService.users.TryGetValue(senderId, out var sender) || sender is null)
        {
            return NotFound("Sender not found");
        }

        if (!_dataService.users.TryGetValue(receiverId, out var receiver) || receiver is null)
        {
            return NotFound("Receiver not found");
        }

        var createMessage = new Message
        {
            Type = "text",
            SenderId = senderId,
            ReceiverId = receiverId,
            Timestamp = DateTime.Now,
            Data = content
        };

        sender.Messages ??= new List<Message>();
        receiver.Messages ??= new List<Message>();

        if (senderId != receiverId)
        {
            receiver.Messages.Add(createMessage);
        }
        _dataService.users[sender.Id!] = sender;
        _dataService.users[receiver.Id!] = receiver;

        return Ok(createMessage);
    }

    // Request to get all messages for a user (Chat history)
    [HttpGet("history/{userId}/{chatPartnerId}")]
    public IActionResult GetMessages(string userId, string chatPartnerId)
    {
        if (!_dataService.users.TryGetValue(userId, out var user) || user is null)
        {
            return NotFound("User not found");
        }

        var history = (user.Messages ?? new List<Message>())
            .Where(message =>
                (message.SenderId == userId && message.ReceiverId == chatPartnerId) ||
                (message.SenderId == chatPartnerId && message.ReceiverId == userId))
            .OrderBy(message => message.Timestamp)
            .ToList();

        return Ok(history);
    }

    private List<User> GetUsers()
    {
        var users = new List<User>();
        foreach (var connection in _dataService.users)
        {
            if (connection.Value is User user)
            {
                users.Add(user);
            }
        }
        return users;
    }
}
