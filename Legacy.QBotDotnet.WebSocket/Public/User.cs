using QBotDotnet.GoCqHttp;
using QBotDotnet.GoCqHttp.Internal;

namespace QBotDotnet.Public;

public class User : IEquatable<User?>
{
    internal Client client;
    protected bool isApiCalled = false;

    //下面这几个字段对应的属性第一次调用需要调用一次api
    protected Sex sex = Sex.Unknown;
    protected int loginDays = -1;
    protected int level = -1;
    protected Lazy<CQGetStrangerInfoAction.Result> apiResult;

    //从sender得来,可以直接使用
    public long Id { get; internal set; } = -1;
    public string Name { get; internal set; } = string.Empty;
    public int Age { get; internal set; } = -1;
    /// <summary>
    /// 用户的性别,注意此属性初次获取会阻塞
    /// </summary>
    public Sex Sex
    {
        get => GetLoadedThis().sex;
        set => sex = value;
    }
    /// <summary>
    /// 用户的连续登录天数,注意此属性初次获取会阻塞
    /// </summary>
    public int LoginDays
    {
        get => GetLoadedThis().loginDays;
        set => loginDays = value;
    }
    /// <summary>
    /// 用户的等级,注意此属性初次获取会阻塞
    /// </summary>
    public int Level
    {
        get => GetLoadedThis().level;
        set => level = value;
    }
    internal protected User(Client client)
    {
        this.client = client;

    }
    public override bool Equals(object? obj) => Equals(obj as User);
    public bool Equals(User? other) => other is not null && Id == other.Id;
    public override int GetHashCode() => Id.GetHashCode();
    public static bool operator ==(User? left, User? right) => Equals(left, right);
    public static bool operator !=(User? left, User? right) => !(left == right);

    internal static User GetFromAsync(CQMessageSender sender, Client client)
    {
        Logger.LogInfo((sender is CQGroupMessageSender).ToString());
        User u = new(client)
        {
            Sex = sender.Sex,
            Name = sender.NickName,
            Age = sender.Age,
            Id = sender.UserId
        };
        return u;
    }
    internal User GetLoadedThis()
    {
        if (!isApiCalled)
        {
            isApiCalled = true;
            return DoLoadFromApiCall().GetAwaiter().GetResult();
        }
        else
        {
            return this;
        }
    }
    internal async Task<User> DoLoadFromApiCall()
    {
        ApiActionResult result = await client.ApiCaller.SendAsync(new CQGetStrangerInfoAction(Id));
        Sex = QTypeHelper.ParseSex(result.Data.String("sex", string.Empty));
        LoginDays = result.Data.Int("login_days") ?? -1;
        Level = result.Data.Int("level") ?? -1;
        return this;
    }

    public async void SendMessageAsync(CQMessage message)
    {
        await client.ApiCaller.SendAsync(new CQSendPrivateMessageAction(Id, message));
    }

}