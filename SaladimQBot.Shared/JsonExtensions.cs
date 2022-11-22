using System.Text.Json;

namespace SaladimQBot.Shared;

public static class JsonExtensions
{
    /// <summary>
    /// 查询指定JsonElement是否包含指定string的键
    /// </summary>
    /// <param name="element">元素</param>
    /// <param name="propertyName">键名</param>
    /// <returns>是否存在</returns>
    public static bool ExistsProperty(this JsonElement element, in string propertyName)
        => element.TryGetProperty(propertyName, out _);
}