using System.Reflection;
using SaladimQBot.Core;

namespace SaladimQBot.SimCommand;

public sealed class SimCommandService
{
    private readonly List<MethodBasedCommand> commands;

    public IClient Client { get; }

    public string RootCommandPrefix { get; }

    /// <summary>
    /// 创建一个服务实例
    /// </summary>
    /// <param name="client">client实例</param>
    /// <param name="rootCommandPrefix">指令的前缀, 不需要前缀时请显式指定为空字符串</param>
    public SimCommandService(IClient client, string rootCommandPrefix)
    {
        this.Client = client;
        this.RootCommandPrefix = rootCommandPrefix;
        commands = new();
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
        var matchedNodeTexts = allTextNodes
            .Where(node =>
            {
                var text = node.Text;
                var trimedText = text.AsSpan().Trim();
                if (trimedText.StartsWith(RootCommandPrefix.AsSpan()))
                    return true;
                return false;
            })
            .Select(node => node.Text.Trim());
        foreach (var matchedNodeText in matchedNodeTexts)
        {
            var cmdTextWithoutRootPrefix = matchedNodeText.AsSpan(RootCommandPrefix.Length);
            foreach (var cmd in commands)
            {
                if (cmdTextWithoutRootPrefix.StartsWith(cmd.Name.AsSpan()))
                {
                    //现在我们有一个文本以我们想要的前缀开头
                    //但是可能后面还有其他字符,我们再截取然后看看开头是不是空格
                    var cmdTextWithoutPrefix = cmdTextWithoutRootPrefix.Slice(cmd.Name.Length);
                    if (cmdTextWithoutPrefix.Length != 0)
                    {
                        if (cmdTextWithoutPrefix[0] == ' ')
                        {
                            //现在开头是空格了, 
                            //s/add 1 2, 此时cmdTextWithoutRootPrefix就是"add 1 2", 并且已经找到了想要的add指令
                            //add前缀去除, 得到形如" 1 2"的文本
                            //然后移除前导空格, 得到参数字符串
                            var paramString = cmdTextWithoutPrefix.Slice(1);
                            //此时形如"1 2"
                            //TODO 解析带引号的参数
                            var cmdParams = paramString.ToString().Split(' ');
                            if (cmdParams.Length != cmd.Parameters.Length)
                                continue;
                            if (Activator.CreateInstance(cmd.Method.DeclaringType!) is not CommandModule moduleIns) continue;
                            moduleIns.Content = new(this, msg);
                            if (moduleIns.PreCheck(moduleIns.Content))
                            {
                                cmd.Method.Invoke(moduleIns, cmdParams);
                            }
                        }
                    }
                    else
                    {
                        if (Activator.CreateInstance(cmd.Method.DeclaringType!) is not CommandModule moduleIns) continue;
                        moduleIns.Content = new(this, msg);
                        if (moduleIns.PreCheck(moduleIns.Content))
                        {
                            cmd.Method.Invoke(moduleIns, null);
                        }
                    }
                }
            }
        }
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