namespace ChatAppApi.Models;

public class ChatMessageDto
{
    public string? Type { get; set; }

    public string? SenderId { get; set; }

    public string? ReceiverId { get; set; }

    public string? Data { get; set; }
}

