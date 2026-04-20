using ChatAppApi.Models;

namespace ChatAppApiTests.Tests;

public class ConnectTests : TestBase
{

    [Test]
    public async Task Connect_ExistingUser_UpdatesStatusToOnline()
    {
        var stored = await ConnectUser("Hekmat");
        stored.Status = Status.Offline;
        stored.ConnectionId = null;

        MockContext.Setup(c => c.ConnectionId).Returns("conn-2");
        await Hub.Connect(new User { UserName = "Hekmat", ChatRoom = "general" });

        Assert.That(stored.Status, Is.EqualTo(Status.Online));
        Assert.That(stored.ConnectionId, Is.EqualTo("conn-2"));
    }

}