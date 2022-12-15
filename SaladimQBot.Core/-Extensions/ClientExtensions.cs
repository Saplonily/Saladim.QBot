namespace SaladimQBot.Core;

public static class ClientExtensions
{
    /// <summary>
    /// 使用client获取一个群用户实体
    /// </summary>
    /// <param name="client">client实例</param>
    /// <param name="group">群</param>
    /// <param name="userId">用户Id</param>
    /// <returns>群用户实体</returns>
    public static IGroupUser GetGroupUser(this IClient client, IJoinedGroup group, long userId)
        => client.GetGroupUser(group.GroupId, userId);

    /// <summary>
    /// 使用client获取一个群用户实体
    /// </summary>
    /// <param name="client">client实例</param>
    /// <param name="groupId">群Id</param>
    /// <param name="user">用户</param>
    /// <returns>群用户实体</returns>
    public static IGroupUser GetGroupUser(this IClient client, long groupId, IUser user)
        => client.GetGroupUser(groupId, user.UserId);

    /// <summary>
    /// 使用client获取一个群用户实体
    /// </summary>
    /// <param name="client">client实例</param>
    /// <param name="group">群</param>
    /// <param name="user">用户</param>
    /// <returns>群用户实体</returns>
    public static IGroupUser GetGroupUser(this IClient client, IJoinedGroup group, IUser user)
        => client.GetGroupUser(group.GroupId, user.UserId);

    public static IMessageEntityBuilder CreateMessageBuilder(this IClient client, IMessage messageToReply)
    {
        var builder = client.CreateMessageBuilder();
        builder.WithReply(messageToReply);
        return builder;
    }
}
