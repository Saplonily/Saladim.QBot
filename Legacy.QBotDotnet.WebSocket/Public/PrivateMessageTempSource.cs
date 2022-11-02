namespace QBotDotnet.Public;

public enum PrivateMessageTempSource
{
    Invalid,
    None,                    //不是temp
    Group,                   //群聊
    QQConsultation,          //QQ咨询
    Search,                  //查找
    QQFilm,                  //QQ电影
    HotChat,                 //热聊
    ValidationMessage,       //验证消息
    MultiPersonChat,         //多人聊天
    Date,                    //约会
    AddressList              //通讯录
}