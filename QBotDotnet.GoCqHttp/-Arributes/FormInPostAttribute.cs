namespace QBotDotnet.GoCqHttp;

/// <summary>
/// <para>放置在枚举的值上，用来标明该枚举对应的上报昵称</para>
/// <para>仅支持枚举根类型为int的枚举，因为底层通用的实现是<see cref="int"/></para>
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public class FormInPostAttribute : Attribute
{
    public object Value { get; }
    public FormInPostAttribute(object value)
    {
        Value = value;
    }

}