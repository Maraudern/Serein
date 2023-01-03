using Microsoft.Web.WebView2.Core;
using Serein.Base;
using Serein.Items;
using System;
using System.Timers;
using System.Windows;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace Serein.Windows.Pages.Function
{
    public partial class Bot : UiPage, IDisposable
    {
        public Bot()
        {
            InitializeComponent();
            ResourcesManager.InitConsole();
            Catalog.Function.Bot?.Dispose();
            Timer UpdateInfoTimer = new Timer(2000) { AutoReset = true };
            UpdateInfoTimer.Elapsed += (sender, e) => UpdateInfos();
            UpdateInfoTimer.Start();
            BotWebBrowser.EnsureCoreWebView2Async(CoreWebView2Environment.CreateAsync(userDataFolder: IO.GetPath("cache", "WebViewData")).GetAwaiter().GetResult());
            Catalog.Function.Bot = this;
        }

        public void Dispose()
        {
            Dispatcher.Invoke(() =>
            {
                BotWebBrowser.Stop();
                BotWebBrowser.Dispose();
            });
        }

        private void BotWebBrowser_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            Logger.Out(LogType.Debug, string.Join(";", Catalog.Function.BotCache));
            Catalog.Function.BotCache.ForEach((Text) => AppendText(Text));
        }

        private void BotWebBrowser_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                BotWebBrowser.CoreWebView2.Settings.IsGeneralAutofillEnabled = false;
                if (!Global.Settings.Serein.DevelopmentTool.EnableDebug)
                {
                    BotWebBrowser.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;
                    BotWebBrowser.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
                    BotWebBrowser.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = false;
                    BotWebBrowser.CoreWebView2.Settings.AreDevToolsEnabled = false;
                    BotWebBrowser.CoreWebView2.Settings.AreHostObjectsAllowed = false;
                    BotWebBrowser.CoreWebView2.Settings.IsSwipeNavigationEnabled = false;
                    BotWebBrowser.CoreWebView2.Settings.IsStatusBarEnabled = false;
                }
                BotWebBrowser.CoreWebView2.Navigate(@"file:\\\" + Global.Path + $"console\\console.html?type=bot&theme={(Theme.GetAppTheme() == ThemeType.Light ? "light" : "dark")}");
            }
            else
            {
                Logger.Out(LogType.Debug, e.InitializationException);
                Logger.MsgBox("控制台加载失败\n" + e.InitializationException.Message, "Serein", 0, 48);
            }
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
            => Websocket.Connect();

        private void Disconnect_Click(object sender, RoutedEventArgs e)
            => Websocket.Close();

        public void AppendText(string Line)
            => Dispatcher.Invoke(() =>
                BotWebBrowser.CoreWebView2.ExecuteScriptAsync($"AppendText(\"{Line.Replace(@"\", "\\").Replace("\"", "\\\"")}\")"));

        private void UpdateInfos()
            => Dispatcher.Invoke(() =>
            {
                Status.Content = Websocket.Status ? "已连接" : "未连接";
                ID.Content = Websocket.Status ? (Matcher.SelfId ?? "-") : "-";
                MessageReceived.Content = Websocket.Status ? (Matcher.MessageReceived ?? "-") : "-";
                MessageSent.Content = Websocket.Status ? (Matcher.MessageSent ?? "-") : "-";
                TimeSpan t = DateTime.Now - Websocket.StartTime;
                Time.Content = Websocket.Status ? t.TotalSeconds < 3600 ? $"{t.TotalSeconds / 60:N1}m" : t.TotalHours < 120 ? $"{t.TotalMinutes / 60:N1}h" : $"{t.TotalHours / 24:N2}d" : "-";
            });
    }
}
