using QBotDotnet.Core;

namespace QBotDotnet.GoCqHttp.Apis;

public class GetGroupInfoAction : CqCacheableApi
{
    public override string ApiName { get => "get_group_info"; }

    public override Type ApiResultDataType { get => typeof(GetGroupInfoActionResultData); }

    [Name("group_id")]
    public long GroupId { get; set; }
}