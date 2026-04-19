using ChatAppApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace ChatAppApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly Services.DataService _dataService;

    public ChatController(Services.DataService dataService)
    {
        _dataService = dataService;
    }

    /// <summary>
    /// Sends a message from the specified sender to the specified receiver.
    /// </summary>
    [HttpPost("message/{senderId}/{receiverId}/{content}")]
    public IActionResult SendMessage(string senderId, string receiverId, string content)
    {
        if (string.IsNullOrWhiteSpace(senderId)
            || string.IsNullOrWhiteSpace(receiverId)
            || string.IsNullOrWhiteSpace(receiverId))
        {
            return BadRequest("Invalid message format");
        }

        if (!_dataService.users.TryGetValue(senderId, out var sender) || sender == null)
        {
            return NotFound("Sender not found");
        }

        if (!_dataService.users.TryGetValue(receiverId, out var receiver) || receiver == null)
        {
            return NotFound("Receiver not found");
        }

        var createMessage = new Message
        {
            Type = "chat",
            SenderId = senderId,
            ReceiverId = receiverId,
            Timestamp = DateTime.Now,
            Data = content
        };

        sender.Messages ??= new List<Message>();
        receiver.Messages ??= new List<Message>();

        sender.Messages.Add(createMessage);

        if (senderId != receiverId)
        {
            receiver.Messages.Add(createMessage);
        }
        _dataService.users[sender.Id!] = sender;
        _dataService.users[receiver.Id!] = receiver;

        return Ok(createMessage);
    }

    /// <summary>
    /// Retrieves the chat history between the specified user and their chat partner.
    /// </summary>
    [HttpGet("history/{userId}/{chatPartnerId}")]
    public IActionResult GetMessages(string userId, string chatPartnerId)
    {
        if (!_dataService.users.TryGetValue(userId, out var user) || user == null)
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
