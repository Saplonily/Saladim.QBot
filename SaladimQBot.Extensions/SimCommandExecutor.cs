using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;
using SaladimQBot.Core;

namespace SaladimQBot.Extensions;

public sealed partial class SimCommandExecutor
{
    public static readonly Type StringType = typeof(string);
    public static readonly Regex CommandParamRegex = new("(\"[^\"]*\")|[^\\s]+", RegexOptions.Compiled);
    public static char ArraySplitChar { get; set; } = ',';

    private readonly List<MethodBasedCommand> commands;
    private readonly Func<Type, object?> moduleInstanceFactory;

    public Dictionary<Type, Func<string, object>> CommandParamParsers { get; private set; }

    public string RootCommandPrefix { get; }

    /// <summary>
    /// 创建一个服务实例
    /// </summary>
    /// <param name="client">client实例</param>
    /// <param name="rootCommandPrefix">指令的前缀, 不需要前缀时请显式指定为空字符串</param>
    public SimCommandExecutor(string rootCommandPrefix)
    {
        this.RootCommandPrefix = rootCommandPrefix;
        commands = new();
        CommandParamParsers = new()
        {
            [typeof(int)] = s => CommonTypeParsers.Int(s),
            [typeof(uint)] = s => CommonTypeParsers.Uint(s),
            [typeof(byte)] = s => CommonTypeParsers.Byte(s),
            [typeof(char)] = s => CommonTypeParsers.Char(s),
            [typeof(long)] = s => CommonTypeParsers.Long(s),
            [typeof(short)] = s => CommonTypeParsers.Short(s),
            [typeof(ulong)] = s => CommonTypeParsers.Ulong(s),
            [typeof(float)] = s => CommonTypeParsers.Float(s),
            [typeof(Color)] = s => CommonTypeParsers.Color(s),
            [typeof(sbyte)] = s => CommonTypeParsers.Sbyte(s),
            [typeof(ushort)] = s => CommonTypeParsers.Ushort(s),
            [typeof(double)] = s => CommonTypeParsers.Double(s),
            [typeof(Vector2)] = s => CommonTypeParsers.Vector2(s),
            [typeof(Vector3)] = s => CommonTypeParsers.Vector3(s),

            [typeof(int[])] = s => CommonTypeParsers.ArrayPacker(s, CommonTypeParsers.Int, ArraySplitChar),
            [typeof(uint[])] = s => CommonTypeParsers.ArrayPacker(s, CommonTypeParsers.Uint, ArraySplitChar),
            [typeof(byte[])] = s => CommonTypeParsers.ArrayPacker(s, CommonTypeParsers.Byte, ArraySplitChar),
            [typeof(char[])] = s => CommonTypeParsers.ArrayPacker(s, CommonTypeParsers.Char, ArraySplitChar),
            [typeof(long[])] = s => CommonTypeParsers.ArrayPacker(s, CommonTypeParsers.Long, ArraySplitChar),
            [typeof(sbyte[])] = s => CommonTypeParsers.ArrayPacker(s, CommonTypeParsers.Sbyte, ArraySplitChar),
            [typeof(float[])] = s => CommonTypeParsers.ArrayPacker(s, CommonTypeParsers.Float, ArraySplitChar),
            [typeof(short[])] = s => CommonTypeParsers.ArrayPacker(s, CommonTypeParsers.Short, ArraySplitChar),
            [typeof(ulong[])] = s => CommonTypeParsers.ArrayPacker(s, CommonTypeParsers.Ulong, ArraySplitChar),
            [typeof(Color[])] = s => CommonTypeParsers.ArrayPacker(s, CommonTypeParsers.Color, ArraySplitChar),
            [typeof(ushort[])] = s => CommonTypeParsers.ArrayPacker(s, CommonTypeParsers.Ushort, ArraySplitChar),
            [typeof(double[])] = s => CommonTypeParsers.ArrayPacker(s, CommonTypeParsers.Double, ArraySplitChar),
            [typeof(Vector2[])] = s => CommonTypeParsers.ArrayPacker(s, CommonTypeParsers.Vector2, ArraySplitChar),
            [typeof(Vector3[])] = s => CommonTypeParsers.ArrayPacker(s, CommonTypeParsers.Vector3, ArraySplitChar),

            [typeof(string[])] = s => CommonTypeParsers.ArrayPacker(s, s => s, ArraySplitChar),
        };
        moduleInstanceFactory = Activator.CreateInstance;
    }

    public SimCommandExecutor(string rootCommandPrefix, Func<Type, object?> moduleInstanceFactory)
        : this(rootCommandPrefix)
    {
        this.moduleInstanceFactory = moduleInstanceFactory;
    }

    public void AddModule(Type moduleClassType)
    {
        var cmdToAdd =
            from methodInfo in moduleClassType.GetMethods()
            let attr = methodInfo.GetCustomAttribute<CommandAttribute>()
            where attr is not null
            select new MethodBasedCommand(attr.Name, methodInfo);
        commands.AddRange(cmdToAdd);
    }

