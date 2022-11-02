using System.Text.Json.Serialization;
using QBotDotnet.Public;

namespace QBotDotnet.GoCqHttp.Internal;

public class CQGetStrangerInfoAction : CQApi
{
    //字段名 数据类型    说明
    //user_id int64 QQ 号
    //nickname    string 昵称
    //sex string 性别, male 或 female 或 unknown
    //age int32   年龄
    //qid string qid ID身份卡
    //level   int32 等级
    //login_days int32   等级
    public class Result
    {
        public long Id { get; init; }
        public string NickName { get; init; }
        public Sex Sex { get; init; }
        public int Age { get; init; }
        public string Qid { get; init; }
        public int Level { get; init; }
        public int LoginDays { get; init; }
        public Result(long id, string nickName, Sex sex, int age, string qid, int level, int loginDays)
        {
            Id = id;
            NickName = nickName;
            Sex = sex;
            Age = age;
            Qid = qid;
            Level = level;
            LoginDays = loginDays;
        }
    }
    public override string ApiName { get => "get_stranger_info"; }

    [JsonPropertyName("user_id")]
    public long UserId { get; set; }
    public CQGetStrangerInfoAction(long userId)
    {
        UserId = userId;
    }
}