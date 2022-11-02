using System.Text.Json;
using QBotDotnet.Public;

namespace QBotDotnet.GoCqHttp.Internal;

public class CQPrivateMessagePost : CQMessagePost
{
    public PrivateMessageTempSource TempSource { get; set; }
    internal static CQPrivateMessagePost GetFrom(JsonElement je)
    {
        var post = new CQPrivateMessagePost();
        post.LoadFrom(je);
        return post;
    }
    internal override void LoadFrom(JsonElement je)
    {
        base.LoadFrom(je);
        try
        {
            bool hasTempSource = je.TryGetProperty("temp_source", out JsonElement jeTempSource);
            if (hasTempSource)
            {
                TempSource = jeTempSource.GetInt32() switch
                {
                    0 => PrivateMessageTempSource.Group,
                    1 => PrivateMessageTempSource.QQConsultation,
                    2 => PrivateMessageTempSource.Search,
                    3 => PrivateMessageTempSource.QQFilm,
                    4 => PrivateMessageTempSource.HotChat,
                    6 => PrivateMessageTempSource.ValidationMessage,
                    7 => PrivateMessageTempSource.MultiPersonChat,
                    8 => PrivateMessageTempSource.Date,
                    9 => PrivateMessageTempSource.AddressList,
                    _ => PrivateMessageTempSource.Invalid
                };
            }
            else
            {
                TempSource = PrivateMessageTempSource.None;
            }
        }
        catch (KeyNotFoundException e)
        {
            throw new CQPostTypeInvalidLoadException(nameof(je), e);
        }
    }


}
