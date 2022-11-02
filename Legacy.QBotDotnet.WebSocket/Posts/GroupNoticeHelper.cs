using System.Text.Json;

namespace QBotDotnet.GoCqHttp.Internal;

internal static class GroupNoticeHelper
{
    internal static void LoadFrom(JsonElement je, out long groupId, out long userId)
    {
        try
        {
            groupId = je.GetProperty("group_id").GetInt64();
            userId = je.GetProperty("user_id").GetInt64();
        }
        catch (KeyNotFoundException e)
        {
            throw new CQPostTypeInvalidLoadException(nameof(je), e, "key not found");
        }
    }
}