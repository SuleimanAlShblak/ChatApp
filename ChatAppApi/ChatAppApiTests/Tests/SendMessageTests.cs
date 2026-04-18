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

    [TestCase("chat", "s1", "r1", "")]
    [TestCase("", "s1", "r1", "hello")]
    [TestCase("unknown", "s1", "r1", "hello")]
    [TestCase("chat", "", "r1", "hello")]
    [TestCase("chat", "s1", "", "hello")]
    public async Task SendMessage_InvalidMessage(string type, string senderId, string receiverId, string data)
    {
        var invalidMessage = new Message
        {
            Type = type,
            SenderId = senderId,
            ReceiverId = receiverId,
            Data = data
        };

        await AssertInvalidMessageHandled(invalidMessage);
    }

    [Test]
    public async Task SendMessage_InvalidMessage_NullMessage_DoesNotCrash_AndSendsError()
    {
        Message? invalidMessage = null;
        await AssertInvalidMessageHandled(invalidMessage);
    }

    private async Task AssertInvalidMessageHandled(Message? invalidMessage)
    {
        await ConnectUser("user_3432", "room1");
        MockClientProxy.Invocations.Clear();
        MockSingleClientProxy.Invocations.Clear();

        Assert.DoesNotThrowAsync(async () => await Hub.SendMessage(invalidMessage!));

        MockSingleClientProxy.Verify(
            c => c.SendCoreAsync("ReceiveError", It.IsAny<object?[]>(), default), Times.Once);

        MockClientProxy.Verify(
            c => c.SendCoreAsync("ReceiveSpecificMessage", It.IsAny<object?[]>(), default), Times.Never);
    }
}