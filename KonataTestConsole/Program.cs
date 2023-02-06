using System.Text.Json;
using System.Text.Json.Nodes;
using Konata.Core;
using Konata.Core.Common;
using Konata.Core.Events.Model;
using Konata.Core.Interfaces;
using Konata.Core.Interfaces.Api;
using Konata.Core.Message;
using SaladimQBot.Konata;

namespace KonataTestConsole;

public class Program
{
    public static async Task Main(string[] args)
    {
        KqClient client = new();

        Bot bot = BotFather.Create(BotConfig.Default(), BotDevice.Default(), GetKeyStore());

        bot.OnLog += (s, e) =>
        {
            if (e.Level is not Konata.Core.Events.LogLevel.Verbose)
                Console.WriteLine(e.EventMessage);
        };

        bot.OnCaptcha += (s, e) =>
        {
            switch (e.Type)
            {
                case CaptchaEvent.CaptchaType.Sms:
                    Console.WriteLine($"请检查 {e.Phone} 的验证码信息: ");
                    s.SubmitSmsCode(Console.ReadLine());
                    break;

                case CaptchaEvent.CaptchaType.Slider:
                    Console.WriteLine($"{e.SliderUrl}\n请输入上述验证码完成所获ticket");
                    s.SubmitSliderTicket(Console.ReadLine());
                    break;

                default:
                case CaptchaEvent.CaptchaType.Unknown:
                    break;
            }
        };

        //bool logined = await bot.Login();
        //if (logined)
        //{
        //    File.WriteAllText("keystore.json", JsonSerializer.Serialize(bot.KeyStore));
        //}

        //bot.OnGroupMessage += (s, e) =>
        //{
        //    Console.WriteLine($"{e.MemberUin}在{e.GroupUin}里说: {e.Message.Chain}");
        //    JsonArray rootArray = new();
        //    foreach (var chainNode in e.Message.Chain)
        //    {
        //        rootArray.Add(JsonSerializer.SerializeToNode(chainNode, chainNode.GetType()));
        //    }
        //    string json = rootArray.ToJsonString();
        //    Console.WriteLine(json);
        //};
    }

    public static BotKeyStore? GetKeyStore()
    {
        if (File.Exists("keystore.json"))
        {
            return JsonSerializer.Deserialize<BotKeyStore>(File.ReadAllText("keystore.json"));
        }

        Console.WriteLine("For first running, please " +
                          "type your account and password.");

        Console.Write("Account: ");
        var account = Console.ReadLine();

        Console.Write("Password: ");
        var password = Console.ReadLine();

        Console.WriteLine("Bot created.");
        var keyStore = new BotKeyStore(account, password);
        File.WriteAllText("keystore.json", JsonSerializer.Serialize(keyStore));
        return keyStore;
    }
}
