using System;
using Microsoft.Extensions.DependencyInjection;
using SaladimQBot.Core;

namespace SaladimWpf.Services;

public class Auto1A2BService
{
    public enum Auto1A2BState
    {
        Idle,
        WaitForAnnounceStarted,
        GuessWaiting
    }

    protected Auto1A2BState state;
    protected IGroupUser? auto1A2BDoingUser = null;
    protected HashSet<string> currentAuto1A2BHashSet = new();
    protected string? auto1A2BLastGuess = null;

    protected readonly IServiceProvider serviceProvider;

    public Auto1A2BService(IServiceProvider serviceProvider)
    {
        state = Auto1A2BState.Idle;
        this.serviceProvider = serviceProvider;
    }

    public async Task Middleware(IMessage msg, Func<Task> next)
    {
        if (msg is IGroupMessage groupMessage)
        {
            var me = msg.MessageEntity;
            var msgStr = me.RawString;
            switch (state)
            {
                case Auto1A2BState.Idle:
                    if (msgStr.StartsWith("开始自动1a2b", StringComparison.OrdinalIgnoreCase))
                    {
                        currentAuto1A2BHashSet = new();
                        await msg.MessageWindow.SendMessageAsync("开始1a2b").ConfigureAwait(false);
                        state = Auto1A2BState.WaitForAnnounceStarted;
                    }
                    break;
                case Auto1A2BState.WaitForAnnounceStarted:
                    if (msgStr.StartsWith("1A2B开始啦，来猜结果吧~"))
                    {
                        auto1A2BLastGuess = "1234";
                        auto1A2BDoingUser = groupMessage.Sender;
                        await msg.MessageWindow.SendMessageAsync("我猜1234").ConfigureAwait(false);
                        state = Auto1A2BState.GuessWaiting;
                    }
                    break;
                case Auto1A2BState.GuessWaiting:
                    if (auto1A2BLastGuess is null)
                    {
                        state = Auto1A2BState.Idle;
                        break;
                    }
                    bool sameGroup = auto1A2BDoingUser!.Group.GroupId == groupMessage.Group.GroupId;
                    bool sameUser = auto1A2BDoingUser!.UserId == groupMessage.Sender.UserId;
                    if (sameGroup && sameUser)
                    {
                        if (msgStr.StartsWith("猜错咯，"))
                        {
                            if (msgStr.Contains('A') && msgStr.Contains('B'))
                            {
#if !DEBUG
                                await Task.Delay(2000).ConfigureAwait(false);
#endif
                                string abInfoStr = msgStr[(msgStr.IndexOf('A') - 1)..(msgStr.IndexOf('B') + 1)];
                                int aCount = int.Parse(abInfoStr[..1]);
                                int bCount = int.Parse(abInfoStr.Substring(2, 1));
                                var possibilities =
                                    CaculateHelper.Game1A2B.Game1A2BGetAllPossibility(auto1A2BLastGuess!.ToCharArray(), aCount, bCount);
                                var lastCurrentAuto1A2bHashSet = new HashSet<string>();
                                foreach (var item in currentAuto1A2BHashSet)
                                {
                                    lastCurrentAuto1A2bHashSet.Add(item);
                                }
                                if (currentAuto1A2BHashSet.Count == 0)
                                {
                                    currentAuto1A2BHashSet.UnionWith(possibilities);
                                }
                                else
                                {
                                    currentAuto1A2BHashSet.IntersectWith(possibilities);
                                    if (auto1A2BLastGuess != null)
                                        currentAuto1A2BHashSet.ExceptWith(new string[] { auto1A2BLastGuess });
                                }
                                Random r = serviceProvider.GetRequiredService<RandomService>().Random;
                                if (currentAuto1A2BHashSet.Count == 0)
                                {
                                    await groupMessage.MessageWindow.SendMessageAsync("内部计算程序出现错误").ConfigureAwait(false);
                                    state = Auto1A2BState.Idle;
                                    currentAuto1A2BHashSet.Clear();
                                    auto1A2BDoingUser = null;
                                    auto1A2BLastGuess = null;
                                    break;
                                }
                                string resultStr = currentAuto1A2BHashSet.ElementAt(r.Next(currentAuto1A2BHashSet.Count));
                                await msg.MessageWindow.SendMessageAsync($"我猜{resultStr}").ConfigureAwait(false);
                                auto1A2BLastGuess = resultStr;
                            }
                        }
                        if (msgStr.StartsWith("猜对啦，本局1A2B结束，本次结果为"))
                        {
                            state = Auto1A2BState.Idle;
                            currentAuto1A2BHashSet.Clear();
                            auto1A2BDoingUser = null;
                            auto1A2BLastGuess = null;
                        }
                    }
                    break;
            }

        }
        await next();
    }
}
