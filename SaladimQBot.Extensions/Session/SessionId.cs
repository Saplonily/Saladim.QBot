namespace SaladimQBot.Extensions;

public struct SessionId
{
    public long GroupId;
    public long UserId;

    public SessionId(long userId = 0, long groupId = 0)
    {
        GroupId = groupId;
        UserId = userId;
    }

    public override bool Equals(object? obj)
    {
        return obj is SessionId id &&
               GroupId == id.GroupId &&
               UserId == id.UserId;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(GroupId, UserId);
    }

    public override string? ToString()
    {
        return $"(g:{GroupId}, u:{UserId})";
    }

    public static bool operator ==(SessionId left, SessionId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(SessionId left, SessionId right)
    {
        return !(left == right);
    }
}