using ChatAppApi.Models;

namespace ChatAppApiTests.Tests;

public class ConnectTests : TestBase
{
    [Test]
    public async Task Connect_EmptyChatRoom()
    {
        var user = new User { UserName = "Suleiman", ChatRoom = "" };
        await Hub.Connect(user);

        var stored = DataService.users.Values.First(u => u.UserName == "Suleiman");
        Assert.That(stored.ChatRoom, Is.EqualTo("general"));
    }

    [Test]
    public async Task Connect_ExistingUser_UpdatesStatusToOnline()
    {
        var stored = await ConnectUser("Hikmat");
        stored.Status = Status.Offline;
        stored.ConnectionId = null;

        MockContext.Setup(c => c.ConnectionId).Returns("conn-2");
        await Hub.Connect(new User { UserName = "Hikmat", ChatRoom = "general" });

        Assert.That(stored.Status, Is.EqualTo(Status.Online));
        Assert.That(stored.ConnectionId, Is.EqualTo("conn-2"));
    }

}