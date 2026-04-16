namespace ChatAppApi.Models;

public class Message
{
    public string? Type { get; set; }

    public string? SenderId { get; set; }

    public string? ReceiverId { get; set; }

    public DateTime Timestamp { get; set; }

    public string? Data { get; set; }
}

