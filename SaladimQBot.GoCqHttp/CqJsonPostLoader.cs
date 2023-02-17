using System.Text.Json;
using SaladimQBot.Shared;

namespace SaladimQBot.GoCqHttp;

public sealed class CqJsonPostLoader
{
    public JsonElement Element { get; }

    public CqJsonPostLoader(JsonElement element)
    {
        Element = element;
    }

    public CqJsonPostLoader Sub(string keyName) => new(Element.GetProperty(keyName));

    public Int32 Int32(string keyName) => Element.GetProperty(keyName).GetInt32();

    public Int64 Int64(string keyName) => Element.GetProperty(keyName).GetInt64();

    public string? String(string keyName) => Element.GetProperty(keyName).GetString();

    public int EnumFromString(Type enumType, string keyName)
    {
        string rawStr;
        try
        {
            rawStr = Element.GetProperty(keyName).GetString()!;
        }
        catch (KeyNotFoundException)
        {
            throw new ArgumentException($"not found enum for string \"{keyName}\"");
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("not a string token type");
        }
        return EnumAttributeCacher.GetEnumFromAttr(enumType, rawStr);
    }

    public int EnumFromInt32(Type enumType, string keyName)
    {
        int rawInt;
        try
        {
            rawInt = Element.GetProperty(keyName).GetInt32();
        }
        catch (KeyNotFoundException ke)
        {
            throw new ArgumentException(keyName, ke);
        }
        catch (InvalidOperationException ioe)
        {
            throw new InvalidOperationException("not a number token type or over range", ioe);
        }
        return EnumAttributeCacher.GetEnumFromAttr(enumType, rawInt);
    }

    public TEnum EnumFromString<TEnum>(string keyName) where TEnum : Enum
        => EnumFromString(typeof(TEnum), keyName).Cast<TEnum>();

    public TEnum EnumFromInt32<TEnum>(string keyName) where TEnum : Enum
        => EnumFromInt32(typeof(TEnum), keyName).Cast<TEnum>();
}