namespace SaladimQBot.Core;

public static class IEntityBuilderExtensions
{
    public static IMessageEntityBuilder WithAt(this IMessageEntityBuilder builder, IUser user)
        => builder.WithAt(user.UserId);

    public static IMessageEntityBuilder WithAt(this IMessageEntityBuilder builder, IUser user, string nameWhenUserNotExists)
        => builder.WithAt(user.UserId, nameWhenUserNotExists);

    public static IMessageEntityBuilder WithTextLine(this IMessageEntityBuilder builder, string text)
        => builder.WithText(text + "\r\n");

    public static IMessageEntity CreateTextOnlyEntity(this IClient client, string text)
    {
        var b = client.CreateMessageBuilder();
        b.WithText(text);
        return b.Build();
    }

    public static IMessageEntityBuilder CreateMessageBuilderWithText(this IClient client, string text)
        => new IMessageEntityBuilder(this).WithText(text);

    public static IForwardEntityBuilder AddMessage(this IForwardEntityBuilder forwardEntityBuilder, IMessageEntity messageEntity)
    {
        forwardEntityBuilder.AddMessage(forwardEntityBuilder.Client.Self, messageEntity, DateTime.Now);
        return forwardEntityBuilder;
    }
}
