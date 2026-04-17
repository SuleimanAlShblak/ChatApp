using ChatAppApi.Models;
using Microsoft.AspNetCore.SignalR;
namespace ChatAppApi.Hubs;

public class ChatHub : Hub
{
    public const string HubUrl = "/Chat";

    // In memory storage for users
    //private static readonly Dictionary<string, UserState> ConnectedUsers = new();

    private readonly DataService.DataService _dataService;
    public ChatHub(DataService.DataService dataService) => _dataService = dataService;

    /// <summary>
    /// 
    /// </summary>
    public async Task Connect(User user) //TODO: Separate of Concern
    {
        //var connectionKey = Context.ConnectionId;

        if (string.IsNullOrEmpty(user.ChatRoom))
        {
            user.ChatRoom = "general";
        }

        var userId = Guid.NewGuid().ToString();

        var dataServiceConnection = new User
        {
            SenderId = userId,
            ReceiverId = userId,
            Id = userId,
            UserName = user.UserName,
            ChatRoom = user.ChatRoom,
            Status = "online"

        };
        _dataService.users[userId] = dataServiceConnection;

        var connectedUsersKey = user.Id ?? user.UserName ?? Context.ConnectionId;

        await Groups.AddToGroupAsync(Context.ConnectionId, user.ChatRoom);

        await Clients.All.SendAsync("UserConnected", dataServiceConnection);
        await Clients.All.SendAsync("UserListUpdated", _dataService.users.Values.ToList());
        Console.WriteLine($"User connected: {user.UserName} (ID: {user.Id}) in ChatRoom: {user.ChatRoom}");
    }

    /// <summary>
    /// 
    /// </summary>
    public async Task SendMessage(Message message)
    {
        if (!IsValidMessage(message))
        {
            await Clients.Caller.SendAsync("ReceiveError", "Invalid message format");
            Console.WriteLine(($"Message from {message.SenderId} to {message.ReceiverId}: {message.Data}"));
            return;
        }

        //if (!ConnectedUsers.TryGetValue(message.ReceiverId, out var receiverState))
        //{
        //    await Clients.Caller.SendAsync("ReceiveError", "Receiver not found");
        //    return;
        //}

        if (_dataService.users.TryGetValue(Context.ConnectionId, out User user))
        {
            if (string.IsNullOrEmpty(user.ChatRoom))
            {
                user.ChatRoom = "general";
            }
            await Clients.Group(user.ChatRoom).SendAsync("ReceiveSpecificMessage", message);
            Console.WriteLine($"Message from {message.SenderId} to {message.ReceiverId}: {message.Data}");
        }
        //await Clients.Client(userConnection.ConnectionId).SendAsync("ReceiveMessage", userConnection);
    }

    /// <summary>
    /// 
    /// </summary>
    public async Task Typing(Message typingEvent, User user)
    {
        var receiver = _dataService.users.Values.FirstOrDefault(u => u.Id == typingEvent.ReceiverId);
        if (receiver != null && !string.IsNullOrEmpty(receiver.SenderId))
        {
            await Clients.Client(receiver.SenderId).SendAsync("Typing", typingEvent);
            user.isTyping = typingEvent.Type == "typing";
        }
    }

    public override async Task OnConnectedAsync()
    {
        Console.WriteLine($"Client connected: {Context.ConnectionId}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var user = _dataService.users.FirstOrDefault(u => u.Value.SenderId == Context.ConnectionId);
        if (user.Key != null)
        {
            _dataService.users.TryRemove(user.Key, out _);

            await Clients.All.SendAsync("UserListUpdated", _dataService.users.Values.ToList());
        }
        Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
        await base.OnDisconnectedAsync(exception);
    }

    private bool IsValidMessage(Message message)
    {
        return !string.IsNullOrEmpty(message.Type) &&
               new[] { "connect", "chat", "typing", "error" }.Contains(message.Type) &&
               !string.IsNullOrEmpty(message.SenderId) &&
               !string.IsNullOrEmpty(message.ReceiverId) &&
               !string.IsNullOrEmpty(message.Data);
    }
}


