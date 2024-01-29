namespace SaladimQBot.GoCqHttp.Apis;

public class GetLoginAction : CqApi
{
    public override string ApiName => "get_login_info";

    public override Type? ApiResultDataType => typeof(GetLoginActionResultData);
}
