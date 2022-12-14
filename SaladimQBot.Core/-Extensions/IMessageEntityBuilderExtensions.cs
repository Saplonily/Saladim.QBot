namespace SaladimQBot.Core;

public static class IMessageEntityBuilderExtensions
{
    public static IMessageEntityBuilder WithAt(this IMessageEntityBuilder builder, IUser user)
        => builder.WithAt(user.UserId);

    public static IMessageEntityBuilder WithAt(this IMessageEntityBuilder builder, IUser user, string nameWhenUserNotExists)
        => builder.WithAt(user.UserId, nameWhenUserNotExists);

    public static IMessageEntityBuilder WithTextLine(this IMessageEntityBuilder builder, string text)
        => builder.WithText(text + "\r\n");

}
