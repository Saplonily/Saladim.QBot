namespace SaladimQBot.GoCqHttp.Apis;

public class GetForwardMessageActionResultData : CqApiCallResultData
{
    [Name("messages")]
    public Message[] Messages { get; set; }

    public GetForwardMessageActionResultData(Message[] messages)
    {
        this.Messages = messages;
    }

    public class Message
    {
        public Message(CqMessageChainModel content, long time, Sender sender)
        {
            this.Content = content;
            this.Time = time;
            this.Sender = sender;
        }

        [Name("content")]
        public CqMessageChainModel Content { get; set; }

        [Name("time")]
        public long Time { get; set; }

        [Name("sender")]
        public Sender Sender { get; set; }
    }

    public class Sender
    {
        public Sender(string nickname, long userId)
        {
            this.Nickname = nickname;
            this.UserId = userId;
        }

        [Name("nickname")]
        public string Nickname { get; set; }

        [Name("user_id")]
        public long UserId { get; set; }
    }
}
