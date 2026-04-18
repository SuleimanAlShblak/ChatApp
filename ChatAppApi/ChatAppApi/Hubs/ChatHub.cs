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
        Console.WriteLine($"User connected: {user.UserName} (ID: {userId}) in ChatRoom: {user.ChatRoom}");
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
        var user = _dataService.users.Values.FirstOrDefault(u => u.ConnectionId == Context.ConnectionId);

        if (user != null)
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
    public async Task Typing(Message typingEvent)
    {
        if (typingEvent is null || string.IsNullOrEmpty(typingEvent.SenderId) || string.IsNullOrEmpty(typingEvent.ReceiverId))
        {
            await Clients.Caller.SendAsync("ReceiveError", "Invalid typing event");
            return;
        }

        var receiver = _dataService.users.Values.FirstOrDefault(u => u.Id == typingEvent.ReceiverId);
        if (receiver != null && !string.IsNullOrEmpty(receiver.ConnectionId))
        {
            await Clients.Client(receiver.ConnectionId).SendAsync("Typing", typingEvent);
        }
    }

    public async Task StopTyping(Message typingEvent)
    {
        if (typingEvent is null || string.IsNullOrEmpty(typingEvent.SenderId) || string.IsNullOrEmpty(typingEvent.ReceiverId))
        {
            await Clients.Caller.SendAsync("ReceiveError", "Invalid typing event");
            return;
        }

        var receiver = _dataService.users.Values.FirstOrDefault(u => u.Id == typingEvent.ReceiverId);
        if (receiver != null && !string.IsNullOrEmpty(receiver.ConnectionId))
        {
            await Clients.Client(receiver.ConnectionId).SendAsync("Typing", typingEvent);
        }
    }

    public override async Task OnConnectedAsync()
    {
        Console.WriteLine($"Client connected: {Context.ConnectionId}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var user = _dataService.users.Values.FirstOrDefault(u => u.ConnectionId == Context.ConnectionId);
        if (user != null)
        {
            user.Status = Status.Offline;
            user.ConnectionId = null;
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

