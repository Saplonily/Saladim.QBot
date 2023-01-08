using Microsoft.Extensions.DependencyInjection;
using SaladimQBot.Core;
using SaladimQBot.Extensions;

namespace SaladimWpf.Services;

public class Auto1A2BService
{
    protected readonly CoroutineService coroutineService;
    protected readonly IServiceProvider serviceProvider;

    protected IEnumerator<EventWaiter>? currentCoroutine;

    public Auto1A2BService(IServiceProvider serviceProvider, CoroutineService coroutineService)
    {
        this.serviceProvider = serviceProvider;
        this.coroutineService = coroutineService;
    }

    public IEnumerator<EventWaiter> Auto1A2BCoroutine(IMessage msg)
    {
        HashSet<string> currentPossibilities = new();
        _ = msg.MessageWindow.SendMessageAsync("开始1a2b");
        IGroupMessage announceMessage = null!;
        yield return new MessageWaiter(
            msg => msg is IGroupMessage groupMessage && (
                groupMessage.MessageEntity.RawString == "1A2B开始啦，来猜结果吧~" ||
                (groupMessage.MessageEntity.MentionedSelf() && groupMessage.MessageEntity.RawString.Contains("1A2B已经开始啦，不能重复开启哦~"))
                ),
            msg => announceMessage = (IGroupMessage)msg
            );
        IGroupUser game1A2BAnnounceUser = announceMessage.Sender;
        string curGuess = "1234";
        _ = msg.MessageWindow.SendMessageAsync("我猜1234");
        while (true)
        {
            IGroupMessage newMsg = null!;
            yield return new MessageWaiter(game1A2BAnnounceUser, msg => newMsg = msg);
            string msgStr = newMsg.MessageEntity.RawString;
            if (msgStr.StartsWith("猜错咯，"))
            {
                if (msgStr.Contains('A') && msgStr.Contains('B'))
                {
#if !DEBUG
                    Task.Delay(2000).Wait();
#endif
                    string abInfoStr = msgStr[(msgStr.IndexOf('A') - 1)..(msgStr.IndexOf('B') + 1)];
                    int aCount = int.Parse(abInfoStr[..1]);
                    int bCount = int.Parse(abInfoStr.Substring(2, 1));
                    var possibilities =
                        CaculateHelper.Game1A2B.Game1A2BGetAllPossibility(curGuess!.ToCharArray(), aCount, bCount);
                    var lastCurrentAuto1A2bHashSet = new HashSet<string>();
                    foreach (var item in currentPossibilities)
                        lastCurrentAuto1A2bHashSet.Add(item);
                    if (currentPossibilities.Count == 0)
                        currentPossibilities.UnionWith(possibilities);
                    else
                    {
                        currentPossibilities.IntersectWith(possibilities);
                        currentPossibilities.ExceptWith(new string[] { curGuess });
                    }
                    Random r = serviceProvider.GetRequiredService<RandomService>().Random;
                    if (currentPossibilities.Count == 0)
                    {
                        _ = newMsg.MessageWindow.SendMessageAsync("内部计算程序出现错误");
                        yield break;
                    }
                    string resultStr = currentPossibilities.ElementAt(r.Next(currentPossibilities.Count));
                    _ = newMsg.MessageWindow.SendMessageAsync($"我猜{resultStr}");
                    curGuess = resultStr;
                }
            }
            if (msgStr.StartsWith("猜对啦，本局1A2B结束，本次结果为"))
                yield break;
        }
    }

    public async Task Middleware(IMessage msg, Func<Task> next)
    {
        if (msg is IGroupMessage)
        {
            var me = msg.MessageEntity;
            var msgStr = me.RawString;
            if (msgStr.Trim().Equals("开始自动1a2b", StringComparison.OrdinalIgnoreCase))
            {
                if (currentCoroutine is null || !coroutineService.IsRunning(currentCoroutine))
                {
                    currentCoroutine = Auto1A2BCoroutine(msg);
                    coroutineService.AddNewCoroutine(currentCoroutine);
                }
                else
                {
                    await msg.MessageWindow.SendMessageAsync("自动1a2b机已经开始了哦~").ConfigureAwait(false);
                }
            }
        }
        await next();

    }
}