namespace ChatAppApi.Models;

public class User
{
    public string? Id { get; set; }
    public string? ConnectionId { get; set; }

    public string? UserName { get; set; }

    public string? DisplayName { get; set; }

    public string? Image { get; set; }

    public Status Status { get; set; } = Status.Offline;

    public string? SenderId { get; set; }// ToDO: Separate of Concern create a new class for message sending and receiving inherit from this class

    public bool isTyping { get; set; } = false;

    public string? ReceiverId { get; set; }

    public string? ChatRoom { get; set; }

    public List<Message>? Messages { get; set; }
}


