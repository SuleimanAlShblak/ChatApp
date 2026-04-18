using ChatAppApi.Models;
using Moq;

namespace ChatAppApiTests.Tests;

public class SendMessageTests : TestBase
{
    [Test]
    public async Task SendMessage_ValidMessage_BroadcastsToGroup()
    {
        await ConnectUser("Sven", "room1");
        MockClientProxy.Invocations.Clear();

        var message = new Message { Type = "chat", SenderId = "s1", ReceiverId = "r1", Data = "Hello" };
        await Hub.SendMessage(message);

        MockClientProxy.Verify(
            c => c.SendCoreAsync("ReceiveSpecificMessage", It.IsAny<object?[]>(), default), Times.Once);
    }

    [Test]
    public async Task SendMessage_NullChatRoom_DefaultsToGeneral()
    {
        var stored = await ConnectUser("Robin");
        stored.ChatRoom = null;
        MockClientProxy.Invocations.Clear();

        var message = new Message { Type = "chat", SenderId = "s1", ReceiverId = "r1", Data = "Hi" };
        await Hub.SendMessage(message);

        Assert.That(stored.ChatRoom, Is.EqualTo("general"));
    }
}