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
    public async Task Connect(User userConnection) //TODO: Separate of Concern
    {
        //var userState = new UserState
        //{
        //    ConnectionId = Context.ConnectionId,
        //    UserId = userConnection.UserId,
        //    UserName = userConnection.UserName,
        //    Status = "online"
        //};
        var connectionKey = Context.ConnectionId;

        if (string.IsNullOrEmpty(userConnection.ChatRoom))
        {
            userConnection.ChatRoom = "general";
        }

        var dataServiceConnection = new User
        {
            SenderId = Context.ConnectionId,
            Id = userConnection.Id,
            UserName = userConnection.UserName,
            ChatRoom = userConnection.ChatRoom,
            Status = "online"

        };
        _dataService.connections[connectionKey] = dataServiceConnection;

        var connectedUsersKey = userConnection.Id ?? userConnection.UserName ?? Context.ConnectionId;
        //ConnectedUsers[connectedUsersKey] = userState;

        await Groups.AddToGroupAsync(Context.ConnectionId, userConnection.ChatRoom);

        await Clients.All.SendAsync("UserConnected", dataServiceConnection);
        await Clients.All.SendAsync("UserListUpdated", _dataService.connections.Values.ToList());
        Console.WriteLine($"User connected: {userConnection.UserName} (ID: {userConnection.Id}) in ChatRoom: {userConnection.ChatRoom}");
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

        if (_dataService.connections.TryGetValue(Context.ConnectionId, out User userConnection))
        {
            if (string.IsNullOrEmpty(userConnection.ChatRoom))
            {
                userConnection.ChatRoom = "general";
            }
            await Clients.Group(userConnection.ChatRoom).SendAsync("ReceiveSpecificMessage", message);
            Console.WriteLine($"Message from {message.SenderId} to {message.ReceiverId}: {message.Data}");
        }
        //await Clients.Client(userConnection.ConnectionId).SendAsync("ReceiveMessage", userConnection);
    }

    /// <summary>
    /// 
    /// </summary>
    public async Task Typing(Message typingEvent)
    {
        var receiver = _dataService.connections.Values.FirstOrDefault(u => u.Id == typingEvent.ReceiverId);
        if (receiver != null)
        {
            await Clients.Client(receiver.SenderId).SendAsync("Typing", typingEvent);
        }
    }

    public override async Task OnConnectedAsync()
    {
        Console.WriteLine($"Client connected: {Context.ConnectionId}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var user = _dataService.connections.FirstOrDefault(u => u.Value.SenderId == Context.ConnectionId);
        if (user.Key != null)
        {
            _dataService.connections.TryRemove(user.Key, out _);

            await Clients.All.SendAsync("UserListUpdated", _dataService.connections.Values.ToList());
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


