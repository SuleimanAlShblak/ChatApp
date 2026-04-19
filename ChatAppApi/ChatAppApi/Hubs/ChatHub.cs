using ChatAppApi.Helpers;
using ChatAppApi.Models;
using Microsoft.AspNetCore.SignalR;
using Serilog;
namespace ChatAppApi.Hubs;

public class ChatHub : Hub
{
    public const string HubUrl = "/Chat";

    private readonly Services.DataService _dataService;
    public ChatHub(Services.DataService dataService) => _dataService = dataService;

    /// <summary>
    /// Connects a user to the chat hub.
    /// </summary>
    public async Task Connect(User user)
    {
        if (string.IsNullOrEmpty(user.ChatRoom))
        {
            user.ChatRoom = "general";
        }

        var existingUser = _dataService.users.Values.FirstOrDefault(u => u.UserName == user.UserName);
        string userId;

        if (existingUser != null)
        {
            userId = existingUser.Id!;
            existingUser.Status = Status.Online;
            existingUser.ConnectionId = Context.ConnectionId;
            existingUser.ChatRoom = user.ChatRoom;
        }
        else
        {
            userId = Guid.NewGuid().ToString();
            var dataServiceConnection = new User
            {
                SenderId = userId,
                ReceiverId = userId,
                Id = userId,
                ConnectionId = Context.ConnectionId,
                UserName = user.UserName,
                ChatRoom = user.ChatRoom,
                Status = Status.Online

            };
            _dataService.users[userId] = dataServiceConnection;
        }
        await Groups.AddToGroupAsync(Context.ConnectionId, user.ChatRoom);

        await Clients.All.SendAsync("UserConnected", _dataService.users[userId]);
        await Clients.All.SendAsync("UserListUpdated", _dataService.users.Values.ToList());
        Log.Information($"User connected: {user.UserName} (ID: {userId}) in ChatRoom: {user.ChatRoom}");
    }


    /// <summary>
    /// Sends a message from the connected user to the specified receiver.
    /// </summary>
    public async Task SendMessage(Message message)
    {
        if (!ValidationHelper.IsValidMessage(message))
        {
            await Clients.Caller.SendAsync("ReceiveError", "Invalid message format");
            return;
        }

        var user = _dataService.users.Values.FirstOrDefault(u => u.ConnectionId == Context.ConnectionId);

        if (user != null)
        {
            if (string.IsNullOrEmpty(user.ChatRoom))
            {
                user.ChatRoom = "general";
            }

            await Clients.Group(user.ChatRoom).SendAsync("ReceiveSpecificMessage", message);
            Log.Debug($"Message from {message.SenderId} to {message.ReceiverId}: {message.Data}");
        }
    }

    /// <summary>
    /// Notifies the receiver that the sender is typing.
    /// </summary>
    public async Task Typing(Message typingEvent)
    {
        if (typingEvent == null || string.IsNullOrEmpty(typingEvent.SenderId) || string.IsNullOrEmpty(typingEvent.ReceiverId))
        {
            await Clients.Caller.SendAsync("ReceiveError", "Invalid typing event");
            return;
        }

        var receiver = _dataService.users.Values.FirstOrDefault(u => u.Id == typingEvent.ReceiverId);
        if (receiver != null && !string.IsNullOrEmpty(receiver.ConnectionId))
        {
            Log.Debug($"Typing event from {typingEvent.SenderId} to {typingEvent.ReceiverId}");
            await Clients.Client(receiver.ConnectionId).SendAsync("Typing", typingEvent);
        }
    }

    public async Task StopTyping(Message typingEvent)
    {
        if (typingEvent == null || string.IsNullOrEmpty(typingEvent.SenderId) || string.IsNullOrEmpty(typingEvent.ReceiverId))
        {
            await Clients.Caller.SendAsync("ReceiveError", "Invalid typing event");
            return;
        }

        var receiver = _dataService.users.Values.FirstOrDefault(u => u.Id == typingEvent.ReceiverId);
        if (receiver != null && !string.IsNullOrEmpty(receiver.ConnectionId))
        {
            Log.Debug($"Stop typing event from {typingEvent.SenderId} to {typingEvent.ReceiverId}");
            await Clients.Client(receiver.ConnectionId).SendAsync("Typing", typingEvent);
        }
    }

    public override async Task OnConnectedAsync()
    {
        Log.Information($"Client connected: {Context.ConnectionId}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var user = _dataService.users.Values.FirstOrDefault(u => u.ConnectionId == Context.ConnectionId);
        if (user != null)
        {
            user.Status = Status.Offline;
            user.ConnectionId = null;
            Log.Debug($"User disconnected: {user.UserName} (ID: {user.Id})");
            await Clients.All.SendAsync("UserListUpdated", _dataService.users.Values.ToList());
        }
        Log.Information($"Client disconnected: {Context.ConnectionId}");
        await base.OnDisconnectedAsync(exception);
    }
}
