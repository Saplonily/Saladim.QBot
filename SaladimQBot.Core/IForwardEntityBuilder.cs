namespace SaladimQBot.Core;

public interface IForwardEntityBuilder : IClientEntity
{
    IForwardEntityBuilder AddMessage(IMessage msg);

    IForwardEntityBuilder AddMessage(string senderShowName, IUser sender, IMessageEntity entity, DateTime sendTime);

    IForwardEntityBuilder AddMessage(IUser sender, IMessageEntity entity, DateTime sendTime);

    IForwardEntity Build();
}
