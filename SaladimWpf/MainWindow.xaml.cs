using System.Windows;
using Saladim.SalLogger;
using SaladimQBot.GoCqHttp;

namespace SaladimWpf;

public partial class MainWindow : Window
{
    protected Logger logger;
    protected BotInstance bot;

    public MainWindow()
    {
        InitializeComponent();
        App app = (Application.Current as App)!;
        app.TextBoxLogging = TextBoxLogging;
        app.ScrollToEndCheckBox = ScrollToEndCheckBox;
        logger = app.Logger;
        bot = new("ws://127.0.0.1:5000");
        bot.OnLog += s =>
        {
            logger.LogRaw(LogLevel.Info, s);
        };
        bot.OnClientLog += s =>
        {
            logger.LogInfo("Client", s);
        };
    }

    private async void StartClientButton_ClickAsync(object sender, RoutedEventArgs e)
    {
        logger.LogInfo("WpfConsole", "开启Client中...");
        try
        {
            await bot.StartAsync();
        }
        catch (ClientException ex)
        {
            logger.LogWarn("WpfConsole", "Exception", ex);
            return;
        }
        logger.LogInfo("WpfConsole", "开启完成.");
    }

    private async void StopClientButton_ClickAsync(object sender, RoutedEventArgs e)
    {
        logger.LogInfo("WpfConsole", "关闭Client中...");
        try
        {
            await bot.StopAsync();
        }
        catch (ClientException ex)
        {
            logger.LogWarn("WpfConsole", "Exception", ex);
            return;
        }
        logger.LogInfo("WpfConsole", "关闭完成.");
    }

    private void StartGuessNumBotButton_Click(object sender, RoutedEventArgs e)
    {
        bot.OpenGuessNumberBot = true;
        logger.LogInfo("WpfConsole", "开启自动猜数.");
    }

    private void StopGuessNumBotButton_Click(object sender, RoutedEventArgs e)
    {
        bot.OpenGuessNumberBot = false;
        logger.LogInfo("WpfConsole", "关闭自动猜数.");
    }

    private void UpdateGuessNumBotDelay_Click(object sender, RoutedEventArgs e)
    {
        if (int.TryParse(GuessNumBotDelayTextBox.Text, out int v))
        {
            bot.GuessNumberBotDelay = v;
            logger.LogInfo("WpfConsole", $"更新完成! 延迟为{v}ms");
        }
        else
        {
            logger.LogInfo("WpfConsole", $"更新失败! 解析值时出错");
        }
    }

    private void ClearOutPut_Click(object sender, RoutedEventArgs e)
    {
        TextBoxLogging.Text = "";
        logger.LogInfo("WpfConsole", "Wpf控制台清空完成.");
    }
}
