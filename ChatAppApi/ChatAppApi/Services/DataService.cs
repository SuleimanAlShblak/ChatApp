using System.Collections.Concurrent;

namespace ChatAppApi.Services;

public class DataService
{
    private readonly ConcurrentDictionary<string, Models.User> _users = new();
    public ConcurrentDictionary<string, Models.User> users => _users;

}

