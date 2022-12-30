namespace SaladimQBot.Core;

public interface IMessageEntityBuilder : IClientEntity
{
    IMessageEntityBuilder WithText(string text);

    IMessageEntityBuilder WithImage(string uri);

    IMessageEntityBuilder WithAt(long userId);

    IMessageEntityBuilder WithAt(long userId, string nameWhenUserNotExists);

    IMessageEntityBuilder WithFace(int faceId);

    IMessageEntityBuilder WithReply(IMessage message);

    IMessageEntityBuilder WithUnImpl(string name, IDictionary<string, string> args);

    IMessageEntity Build(bool prepareRawString = false);
}
