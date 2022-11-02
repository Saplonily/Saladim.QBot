namespace QBotDotnet.Public;

public static class QTypeHelper
{
    public static Sex ParseSex(string str)
    {
        return str switch
        {
            "male" => Sex.Male,
            "female" => Sex.Female,
            _ => Sex.Unknown
        };
    }

    public static GroupRole ParseGroupRole(string str)
    {
        return str switch
        {
            "owner" => GroupRole.Owner,
            "admin" => GroupRole.Admin,
            "member" => GroupRole.Member,
            _ => GroupRole.Invalid
        };
    }
}