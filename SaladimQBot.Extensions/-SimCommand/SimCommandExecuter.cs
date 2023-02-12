using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;
using SaladimQBot.Core;

namespace SaladimQBot.Extensions;

public sealed partial class SimCommandExecuter
{
    public static readonly Type StringType = typeof(string);
    public static readonly Regex CommandParamRegex = new(@"(""[^""]*"")|[\S\n]+", RegexOptions.Compiled | RegexOptions.Multiline);
    public static char ArraySplitChar { get; set; } = ',';

    private readonly List<MethodBasedCommand> commands;
    private readonly Func<Type, object?> moduleInstanceFactory;

    public Dictionary<Type, Func<string, object>> CommandParamParsers { get; private set; }

    public string[] RootCommandPrefixes { get; }

    public delegate void OnCommandExecutedHandler(MethodBasedCommand command, CommandContent content, object[] @params);
    public event OnCommandExecutedHandler? OnCommandExecuted;

    /// <summary>
    /// 创建一个Executer实例
    /// </summary>
    /// <param name="rootCommandPrefixes">指令的前缀, 不需要前缀时请显式指定为空字符串</param>
    public SimCommandExecuter(params string[] rootCommandPrefixes)
    {
        this.RootCommandPrefixes = rootCommandPrefixes;
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
            [typeof(Point)] = s => CommonTypeParsers.Point(s),
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
            [typeof(Point[])] = s => CommonTypeParsers.ArrayPacker(s, CommonTypeParsers.Point, ArraySplitChar),
            [typeof(ushort[])] = s => CommonTypeParsers.ArrayPacker(s, CommonTypeParsers.Ushort, ArraySplitChar),
            [typeof(double[])] = s => CommonTypeParsers.ArrayPacker(s, CommonTypeParsers.Double, ArraySplitChar),
            [typeof(Vector2[])] = s => CommonTypeParsers.ArrayPacker(s, CommonTypeParsers.Vector2, ArraySplitChar),
            [typeof(Vector3[])] = s => CommonTypeParsers.ArrayPacker(s, CommonTypeParsers.Vector3, ArraySplitChar),

            [typeof(string[])] = s => CommonTypeParsers.ArrayPacker(s, s => s, ArraySplitChar),
        };
        moduleInstanceFactory = Activator.CreateInstance;
    }

    public SimCommandExecuter(Func<Type, object?> moduleInstanceFactory, params string[] rootCommandPrefix)
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
            select new MethodBasedCommand(attr.Name, methodInfo, attr.MergeExcess);
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

        IEnumerable<(string TrimedText, string FirstMatchedPrefix)> matchResults =
            from node in allTextNodes
            let trimedText = node.Text.Trim()
            let firstMatchedPrefix = RootCommandPrefixes.Where(trimedText.StartsWith).FirstOrDefault()
            where firstMatchedPrefix is not null
            select (trimedText, firstMatchedPrefix);

        //遍历这些文本节点, 执行所有的指令
        foreach (var (trimedText, firstMatchedPrefix) in matchResults)
        {
            var cmdTextWithoutRootPrefix = trimedText.AsSpan(firstMatchedPrefix.Length);
            //现在这个字符串就形如 "add 1 2 3" 了
            //以空格切分, 但忽略引号内的空格
            var matches = CommandParamRegex.Matches(cmdTextWithoutRootPrefix.ToString());
            if (matches.Count == 0) continue;
            string[] argAsString = new string[matches.Count - 1];
            for (int i = 0; i < argAsString.Length; i++)
            {
                //我们只trim 非va和非mergeExcess部分的参数左右的引号
                argAsString[i] = matches[i + 1].Value;
            }
            //现在我们得到了它的参数的以字符串形式的参数数组
            //开始查找所有符合的指令
            foreach (var cmd in commands)
            {
                if (cmd.Name == matches[0].Value)
                {
                    //昵称相同, 现在检查参数数目是否相同(如果不是va指令或mergeExcess指令的话)
                    if (!cmd.IsVACommand && !cmd.IsMergeExcessCommand && cmd.Parameters.Length != matches.Count - 1)
                        continue;
                    string[] processedStrArgs = argAsString;
                    //昵称相同参数相同, 生成参数字符串数组传递给ExecuteInternal让其解析为对应值
                    //并调用最后的实体方法
                    if (cmd.IsMergeExcessCommand)
                    {
                        if (argAsString.Length < cmd.Parameters.Length)
                            continue;
                        processedStrArgs = (string[])argAsString.Clone();
                        for (int i = 0; i < cmd.Parameters.Length - 1; i++)
                        {
                            processedStrArgs[i] = processedStrArgs[i].Trim('"');
                        }
                    }
                    CommandContent content = new(this, msg);
                    ExecuteInternal(cmd, processedStrArgs, content);
                }
            }
        }
    }

    internal bool ExecuteInternal(MethodBasedCommand cmd, string[] cmdArguments, CommandContent content)
    {
        //算了你也别尝试看懂了, 我已经看不懂了qwq
        //2023-1-25因为支持更高级的一些特性的需要, 暂时加上一些注释

        //非params指令或者mergeExcess时如果方法的参数个数和传入的个数不相同, 执行失败
        var paramsLength = cmd.Parameters.Length;
        if (cmd.Parameters.Length != cmdArguments.Length && !cmd.IsVACommand && !cmd.IsMergeExcessCommand)
            return false;

        //使用module实例化委托创建对应类型, 失败则退出执行
        if (moduleInstanceFactory?.Invoke(cmd.Method.DeclaringType!) is not CommandModule moduleIns) return false;
        moduleIns.Content = content;

        //执行module预检查, 失败则退出
        if (!moduleIns.PreCheck(moduleIns.Content))
            return false;

        //反射获取方法的所有参数
        var paramsTypes = from p in cmd.Parameters select p.ParameterType;
        int methodParametersCount = cmd.Parameters.Length;
        //va指令特殊处理
        if (cmd.IsVACommand)
        {
            //获取方法params参数的类型
            Type baseType = cmd.VACommandParameterInfo!.ParameterType.GetElementType()!;
            if (cmdArguments.Length is 0 && cmd.Parameters.Length == 1)
            {
                //消息串中没有参数, 并且方法参数个数为1时使用空参数列表传入方法并执行
                var arr = Array.CreateInstance(baseType, 0);
                cmd.Method.Invoke(moduleIns, new object[] { arr });
                OnCommandExecuted?.Invoke(cmd, content, new object[] { arr });
                return true;
            }
            else
            {
                //消息串中有参数
                //可变参数之前的参数没填入, 退出
                if (cmdArguments.Length - (methodParametersCount - 1) < 0)
                    return false;

                //获得一堆type组成的参数ienumerable, 然后新建个数组作为目标参数扔进去
                //实参里的VA参数个数
                var countOfVAArgs = cmdArguments.Length - (methodParametersCount - 1);

                //指令实参va参数的types
                var cmdArgsVATypes = Enumerable.Repeat(baseType, countOfVAArgs);
                //建立最后传入invoker的va参数的数组
                var vaArgsArray = Array.CreateInstance(baseType, countOfVAArgs);

                //最终传入参数解析器的类型迭代器
                var cmdArgsTypes = cmdArgsVATypes
                    .Concat(
                        cmd.Parameters
                            .Take(methodParametersCount - 1)
                            .Select(p => p.ParameterType)
                    );

                //将类型和实参数组传入解析器
                object[]? parsedArgs = ParseCmdArgs(cmdArguments, cmdArgsTypes);

                //解析失败退出执行
                if (parsedArgs is null) return false;

                //把解析结果的va部分放入新数组中, 之后会放入最终invoker需要的参数数组内
                if (vaArgsArray.Length != 0)
                    parsedArgs.Skip(methodParametersCount - 1).ToArray().CopyTo(vaArgsArray, 0);

                //组装invoker需要的参数数组
                object[] resultArgs = new object[methodParametersCount];
                //把非va参数放入resultArgs内
                parsedArgs.Take(methodParametersCount - 1).ToArray().CopyTo(resultArgs, 0);
                //把va参数放到resultArgs最后一项
                resultArgs[methodParametersCount - 1] = vaArgsArray;

                //执行然后通知
                cmd.Method.Invoke(moduleIns, resultArgs);
                OnCommandExecuted?.Invoke(cmd, content, resultArgs);
                return true;
            }
        }

        //所有多余参数合并到最后一个参数里如果指定了IsMergeExcessCommand
        if (cmd.IsMergeExcessCommand)
        {
            if (methodParametersCount < cmdArguments.Length)
            {
                var extensionArgsString = string.Join(" ", cmdArguments.Skip(methodParametersCount - 1));
                var newArgs = new string[methodParametersCount];
                cmdArguments.AsSpan(0, methodParametersCount).CopyTo(newArgs.AsSpan());
                newArgs[newArgs.Length - 1] = extensionArgsString;
                cmdArguments = newArgs;
            }
        }

        if (!paramsTypes.Any())
        {
            //空参数, 直接执行
            cmd.Method.Invoke(moduleIns, Array.Empty<object>());
            OnCommandExecuted?.Invoke(cmd, content, Array.Empty<object>());
            return true;
        }
        else if (paramsTypes.All(t => t == StringType))
        {
            //全是字符串参数, 从优化方面直接传入原始cmdParams
            cmd.Method.Invoke(moduleIns, cmdArguments);
            OnCommandExecuted?.Invoke(cmd, content, cmdArguments);
            return true;
        }
        else
        {
            //否则解析这些参数, 然后传入
            object[]? paramObjects = ParseCmdArgs(cmdArguments, paramsTypes);
            if (paramObjects is null) return false;
            cmd.Method.Invoke(moduleIns, paramObjects);
            OnCommandExecuted?.Invoke(cmd, content, paramObjects);
            return true;
        }
    }

    internal object[]? ParseCmdArgs(string[] stringParams, IEnumerable<Type> paramsTypes)
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

