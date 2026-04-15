using System.Collections.Concurrent;

namespace ChatAppApi.DataService;

public class DataService
{
    private readonly ConcurrentDictionary<string, Models.User> _connections = new();
    public ConcurrentDictionary<string, Models.User> connections => _connections;

    private readonly ConcurrentDictionary<string, Models.Room> _rooms = new();
    public ConcurrentDictionary<string, Models.Room> rooms => _rooms;

    public DataService()
    {
        // Add default room
        var generalRoom = new Models.Room
        {
            Id = "general",
            Name = "General",
            Description = "General chat room"
        };
        _rooms["general"] = generalRoom;
    }
}

