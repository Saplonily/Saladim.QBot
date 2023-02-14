using Microsoft.VisualStudio.TestTools.UnitTesting;
using SaladimQBot.Core;

namespace SaladimQBot.Tests;

[TestClass]
public class TypedUniNodeTests
{
    [TestMethod]
    public void AtNodeToTextTest()
    {
        AtUniNode atUniNode = new(new TestClient(), 233);
        Assert.AreEqual("<at,id=233>", atUniNode.ToFormattedText());
    }

    [TestMethod]
    public void TextToAtNodeTest()
    {
        AtUniNode atUniNode = new(GenericUniNode.Parse(new TestClient(), "<at:233>"));
        Assert.AreEqual("233", atUniNode.PrimaryValue);
        Assert.AreEqual(233, atUniNode.UserId);
    }
}


//呃, 有点地狱绘图说实话
#nullable disable
public class TestClient : IClient
{
    public IGroupMessage GetGroupMessageById(int messageId)
    {
        throw new NotImplementedException();
    }

    public IPrivateMessage GetPrivateMessageById(int messageId)
    {
        throw new NotImplementedException();
    }

    public IFriendMessage GetFriendMessageById(int messageId)
    {
        throw new NotImplementedException();
    }

    public IMessage GetMessageById(int messageId)
    {
        throw new NotImplementedException();
    }

    public Task<IPrivateMessage> SendPrivateMessageAsync(long userId, long? groupId, IMessageEntity messageEntity)
    {
        throw new NotImplementedException();
    }

    public Task<IPrivateMessage> SendPrivateMessageAsync(long userId, long? groupId, string rawString)
    {
        throw new NotImplementedException();
    }

    public Task<IFriendMessage> SendFriendMessageAsync(long friendUserId, IMessageEntity messageEntity)
    {
        throw new NotImplementedException();
    }

    public Task<IFriendMessage> SendFriendMessageAsync(long friendUserId, string rawString)
    {
        throw new NotImplementedException();
    }

    public Task<IGroupMessage> SendGroupMessageAsync(long groupId, IMessageEntity messageEntity)
    {
        throw new NotImplementedException();
    }

    public Task<IGroupMessage> SendGroupMessageAsync(long groupId, string rawString)
    {
        throw new NotImplementedException();
    }

    public Task<IGroupMessage> SendGroupMessageAsync(long groupId, IForwardEntity forwardEntity)
    {
        throw new NotImplementedException();
    }

    public Task<IPrivateMessage> SendPrivateMessageAsync(long userId, IForwardEntity forwardEntity)
    {
        throw new NotImplementedException();
    }

    public Task<IFriendMessage> SendFriendMessageAsync(long friendUserId, IForwardEntity forwardEntity)
    {
        throw new NotImplementedException();
    }

    public Task RecallMessageAsync(int messageId)
    {
        throw new NotImplementedException();
    }

    public Task BanGroupUserAsync(long groupId, long userId, TimeSpan time)
    {
        throw new NotImplementedException();
    }

    public Task LiftBanGroupUserAsync(long groupId, long userId)
    {
        throw new NotImplementedException();
    }

    public Task SetGroupNameAsync(long groupId, string newGroupName)
    {
        throw new NotImplementedException();
    }

    public Task SetGroupCardAsync(long groupId, long userId, string newCard)
    {
        throw new NotImplementedException();
    }

    public IGroupUser GetGroupUser(long groupId, long userId)
    {
        throw new NotImplementedException();
    }

    public IUser GetUser(long userId)
    {
        return new TestUser(userId);
    }

    public class TestUser : IUser
    {
        public long UserId { get; }
        public string Nickname { get; }
        public Sex Sex { get; }
        public int Age { get; }
        public string Qid { get; }
        public int Level { get; }
        public int LoginDays { get; }
        public Uri AvatarUrl { get; }

        public TestUser(long uid)
        {
            UserId = uid;
        }

        public Task<IMessage> SendMessageAsync(IMessageEntity messageEntity)
        {
            throw new NotImplementedException();
        }

        public Task<IMessage> SendMessageAsync(string rawString)
        {
            throw new NotImplementedException();
        }

        public Task<IMessage> SendMessageAsync(IForwardEntity forwardEntity)
        {
            throw new NotImplementedException();
        }

        public IClient Client { get; }
    }

    public IGroup GetGroup(long groupId)
    {
        throw new NotImplementedException();
    }

    public IJoinedGroup GetJoinedGroup(long groupId)
    {
        throw new NotImplementedException();
    }

    public IFriendUser GetFriendUser(long friendUserId)
    {
        throw new NotImplementedException();
    }

    public event IClient.OnClientEventOccuredHandler<IClientEvent> OnClientEventOccurred;
    public event Action<Exception> OnStoppedUnexpectedly;

    public IUser Self { get; }

    public IMessageEntityBuilder CreateMessageBuilder()
    {
        throw new NotImplementedException();
    }

    public IForwardEntityBuilder CreateForwardBuilder()
    {
        throw new NotImplementedException();
    }

    public Task StartAsync()
    {
        throw new NotImplementedException();
    }

    public Task StopAsync()
    {
        throw new NotImplementedException();
    }

    public Task<IPrivateMessage> ReplyMessageAsync(IPrivateMessage privateMessage, IMessageEntity msg)
    {
        throw new NotImplementedException();
    }

    public Task<IPrivateMessage> ReplyMessageAsync(IPrivateMessage privateMessage, string formattedString)
    {
        throw new NotImplementedException();
    }

    public Task<IGroupMessage> ReplyMessageAsync(IGroupMessage groupMessage, IMessageEntity msg)
    {
        throw new NotImplementedException();
    }

    public Task<IGroupMessage> ReplyMessageAsync(IGroupMessage groupMessage, string formattedString)
    {
        throw new NotImplementedException();
    }
}
#nullable restore