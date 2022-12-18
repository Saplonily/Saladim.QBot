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
    public static char SplitChar { get; set; } = ',';

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

            [typeof(int[])] = s => CommonTypeParsers.ArrayPacker(s, CommonTypeParsers.Int, SplitChar),
            [typeof(uint[])] = s => CommonTypeParsers.ArrayPacker(s, CommonTypeParsers.Uint, SplitChar),
            [typeof(byte[])] = s => CommonTypeParsers.ArrayPacker(s, CommonTypeParsers.Byte, SplitChar),
            [typeof(char[])] = s => CommonTypeParsers.ArrayPacker(s, CommonTypeParsers.Char, SplitChar),
            [typeof(long[])] = s => CommonTypeParsers.ArrayPacker(s, CommonTypeParsers.Long, SplitChar),
            [typeof(sbyte[])] = s => CommonTypeParsers.ArrayPacker(s, CommonTypeParsers.Sbyte, SplitChar),
            [typeof(float[])] = s => CommonTypeParsers.ArrayPacker(s, CommonTypeParsers.Float, SplitChar),
            [typeof(short[])] = s => CommonTypeParsers.ArrayPacker(s, CommonTypeParsers.Short, SplitChar),
            [typeof(ulong[])] = s => CommonTypeParsers.ArrayPacker(s, CommonTypeParsers.Ulong, SplitChar),
            [typeof(Color[])] = s => CommonTypeParsers.ArrayPacker(s, CommonTypeParsers.Color, SplitChar),
            [typeof(ushort[])] = s => CommonTypeParsers.ArrayPacker(s, CommonTypeParsers.Ushort, SplitChar),
            [typeof(double[])] = s => CommonTypeParsers.ArrayPacker(s, CommonTypeParsers.Double, SplitChar),
            [typeof(Vector2[])] = s => CommonTypeParsers.ArrayPacker(s, CommonTypeParsers.Vector2, SplitChar),
            [typeof(Vector3[])] = s => CommonTypeParsers.ArrayPacker(s, CommonTypeParsers.Vector3, SplitChar),

            [typeof(string[])] = s => CommonTypeParsers.ArrayPacker(s, s => s, SplitChar),
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
            //trim它
            .Select(node => node.Text.Trim());
        //遍历这些文本节点
        foreach (var matchedNodeText in matchedNodeTexts)
        {
            var cmdTextWithoutRootPrefix = matchedNodeText.AsSpan(RootCommandPrefix.Length);
            //现在这个字符串就形如 "add 1 2 3" 了
            //切分, 但忽略引号内的空格, 同时去除引号
            var matches = CommandParamRegex.Matches(cmdTextWithoutRootPrefix.ToString());
            if (matches.Count == 0) continue;
            string[] argAsString = new string[matches.Count - 1];
            foreach (var cmd in commands)
            {
                if (cmd.Name == matches[0].Value)
                {
                    //昵称相同, 现在检查参数数目是否相同
                    if (cmd.Parameters.Length != matches.Count - 1)
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
        Debug.Assert(cmd.Parameters.Length == cmdParams?.Length);
        if (moduleInstanceFactory?.Invoke(cmd.Method.DeclaringType!) is not CommandModule moduleIns) return false;
        moduleIns.Content = new(this, msg);
        if (moduleIns.PreCheck(moduleIns.Content))
        {
            var paramsTypes =
                from p in cmd.Parameters
                select p.ParameterType;
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
                object[] paramObjects = new object[paramsLength];
                for (int i = 0; i < paramsLength; i++)
                {
                    Type paramsType = paramsTypes.ElementAt(i);
                    if (paramsType == StringType)
                    {
                        paramObjects[i] = cmdParams[i];
                        continue;
                    }
                    if (!CommandParamParsers.TryGetValue(paramsType, out var parser))
                        throw new KeyNotFoundException($"Not found parser for type `{paramsType}`");
                    try
                    {
                        var parsedValue = parser(cmdParams[i]);
                        if (parsedValue is null)
                            return false;
                        paramObjects[i] = parsedValue;
                    }
                    catch
                    {
                        return false;
                    }
                }
                cmd.Method.Invoke(moduleIns, paramObjects);
                return true;
            }
        }
        return false;
    }

    public async Task MatchAndExecuteAllAsync(IMessage msg)
        => await Task.Run(() => MatchAndExecuteAll(msg)).ConfigureAwait(false);
}

internal class MethodBasedCommand
{
    public string Name;
    public MethodInfo Method;
    public ParameterInfo[] Parameters;

    public MethodBasedCommand(string name, MethodInfo method)
    {
        Name = name;
        Method = method;
        Parameters = method.GetParameters();
    }
}