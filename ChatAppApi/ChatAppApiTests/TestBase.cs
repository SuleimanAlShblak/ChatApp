using ChatAppApi.DataService;
using ChatAppApi.Hubs;
using ChatAppApi.Models;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace ChatAppApiTests;

public abstract class TestBase
{
    protected DataService DataService = null!;
    protected ChatHub Hub = null!;
    protected Mock<IHubCallerClients> MockClients = null!;
    protected Mock<IGroupManager> MockGroups = null!;
    protected Mock<HubCallerContext> MockContext = null!;
    protected Mock<IClientProxy> MockClientProxy = null!;
    protected Mock<ISingleClientProxy> MockSingleClientProxy = null!;

    [SetUp]
    public void BaseSetUp()
    {
        DataService = new DataService();
        Hub = new ChatHub(DataService);

        MockClients = new Mock<IHubCallerClients>();
        MockGroups = new Mock<IGroupManager>();
        MockContext = new Mock<HubCallerContext>();
        MockClientProxy = new Mock<IClientProxy>();
        MockSingleClientProxy = new Mock<ISingleClientProxy>();

        MockContext.Setup(c => c.ConnectionId).Returns("conn-1");
        MockClients.Setup(c => c.All).Returns(MockClientProxy.Object);
        MockClients.Setup(c => c.Caller).Returns(MockSingleClientProxy.Object);
        MockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(MockClientProxy.Object);
        MockClients.Setup(c => c.Client(It.IsAny<string>())).Returns(MockSingleClientProxy.Object);

        Hub.Clients = MockClients.Object;
        Hub.Groups = MockGroups.Object;
        Hub.Context = MockContext.Object;
    }
    [TearDown]
    public void BaseTearDown()
    {
        Hub?.Dispose();
    }

    /// <summary>
    /// Connects a user to the chat hub.
    /// </summary>
    protected async Task<User> ConnectUser(string userName, string chatRoom = "general")
    {
        var user = new User { UserName = userName, ChatRoom = chatRoom };
        await Hub.Connect(user);
        return DataService.users.Values.First(u => u.UserName == userName);
    }
}