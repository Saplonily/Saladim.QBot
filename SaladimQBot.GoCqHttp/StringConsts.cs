namespace SaladimQBot.GoCqHttp;

public static class StringConsts
{
    public const string PostTypeProperty = "post_type";
    public const string MessagePostSubTypeProperty = "sub_type";
    public const string NoticeTypeProperty = "notice_type";
    public const string NotifySubTypeProperty = "sub_type";
    public const string RequestTypeProperty = "request_type";

    public const string ActionProperty = "action";
    public const string ApiEchoProperty = "echo";
    public const string ParamsProperty = "params";
    public const string DataProperty = "data";

    //用来区分是 CqMessageSender 还是 CqGroupMessageSender的关键Property名
    //可更改为下列其中任意一项:
    //card
    //area
    //level
    //role
    //title
    //参考 https://docs.go-cqhttp.org/reference/data_struct.html#post-message-messagesender
    //2022-10-20 16:19:09 Saplonily
    public const string GroupSenderIdentifier = "role";

    public const string GroupTempMessageSenderIdentifier = "group_id";

    public const string CqCodeTypeProperty = "type";
    public const string CqCodeParamsProperty = "data";

    public const string CqPostEndpoint = "event";
    public const string CqApiEndpoint = "api";
}