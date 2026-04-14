using ChatAppApi.Models;
using Microsoft.AspNetCore.SignalR;

namespace ChatAppApi.Hubs;

public class ChatHub : Hub
{
    public const string HubUrl = "/Chat";

    // In memory storage for users
    private static readonly Dictionary<string, UserState> ConnectedUsers = new();

    /// <summary>
    /// 
    /// </summary>
    public async Task Connect(UserConnection userConnection)
    {
        var userState = new UserState
        {
            ConnectionId = Context.ConnectionId,
            UserId = userConnection.UserId,
            UserName = userConnection.UserName,
            Status = "online"
        };
        ConnectedUsers[userConnection.UserName] = userState;

        await Clients.All.SendAsync("UserConnected", userState);
    }

    /// <summary>
    /// 
    /// </summary>
    public async Task SendMessage(ChatMessageDto message)
    {
        if (!IsValidMessage(message))
        {
            await Clients.Caller.SendAsync("ReceiveError", "Invalid message format");
            return;
        }

        if (!ConnectedUsers.TryGetValue(message.ReceiverId, out var receiverState))
        {
            await Clients.Caller.SendAsync("ReceiveError", "Receiver not found");
            return;
        }

        await Clients.Client(receiverState.ConnectionId).SendAsync("ReceiveMessage", message);
    }

    /// <summary>
    /// 
    /// </summary>
    public async Task Typing(ChatMessageDto typingEvent)
    {
        if (!ConnectedUsers.TryGetValue(typingEvent.ReceiverId, out var receiverState))
        {
            return;
        }

        await Clients.Client(receiverState.ConnectionId).SendAsync("Typing", typingEvent);
    }

    public override async Task OnConnectedAsync()
    {
        Console.WriteLine($"Client connected: {Context.ConnectionId}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var user = ConnectedUsers.FirstOrDefault(u => u.Value.ConnectionId == Context.ConnectionId);
        if (user.Key != null)
        {
            ConnectedUsers.Remove(user.Key);

            await Clients.All.SendAsync("UserListUpdated", ConnectedUsers.Values.ToList());
        }
        Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
        await base.OnDisconnectedAsync(exception);
    }

    private bool IsValidMessage(ChatMessageDto message)
    {
        return !string.IsNullOrEmpty(message.Type) &&
               new[] { "connect", "chat", "typing", "error" }.Contains(message.Type) &&
               !string.IsNullOrEmpty(message.SenderId) &&
               !string.IsNullOrEmpty(message.ReceiverId) &&
               !string.IsNullOrEmpty(message.Data);
    }
}


