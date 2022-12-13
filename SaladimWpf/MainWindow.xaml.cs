using System.Windows;
using Saladim.SalLogger;
using SaladimQBot.GoCqHttp;
using SaladimQBot.Shared;

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

    private void UpdateGuessNumBotDelay_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
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

    private void ClearOutPutButton_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        TextBoxLogging.Text = "";
        logger.LogInfo("WpfConsole", "Wpf控制台清空完成.");
    }

    private void GuessNumBotCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        bot.OpenGuessNumberBot = true;
        logger.LogInfo("WpfConsole", "开启自动猜数.");
    }

    private void GuessNumBotCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        bot.OpenGuessNumberBot = false;
        logger.LogInfo("WpfConsole", "关闭自动猜数.");
    }

    private async void ClientStateCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        logger.LogInfo("WpfConsole", "开启Client中...");
        try
        {
            await bot.StartAsync().ConfigureAwait(false);
        }
        catch (ClientException ex)
        {
            logger.LogWarn("WpfConsole", "Exception", ex);
            return;
        }
        logger.LogInfo("WpfConsole", "开启完成.");
    }

    private async void ClientStateCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        logger.LogInfo("WpfConsole", "关闭Client中...");
        try
        {
            await bot.StopAsync().ConfigureAwait(false);
        }
        catch (ClientException ex)
        {
            logger.LogWarn("WpfConsole", "Exception", ex);
            return;
        }
        logger.LogInfo("WpfConsole", "关闭完成.");
    }

    private void FlushLogButton_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        logger.LogInfo("WpfConsole", "Flush完成.");
        App.Current.Cast<App>().streamWriter.Flush();
    }
}
