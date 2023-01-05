using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Saladim.SalLogger;
using SaladimQBot.GoCqHttp;
using SaladimWpf.Services;

namespace SaladimWpf;

public partial class MainWindow : Window
{
    protected Logger logger;
    protected SalLoggerService salLoggerService;
    protected SaladimWpfService swpfService;

    public MainWindow()
    {
        InitializeComponent();
        App app = App.Current;
        swpfService = app.AppHost.Services.GetRequiredService<SaladimWpfService>();
        salLoggerService = app.AppHost.Services.GetRequiredService<SalLoggerService>();
        salLoggerService.OnLog += s =>
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                TextBoxLogging?.AppendText(s + Environment.NewLine);
                if (ScrollToEndCheckBox?.IsChecked is true)
                {
                    TextBoxLogging?.ScrollToEnd();
                }
            });
        };
        logger = salLoggerService.SalIns;
        ClientStateCheckBox.IsChecked = true;
        
    }

    private void UpdateGuessNumBotDelay_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        if (int.TryParse(GuessNumBotDelayTextBox.Text, out int v))
        {
            swpfService.GuessNumberBotDelay = v;
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
        swpfService.OpenGuessNumberBot = true;
        logger.LogInfo("WpfConsole", "开启自动猜数.");
    }

    private void GuessNumBotCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        swpfService.OpenGuessNumberBot = false;
        logger.LogInfo("WpfConsole", "关闭自动猜数.");
    }

    private async void ClientStateCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        logger.LogInfo("WpfConsole", "开启Client中...");
        try
        {
            await swpfService.StartAsync().ConfigureAwait(false);
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
            await swpfService.StopAsync().ConfigureAwait(false);
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
        salLoggerService.FlushFileStream();
        logger.LogInfo("WpfConsole", "Flush完成.");
    }
}
