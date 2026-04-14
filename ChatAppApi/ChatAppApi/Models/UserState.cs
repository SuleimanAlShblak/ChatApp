namespace ChatAppApi.Models;

public class UserState
{

    public string? UserId { get; set; }

    public string? ConnectionId { get; set; }

    public string? UserName { get; set; }

    public DateTime LastInteractionTime { get; set; }

    public bool isTyping { get; set; } = false;

    public string Status { get; set; } = "offline";
}

