using ChatAppApi.Models;
using Moq;

namespace ChatAppApiTests;

public class DisconnectTests : TestBase
{
    [Test]
    public async Task OnDisconnected_SetsUserOffline()
    {
        var stored = await ConnectUser("CR7");

        await Hub.OnDisconnectedAsync(null);

        Assert.That(stored.Status, Is.EqualTo(Status.Offline));
        Assert.That(stored.ConnectionId, Is.Null);
    }

    [Test]
    public async Task OnDisconnected_BroadcastsUpdatedUserList()
    {
        await ConnectUser("Hasan");
        MockClientProxy.Invocations.Clear();

        await Hub.OnDisconnectedAsync(null);

        MockClientProxy.Verify(
            c => c.SendCoreAsync("UserListUpdated", It.IsAny<object?[]>(), default), Times.Once);
    }

    [Test]
    public async Task OnDisconnected_UnknownConnection_DoesNotThrow()
    {
        MockContext.Setup(c => c.ConnectionId).Returns("unknown");
        Assert.DoesNotThrowAsync(() => Hub.OnDisconnectedAsync(null));
    }
}