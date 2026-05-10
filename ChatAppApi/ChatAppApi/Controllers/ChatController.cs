using ChatAppApi.Hubs;
using ChatAppApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace ChatAppApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly Services.DataService _dataService;
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatController(Services.DataService dataService, IHubContext<ChatHub> hubContext)
    {
        _dataService = dataService;
        _hubContext = hubContext;
    }

    /// <summary>
    /// Sends a message from the specified sender to the specified receiver.
    /// </summary>
    [HttpPost("message")]
    public async Task<IActionResult> SendMessage([FromBody] Message MessageBody)
    {
        if (MessageBody == null || string.IsNullOrWhiteSpace(MessageBody.SenderId)
            && string.IsNullOrWhiteSpace(MessageBody.ReceiverId)
            && string.IsNullOrWhiteSpace(MessageBody.MessageContent))
        {
            return BadRequest("Invalid message format");
        }

        var senderId = MessageBody.SenderId;
        var receiverId = MessageBody.ReceiverId;

        if (!_dataService.users.TryGetValue(senderId, out var sender) || sender == null)
        {
            return NotFound("Sender not found");
        }

        if (!_dataService.users.TryGetValue(receiverId, out var receiver) || receiver == null)
        {
            return NotFound("Receiver not found");
        }

        var createdMessage = new Message
        {
            Type = "chat",
            SenderId = senderId,
            ReceiverId = receiverId,
            Timestamp = DateTime.Now,
            MessageContent = MessageBody.MessageContent
        };

        sender.Messages ??= new List<Message>();
        receiver.Messages ??= new List<Message>();
        sender.Messages.Add(createdMessage);
        if (senderId != receiverId)
        {
            receiver.Messages.Add(createdMessage);
        }

        _dataService.users[sender.Id!] = sender;
        _dataService.users[receiver.Id!] = receiver;

        if (!string.IsNullOrEmpty(receiver.ConnectionId))
        {
            var hubContext = HttpContext.RequestServices.GetService(typeof(Microsoft.AspNetCore.SignalR.IHubContext<Hubs.ChatHub>)) as Microsoft.AspNetCore.SignalR.IHubContext<Hubs.ChatHub>;
            if (hubContext != null)
            {
                await hubContext.Clients.Client(receiver.ConnectionId).SendAsync("ReceiveMessage", createdMessage);
            }
        }

        if (!string.IsNullOrEmpty(sender.ConnectionId))
            await _hubContext.Clients.Client(sender.ConnectionId)
                .SendAsync("ReceiveSpecificMessage", createdMessage);

        return Ok(createdMessage);
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
}
