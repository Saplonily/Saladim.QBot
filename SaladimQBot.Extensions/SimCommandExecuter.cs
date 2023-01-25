using System.Drawing;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;
using SaladimQBot.Core;

namespace SaladimQBot.Extensions;

public sealed partial class SimCommandExecuter
{
    public static readonly Type StringType = typeof(string);
    public static readonly Regex CommandParamRegex = new("(\"[^\"]*\")|[^\\s]+", RegexOptions.Compiled);
    public static char ArraySplitChar { get; set; } = ',';

    private readonly List<MethodBasedCommand> commands;
    private readonly Func<Type, object?> moduleInstanceFactory;

    public Dictionary<Type, Func<string, object>> CommandParamParsers { get; private set; }

    public string RootCommandPrefix { get; }

    public delegate void OnCommandExecutedHandler(MethodBasedCommand command, CommandContent content, object[] @params);
    public event OnCommandExecutedHandler? OnCommandExecuted;

    /// <summary>
    /// 创建一个服务实例
    /// </summary>
    /// <param name="client">client实例</param>
    /// <param name="rootCommandPrefix">指令的前缀, 不需要前缀时请显式指定为空字符串</param>
    public SimCommandExecuter(string rootCommandPrefix)
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

    public SimCommandExecuter(string rootCommandPrefix, Func<Type, object?> moduleInstanceFactory)
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
        var firstAt = msg.MessageEntity.FirstAtOrNull();
        //有@但是没有提及自己取消执行
        if (firstAt is not null)
            if (!msg.MessageEntity.MentionedSelf()) return;
        
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
                    CommandContent content = new(this, msg);
                    ExecuteInternal(cmd, argAsString, content);
                }
            }
        }
    }

    internal bool ExecuteInternal(MethodBasedCommand cmd, string[]? cmdParams, CommandContent content)
    {
        //算了你也别尝试看懂了, 我已经看不懂了qwq
        //2023-1-25因为支持更高级的一些特性的需要, 暂时加上一些注释

        //非params指令时如果方法的参数个数和传入的个数不相同, 执行失败
        var paramsLength = cmd.Parameters.Length;
        if (cmd.Parameters.Length != cmdParams?.Length && !cmd.IsParamsCommand)
            return false;

        //使用module实例化委托创建对应类型, 失败则退出执行
        if (moduleInstanceFactory?.Invoke(cmd.Method.DeclaringType!) is not CommandModule moduleIns) return false;
        moduleIns.Content = content;

        //执行module预检查, 失败则退出
        if (!moduleIns.PreCheck(moduleIns.Content))
            return false;

        //反射获取方法的所有参数
        var paramsTypes = from p in cmd.Parameters select p.ParameterType;
        //params指令特殊处理
        if (cmd.IsParamsCommand)
        {
            //消息串中没有参数, 使用空参数列表传入方法并执行
            if (cmdParams is null)
            {
                cmd.Method.Invoke(moduleIns, Array.Empty<object>());
                OnCommandExecuted?.Invoke(cmd, content, Array.Empty<object>());
                return true;
            }
            else
            {
                //消息串中有参数, 获取方法第一个参数(params参数)的类型
                Type baseType = cmd.Parameters[0].ParameterType.GetElementType()!;

                //获得一堆type组成的参数ienumerable, 然后新建个数组扔进去...
                var invokingParamsTypes = Enumerable.Repeat(baseType, cmdParams.Length);
                var array = Array.CreateInstance(baseType, cmdParams.Length);

                //解析参数
                object[]? paramObjects = ParseParams(cmdParams, invokingParamsTypes);
                //解析失败退出执行
                if (paramObjects is null) return false;
                //把object数组转成对应类型数组
                paramObjects.CopyTo(array, 0);
                //执行
                cmd.Method.Invoke(moduleIns, new object[] { array });
                OnCommandExecuted?.Invoke(cmd, content, new object[] { array });
                return true;
            }
        }

        if (cmdParams is null)
        {
            //空参数, 直接执行
            cmd.Method.Invoke(moduleIns, Array.Empty<object>());
            OnCommandExecuted?.Invoke(cmd, content, Array.Empty<object>());
            return true;
        }
        else if (paramsTypes.All(t => t == StringType))
        {
            //全是字符串参数, 从优化方面直接传入原始cmdParams
            cmd.Method.Invoke(moduleIns, cmdParams);
            OnCommandExecuted?.Invoke(cmd, content, cmdParams);
            return true;
        }
        else
        {
            //否则解析这些参数, 然后传入
            object[]? paramObjects = ParseParams(cmdParams, paramsTypes);
            if (paramObjects is null) return false;
            cmd.Method.Invoke(moduleIns, paramObjects);
            OnCommandExecuted?.Invoke(cmd, content, paramObjects);
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

public class MethodBasedCommand
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