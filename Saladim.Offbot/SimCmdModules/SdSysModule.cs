using Saladim.Offbot.Services;
using SaladimQBot.Core;
using SaladimQBot.Extensions;

namespace Saladim.Offbot.SimCmdModules;

public class SdSysModule : CommandModule
{
    private readonly SdSysService sdSysService;

    public SdSysModule(SdSysService sdSysService)
    {
        this.sdSysService = sdSysService;
    }

    [Command("sd_check")]
    public void Check()
    {
        Content.MessageWindow.SendMessageAsync(
            Content.Client.CreateMessageBuilder()
                .WithAt(Content.Executor)
                .WithText($"您现在拥有 {sdSysService.GetUserSd(Content.Executor.UserId):###.###} sd.")
                .Build()
                );
    }

    [Command("sd_add")]
    public void AddCheck(long count)
    {
        var before = sdSysService.GetUserSd(Content.Executor.UserId);
        var after = before + count;
        sdSysService.SetUserSd(Content.Executor.UserId, after);
        Content.MessageWindow.SendMessageAsync(Content.Client.CreateTextOnlyEntity($"您的sd变化: {before} -> {after}"));
    }
}
