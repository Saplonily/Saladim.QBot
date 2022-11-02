using System.Reflection;
using QBotDotnet.SharedImplement;

using EnumRawInt = System.Int32;

namespace QBotDotnet.GoCqHttp;

public static class EnumAttributeCacher
{
    //Type对一个字典，内部字典是object(json解析的到的值,为string/int,int为enum内部值)
    private static readonly Dictionary<Type, BiDictionary<object, int>> cache = new();

    public static EnumRawInt GetEnumFromAttr(Type type, object valueToGet)
    {
        EnsureTypeIsEnum(type);
        return InternalGetEnumFromAttr(type, valueToGet);
    }
    public static object GetAttrFromEnum(Type type, EnumRawInt rawEnumValue)
    {
        EnsureTypeIsEnum(type);
        return InternalGetAttrFromEnum(type, rawEnumValue);
    }

    public static TEnum GetEnumFromAttr<TEnum>(object valueToGet) where TEnum : Enum
        => EnumHelper.ToObject<TEnum>(InternalGetEnumFromAttr(typeof(TEnum), valueToGet));

    public static object GetAttrFromEnum<TEnum>(EnumRawInt rawEnumValue) where TEnum : Enum
        => InternalGetAttrFromEnum(typeof(TEnum), rawEnumValue);


    private static EnumRawInt InternalGetEnumFromAttr(Type type, object valueToGet)
    {
        CheckAndLoadType(type);
        try
        {
            return cache[type][valueToGet];
        }
        catch (KeyNotFoundException)
        {
            throw new KeyNotFoundException($"Not found {valueToGet}, type is {type}");
        }
    }

    private static object InternalGetAttrFromEnum(Type type, EnumRawInt rawEnumValue)
    {
        CheckAndLoadType(type);
        try
        {
            return cache[type].Reverse[rawEnumValue];
        }
        catch (KeyNotFoundException)
        {
            throw new KeyNotFoundException($"Not found raw value {rawEnumValue}, type is {type}");
        }
    }
    private static void CheckAndLoadType(Type type)
    {
        if (!cache.ContainsKey(type))
        {
            //为了线程安全操作
            lock (cache)
            {
                if (!cache.ContainsKey(type))
                {
                    BiDictionary<object, int> valueDic = new();
                    var fields = type.GetFields();
                    foreach (var field in fields)
                    {
                        var attr = field.GetCustomAttribute<NameIn>();
                        if (attr is null)
                            continue;
                        valueDic.Add(attr.Value, (int)field.GetRawConstantValue()!);

                    }
                    cache.Add(type, valueDic);
                }
            }
        }
    }
    private static void EnsureTypeIsEnum(Type type)
    {
        if (!typeof(Enum).IsAssignableFrom(type))
            throw new ArgumentException($"{nameof(type)} must be an enum type.", nameof(type));
    }
}