[DebuggerDisplay("{Name}, {Parameters.Length}args, {(IsVACommand ? \"VA, \" : \"\"),nq}{(IsMergeExcessCommand ? \"Merge excess\" : \"\"),nq}")]
public class MethodBasedCommand
{
    /// <summary>
    /// 方法名称, 指令前缀后的部分
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 对应方法
    /// </summary>
    public MethodInfo Method { get; set; }

    /// <summary>
    /// 对应方法的参数数组
    /// </summary>
    public ParameterInfo[] Parameters { get; set; }

    /// <summary>
    /// VA指令的params参数的位置
    /// </summary>
    public ParameterInfo? VACommandParameterInfo { get; set; }

    /// <summary>
    /// 是否是VA指令(方法带可变参数, 在C#表现为params)
    /// </summary>
    public bool IsVACommand { get => VACommandParameterInfo is not null; }

    /// <summary>
    /// 是否是合并冗余指令(所有冗余参数都合并到最后一个参数进行解析)
    /// </summary>
    public bool IsMergeExcessCommand { get; set; }

    public MethodBasedCommand(string name, MethodInfo method, bool isMergeExcessCommand)
    {
        Name = name;
        Method = method;
        IsMergeExcessCommand = isMergeExcessCommand;
        Parameters = method.GetParameters();
        if (Parameters.Length != 0)
        {
            VACommandParameterInfo =
                Parameters.Where(p => p.GetCustomAttribute<ParamArrayAttribute>() is not null).FirstOrDefault();
        }
    }
}