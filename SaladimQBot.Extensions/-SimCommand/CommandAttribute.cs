namespace SaladimQBot.Extensions;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class CommandAttribute : Attribute
{
    public string Name { get; protected set; }

    public bool IsSingleParam { get; protected set; }

    /// <summary>
    /// 声明该方法为模块的一个实体指令
    /// </summary>
    /// <param name="name">指令昵称</param>
    /// <param name="isSingleParam">是否单参数且将所有后续参数作为一项参数解析入指令</param>
    public CommandAttribute(string name, bool isSingleParam = false)
    {
        Name = name;
        IsSingleParam = isSingleParam;
    }
}
