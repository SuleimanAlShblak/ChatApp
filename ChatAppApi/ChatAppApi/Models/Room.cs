namespace ChatAppApi.Models;

public class Room
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public List<string>? Members { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}