    /// <summary>
    /// 尝试匹配并且执行这个消息中的所有指令
    /// </summary>
    /// <param name="msg">消息</param>
    public void MatchAndExecuteAll(IMessage msg)
    {
        var allTextNodes = msg.MessageEntity.AllText();
        //寻找所有以根指令前缀开头的文本节点
        var matchedNodeTexts = allTextNodes
            .Where(node =>
            {
                var text = node.Text;
                var trimedText = text.AsSpan().Trim();
                if (trimedText.StartsWith(RootCommandPrefix.AsSpan()))
                    return true;
                return false;
            })
            //trim掉前后的空格
            .Select(node => node.Text.Trim());

        //遍历这些文本节点, 执行所有的指令
        foreach (var matchedNodeText in matchedNodeTexts)
        {
            var cmdTextWithoutRootPrefix = matchedNodeText.AsSpan(RootCommandPrefix.Length);
            //现在这个字符串就形如 "add 1 2 3" 了
            //以空格切分, 但忽略引号内的空格, 同时去除引号
            var matches = CommandParamRegex.Matches(cmdTextWithoutRootPrefix.ToString());
            if (matches.Count == 0) continue;
            string[] argAsString = new string[matches.Count - 1];
            //现在我们得到了它的参数的以字符串形式的参数数组
            //开始查找所有符合的指令
            foreach (var cmd in commands)
            {
                if (cmd.Name == matches[0].Value)
                {
                    //昵称相同, 现在检查参数数目是否相同(如果不是params指令的话)
                    if (cmd.Parameters.Length != matches.Count - 1 && !cmd.IsParamsCommand)
                        continue;
                    //昵称相同参数相同, 生成参数字符串数组传递给ExecuteInternal让其解析为对应值
                    //并调用最后的实体方法
                    for (int i = 0; i < argAsString.Length; i++)
                    {
                        argAsString[i] = matches[i + 1].Value.Trim('"');
                    }
                    ExecuteInternal(msg, cmd, argAsString);
                }
            }
        }
    }

    internal bool ExecuteInternal(IMessage msg, MethodBasedCommand cmd, string[]? cmdParams)
    {
        var paramsLength = cmd.Parameters.Length;
        if (cmd.Parameters.Length != cmdParams?.Length && !cmd.IsParamsCommand)
            return false;

        if (moduleInstanceFactory?.Invoke(cmd.Method.DeclaringType!) is not CommandModule moduleIns) return false;
        moduleIns.Content = new(this, msg);
        if (!moduleIns.PreCheck(moduleIns.Content))
            return false;

        var paramsTypes = from p in cmd.Parameters select p.ParameterType;
        if (cmd.IsParamsCommand)
        {
            if (cmdParams is null)
            {
                cmd.Method.Invoke(moduleIns, new object[] { });
                return true;
            }
            else
            {
                Type baseType = cmd.Parameters[0].ParameterType.GetElementType();
                var invokingParamsTypes = Enumerable.Repeat(baseType, cmdParams.Length);
                var array = Array.CreateInstance(baseType, cmdParams.Length);
                object[]? paramObjects = ParseParams(cmdParams, invokingParamsTypes);
                if (paramObjects is null) return false;
                paramObjects.CopyTo(array, 0);
                cmd.Method.Invoke(moduleIns, new object[] { array });
                return true;
            }
        }

        if (cmdParams is null)
        {
            cmd.Method.Invoke(moduleIns, null);
            return true;
        }
        else if (paramsTypes.All(t => t == StringType))
        {
            cmd.Method.Invoke(moduleIns, cmdParams);
            return true;
        }
        else
        {
            object[]? paramObjects = ParseParams(cmdParams, paramsTypes);
            if (paramObjects is null) return false;
            cmd.Method.Invoke(moduleIns, paramObjects);
            return true;
        }
    }

    internal object[]? ParseParams(string[] stringParams, IEnumerable<Type> paramsTypes)
    {
        var paramsLength = stringParams.Length;
        object[] paramObjects = new object[paramsLength];
        for (int i = 0; i < paramsLength; i++)
        {
            Type paramsType = paramsTypes.ElementAt(i);
            if (paramsType == StringType)
            {
                paramObjects[i] = stringParams[i];
                continue;
            }
            if (!CommandParamParsers.TryGetValue(paramsType, out var parser))
                throw new KeyNotFoundException($"Not found parser for type `{paramsType}`");
            try
            {
                var parsedValue = parser(stringParams[i]);
                if (parsedValue is null)
                    return null;
                paramObjects[i] = parsedValue;
            }
            catch
            {
                return null;
            }
        }
        return paramObjects;
    }

    public async Task MatchAndExecuteAllAsync(IMessage msg)
        => await Task.Run(() => MatchAndExecuteAll(msg)).ConfigureAwait(false);
}

internal class MethodBasedCommand
{
    public string Name;
    public MethodInfo Method;
    public ParameterInfo[] Parameters;
    public bool IsParamsCommand = false;

    public MethodBasedCommand(string name, MethodInfo method)
    {
        Name = name;
        Method = method;
        Parameters = method.GetParameters();
        if (Parameters.Length != 0)
        {
            IsParamsCommand = Parameters[0].GetCustomAttributes<ParamArrayAttribute>().Any();
        }
    }
}