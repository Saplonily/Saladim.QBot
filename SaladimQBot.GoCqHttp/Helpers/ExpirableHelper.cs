using SaladimQBot.Shared;

namespace SaladimQBot.GoCqHttp;

public static class ExpirableHelper
{
    /// <summary>
    /// <para>通过反射尝试获取该对象所有Expirable<>类型对象并进行一次取值尝试</para>
    /// </summary>
    public static void ActiveAllExpirable(object obj)
    {
        if (obj is null) throw new ArgumentNullException(nameof(obj));
        Type objType = obj.GetType();
        var expirableProps =
            from prop in objType.GetProperties()
            let propType = prop.PropertyType
            where propType.IsGenericType is true
            where propType.GetGenericTypeDefinition() == typeof(IExpirable<>)
            select prop;
        foreach (var p in expirableProps)
        {
            var v = p.GetValue(obj);
            var t = v?.GetType()?.GetProperty("Value");
            _ = t?.GetValue(v);
        }
    }
}
